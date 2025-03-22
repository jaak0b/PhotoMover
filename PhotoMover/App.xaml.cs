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

    private AutofacServiceProvider? ServiceProvider { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            ServiceProvider = PhotoMoverServiceProvider.CreateServiceProvider<PhotoMoverPhotoMoverServiceProvider>();
            ServiceProvider.GetRequiredService<MainWindow>().Show();
            base.OnStartup(e);
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        ServiceProvider?.Dispose();
        base.OnExit(e);
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        Log.Logger.Error($"{ex?.Message}");
    }
}