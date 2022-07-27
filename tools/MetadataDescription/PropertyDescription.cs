using System;
using LocalizationHelper.Resx;
using Sungero.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace MetadataDescription
{
  public class PropertyDescription
    {
        public string Name;
        public string LocalizationName;
        public string TypeName;
        public PropertyType PropertyType;
        public Dictionary<string, string> EnumerationValues;
        public EntityDescription ChildEntity;

        public string GetPropertyType(PropertyMetadata property)
        {
            switch (property.PropertyType)
            {
                case PropertyType.String:
                    var stringProperty = property as StringPropertyMetadata;
                    return string.Format("{0} ({1})", "Строка", stringProperty.Length);
                case PropertyType.Navigation:
                    var navProperty = property as NavigationPropertyMetadata;
                    var entityGuid = navProperty.EntityGuid;
                    return GetEntityTypeName(entityGuid);
                case PropertyType.Enumeration:
                    return "Перечисление";
                case PropertyType.Integer:
                    return "Целое";
                case PropertyType.Double:
                    return "Вещественное";
                case PropertyType.Boolean:
                    return "Логическое";
                case PropertyType.DateTime:
                    return "Дата и время";
                case PropertyType.Collection:
                    return "Коллекция";                
            }
            return string.Empty;
        }

        private string GetEntityTypeName(Guid entityGuid)
        {
            var modulePath = @"..\..\..\..\src\Packages";
            var modules = MetadataParser.Core.MetadataLoader.GetModules(modulePath, false).ToList();
            foreach (var module in modules)
            {
                var entity = module.Children.Cast<ModuleItemMetadata>().ToList().FirstOrDefault(l => l.NameGuid == entityGuid);
                if (entity != null)
                    return entity.FullName;
            }
            return string.Empty;
        }

        public string GetPropertyLocalizationName(PropertyMetadata property, List<ResxLine> entityResources)
        {
            var propertyName = property.Name;
            var propertyCode = "Property_" + propertyName;
            var resource = entityResources.Where(r => r.Code == propertyCode).FirstOrDefault();
            if (resource != null)
                return resource.Russian;
            return string.Empty;
        }
    }

}