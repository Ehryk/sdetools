using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using CommandLine;
using Core.ApplicationInfo;
using Core.Encodings;
using Core.ArcObjects;
using ESRI.ArcGIS.Geodatabase;

namespace gdbconfig
{
    public class Program
    {
        public const int SUCCESS = 0;
        public const int FAILURE_UNSPECIFIED = 1;
        public const int FAILURE_ARGUMENTS = 2;
        public const int FAILURE_NO_INPUT = 3;
        public const int FAILURE_FILE_NOT_FOUND = 4;
        public const int FAILURE_LICENSE_CHECKOUT = 5;
        public const int FAILURE_CONNECTION = 6;
        public const int FAILURE_EXECUTING = 7;

        static int Main(string[] args)
        {
            int retCode = FAILURE_UNSPECIFIED;
            bool pause = false;

            try
            {
                var options = new Options();
                //log4net.Config.XmlConfigurator.Configure();

                var parser = new Parser(with => with.CaseSensitive = true);
                if (parser.ParseArguments(args, options))
                {
                    // Values are available here
                    pause = options.Pause;

#if DEBUG
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    if (options.Verbose) Console.WriteLine("DEBUG BUILD");
                    Console.ResetColor();
#endif

                    if (options.Version)
                    {
                        if (options.Newline)
                            Console.Write("{0} v{1}", ApplicationInfo.Title, ApplicationInfo.Version);
                        else
                            Console.WriteLine("{0} v{1}", ApplicationInfo.Title, ApplicationInfo.Version);

                        return SUCCESS;
                    }
                    else if (options.Help || args.Any(a => a.Equals("?") || a.Equals("-?") || a.Equals("/?") || a.Equals("/h") || a.Equals("/help") || a.Equals("--?")))
                    {
                        //Show Usage/Help and exit
                        ShowHelp();
                        return SUCCESS;
                    }

                    if (String.IsNullOrWhiteSpace(options.InputSDEFile))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No .sde file provided. Use -h or --help to display usage.");
                        return FAILURE_NO_INPUT;
                    }

                    if (!File.Exists(options.InputSDEFile))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("File not found: {0}", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, options.InputSDEFile));
                        return FAILURE_FILE_NOT_FOUND;
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    if (options.Verbose)
                        Console.WriteLine("File Found: {0}", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, options.InputSDEFile));
                    Console.ResetColor();

                    bool commandSuccess = false;
                    
                    //Establish the SDE Connection and Examine the Connection Properties
                    try
                    {
                        retCode = FAILURE_LICENSE_CHECKOUT;

                        //Checkout appropriate license(s)
                        if (options.EsriLicenseRequired)
                        {
                            LicenseHelper.BindProduct_Desktop();
                            LicenseHelper.GetArcGISLicense_Standard();

                            if (options.Verbose)
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("ESRI License Checkout Successful");
                                Console.ResetColor();
                            }
                        }
                        if (options.ArcFMLicenseRequired)
                        {
                            LicenseHelper.GetArcFMLicense_ArcFM();
                            if (options.Verbose)
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;

                                Console.WriteLine("ArcFM License Checkout Successful");
                                Console.ResetColor();
                            }
                        }

                        retCode = FAILURE_CONNECTION;
                        
                        Console.ForegroundColor = ConsoleColor.Green;

                        IWorkspace workspace = SDEFileHelper.GetWorkspaceFromSDEFile(options.InputSDEFile);
                        
                        retCode = FAILURE_EXECUTING;

                        if (options.Test)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Connection with {0}: Successful", options.InputSDEFile);

                            commandSuccess = true;
                        }
                        else if (options.AddDomain)
                        {
                            var domainName = options.Parameter1;
                            var code = options.Parameter2;
                            var name = options.Parameter3 ?? options.Parameter2;

                            ICodedValueDomain domain = DomainExtensions.GetCodedValueDomain(workspace, domainName);

                            if (domain == null)
                                throw new Exception(String.Format("Domain Not Found: {0}", domainName));

                            Console.WriteLine("Adding Domain (Code|Name) ({0}|{1}) to {2}", code, name, domainName);

                            if (domain.HasCode(code))
                                throw new Exception(String.Format("Domain {0} already has code {1}", domainName, code));

                            if (options.DryRun)
                            {
                                Console.WriteLine("(dry run)");
                                commandSuccess = true;
                            }
                            else
                            {
                                commandSuccess = domain.AddCodedValue(code, name);
                                ((IWorkspaceDomains2)workspace).AlterDomain(domain as IDomain);
                            }
                        }
                        else if (options.OrderDomain)
                        {
                            var domainName = options.Parameter1;
                            var orderBy = options.Parameter2 ?? "CODE";
                            var direction = options.Parameter3 ?? "ASC";

                            Console.WriteLine("Ordering Domain {0} by {1} {2}", domainName, orderBy, direction);

                            ICodedValueDomain2 domain = DomainExtensions.GetCodedValueDomain2(workspace, domainName);

                            if (domain == null)
                                throw new Exception(String.Format("Domain Not Found: {0}", domainName));

                            bool byValue;
                            switch (orderBy.ToUpper())
                            {
                                case "CODE":
                                case "VALUE":
                                default:
                                    byValue = true;
                                    break;

                                case "NAME":
                                case "DESCRIPTION":
                                case "TEXT":
                                    byValue = false;
                                    break;
                            }

                            bool descending;
                            switch (direction.ToUpper())
                            {
                                case "ASC":
                                case "ASCENDING":
                                case "UP":
                                default:
                                    descending = true;
                                    break;

                                case "DSC":
                                case "DESC":
                                case "DESCENDING":
                                case "DOWN":
                                    descending = false;
                                    break;
                            }

                            if (options.DryRun)
                            {
                                Console.WriteLine("(dry run)");
                                commandSuccess = true;
                            }
                            else
                            {
                                commandSuccess = domain.OrderCodedValue(byValue, descending);
                                ((IWorkspaceDomains2)workspace).AlterDomain(domain as IDomain);
                            }
                        }
                        else if (options.ListDomain)
                        {
                            var domainName = options.Parameter1;

                            Console.WriteLine("Listing Domain {0}:", domainName);
                            Console.ForegroundColor = ConsoleColor.White;

                            ICodedValueDomain domain = DomainExtensions.GetCodedValueDomain(workspace, domainName);

                            if (domain == null)
                                throw new Exception(String.Format("Domain Not Found: {0}", domainName));
                            
                            foreach(var code in domain.ListCodedValues())
                            {
                                Console.WriteLine(" - {0} = {1}", code.Key, code.Value);
                            }

                            commandSuccess = true;
                        }
                        else if (options.RemoveDomain)
                        {
                            var domainName = options.Parameter1;
                            var code = options.Parameter2;

                            Console.WriteLine("Removing Domain (CODE) ({0}) from {1}", code, domainName);

                            ICodedValueDomain domain = DomainExtensions.GetCodedValueDomain(workspace, domainName);

                            if (domain == null)
                                throw new Exception(String.Format("Domain Not Found: {0}", domainName));

                            if (!domain.HasCode(code))
                                throw new Exception(String.Format("Domain {0} does not have code {1}", domainName, code));

                            if (options.DryRun)
                            {
                                Console.WriteLine("(dry run)");
                                commandSuccess = true;
                            }
                            else
                            {
                                commandSuccess = domain.RemoveCodedValue(code);
                                ((IWorkspaceDomains2)workspace).AlterDomain(domain as IDomain);
                            }
                        }
                        else if (options.AddClassModelName)
                        {
                            var className = options.Parameter1;
                            var modelName = options.Parameter2;

                            Console.WriteLine("Adding Class Model Name {0} to {1}", modelName, className);

                            IObjectClass objectClass = workspace.GetObjectClass(className);

                            if (objectClass == null)
                                throw new Exception(String.Format("Class Not Found: {0}", className));

                            if (objectClass.HasModelName(modelName))
                                throw new Exception(String.Format("Class {0} already has class model name {1} assigned", className, modelName));

                            if (options.DryRun)
                            {
                                Console.WriteLine("(dry run)");
                                commandSuccess = true;
                            }
                            else
                            {
                                commandSuccess = objectClass.AddClassModelName(modelName);
                            }
                        }
                        else if (options.ListClassModelNames)
                        {
                            var className = options.Parameter1;

                            Console.WriteLine("Listing Class Model Names on {0}:", className);
                            Console.ForegroundColor = ConsoleColor.White;

                            IObjectClass objectClass = workspace.GetObjectClass(className);

                            if (objectClass == null)
                                throw new Exception(String.Format("Class Not Found: {0}", className));

                            foreach (var code in objectClass.ListModelNames())
                            {
                                Console.WriteLine(" - {0}", code);
                            }

                            commandSuccess = true;
                        }
                        else if (options.RemoveClassModelName)
                        {
                            var className = options.Parameter1;
                            var modelName = options.Parameter2;

                            Console.WriteLine("Removing Class Model Name {0} from {1}", modelName, className);

                            IObjectClass objectClass = workspace.GetObjectClass(className);

                            if (!objectClass.HasModelName(modelName))
                                throw new Exception(String.Format("Class {0} does not have a class model name {1} to remove", className, modelName));

                            if (options.DryRun)
                            {
                                Console.WriteLine("(dry run)");
                                commandSuccess = true;
                            }
                            else
                            {
                                commandSuccess = objectClass.RemoveClassModelName(modelName);
                            }
                        }
                        else if (options.AddFieldModelName)
                        {
                            var className = options.Parameter1;
                            var fieldName = options.Parameter2;
                            var modelName = options.Parameter3;
                            
                            Console.WriteLine("Adding Field Model Name {0} to {1}.{2}", modelName, className, fieldName);

                            IObjectClass objectClass = workspace.GetObjectClass(className);
                            if (objectClass == null)
                                throw new Exception(String.Format("Class Not Found: {0}", className));

                            IField field = objectClass.GetField(fieldName);
                            if (objectClass == null)
                                throw new Exception(String.Format("Field {0} Not Found on Class {1}", fieldName, className));

                            if (objectClass.HasFieldModelName(field, modelName))
                                throw new Exception(String.Format("Field {0}.{1} already has field model name {2} attached", className, fieldName, modelName));

                            if (options.DryRun)
                            {
                                Console.WriteLine("(dry run)");
                                commandSuccess = true;
                            }
                            else
                            {
                                commandSuccess = objectClass.AddFieldModelName(field, modelName);
                            }
                        }
                        else if (options.ListFieldModelNames)
                        {
                            var className = options.Parameter1;
                            var fieldName = options.Parameter2;

                            Console.WriteLine("Listing Class Model Names on {0}:", className);
                            Console.ForegroundColor = ConsoleColor.White;

                            IObjectClass objectClass = workspace.GetObjectClass(className);
                            if (objectClass == null)
                                throw new Exception(String.Format("Class Not Found: {0}", className));

                            IField field = objectClass.GetField(fieldName);
                            if (objectClass == null)
                                throw new Exception(String.Format("Field {0} Not Found on Class {1}", fieldName, className));

                            foreach (var code in objectClass.ListFieldModelNames(field))
                            {
                                Console.WriteLine(" - {0}", code);
                            }

                            commandSuccess = true;
                        }
                        else if (options.RemoveFieldModelName)
                        {
                            var className = options.Parameter1;
                            var fieldName = options.Parameter2;
                            var modelName = options.Parameter3;
                            
                            Console.WriteLine("Removing Field Model Name {0} from {1}.{2}", modelName, className, fieldName);

                            IObjectClass objectClass = workspace.GetObjectClass(className);
                            if (objectClass == null)
                                throw new Exception(String.Format("Class Not Found: {0}", className));

                            IField field = objectClass.GetField(fieldName);
                            if (objectClass == null)
                                throw new Exception(String.Format("Field {0} Not Found on Class {1}", fieldName, className));

                            if (!objectClass.HasFieldModelName(field, modelName))
                                throw new Exception(String.Format("Field {0}.{1} does not have a field model name {2} to remove", className, fieldName, modelName));

                            if (options.DryRun)
                            {
                                Console.WriteLine("(dry run)");
                                commandSuccess = true;
                            }
                            else
                            {
                                commandSuccess = objectClass.RemoveFieldModelName(field, modelName);
                            }
                        }
                        else
                        {
                            retCode = FAILURE_ARGUMENTS;
                            commandSuccess = true;
                        }

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine();
                        Console.Write("Command Result: ");
                        Console.ForegroundColor = commandSuccess ? ConsoleColor.Green : ConsoleColor.Red;
                        Console.WriteLine(commandSuccess ? "Success" : "Failure");
                        
                        retCode = commandSuccess ? SUCCESS : FAILURE_EXECUTING;
                    }
                    catch (Exception e)
                    {
                        string exceptionType = "Exception";

                        switch (retCode)
                        {
                            case FAILURE_LICENSE_CHECKOUT:
                                exceptionType = "License Exception";
                                break;

                            case FAILURE_CONNECTION:
                                exceptionType = "Connection Exception";
                                break;

                            case FAILURE_EXECUTING:
                                exceptionType = "Command Exception";
                                break;
                        }

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("{0}: {1}", exceptionType, e.Message);
#if DEBUG
                        Console.WriteLine();
                        Console.WriteLine("Stack Trace: {0}", e.StackTrace);
#endif
                    }
                    finally
                    {
                        LicenseHelper.ReleaseLicenses();

                        if (options.Verbose)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("License(s) Released");
                        }
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Could not parse arguments. Use -h or --help to display usage.");
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Exception: {0}", e.Message);
#if DEBUG
                Console.WriteLine();
                Console.WriteLine("Stack Trace: {0}", e.StackTrace);
#endif
            }
            finally
            {
                if (pause)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }

                Console.ResetColor();
            }

            return retCode;
        }
        
        public static void ShowHelp()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" === {0} v{1}.{2} ===", ApplicationInfo.Title, ApplicationInfo.Version.Major, ApplicationInfo.Version.Minor);
            Console.ResetColor();

            Console.WriteLine("Performs configuration tasks against an ESRI geodatabase");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Usage and Examples: ");
            Console.ResetColor();
            Console.WriteLine(" > gdbconfig.exe -h");
            Console.WriteLine(" > gdbconfig --test -i=connection.sde");
            Console.WriteLine(" > gdbconfig gdb.sde --add-domain Domain Value [Name]");
            Console.WriteLine(" > gdbconfig gdb.sde --list-domain Domain");
            Console.WriteLine(" > gdbconfig gdb.sde --order-domain Domain [VALUE|Name] [ASC|DESC]");
            Console.WriteLine(" > gdbconfig gdb.sde --remove-domain Domain Value");
            Console.WriteLine(" > gdbconfig gdb.sde --add-class-model-name ClassName ModelName");
            Console.WriteLine(" > gdbconfig gdb.sde --list-class-model-name ClassName");
            Console.WriteLine(" > gdbconfig gdb.sde --remove-class-model-name ClassName ModelName");
            Console.WriteLine(" > gdbconfig gdb.sde --add-field-model-name ClassName FieldName ModelName");
            Console.WriteLine(" > gdbconfig gdb.sde --list-field-model-name ClassName FieldName");
            Console.WriteLine(" > gdbconfig gdb.sde --remove-field-model-name ClassName FieldName ModelName");
            Console.WriteLine(" > gdbconfig --dry-run -p1 Domain -p2 Value -p3 Name --input connection.sde -Vp");
            Console.WriteLine(" > gdbconfig [? | /? | -? | -h | --help | --version]");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Options");
            Console.ResetColor();
            Console.WriteLine(" -i/--input     : Path to an .sde file for connection to an ESRI geodatabse");
            Console.WriteLine(" -t/--test      : Test the connnection and exit");
            Console.WriteLine(" -d/--dry-run   : Perform a dry run without making changes");
            Console.WriteLine(" -v/--version   : Report the version and exit");
            Console.WriteLine(" -V/--verbose   : Add additional output");
            Console.WriteLine(" -n/--nonewline : Output without trailing newline");
            Console.WriteLine(" -p/--pause     : Pause before exiting");
        }
    }
}
