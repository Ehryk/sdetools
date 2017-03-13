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

namespace gdbconfig
{
    public class Program
    {
        public const int SUCCESS = 0;
        public const int FAILURE_UNSPECIFIED = 1;
        public const int FAILURE_ARGUMENTS = 2;
        public const int FAILURE_NO_INPUT = 3;
        public const int FAILURE_FILE_NOT_FOUND = 4;
        public const int FAILURE_CONNECTION = 5;

        static int Main(string[] args)
        {
            int retCode = FAILURE_UNSPECIFIED;
            bool pause = false;

            try
            {
                var options = new Options();

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
                    if (options.Verbose) Console.WriteLine("File Found: {0}", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, options.InputSDEFile));
                    Console.ResetColor();
                    
                    //Establish the SDE Connection and Examine the Connection Properties
                    try
                    {
                        //Checkout a license
                        LicenseHelper.GetArcGISLicense_Basic();

                        if (options.Verbose)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("ESRI License Checkout Successful");
                            Console.ResetColor();
                        }

                        Console.ForegroundColor = ConsoleColor.Green;

                        //if (options.List)
                        //{
                        //    foreach (KeyValuePair<string, string> property in SDEFileHelper.GetDictionaryFromSDEFile(options.InputFile))
                        //    {
                        //        if (options.Bracketless)
                        //            Console.WriteLine(String.Format("{0}={1}", property.Key, property.Value));
                        //        else
                        //            Console.WriteLine(String.Format("[{0}]={1}", property.Key, property.Value));
                        //    }
                        //}
                        //else if (options.Newline)
                        //    Console.Write(SDEFileHelper.GetConnectionStringFromSDEFile(options.InputFile, options.Bracketless));
                        //else
                        //    Console.WriteLine(SDEFileHelper.GetConnectionStringFromSDEFile(options.InputFile, options.Bracketless));

                        retCode = SUCCESS;
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Unable to establish a connection: {0}", e.Message);
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
                            Console.WriteLine("ESRI License Released");
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
