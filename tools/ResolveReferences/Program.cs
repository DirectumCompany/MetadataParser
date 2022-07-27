using CommonLibrary.Dependencies;
using Sungero.Domain.Shared;
using Sungero.Metadata;
using Sungero.Metadata.Services;
using Sungero.Services.Deploy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Sungero.Domain;

namespace ResolveReferences
{
  class Program
  {
    const string Module = "Module.mtd";

    string[] failTimes = new string[]
    {
      "(2016-01-28 18-00)",
      @"Перемещение проектной команды из Docflow в Project, померли ссылки на справочник (2016-03-22 17-00)",
      "(2016-07-08 12-00)",
      @"Неудачно добавленное свойство Box в модуль Parties, без ссылок на ExchangeCore (2016-09-02 15-00)"
    };

    static void Main(string[] args)
    {
      var option = new Option();
      if (!CommandLine.Parser.Default.ParseArguments(args, option))
      {
        ExitOnError(string.Format("Args '{0}' not parsed.", string.Join(", ", args)));
      }

      if (option.ThrowException)
      {
        Run(option);
      }
      else
      {
        try
        {
          Run(option);
        }
        catch (ModuleDeployEnumerateException e)
        {
          // TODO Reshetnikov_MA Закомментил некомпил, добавил обработку ошибки из следующего блока catch.
          //ExitOnError(new Sungero.Services.Shared.DependencyResolveWarning(e.ConflictModules).Details);
          ExitOnError(new Sungero.Services.Shared.DataStructureGenerationWarning(e.Message).Details);
        }
        catch (System.IO.InvalidDataException ex)
        {
          ExitOnError(new Sungero.Services.Shared.DataStructureGenerationWarning(ex.Message).Details);
        }
        catch (Exception ex)
        {
          ExitOnError(ex);
        }
      }
    }

    private static void ExitOnError(object text)
    {
      Console.WriteLine("Deploy failed: {0}", text);
      Environment.Exit(1);
    }

    private static void Run(Option option)
    {
      InitReference(option);
      var modules = new List<ModuleMetadata>();
      var files = System.IO.Directory.GetFiles(option.WorkFolder, Module, System.IO.SearchOption.AllDirectories)
        .Where(f => !f.EndsWith(@"\Metadata\" + Module));
      foreach (var file in files)
      {
        var module = GetModuleMetadata(file);
        modules.Add(module);
        if (option.VerboseMode)
          Console.WriteLine("Loaded {0} ({1})", module.Name, module.Version);
      }

      if (option.ShowMissedVersions)
        SearchMissedVersions(modules);

      DatabaseEngine.SetEngine(new Sungero.DataAccess.MSSqlEngine());
      using (IEnumerator<IEnumerable<ModuleMetadata>> enumerator = new ModuleDeployEnumerator(modules))
      {
        while (enumerator.MoveNext())
        {
          var deployingModules = enumerator.Current.ToList();
          if (option.VerboseMode)
            Console.WriteLine("Try deploy modules: \r\n - {0}", 
              string.Join("\r\n - ", deployingModules.Select(m => string.Format("{0} ({1})", m.FullName, m.VersionString))));

          var generator = new MetadataDatabaseGenerator(Sungero.Metadata.Services.MetadataService.Instance.ModuleList.Modules);
          generator.GenerateScriptsForDeployingModules(deployingModules);
        }
      }
    }

    private static void SearchMissedVersions(List<ModuleMetadata> modules)
    {
      foreach (var module in modules)
      {
        foreach (var dependency in module.Dependencies.Where(d => !d.IsSolutionModule))
        {
          var dependencyModule = modules.FirstOrDefault(m => m.NameGuid == dependency.Id);
          if (dependencyModule != null && !modules.Any(m => m.Version == dependency.MinVersion && m.NameGuid == dependency.Id))
          {
            Console.WriteLine("{0} ({1}) : not found {2} ({3})",
              module.Name, module.Version, dependencyModule.Name, dependency.MinVersionString);
          }
        }
      }
    }

    private static void InitReference(Option option)
    {
      var report = typeof(CoverReportActionMetadata);
      var wf = typeof(AssignmentBaseMetadata);
      var content = typeof(Sungero.Content.AssociatedApplications);
      if (option.VerboseMode)
        Console.WriteLine("Load additional metadata: {0} {1} {2}", report, wf, content);
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
      var dicField = typeExt.GetField("TypeGuidCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
      var dic = dicField.GetValue(typeExt) as ConcurrentDictionary<Type, Guid>;
      dic.AddOrUpdate(typeof(Interface), guid, (t, g) => { return g; });

      EntityInfoRegister.RegisterEntityInfo(guid, info);
    }

    public static ModuleMetadata GetModuleMetadata(string path)
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
        }
        filemeta.ParentModuleMetadata = module;
        module.Items.Add(filemeta);
      }
      return module;
    }
  }
}
