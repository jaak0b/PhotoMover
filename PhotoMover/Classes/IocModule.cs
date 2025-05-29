using Autofac;
using PhotoMover.Windows;

namespace PhotoMover.Classes
{
  public class IocModule : Module
  {
    protected override void Load(ContainerBuilder builder)
    {
      base.Load(builder);
      builder.RegisterType<MainWindow>();
    }
  }
}