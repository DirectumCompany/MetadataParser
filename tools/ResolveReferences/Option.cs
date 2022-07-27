using CommandLine;
using CommandLine.Text;

namespace ResolveReferences
{
  public class Option
  {
    [Option('t', "throw", DefaultValue = false, HelpText = "Throw exception for debug.", Required = false)]
    public bool ThrowException { get; set; }

    [Option('m', "missed", DefaultValue = false, HelpText = "Show dependency, where metadata not found.", Required = false)]
    public bool ShowMissedVersions { get; set; }

    [Option('f', "folder", HelpText = "Path to folder with applied metadata.", Required = true)]
    public string WorkFolder { get; set; }

    [Option('v', "verbose", DefaultValue = false, HelpText = "Show all log info.", Required = false)]
    public bool VerboseMode { get; set; }

    [HelpOption]
    public string GetUsage()
    {
      return HelpText.AutoBuild(this, (current) => HelpText.DefaultParsingErrorsHandler(this, current));
    }
  }
}
