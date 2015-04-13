using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

using CommandLine;
using CommandLine.Text;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace sde2string
{
    public class Program
    {
        public const int SUCCESS = 0;
        public const int FAILURE_UNSPECIFIED = 1;
        public const int FAILURE_ARGUMENTS = 2;
        public const int FAILURE_NO_INPUT = 3;
        public const int FAILURE_FILE_NOT_FOUND = 4;
        
        private static esriLicenseProductCode licenseProductCode = esriLicenseProductCode.esriLicenseProductCodeBasic;

        static int Main(string[] args)
        {
            int retCode = FAILURE_UNSPECIFIED;
            bool pause = false;

            try
            {
                IAoInitialize license = null;
                var options = new Options();

                if (CommandLine.Parser.Default.ParseArgumentsStrict(args, options))
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

                    if (options.InputFile == null)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(options.GetUsage());
                        Console.ResetColor();
                        return FAILURE_NO_INPUT;
                    }

                    if (!File.Exists(options.InputFile))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("File not found: {0}", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, options.InputFile));
                        Console.ResetColor();
                        return FAILURE_FILE_NOT_FOUND;
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    if (options.Verbose) Console.WriteLine("File Found: {0}", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, options.InputFile));
                    Console.ResetColor();

                    if (options.Connect)
                    {
                        //Establish the SDE Connection and Examine the Connection Properties
                        try
                        {
                            license = GDBUtilities.CheckoutESRILicense(licenseProductCode);

                            if (options.Verbose && license != null)
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("License Checkout Successful: {0}", licenseProductCode);
                            }

                            Console.ForegroundColor = ConsoleColor.Green;

                            if (options.List)
                            {
                                foreach (KeyValuePair<string, object> property in GDBUtilities.PropertySetToDictionary(GDBUtilities.GetPropertySetFromSDEFile(options.InputFile)))
                                {
                                    if (options.Bracketless)
                                        Console.WriteLine(String.Format("{0}={1}", property.Key, property.Value));
                                    else
                                        Console.WriteLine(String.Format("[{0}]={1}", property.Key, property.Value));
                                }
                            }
                            else if (options.Newline)
                                Console.Write(GDBUtilities.GetConnectionStringFromSDEFile(options.InputFile, options.Bracketless));
                            else
                                Console.WriteLine(GDBUtilities.GetConnectionStringFromSDEFile(options.InputFile, options.Bracketless));

                            retCode = SUCCESS;
                        }
                        finally
                        {
                            GDBUtilities.ReturnESRILicense(license);
                            if (options.Verbose)
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("License Released: {0}", licenseProductCode);
                            }
                        }
                    }
                    else
                    {
                        //Examine the raw hex of the .sde
                        Console.ForegroundColor = ConsoleColor.Green;
                        string encoded = Encodings.GetEncoding(options.Encoding).GetString(File.ReadAllBytes(options.InputFile));

                        //Regex Conversion
                        string ascii = Regex.Replace(encoded, @"[^\u0020-\u007F|\u0000]", string.Empty);

                        //.NET Conversion
                        //string ascii = Encoding.ASCII.GetString(
                        //    Encoding.Convert(
                        //        Encoding.UTF8,
                        //        Encoding.GetEncoding(
                        //            Encoding.ASCII.EncodingName,
                        //            new EncoderReplacementFallback(string.Empty),
                        //            new DecoderExceptionFallback()
                        //            ),
                        //        Encoding.UTF8.GetBytes(contents)
                        //    )
                        //);

                        //Join Spacing
                        ascii = Regex.Replace(ascii, @"[\u0000]{3,}[\u0008][\u0000].?[\u0000]{3,}", "|");
                        ascii = Regex.Replace(ascii, @"[\u0000]{3,}.?[\u0000]{3,}", "|");
                        ascii = Regex.Replace(ascii, @"[\u0000]{3,}", "|");
                        ascii = Regex.Replace(ascii, @"[\u0000]", String.Empty);
                        ascii = Regex.Replace(ascii, @"\s+", " ");

                        string raw = ascii;

                        //SDE Specific Parsing
                        string parsed;
                        ascii = ascii.Substring(ascii.IndexOf("SERVER"));
                        if (ascii.Contains("VERSION"))
                            ascii = Regex.Replace(ascii, @"PASSWORD.*VERSION", "PASSWORD||VERSION");
                        else
                            ascii = Regex.Replace(ascii, @"PASSWORD.*CONNPROP", "PASSWORD||CONNPROP");
                        ascii = Regex.Replace(ascii, @"Rev1\.0\|Rev.*$", "Rev1.0");
                        ascii = Regex.Replace(ascii, @"Rev1\.0\|ev.*$", "Rev1.0");
                        ascii = Regex.Replace(ascii, @"Rev1\.0\|\.0.*$", "Rev1.0");
                        string[] segments = ascii.Split('|');

                        StringBuilder result = new StringBuilder();
                        for (int i = 0; i < segments.Count(); i++)
                        {
                            if (i % 2 == 0)
                            {
                                if (i == segments.Count() - 1) continue;
                                if (!options.Bracketless) result.Append("[");
                                result.Append(segments[i]);
                                if (!options.Bracketless) result.Append("]");
                                result.Append("=");
                            }
                            else
                            {
                                result.Append(segments[i]);
                                result.Append(";");
                            }
                        }
                        parsed = result.ToString();

                        if (options.List)
                        {
                            foreach (string element in parsed.Split(';').Where(e => !String.IsNullOrWhiteSpace(e)))
                            {
                                Console.WriteLine(element);
                            }
                        }
                        else if (options.Newline)
                            Console.Write(options.Unicode ? encoded : options.Raw ? raw : parsed);
                        else
                            Console.WriteLine(options.Unicode ? encoded : options.Raw ? raw : parsed);
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
