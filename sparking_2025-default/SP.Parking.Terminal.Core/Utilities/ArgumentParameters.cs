using CommandLine;
using CommandLine.Text;
using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Utilities
{
    //public class ArgumentParameter
    //{
    //    public RunMode Mode { get; set; }
    //    public string[] Host { get; set; }
    //    public string TestHost { get; set; }
    //}

    public class ArgumentParameterManager
    {
        [Option('m', "mode", HelpText = "<type: string> (production / test)", DefaultValue = "production")]
        public string RunningMode { get; set; }

        [OptionArray('f', "first", HelpText = "<type: string>")]
        public string[] Host { get; set; }

        [Option('s', "secondary", HelpText = "<type: string>", DefaultValue = "")]
        public string TestHost { get; set; }

        //[Option('v', "version", HelpText = "Get application version")]
        //public bool Version { get; set; }

        //[Option('p', "person-to-greet", Required = true, HelpText = "The person to greet.")]
        //public string PersonToGreet { get; set; }

        ArgumentParameter _parameters;
        public ArgumentParameter Parameters
        {
            get
            {
                return _parameters = _parameters ?? new ArgumentParameter { Host = this.Host, Mode = this.Mode, TestHost = this.TestHost };
            }
            set { _parameters = value; }
        }

        public RunMode Mode
        {
            get
            {
                if (string.Compare(RunningMode, "test", true) == 0)
                    return RunMode.Testing;

                if (string.Compare(RunningMode, "production", true) == 0)
                    return RunMode.Production;

                return RunMode.Production;
            }
        }

        //[ParserState]
        //public IParserState LastParserState { get; set; }

        [HelpOption(HelpText = "Dispaly this help screen.")]
        public string GetUsage()
        {

            var usage = new StringBuilder();
            usage.AppendLine(Assembly.GetExecutingAssembly().GetName().Version.ToString());
            return usage.ToString();

            //HelpText help;
            
            //if (this.LastParserState != null)
            //{
            //    help = new HelpText();

            //    if (this.LastParserState.Errors.Any())
            //    {
            //        var errors = help.RenderParsingErrorsText(this, 2); // indent with two spaces

            //        help.AddPreOptionsLine(string.Concat(Environment.NewLine, "ERROR(S):"));

            //        if (!string.IsNullOrEmpty(errors))
            //        {                        
            //            help.AddPreOptionsLine(errors);
            //        }
            //    }
            //    else
            //    {
            //        help.AddPreOptionsLine("Command does not exist");
            //    }
            //}
            //else
            //{
            //    help = HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            //    help.AddPreOptionsLine(string.Concat(Environment.NewLine, "HELP:"));
            //}

            //return help;

        }
    }
}
