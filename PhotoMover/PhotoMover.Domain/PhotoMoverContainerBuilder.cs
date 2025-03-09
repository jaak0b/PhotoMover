using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem.DotNet;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace Domain;

[UsedImplicitly]
// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class PhotoMoverServiceProvider()
{
    private const string KeyWord = "PhotoMover";

    public static AutofacServiceProvider CreateServiceProvider() => CreateServiceProvider<PhotoMoverServiceProvider>();

    public static AutofacServiceProvider CreateServiceProvider<T>() where T : PhotoMoverServiceProvider =>
        Activator.CreateInstance<T>().Build();

    protected virtual void ConfigureServices(ServiceCollection services)
    {
        services.AddDbContext<Database>(ConfigureDatabase);
    }

    protected virtual void ConfigureDatabase(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("PhotoMover");
    }

    protected virtual void CreateDatabase(AutofacServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetRequiredService<Database>();
        db.Database.EnsureCreated();
    }

    private AutofacServiceProvider Build()
    {
        ServiceCollection serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        var builder = new ContainerBuilder();
        builder.Populate(serviceCollection);
        var assemblies = GetAllAssemblies();
        Log.Logger.Debug(
            $"Register modules for assemblies: {string.Join(Environment.NewLine, assemblies.Select(e => e.FullName))}");
        builder.RegisterAssemblyModules(assemblies);
        IContainer container = builder.Build();
        var serviceLocator = new AutofacServiceLocator(container);
        ServiceLocator.SetLocatorProvider(() => serviceLocator);
        AutofacServiceProvider serviceProvider = new AutofacServiceProvider(container);
        CreateDatabase(serviceProvider);
        return serviceProvider;
    }

    private Assembly[] GetAllAssemblies() => AppDomain.CurrentDomain.GetAssemblies()
        .Where(e => e.FullName?.StartsWith(KeyWord) == true).ToArray();
}