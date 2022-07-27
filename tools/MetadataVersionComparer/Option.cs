using CommandLine;
using CommandLine.Text;

namespace MetadataVersionComparer
{
  public class Option
  {
    [Option('n', "new", HelpText = "Path to folder with new applied metadata.", Required = true)]
    public string NewVersionFolder { get; set; }

    [Option('o', "old", HelpText = "Path to folder with old applied metadata.", Required = true)]
    public string OldVersionFolder { get; set; }

    [Option('d', "diff", HelpText = "Path to file with diff data.", Required = true)]
    public string DiffPath { get; set; }

    [HelpOption]
    public string GetUsage()
    {
      return HelpText.AutoBuild(this, (current) => HelpText.DefaultParsingErrorsHandler(this, current));
    }
  }
}