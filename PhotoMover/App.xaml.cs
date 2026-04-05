using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PhotoMover.Core.Services;
using PhotoMover.Infrastructure.Services;
using PhotoMover.ViewModels;

namespace PhotoMover
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            _serviceProvider = ConfigureServices();
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Core Services
            services.AddSingleton<IMetadataExtractor, MetadataExtractorService>();
            services.AddSingleton<IGroupingRuleEngine, GroupingRuleEngine>();
            services.AddSingleton<IFileSystem, FileSystemService>();
            services.AddSingleton<ISdCardDetector, SdCardDetector>();
            services.AddSingleton<IFtpServer, EmbeddedFtpServer>();
            services.AddSingleton<IImportPipeline, ImportPipeline>();

            // Repository
            var rulesPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PhotoMover",
                "Rules");
            services.AddSingleton<IRuleRepository>(sp => 
                new JsonRuleRepository(sp.GetRequiredService<IFileSystem>(), rulesPath));

            // ViewModels
            services.AddSingleton<RuleEditorViewModel>();
            services.AddSingleton<SdImportViewModel>();
            services.AddSingleton<FtpServerViewModel>(sp =>
                new FtpServerViewModel(sp.GetRequiredService<IFtpServer>(), sp.GetRequiredService<IRuleRepository>()));
            services.AddSingleton<MainWindowViewModel>();

            // Views
            services.AddSingleton<MainWindow>();

            return services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            var viewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();

            mainWindow.DataContext = viewModel;
            mainWindow.Show();

            // Initialize async operations
            _ = viewModel.InitializeAsync();
        }
    }
}
