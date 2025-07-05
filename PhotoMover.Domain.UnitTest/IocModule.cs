using Autofac;
using Config.Net;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.UnitTest
{
  [UsedImplicitly]
  public class IocModule : Module
  {
    protected override void Load(ContainerBuilder builder)
    {
      base.Load(builder);
      RegisterAppConfig(builder);
      RegisterDatabase(builder);
    }

    protected virtual void RegisterAppConfig(ContainerBuilder builder)
    {
      builder.Register(
                       _ => new ConfigurationBuilder<IAppConfig>()
                           .UseInMemoryDictionary()
                           .Build())
             .As<IAppConfig>()
             .SingleInstance();
    }

    protected virtual void RegisterDatabase(ContainerBuilder builder)
    {
      builder.Register(
                       _ =>
                       {
                         DbContextOptionsBuilder<Database> optionsBuilder = new();
                         optionsBuilder.UseInMemoryDatabase(Constants.AppName + "Database");
                         return new Database(optionsBuilder.Options);
                       })
             .As<Database>()
             .InstancePerLifetimeScope();
    }
  }
}