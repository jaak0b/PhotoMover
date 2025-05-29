using Autofac;
using Domain;
using PhotoMover.Commands;
using PhotoMover.ViewModel;
using PhotoMover.Windows;

namespace PhotoMover;

public class IocModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        builder.RegisterType<MainWindow>();
        builder.RegisterType<MainWindowViewModel>();
        builder.RegisterType<OpenPresetSettingsCommand>();
        builder.RegisterType<AddPresetCommand>();
        builder.RegisterType<RunPresetCommand>();
    }
}