using Sungero.Metadata.Services;
using System;
using Sungero.Metadata;

namespace ResolveReferences
{
  class MetadataService : IMetadataService
  {
    private ModuleList moduleList;

    public ModuleList ModuleList
    {
      get
      {
        if (moduleList == null)
          return moduleList = new ModuleList();
        return moduleList;
      }
    }

    public void RefreshModuleList()
    {
      throw new NotImplementedException();
    }

    public MetadataService()
    {
      ModuleMetadataLoader.LoadAllDeployedModuleMetadata(this.ModuleList, new[] { AppDomain.CurrentDomain.BaseDirectory });
    }
  }
}
