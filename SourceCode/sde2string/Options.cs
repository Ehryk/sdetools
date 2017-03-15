using System;
using CommandLine;
using CommandLine.Text;

namespace sde2string
{
    // Class to receive parsed values
    public class Options
    {
        #region Command Line Options

        [ValueOption(0)]
        [Option('i', "input", Required = false, HelpText = "Input .sde file to be processed.")]
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

        #endregion

        #region Calculated

        public bool EsriLicenseRequired
        {
            get { return Connect; }
        }

        #endregion

        #region Help Generation

        [ParserState]
        public IParserState LastParserState { get; set; }

        //[HelpOption]
        public string GetUsage()
        {
            string helpText = HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            helpText += "  Example: sde2string Sample.sde -lv \r\n";
            return helpText;
        }

        #endregion
    }
}
