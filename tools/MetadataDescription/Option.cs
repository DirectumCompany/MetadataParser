using CommandLine;
using CommandLine.Text;

namespace MetadataDescription
{
    public class Option
    {
        [Option('m', "module", HelpText = "Module name.", Required = true)]
        public string ModuleName { get; set; }

        [Option('e', "entity", HelpText = "Entity name.", Required = true)]
        public string EntityName { get; set; }

        [Option('f', "file format", HelpText = "Output file format: txt or rst.", Required = true)]
        public string FileFormat { get; set; }
        
        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}