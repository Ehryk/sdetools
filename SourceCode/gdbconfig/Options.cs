﻿using System;
using CommandLine;
using CommandLine.Text;

namespace gdbconfig
{
    // Class to receive parsed values
    public class Options
    {
        [ValueOption(0)]
        [Option('i', "input", Required = false, HelpText = "Input .sde file for connection")]
        public string InputSDEFile { get; set; }

        #region Modes

        [Option("version", DefaultValue = false, HelpText = "Display Version and Exit")]
        public bool Version { get; set; }

        [Option("add-domain", DefaultValue = false, HelpText = "Add a Domain value")]
        public bool AddDomain { get; set; }

        [Option("order-domain", DefaultValue = false, HelpText = "Order a Domain")]
        public bool OrderDomain { get; set; }

        [Option("remove-domain", DefaultValue = false, HelpText = "Remove a Domain value")]
        public bool RemoveDomain { get; set; }

        [Option("add-class-model-name", DefaultValue = false, HelpText = "Add a Class Model Name")]
        public bool AddClassModelName { get; set; }

        [Option("remove-class-model-name", DefaultValue = false, HelpText = "Remove a Class Model Name")]
        public bool RemoveClassModelName { get; set; }

        [Option("add-field-model-name", DefaultValue = false, HelpText = "Add a Field Model Name")]
        public bool AddFieldModelName { get; set; }

        [Option("remove-field-model-name", DefaultValue = false, HelpText = "Remove a Field Model Name")]
        public bool RemoveFieldModelName { get; set; }

        #endregion

        #region Parameters

        [ValueOption(1)]
        [Option("p1", Required = false, HelpText = "Parameter 1")]
        public string Parameter1 { get; set; }

        [ValueOption(2)]
        [Option("p2", Required = false, HelpText = "Parameter 2")]
        public string Parameter2 { get; set; }

        [ValueOption(3)]
        [Option("p3", Required = false, HelpText = "Parameter 3")]
        public string Parameter3 { get; set; }

        [ValueOption(4)]
        [Option("p4", Required = false, HelpText = "Parameter 4")]
        public string Parameter4 { get; set; }

        #endregion

        #region Modifiers

        [Option('v', "verbose", DefaultValue = false, HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        [Option('d', "dry-run", DefaultValue = false, HelpText = "Connects and validates, but does not perform any modifications.")]
        public bool DryRun { get; set; }
        
        [Option('p', "pause", DefaultValue = false, HelpText = "Pause for a key press before terminating.")]
        public bool Pause { get; set; }

        [Option('n', "newline", DefaultValue = false, HelpText = "Do not output the trailing newline.")]
        public bool Newline { get; set; }

        #endregion

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
    }
}
