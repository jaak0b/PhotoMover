using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using JetBrains.Annotations;
using Serilog;

namespace Domain
{
  [UsedImplicitly]
  // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
  public class PhotoMoverServiceProvider()
  {
    public static AutofacServiceProvider CreateServiceProvider()
    {
      return CreateServiceProvider<PhotoMoverServiceProvider>();
    }

    public static AutofacServiceProvider CreateServiceProvider<T>() where T : PhotoMoverServiceProvider
    {
      return Activator.CreateInstance<T>().Build();
    }
    
    private AutofacServiceProvider Build()
    {
      ContainerBuilder builder = new();
      
      RegisterAssemblyModules(builder);

      IContainer container = builder.Build();
      AutofacServiceLocator serviceLocator = new(container);
      ServiceLocator.SetLocatorProvider(() => serviceLocator);
      AutofacServiceProvider serviceProvider = new(container);
      return serviceProvider;
    }

    private void RegisterAssemblyModules(ContainerBuilder builder)
    {
      Assembly[] assemblies = AppDomain.CurrentDomain
                                       .GetAssemblies()
                                       .Where(assembly => assembly.FullName?.StartsWith(Constants.AppName) == true)
                                       .ToArray();
      Log.Logger.Debug(
                       $"Register modules for assemblies: {string.Join(Environment.NewLine, assemblies.Select(assembly => assembly.FullName))}");
      builder.RegisterAssemblyModules(assemblies);
    }
  }
}