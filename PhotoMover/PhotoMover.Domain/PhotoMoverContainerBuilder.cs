using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;

namespace Domain;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class PhotoMoverServiceProvider()
{
    private const string KeyWord = "PhotoMover";


    public static AutofacServiceProvider CreateServiceProvider() => new PhotoMoverServiceProvider().Build();

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
        using var db = serviceProvider.GetRequiredService<Database>();
        db.Database.EnsureCreated();
    }

    protected AutofacServiceProvider Build()
    {
        ServiceCollection serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        var builder = new ContainerBuilder();
        builder.Populate(serviceCollection);
        var assemblies = GetAllAssemblies();
        Log.Logger.Debug(
            $"Register modules for assemblies: {string.Join(Environment.NewLine, assemblies.Select(e => e.FullName))}");
        builder.RegisterAssemblyModules(assemblies);
        IContainer? container = builder.Build();
        AutofacServiceProvider serviceProvider = new AutofacServiceProvider(container);
        CreateDatabase(serviceProvider);
        return serviceProvider;
    }

    private Assembly[] GetAllAssemblies() => AppDomain.CurrentDomain.GetAssemblies()
        .Where(e => e.FullName?.StartsWith(KeyWord) == true).ToArray();
}