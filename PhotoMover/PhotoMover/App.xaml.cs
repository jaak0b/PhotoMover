using System.Windows;
using Autofac.Extensions.DependencyInjection;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace PhotoMover;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }


    protected override void OnStartup(StartupEventArgs e)
    {
        AutofacServiceProvider serviceProvider = PhotoMoverServiceProvider.CreateServiceProvider();
        MainWindow mainWindow = serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        Log.Logger.Error($"{ex?.Message}");
    }
}