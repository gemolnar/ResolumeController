using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResolumeController
{
    // Define a class to receive parsed values
    class ProgramOptions
    {
        [Option('i', "ResolumeInputPort", DefaultValue = 7001, HelpText = "Will listen on this port to OSC messages sent by Resolume.")]
        public int ResolumeInputPort { get; set; }

        [Option('o', "ResolumeOutputPort", DefaultValue = 7000, HelpText = "Will send OSC messages to this port (Resolume should be listening on this port).")]
        public int ResolumeOutputPort { get; set; }


        [Option('v', "verbose", DefaultValue = true,
          HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
