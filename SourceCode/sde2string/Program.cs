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
    // Class to receive parsed values
    class Options
    {
        [ValueOption(0)]
        [Option('i', "input", Required = false, HelpText = "Input file to be processed.")]
        public string InputFile { get; set; }

        [Option('c', "connect", DefaultValue = false, HelpText = "Establishes the SDE Connection and examines with ESRI libraries.")]
        public bool Connect { get; set; }

        [Option('v', "verbose", DefaultValue = false, HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        [Option('l', "list", DefaultValue = false, HelpText = "Lists each property on a single line.")]
        public bool List { get; set; }

        [Option('b', "bracketless", DefaultValue = false, HelpText = "Remove the brackets from the keys.")]
        public bool Bracketless { get; set; }

        [Option('p', "pause", DefaultValue = false, HelpText = "Pause for a key press before terminating.")]
        public bool Pause { get; set; }

        [Option('n', "newline", DefaultValue = false, HelpText = "Do not output the trailing newline.")]
        public bool Newline { get; set; }

        [Option('r', "raw", DefaultValue = false, HelpText = "Output the raw (semi-parsed) contents of the .sde file as ascii.")]
        public bool Raw { get; set; }

        [Option('u', "unparsed", DefaultValue = false, HelpText = "Output the raw (unparsed) contents of the .sde file as ascii.")]
        public bool Unicode { get; set; }

        [Option('e', "encoding", DefaultValue = "ASCII", HelpText = "Specify the .sde encoding. May cause errors without -u (unparsed).")]
        public String Encoding { get; set; }

        [Option("version", DefaultValue = false, HelpText = "Display Version and Exit.")]
        public bool Version { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            string helpText = HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            helpText += "  Example: sde2string Sample.sde -lv \r\n";
            return helpText;
        }
    }

    class Program
    {
        private static esriLicenseProductCode licenseProductCode = esriLicenseProductCode.esriLicenseProductCodeBasic;

        static void Main(string[] args)
        {
            try
            {
                IAoInitialize license = null;
                var options = new Options();
                bool pause = false;

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
                        return;
                    }

                    if (options.InputFile == null)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(options.GetUsage());
                        Console.ResetColor();
                        return;
                    }

                    if (!File.Exists(options.InputFile))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("File not found: {0}", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, options.InputFile));
                        Console.ResetColor();
                        return;
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
                        string unicode = Encoding.GetEncoding(options.Encoding).GetString(File.ReadAllBytes(options.InputFile));

                        //Raw
                        //string ascii = contents;

                        //Regex
                        string ascii = Regex.Replace(unicode, @"[^\u0020-\u007F|\u0000]", string.Empty);
                        //ascii = System.Text.RegularExpressions.Regex.Replace(ascii, @"\u0000+", " ");

                        //.NET
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
                            Console.Write(options.Unicode ? unicode : options.Raw ? raw : parsed);
                        else
                            Console.WriteLine(options.Unicode ? unicode : options.Raw ? raw : parsed);
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Could not parse arguments.");
                }

                Console.ResetColor();

#if DEBUG
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
#else
                if (pause)
                {
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
#endif
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
                Console.ResetColor();
            }
        }
    }
}
