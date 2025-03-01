using System.Diagnostics.CodeAnalysis;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.UnitTest;

[TestFixture]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public abstract class PhotoMoverBaseTest
{
    protected AutofacServiceProvider ServiceProvider { get; private set; }

    protected Database Database { get; private set; }

    [SetUp]
    protected virtual void Setup()
    {
        CreateOrClearFolder(Domain.SourceFolder);
        CreateOrClearFolder(Domain.TargetFolder);

        ServiceProvider = new PhotoMoverServiceProvider().Build();
        Database = ServiceProvider.GetRequiredService<Database>();
        Database.Database.EnsureCreated();
    }

    [TearDown]
    protected virtual void TearDown()
    {
        Database.Database.EnsureDeleted();
        ServiceProvider.Dispose();
        Database.Dispose();
        ClearFolder(Domain.BaseFolder);
    }

    private static void CreateOrClearFolder(DirectoryInfo path)
    {
        if (path.Exists)
        {
            ClearFolder(path);
        }

        path.Create();
    }

    private static void ClearFolder(DirectoryInfo path)
    {
        path.Delete(true);
    }
}