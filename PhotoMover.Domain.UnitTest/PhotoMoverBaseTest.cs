using System.Diagnostics.CodeAnalysis;
using Autofac.Extensions.DependencyInjection;
using Domain.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.UnitTest
{
  [NonParallelizable]
  [TestFixture]
  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
  public abstract class PhotoMoverBaseTest
  {
    protected AutofacServiceProvider ServiceProvider { get; private set; }

    protected Database Database { get; private set; }

    protected PresetModel TestPreset { get; private set; }


    [SetUp]
    protected virtual void Setup()
    {
      CreateFolder(Domain.SourceFolder);
      CreateFolder(Domain.TargetFolder);

      ServiceProvider = PhotoMoverServiceProvider.CreateServiceProvider<PhotoMoverServiceProvider>();
      Database = ServiceProvider.GetRequiredService<Database>();
      Database.Database.EnsureCreated();
      CreateTestConfiguration();
      CopyTestData();
    }

    protected virtual void CreateTestConfiguration()
    {
      TestPreset = new()
                   {
                     SourceFolder = Domain.SourceFolder.FullName,
                     DestinationFolder = Domain.TargetFolder.FullName,
                     Name = "UnitTest configuration"
                   };
      Database.Add(TestPreset);
    }

    protected virtual void CopyTestData()
    {
      foreach (FileInfo file in Domain.TestData.Folder.GetFiles())
      {
        file.CopyTo(Path.Combine(Domain.SourceFolder.FullName, file.Name));
      }
    }

    [TearDown]
    protected virtual void TearDown()
    {
      Database.Database.EnsureDeleted();
      Database.Dispose();
      ServiceProvider.Dispose();
      DeleteFolder(Domain.BaseFolder);
    }

    private static void CreateFolder(DirectoryInfo path)
    {
      if (path.Exists)
      {
        DeleteFolder(path);
      }

      path.Create();
    }

    private static void DeleteFolder(DirectoryInfo path)
    {
      path.Delete(true);
    }
  }
}