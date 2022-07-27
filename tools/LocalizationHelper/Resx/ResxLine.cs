using System.Linq;
using System.Text.RegularExpressions;

namespace LocalizationHelper.Resx
{
  public class ResxLine
  {
    public string Code { get; set; }

    public string Default { get; set; }

    public string Russian { get; set; }

    public bool HasRussianCharactersInDefault
    {
      get
      {
        return !string.IsNullOrWhiteSpace(Default) && string.IsNullOrWhiteSpace(Russian) &&
          (Default != "(Значок)") &&
          (System.Text.RegularExpressions.Regex.IsMatch(Default, "[а-я]+", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
      }
    }

    public bool HasEnglishCharactersInRussian
    {
      get
      {
        if (this.Code == "AdditionalInfoTemplate")
          return false;
        if (System.Text.RegularExpressions.Regex.IsMatch(this.Code, "Initialize_FileTypes+", System.Text.RegularExpressions.RegexOptions.IgnoreCase) ||
            System.Text.RegularExpressions.Regex.IsMatch(this.Code, "CurrencyAlphaCode+", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
          return false;
        return !string.IsNullOrWhiteSpace(Russian) &&
          System.Text.RegularExpressions.Regex.IsMatch(Russian, "[a-z]+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
      }
    }

    public bool HasIncorrectSpacing()
    {
      if (Code == "MailTemplate")
        return false;

      var doubleSpace = Default.Contains("  ") || Russian.Contains("  ");
      if (doubleSpace)
        return true;

      var leftDefault = this.Default.TakeWhile(c => c == ' ').ToList();
      var leftRussian = this.Russian.TakeWhile(c => c == ' ').ToList();
      if (!leftDefault.SequenceEqual(leftRussian))
        return true;

      var rightDefault = this.Default.Reverse().TakeWhile(c => c == ' ').ToList();
      var rightRussian = this.Russian.Reverse().TakeWhile(c => c == ' ').ToList();
      if (!rightDefault.SequenceEqual(rightRussian))
        return true;

      var newLinesDefault = Regex.Split(Default, "\r\n", RegexOptions.Compiled).Length;
      var newLinesRussian = Regex.Split(Russian, "\r\n", RegexOptions.Compiled).Length;
      if (newLinesDefault != newLinesRussian)
        return true;

      return false;
    }

    public bool HasResource(string resource)
    {
      return this.Default.Contains(resource) || this.Russian.Contains(resource);
    }

    public ResxLine()
    {
      this.Code = string.Empty;
      this.Default = string.Empty;
      this.Russian = string.Empty;
    }
  }
}