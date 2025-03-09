using Autofac;
using Domain.Service;
using JetBrains.Annotations;

namespace Domain;

[UsedImplicitly]
public class IocModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        builder.RegisterType<FtpConfigurationService>().As<IFtpConfigurationService>().SingleInstance();
        builder.RegisterType<TaskService>().As<ITaskService>().SingleInstance();
    }
}