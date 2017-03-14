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

                if (Parser.Default.ParseArgumentsStrict(args, options))
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
                            Console.Write("{0} v{1}", ApplicationInfo.ProductName, ApplicationInfo.Version);
                        else
                            Console.WriteLine("{0} v{1}", ApplicationInfo.ProductName, ApplicationInfo.Version);
                        return SUCCESS;
                    }

                    if (options.InputSDEFile == null)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(options.GetUsage());
                        Console.ResetColor();
                        return FAILURE_NO_INPUT;
                    }

                    if (!File.Exists(options.InputSDEFile))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("File not found: {0}", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, options.InputSDEFile));
                        Console.ResetColor();
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

                        if (options.AddDomain)
                        {
                            var domainName = options.Parameter1;
                            var code = options.Parameter2;
                            var name = options.Parameter3 ?? options.Parameter2;

                            ICodedValueDomain domain = DomainHelper.GetCodedValueDomain(workspace, domainName);

                            if (domain == null)
                                throw new Exception(String.Format("Domain Not Found: {0}", domainName));

                            Console.WriteLine("Adding Domain (Code|Name) ({0}|{1}) to {2}", code, name, domainName);
                            commandSuccess = domain.AddCodedValue(code, name);
                            ((IWorkspaceDomains2)workspace).AlterDomain(domain as IDomain);
                        }
                        else if (options.OrderDomain)
                        {
                            var domainName = options.Parameter1;
                            var orderBy = options.Parameter2 ?? "CODE";
                            var direction = options.Parameter3 ?? "ASC";

                            Console.WriteLine("Ordering Domain {0} by {1} {2}", domainName, orderBy, direction);

                            ICodedValueDomain domain = DomainHelper.GetCodedValueDomain(workspace, domainName);

                            if (domain == null)
                                throw new Exception(String.Format("Domain Not Found: {0}", domainName));

                            throw new NotImplementedException("Domain Ordering not yet implemented");
                            //commandSuccess = domain.OrderDomain(domainName, orderBy, direction);
                            ((IWorkspaceDomains2)workspace).AlterDomain(domain as IDomain);
                        }
                        else if (options.RemoveDomain)
                        {
                            var domainName = options.Parameter1;
                            var code = options.Parameter2;

                            Console.WriteLine("Removing Domain (CODE) ({0}) from {1}", code, domainName);

                            ICodedValueDomain domain = DomainHelper.GetCodedValueDomain(workspace, domainName);

                            if (domain == null)
                                throw new Exception(String.Format("Domain Not Found: {0}", domainName));

                            commandSuccess = domain.RemoveCodedValue(code);
                            ((IWorkspaceDomains2)workspace).AlterDomain(domain as IDomain);
                        }
                        else if (options.AddClassModelName)
                        {
                            var className = options.Parameter1;
                            var modelName = options.Parameter2;

                            Console.WriteLine("Adding Class Model Name {0} to {1}", modelName, className);

                            IObjectClass objectClass = workspace.GetObjectClass(className);

                            commandSuccess = objectClass.AddClassModelName(modelName);
                        }
                        else if (options.RemoveClassModelName)
                        {
                            var className = options.Parameter1;
                            var modelName = options.Parameter2;

                            Console.WriteLine("Removing Class Model Name {0} from {1}", modelName, className);

                            IObjectClass objectClass = workspace.GetObjectClass(className);

                            commandSuccess = objectClass.RemoveClassModelName(modelName);
                        }
                        else if (options.AddFieldModelName)
                        {
                            var className = options.Parameter1;
                            var fieldName = options.Parameter2;
                            var modelName = options.Parameter3;
                            
                            Console.WriteLine("Adding Field Model Name {0} to {1}.{2}", modelName, className, fieldName);

                            IObjectClass objectClass = workspace.GetObjectClass(className);
                            IField field = objectClass.GetField(fieldName);

                            commandSuccess = objectClass.AddFieldModelName(field, modelName);
                        }
                        else if (options.RemoveFieldModelName)
                        {
                            var className = options.Parameter1;
                            var fieldName = options.Parameter2;
                            var modelName = options.Parameter3;
                            
                            Console.WriteLine("Removing Field Model Name {0} from {1}.{2}", modelName, className, fieldName);

                            IObjectClass objectClass = workspace.GetObjectClass(className);
                            IField field = objectClass.GetField(fieldName);

                            commandSuccess = objectClass.RemoveFieldModelName(field, modelName);
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
                    Console.WriteLine("Could not parse arguments.");
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
    }
}
