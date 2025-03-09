using Autofac;
using Domain;

namespace PhotoMover;

public class IocModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        builder.RegisterType<MainWindow>();
    }
}