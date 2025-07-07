using Domain.Service;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Ftp
{
  public class FtpProvider(IAppConfig appConfig, IFileMoverService fileMoverService) : IDisposable
  {
    private ServiceProvider? _collection;
    private IFtpServerHost? _serverHost;
    private FileSystemWatcher _fileSystemWatcher = new FileSystemWatcher();

    public bool Build()
    {
      if (!appConfig.FtpConfig.IsActive)
      {
        return false;
      }

      if (!Directory.Exists(appConfig.FtpConfig.FolderSource))
        return false;

      _fileSystemWatcher.Path = appConfig.FtpConfig.FolderSource;
      _fileSystemWatcher.Created += FileSystemWatcher_OnCreated;
      _fileSystemWatcher.EnableRaisingEvents = true;

      ServiceCollection services = new();

      services.Configure<DotNetFileSystemOptions>(options => options.RootPath = appConfig.FtpConfig.FolderSource);
      services.AddFtpServer(
                            builder => builder
                                      .UseDotNetFileSystem()
                                      .EnableAnonymousAuthentication());
      services.Configure<FtpServerOptions>(opt => opt.ServerAddress = appConfig.FtpConfig.FtpServerIpAddress);

      _collection = services.BuildServiceProvider();
      _serverHost = _collection.GetRequiredService<IFtpServerHost>();
      return true;
    }

    private void FileSystemWatcher_OnCreated(object sender, FileSystemEventArgs e)
    {
    }

    public async Task<bool> StartAsync()
    {
      if (appConfig.FtpConfig?.IsActive == false || _serverHost is null)
      {
        return false;
      }

      await _serverHost.StartAsync();
      return true;
    }

    public async Task<bool> StopAsync()
    {
      if (appConfig.FtpConfig?.IsActive == false || _serverHost is null)
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