using Domain.Service;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Ftp
{
  public class FtpProvider(ISettingsProvider provider, IFileMoverService fileMoverService) : IDisposable
  {
    private ServiceProvider? _collection;
    private IFtpServerHost? _serverHost;
    private readonly FileSystemWatcher _fileSystemWatcher = new();

    public string FtpFolder => Path.Combine(provider.SettingsFolder, "FTP");

    private void EnsureFptSourceCreated()
    {
      if (!Directory.Exists(Path.Combine(provider.SettingsFolder, "FTP")))
        Directory.CreateDirectory(FtpFolder);
    }

    public bool Build()
    {
      if (!provider.Settings.Value.FtpIsActive)
      {
        return false;
      }

      EnsureFptSourceCreated();

      _fileSystemWatcher.Path = FtpFolder;
      _fileSystemWatcher.Created += FileSystemWatcher_OnCreated;
      _fileSystemWatcher.EnableRaisingEvents = true;

      ServiceCollection services = new();

      services.Configure<DotNetFileSystemOptions>(options => options.RootPath = FtpFolder);
      services.AddFtpServer(builder => builder
                                      .UseDotNetFileSystem()
                                      .EnableAnonymousAuthentication());
      services.Configure<FtpServerOptions>(opt => opt.ServerAddress = provider.Settings.Value.FtpServerIpAddress);

      _collection = services.BuildServiceProvider();
      _serverHost = _collection.GetRequiredService<IFtpServerHost>();
      return true;
    }

    private void FileSystemWatcher_OnCreated(object sender, FileSystemEventArgs e)
    {
    }

    public async Task<bool> StartAsync()
    {
      if (!provider.Settings.Value.FtpIsActive || _serverHost is null)
      {
        return false;
      }

      await _serverHost.StartAsync();
      return true;
    }

    public async Task<bool> StopAsync()
    {
      if (!provider.Settings.Value.FtpIsActive || _serverHost is null)
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