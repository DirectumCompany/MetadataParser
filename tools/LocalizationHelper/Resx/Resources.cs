using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LocalizationHelper.Resx
{
  public static class Resources
  {
    // Операции истории не откатываемые, не ищем даже.
    private const string EnumOperation = "Enum_Operation";

    public static List<Entity> EntityResources { get; set; }

    public static void LoadResources(string path)
    {
      var mtds = Directory.GetFiles(path, "*.mtd", SearchOption.AllDirectories);
      mtds = mtds.Where(s => !s.Contains("VersionData")).ToArray();
      EntityResources.AddRange(mtds.Select(f => new Entity(f)));
    }

    public static List<Entity> GetUnused()
    {
      var tasks = Resources.EntityResources.Select(GetUnusedFromEntity).ToArray();
      Task.WaitAll(tasks);
      return tasks.Select(t => t.Result).ToList();
    }

    private static Task<Entity> GetUnusedFromEntity(Entity entity)
    {
      var resources = new List<ResxLine>();
      var tasks = entity.EntityResource
        .Where(r => !r.Code.StartsWith(EnumOperation))
        .Select(r => Task.Run(() =>
        {
          var res = r;
          var used = Sources.Sources.SourceFiles.Any(s => s.HasResource(res));
          used = used || Sources.Sources.ReportFiles.Any(s => s.HasResource(res));
          if (!used)
            resources.Add(res);
        }))
        .ToArray();
      var whenAll = Task.WhenAll(tasks);
      return whenAll.ContinueWith(t =>
      {
        if (t.Exception != null)
          throw t.Exception;

        return new Entity(new ResourcesCollection(resources), new ResourcesCollection(null, null), entity.Metadata);
      });
    }

    static Resources()
    {
      EntityResources = new List<Entity>();
    }
  }
}