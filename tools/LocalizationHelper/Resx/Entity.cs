using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LocalizationHelper.Resx
{
  public class Entity
  {
    public FileInfo Metadata { get; set; }

    public ResourcesCollection EntityResource { get; set; }

    public ResourcesCollection SystemResource { get; set; }

    public List<ResourcesCollection> Resources
    {
      get
      {
        var list = new List<ResourcesCollection>();
        if (EntityResource.Any())
          list.Add(EntityResource);
        if (SystemResource.Any())
          list.Add(SystemResource);
        return list;
      }
    }

    public Entity GetResources(string searchPattern)
    {
      var er = this.EntityResource.GetResources(searchPattern);
      var sr = this.SystemResource.GetResources(searchPattern);
      return new Entity(er, sr, Metadata);
    }

    public Entity GetRussianMissFilled()
    {
      var er = this.EntityResource.GetRussianMissFilledResources();
      var sr = this.SystemResource.GetRussianMissFilledResources();
      return new Entity(er, sr, Metadata);      
    }

    public Entity GetEnglishInRussian()
    {
      var er = this.EntityResource.GetEnglishInRussianResources();
      var sr = this.SystemResource.GetEnglishInRussianResources();
      return new Entity(er, sr, Metadata);
    }

    public Entity GetIncorrectSpacing()
    {
      var er = this.EntityResource.GetIncorrectSpacing();
      var sr = this.SystemResource.GetIncorrectSpacing();
      return new Entity(er, sr, Metadata);
    }

    public bool HasResource(string resource)
    {
      return this.SystemResource.HasResource(resource) || this.EntityResource.HasResource(resource);
    }

    public bool HasRussianMissFilled()
    {
      return this.SystemResource.HasRussianMissFilled() || this.EntityResource.HasRussianMissFilled();
    }

    public bool HasEnglishInRussian()
    {
      return this.SystemResource.HasEnglishInRussian() || this.EntityResource.HasEnglishInRussian();
    }

    public bool HasIncorrectSpacing()
    {
      return this.SystemResource.HasIncorrectSpacing() || this.EntityResource.HasIncorrectSpacing();
    }

    public override string ToString()
    {
      return this.Metadata.Exists ? Path.GetFileNameWithoutExtension(this.Metadata.DirectoryName) : base.ToString();
    }

    public Entity(string mtd)
    {
      this.Metadata = new FileInfo(mtd);
      
      var folder = Path.GetDirectoryName(mtd);
      var name = Path.GetFileNameWithoutExtension(mtd);
      this.EntityResource = new ResourcesCollection(folder, name);
      this.SystemResource = new ResourcesCollection(folder, name, "System");
    }

    public Entity(ResourcesCollection main, ResourcesCollection system, FileInfo metadata)
    {
      this.EntityResource = main;
      this.SystemResource = system;
      this.Metadata = metadata;
    }
  }
}