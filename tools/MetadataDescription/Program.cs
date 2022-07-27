using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetadataVersionComparer.Core;
using LocalizationHelper.Resx;
using Sungero.Metadata;
using System.IO;
using MetadataParser.Core;

namespace MetadataDescription
{
  class Program
  {
    static void Main(string[] args)
    {
      var option = new Option();
      if (!CommandLine.Parser.Default.ParseArguments(args, option))
      {
        Console.WriteLine("Args '{0}' not parsed.", string.Join(", ", args));
        Console.WriteLine(option.GetUsage());
        Environment.Exit(-1);
      }

      var moduleName = option.ModuleName;
      if (string.IsNullOrEmpty(moduleName))
      {
        Console.WriteLine("Module name is empty.");
        Environment.Exit(-1);
      }        

      var entityName = option.EntityName;
      if (string.IsNullOrEmpty(entityName))
      {
        Console.WriteLine("Entity name is empty.");
        Environment.Exit(-1);
      }

      var fileFormat = option.FileFormat ?? "rst";
      if (fileFormat != "txt" && fileFormat != "rst")
      {
        Console.WriteLine("Invalid file format. Valid format: rst or txt.");
        Environment.Exit(-1);
      }

      var entityDescription = CreateEntityDescription(moduleName, entityName, entityName);
      if (entityDescription == null) 
      {
        Console.WriteLine("Cannot create metadata description.");
        Environment.Exit(-1);
      }   
      
      if (fileFormat == "rst")
        CreateRSTFile(entityDescription);
      else 
        CreateTXTFile(entityDescription);
    }

    private static EntityDescription CreateEntityDescription(string moduleName, string entityPathName, string entityName)
    {
      var modulePath = string.Format(@"..\..\..\..\src\Packages\{0}", moduleName);
      var entityPath = string.Format(@"{0}\{1}.Shared\{2}", modulePath, moduleName, entityPathName);     
      Resources.LoadResources(entityPath);
      var module = MetadataLoader.GetModules(modulePath, false).ToList().FirstOrDefault();
      if (module == null)
        return null;
      var entity = module.Children.Cast<ModuleItemMetadata>().ToList().FirstOrDefault(l => l.Name == entityName);
      if (entity == null)
        return null;
      EntityMetadata entityMetadata = entity as EntityMetadata;
      
      var entityDescription = new EntityDescription();
      entityDescription.Name = entityMetadata.Name;

      var resources = Resources.EntityResources;
      if (resources == null)
      {
        return entityDescription;
      }
      
      var entityResources = new List<ResxLine>();
      entityResources.AddRange(resources.SelectMany(s => s.Resources).SelectMany(r => r));
      var displayName = entityResources.FirstOrDefault(r => r.Code == "DisplayName");
      if (displayName != null)
        entityDescription.LocalizationName = displayName.Russian;
      
      var propertiesDescription = new List<PropertyDescription>();
      foreach (var property in entityMetadata.Properties)
      {
        var propertyDescription = new PropertyDescription();
        propertyDescription.Name = property.Name;
        propertyDescription.LocalizationName = propertyDescription.GetPropertyLocalizationName(property, entityResources);
        propertyDescription.PropertyType = property.PropertyType;
        propertyDescription.TypeName = propertyDescription.GetPropertyType(property);
        propertiesDescription.Add(propertyDescription);
        if (property.PropertyType == PropertyType.Enumeration)
        {
          var values = new Dictionary<string, string>();
          var enumProperty = property as EnumPropertyMetadata;
          var enumValues = enumProperty.DirectValues;        
          foreach (var value in enumValues)
          {
            var propertyCode = string.Format("Enum_{0}_{1}", property.Name, value.Name);
            var resource = entityResources.FirstOrDefault(r => r.Code == propertyCode);
            var localizationName = resource != null ? resource.Russian : string.Empty;
            values.Add(value.Name, localizationName);
          }

          propertyDescription.EnumerationValues = values;
        }
        if (property.PropertyType == PropertyType.Collection)
        {
          var childProperty = property as CollectionPropertyMetadata;  
          var childPropertyDescription = CreateEntityDescription(moduleName,$"{childProperty.Parent.Name}@{childProperty.Name}", $"{childProperty.Parent.Name}{childProperty.Name}"); 
          propertyDescription.ChildEntity = childPropertyDescription;
        }        
      }

      entityDescription.Properties = propertiesDescription;
      return entityDescription;
    }

    private static void CreateTXTFile(EntityDescription entityDescription)
    {
      var file = File.CreateText(string.Format("{0}.{1}", entityDescription.Name, "txt"));
      file.WriteLine("{0} ({1})", entityDescription.Name, entityDescription.LocalizationName);
      AddPropertiesDescription(file, entityDescription.Properties);    
      file.Close();
    }

    private static void CreateRSTFile(EntityDescription entityDescription)
    {
      var file = File.CreateText(string.Format("{0}.{1}", entityDescription.Name, "rst"));
      file.WriteLine(".. vim: syntax=rst\n");
      var entityFullName = string.Format("{0} {1}", entityDescription.Name, entityDescription.LocalizationName);
      file.WriteLine(entityFullName);
      file.WriteLine(new string('^', entityFullName.Length));
      file.WriteLine();
      file.Close();
    }
    
    private static void AddPropertiesDescription(StreamWriter file, List<PropertyDescription> entityDescriptionProperties, string prefix = "")
    {
      foreach (var property in entityDescriptionProperties.OrderBy(p => p.Name))
      {
        file.WriteLine("{0}{1}\t{2}", prefix, property.Name, property.LocalizationName);
      }
      file.WriteLine();
      foreach (var property in entityDescriptionProperties.OrderBy(p => p.Name))
      {
        file.WriteLine("{0}{1} ({2}): {3}", prefix, property.Name, property.LocalizationName, property.TypeName);
        if (property.PropertyType == PropertyType.Enumeration)
        {
          foreach (var enumerationValue in property.EnumerationValues)
          {
            file.WriteLine("{0}\t{1}\t{2}", prefix, enumerationValue.Key, enumerationValue.Value);
          }
        }

        if (property.PropertyType == PropertyType.Collection)
        {
          AddPropertiesDescription(file, property.ChildEntity.Properties, $"{prefix}\t");
        }
      }  
    }
  }
}
