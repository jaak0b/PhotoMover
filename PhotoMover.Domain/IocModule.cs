using Autofac;
using Domain.Service;
using JetBrains.Annotations;

namespace Domain
{
  [UsedImplicitly]
  public class IocModule : Module
  {
    protected override void Load(ContainerBuilder builder)
    {
      base.Load(builder);
      builder.RegisterType<FileMoverService>().As<IFileMoverService>().SingleInstance();
    }
  }
}