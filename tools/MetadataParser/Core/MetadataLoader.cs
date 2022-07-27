using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CommonLibrary.Dependencies;
using Sungero.Domain.Shared;
using Sungero.Metadata;
using Sungero.Metadata.Services;
using MetadataService = ResolveReferences.MetadataService;
using Moqs = ResolveReferences.Moqs;

namespace MetadataParser.Core
{
  public class MetadataLoader
  {
    const string Module = "Module.mtd";

    public static Dictionary<Guid, string> EntityNames = new Dictionary<Guid, string>()
    {
      { Guid.Parse("c612fc41-44a3-428b-a97c-433c333d78e9"), "Recipient" },
      { Guid.Parse("d795d1f6-45c1-4e5e-9677-b53fb7280c7e"), "Task" },
      { Guid.Parse("afa0c3aa-50ff-453e-87a1-39a696f8917b"), "Certificate" },
      { Guid.Parse("91fcb864-f2ee-43d7-854b-23a3dcce65cb"), "Associated Application" },
      { Guid.Parse("243c2d26-f5f7-495f-9faf-951d91215c77"), "User" },
      { Guid.Parse("ea683a63-273e-43ae-bcf1-7a443698008a"), "AssignmentBase" },
      { Guid.Parse("83f2a537-0cf0-4429-ae76-e9a386ca53aa"), "SimpleTask" }
    }; 

    public static IEnumerable<ModuleMetadata> GetModules(string workFolder, bool withVersion)
    {
      InitReference();
      var files = System.IO.Directory.GetFiles(workFolder, Module, System.IO.SearchOption.AllDirectories)
        .Where(f => !f.EndsWith(@"\Metadata\" + Module));
      if (!withVersion)
      {
        files = files.Where(f => !f.Contains(@"\VersionData\"));
      }
      return files.Select(GetModuleMetadata).ToList();
    }

    private static void InitReference()
    {
      var report = typeof(CoverReportActionMetadata);
      var wf = typeof(AssignmentBaseMetadata);
      var content = typeof(Sungero.Content.AssociatedApplications);
      Dependency.RegisterType<IMetadataService, MetadataService>();
      Dependency.RegisterType<ITenantInfo, Moqs.TenantInfo>();

      RegisterInfo<Sungero.CoreEntities.IUser>(Guid.Parse("243C2D26-F5F7-495F-9FAF-951D91215C77"), new Moqs.UsersInfo());
      RegisterInfo<Sungero.CoreEntities.IFolder>(Guid.Parse("271898c8-18ca-4192-9892-e27b273ce5fc"), new Moqs.FolderInfo());
      RegisterInfo<Sungero.CoreEntities.IDatabookHistory>(Guid.Parse("ab3a8eb4-835c-4309-9754-207c8b9ac766"), new Moqs.DatabookHistoryInfo());

      ModuleManager.Instance.Container.RegisterType<IRepositoryImplementer<Sungero.CoreEntities.IUser>, Sungero.CoreEntities.Client.UserRepositoryImplementer<Sungero.CoreEntities.IUser>>();
      ModuleManager.Instance.Container.RegisterType<IRepositoryImplementer<Sungero.CoreEntities.IFolder>, Sungero.CoreEntities.Client.FolderRepositoryImplementer<Sungero.CoreEntities.IFolder>>();
      ModuleManager.Instance.Container.RegisterType<IRepositoryImplementer<Sungero.CoreEntities.IDatabookHistory>, Sungero.CoreEntities.Client.DatabookHistoryRepositoryImplementer<Sungero.CoreEntities.IDatabookHistory>>();

      ModuleManager.Instance.Container.RegisterType<Sungero.CoreEntities.User.IUserResources, Sungero.CoreEntities.Shared.User.UserResources>();
      ModuleManager.Instance.Container.RegisterType<Sungero.CoreEntities.Folder.IFolderResources, Sungero.CoreEntities.Shared.Folder.FolderResources>();
      ModuleManager.Instance.Container.RegisterType<Sungero.CoreEntities.DatabookHistory.IDatabookHistoryResources, Sungero.CoreEntities.Shared.DatabookHistory.DatabookHistoryResources>();
    }

    private static void RegisterInfo<Interface>(Guid guid, IEntityInfo info) where Interface : IEntity
    {
      var typeExt = typeof(TypeExtension);
      var dicField = typeExt.GetField("typeGuidCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
      dicField = dicField ?? typeExt.GetField("TypeGuidCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

      var dic = dicField.GetValue(typeExt) as ConcurrentDictionary<Type, Guid>;
      dic.AddOrUpdate(typeof(Interface), guid, (t, g) => { return g; });

      EntityInfoRegister.RegisterEntityInfo(guid, info);
    }

    private static ModuleMetadata GetModuleMetadata(string path)
    {
      ModuleMetadata module;
      using (var stream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
      {
        module = MetadataSerializer.Instance.Deserialize<ModuleMetadata>(stream);
      }

      var folder = System.IO.Path.GetDirectoryName(path);
      var files = System.IO.Directory.GetFiles(folder, "*.mtd", System.IO.SearchOption.AllDirectories)
        .Where(m => m != path && !m.EndsWith(Module));
      foreach (var file in files)
      {
        ModuleItemMetadata filemeta;
        using (var stream = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        {
          filemeta = MetadataSerializer.Instance.Deserialize<ModuleItemMetadata>(stream);
          EntityNames[filemeta.NameGuid] = filemeta.Name;
        }
        filemeta.ParentModuleMetadata = module;
        module.Items.Add(filemeta);
      }
      return module;
    }
  }
}