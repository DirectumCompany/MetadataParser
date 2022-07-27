using CommandLine;
using CommandLine.Text;

namespace MetadataParser
{
  public class Option
  {
    [Option('s', "source", HelpText = "Path to folder with functions.", Required = true)]
    public string SourceFolder { get; set; }

    [Option('t', "target", HelpText = "Path to file with parsed data.", Required = true)]
    public string TargetPath { get; set; }

    [HelpOption]
    public string GetUsage()
    {
      return HelpText.AutoBuild(this, (current) => HelpText.DefaultParsingErrorsHandler(this, current));
    }
  }
}