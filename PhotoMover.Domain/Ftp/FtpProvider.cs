using Domain.Model;
using Domain.Service;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Domain.Ftp
{
  public class FtpProvider(FtpPresetService ftpPresetService) : IDisposable
  {
    public FtpPresetService FtpPresetService { get; } = ftpPresetService;

    private FtpPresetModel? _config;
    private ServiceProvider? _collection;
    private IFtpServerHost? _serverHost;

    public bool Build()
    {
      _config = FtpPresetService.GetFtpConfigurationModel();
      if (!_config.IsActive)
      {
        return false;
      }

      ServiceCollection services = new();

      services.Configure<DotNetFileSystemOptions>(
                                                  opt => opt.RootPath = _config.SourceFolder);

      services.AddFtpServer(
                            builder => builder
                                      .UseDotNetFileSystem()
                                      .EnableAnonymousAuthentication());

      services.Configure<FtpServerOptions>(opt => opt.ServerAddress = _config.FtpServerIpAddress);

      _collection = services.BuildServiceProvider();
      _serverHost = _collection.GetRequiredService<IFtpServerHost>();
      return true;
    }

    public async Task<bool> StartAsync()
    {
      if (_config?.IsActive == false || _serverHost is null)
      {
        return false;
      }

      await _serverHost.StartAsync();
      return true;
    }

    public async Task<bool> StopAsync()
    {
      if (_config?.IsActive == false || _serverHost is null)
      {
        return true;
      }

      await _serverHost.StopAsync();
      return true;
    }

    public void Dispose()
    {
      _collection?.Dispose();
    }
  }
}