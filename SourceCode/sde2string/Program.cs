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

namespace sde2string
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
                //log4net.Config.XmlConfigurator.Configure();

                var parser = new Parser(with => { with.CaseSensitive = true; with.MutuallyExclusive = true; });
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
                            //Checkout a license
                            LicenseHelper.GetArcGISLicense_Basic();

                            if (options.Verbose)
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("ESRI License Checkout Successful");
                                Console.ResetColor();
                            }

                            Console.ForegroundColor = ConsoleColor.Green;

                            if (options.List)
                            {
                                foreach (KeyValuePair<string, string> property in SDEFileHelper.GetDictionaryFromSDEFile(options.InputFile))
                                {
                                    if (options.Bracketless)
                                        Console.WriteLine(String.Format("{0}={1}", property.Key, property.Value));
                                    else
                                        Console.WriteLine(String.Format("[{0}]={1}", property.Key, property.Value));
                                }
                            }
                            else if (options.Newline)
                                Console.Write(SDEFileHelper.GetConnectionStringFromSDEFile(options.InputFile, options.Bracketless));
                            else
                                Console.WriteLine(SDEFileHelper.GetConnectionStringFromSDEFile(options.InputFile, options.Bracketless));

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
                            retCode = FAILURE_CONNECTION;
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

        public static void ShowHelp()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" === {0} v{1}.{2} ===", ApplicationInfo.Title, ApplicationInfo.Version.Major, ApplicationInfo.Version.Minor);
            Console.ResetColor();

            Console.WriteLine("Decodes .sde file contents");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Usage and Examples: ");
            Console.ResetColor();
            Console.WriteLine(" > sde2string.exe -h");
            Console.WriteLine(" > sde2string -input=connection.sde {Options}");
            Console.WriteLine(" > sde2string gdb.sde --connect --list");
            Console.WriteLine(" > sde2string gdb.sde -cl");
            Console.WriteLine(" > sde2string gdb.sde -clVbpn");
            Console.WriteLine(" > sde2string -v");
            Console.WriteLine(" > sde2string --list --connect --input connection.sde -Vp");
            Console.WriteLine(" > sde2string [? | /? | -? | -h | --help | --version]");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Options:");
            Console.ResetColor();
            Console.WriteLine(" -i/--input       : Input .sde file to be processed");
            Console.WriteLine(" -c/--connect     : Attempts to establish a connection and examines result");
            Console.WriteLine(" -V/--verbose     : Show additional output");
            Console.WriteLine(" -r/--raw         : Output the raw (semi-parsed) contents of the .sde file as ascii");
            Console.WriteLine(" -l/--list        : Lists each property on a single line");
            Console.WriteLine(" -u/--unparsed    : Output the raw (unparsed) contents of the .sde file as ascii");
            Console.WriteLine(" -b/--bracketless : Display key values without surrounding brackets");
            Console.WriteLine(" -e/--encoding    : Specify the .sde encoding. May cause errors without -u (unparsed)");
            Console.WriteLine(" -n/--nonewline   : Output without trailing newline");
            Console.WriteLine(" -p/--pause       : Pause for a key press before terminating");
            Console.WriteLine(" -v/--version     : Display Version and Exit");
            Console.WriteLine(" -h/--help        : Display this help (also ?, -?, /?, or no input)");
            Console.ResetColor();
            Console.WriteLine(" Encodings: DEFAULT, ASCII, UTF7, UTF8, UTF16, UTF32");
        }
    }
}
