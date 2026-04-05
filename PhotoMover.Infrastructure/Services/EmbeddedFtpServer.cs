namespace PhotoMover.Infrastructure.Services;

using PhotoMover.Core.Services;
using System.Net;
using System.Net.Sockets;
using System.Threading;

/// <summary>
/// Full FTP server implementation using raw socket programming.
/// Listens on port 21 and accepts FTP connections from cameras.
/// Uploads are saved to temp folder, then moved to final destination when complete.
/// Events are marshaled to the UI context to ensure proper ObservableCollection updates.
/// </summary>
public sealed class EmbeddedFtpServer : Core.Services.IFtpServer, IDisposable
{
    private TcpListener? _commandListener;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _serverTask;
    private string _uploadDirectory = string.Empty;
    private string _tempDirectory = string.Empty;
    private FileUploadCompletionDetector? _completionDetector;
    private bool _isDisposed;
    private SynchronizationContext? _uiContext;

    public event EventHandler<FtpFileUploadedEventArgs>? FileUploaded;

    public bool IsRunning => _commandListener != null;

    public int Port { get; private set; }

    public string? UploadDirectory => _uploadDirectory;

    public async Task StartAsync(int port, string uploadDirectory, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (IsRunning)
        {
            throw new InvalidOperationException("FTP server is already running");
        }

        Port = port;
        _uploadDirectory = Path.GetFullPath(uploadDirectory);
        _tempDirectory = Path.Combine(_uploadDirectory, ".ftp_temp");
        _uiContext = SynchronizationContext.Current;

        EnsureDirectoriesExist();

        try
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _commandListener = new TcpListener(IPAddress.Any, port);
            _commandListener.Start();

            _completionDetector = new FileUploadCompletionDetector(
                _tempDirectory,
                OnFileUploadCompleted);
            _completionDetector.Start();

            _serverTask = AcceptConnectionsAsync(_cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            Cleanup();
            throw new InvalidOperationException($"Failed to start FTP server on port {port}: {ex.Message}", ex);
        }

        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        ThrowIfDisposed();

        if (!IsRunning)
        {
            throw new InvalidOperationException("FTP server is not running");
        }

        try
        {
            _cancellationTokenSource?.Cancel();
            _completionDetector?.Stop();

            _commandListener?.Stop();

            if (_serverTask != null)
            {
                await _serverTask;
            }

            Cleanup();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to stop FTP server", ex);
        }
    }

    private void EnsureDirectoriesExist()
    {
        if (!Directory.Exists(_uploadDirectory))
        {
            Directory.CreateDirectory(_uploadDirectory);
        }

        if (!Directory.Exists(_tempDirectory))
        {
            Directory.CreateDirectory(_tempDirectory);
        }
    }

    private async Task AcceptConnectionsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _commandListener != null)
        {
            try
            {
                var client = await _commandListener.AcceptTcpClientAsync(cancellationToken);
                _ = HandleClientAsync(client, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        using (client)
        using (var handler = new FtpClientHandler(client, _tempDirectory))
        {
            try
            {
                await handler.ProcessAsync(cancellationToken);
            }
            catch
            {
            }
        }
    }

    private async Task OnFileUploadCompleted(string fileName, string tempFilePath)
    {
        try
        {
            if (!File.Exists(tempFilePath))
            {
                return;
            }

            var finalPath = Path.Combine(_uploadDirectory, fileName);
            var finalDirectory = Path.GetDirectoryName(finalPath);

            if (string.IsNullOrEmpty(finalDirectory))
            {
                return;
            }

            if (!Directory.Exists(finalDirectory))
            {
                Directory.CreateDirectory(finalDirectory);
            }

            if (File.Exists(finalPath))
            {
                File.Delete(finalPath);
            }

            File.Move(tempFilePath, finalPath, overwrite: true);

            var fileInfo = new FileInfo(finalPath);
            var uploadedEventArgs = new FtpFileUploadedEventArgs
            {
                FilePath = finalPath,
                FileName = fileName,
                FileSize = fileInfo.Length,
                UploadedAt = DateTime.Now
            };

            if (_uiContext != null)
            {
                _uiContext.Post(_ => OnFileUploaded(uploadedEventArgs), null);
            }
            else
            {
                OnFileUploaded(uploadedEventArgs);
            }

            await Task.CompletedTask;
        }
        catch
        {
        }
    }

    private void Cleanup()
    {
        _commandListener?.Stop();
        _commandListener = null;
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _completionDetector?.Dispose();
        _completionDetector = null;
        _uiContext = null;
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(EmbeddedFtpServer));
        }
    }

    internal void OnFileUploaded(FtpFileUploadedEventArgs args)
    {
        FileUploaded?.Invoke(this, args);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        try
        {
            if (IsRunning)
            {
                StopAsync().Wait();
            }
        }
        catch
        {
        }

        Cleanup();
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Handles individual FTP client connections with proper protocol implementation.
/// Supports passive mode uploads and USER/PASS/STOR/LIST commands.
/// </summary>
internal sealed class FtpClientHandler : IDisposable
{
    private readonly TcpClient _commandClient;
    private readonly string _uploadDirectory;
    private NetworkStream? _commandStream;
    private TcpListener? _dataListener;
    private string _currentUser = string.Empty;
    private bool _isAuthenticated;

    public FtpClientHandler(TcpClient commandClient, string uploadDirectory)
    {
        _commandClient = commandClient ?? throw new ArgumentNullException(nameof(commandClient));
        _uploadDirectory = uploadDirectory ?? throw new ArgumentNullException(nameof(uploadDirectory));
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        _commandStream = _commandClient.GetStream();

        try
        {
            await SendResponseAsync("220 PhotoMover FTP Server Ready\r\n");

            while (!cancellationToken.IsCancellationRequested)
            {
                var command = await ReadCommandAsync();

                if (string.IsNullOrWhiteSpace(command))
                {
                    break;
                }

                await HandleCommandAsync(command, cancellationToken);
            }
        }
        finally
        {
            _commandStream?.Dispose();
            _dataListener?.Stop();
        }
    }

    private async Task<string> ReadCommandAsync()
    {
        if (_commandStream == null)
        {
            return string.Empty;
        }

        try
        {
            using (var reader = new StreamReader(_commandStream, leaveOpen: true))
            {
                return (await reader.ReadLineAsync()) ?? string.Empty;
            }
        }
        catch
        {
            return string.Empty;
        }
    }

    private async Task HandleCommandAsync(string command, CancellationToken cancellationToken)
    {
        var parts = command.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var cmd = parts[0].ToUpperInvariant();
        var arg = parts.Length > 1 ? parts[1] : string.Empty;

        switch (cmd)
        {
            case "USER":
                _currentUser = arg;
                await SendResponseAsync("331 User name okay, need password\r\n");
                break;

            case "PASS":
                _isAuthenticated = true;
                await SendResponseAsync("230 User logged in\r\n");
                break;

            case "QUIT":
                await SendResponseAsync("221 Goodbye\r\n");
                return;

            case "TYPE":
                await SendResponseAsync("200 Type set to Binary\r\n");
                break;

            case "PASV":
                await HandlePasvAsync();
                break;

            case "STOR":
                if (!_isAuthenticated)
                {
                    await SendResponseAsync("530 Not logged in\r\n");
                    break;
                }
                await HandleStorAsync(arg, cancellationToken);
                break;

            case "LIST":
                if (!_isAuthenticated)
                {
                    await SendResponseAsync("530 Not logged in\r\n");
                    break;
                }
                await HandleListAsync(cancellationToken);
                break;

            case "PWD":
                await SendResponseAsync("257 \"/\" is current directory\r\n");
                break;

            case "CWD":
                await SendResponseAsync("250 CWD successful\r\n");
                break;

            case "SYST":
                await SendResponseAsync("215 UNIX\r\n");
                break;

            case "FEAT":
                await SendResponseAsync("211 No-features\r\n");
                break;

            case "NOOP":
                await SendResponseAsync("200 OK\r\n");
                break;

            default:
                await SendResponseAsync("500 Unknown command\r\n");
                break;
        }
    }

    private async Task HandlePasvAsync()
    {
        try
        {
            _dataListener = new TcpListener(IPAddress.Any, 0);
            _dataListener.Start();

            var endpoint = _dataListener.LocalEndpoint as IPEndPoint;
            if (endpoint == null)
            {
                await SendResponseAsync("425 Can't open data connection\r\n");
                return;
            }

            var port = endpoint.Port;
            var high = port >> 8;
            var low = port & 0xFF;

            await SendResponseAsync($"227 Entering Passive Mode (127,0,0,1,{high},{low})\r\n");
        }
        catch
        {
            await SendResponseAsync("425 Can't open data connection\r\n");
        }
    }

    private async Task HandleStorAsync(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                await SendResponseAsync("550 Invalid filename\r\n");
                return;
            }

            await SendResponseAsync("150 Opening binary mode data connection\r\n");

            if (_dataListener == null)
            {
                await SendResponseAsync("425 Can't open data connection\r\n");
                return;
            }

            var dataClient = await _dataListener.AcceptTcpClientAsync(cancellationToken);

            using (dataClient)
            using (var dataStream = dataClient.GetStream())
            {
                var tempFilePath = Path.Combine(_uploadDirectory, fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath) ?? _uploadDirectory);

                using (var fileStream = File.Create(tempFilePath))
                {
                    await dataStream.CopyToAsync(fileStream, cancellationToken);
                }
            }

            await SendResponseAsync("226 Transfer complete\r\n");
        }
        catch
        {
            await SendResponseAsync("426 Connection closed\r\n");
        }
    }

    private async Task HandleListAsync(CancellationToken cancellationToken)
    {
        try
        {
            await SendResponseAsync("150 Opening ASCII mode data connection\r\n");

            if (_dataListener == null)
            {
                await SendResponseAsync("425 Can't open data connection\r\n");
                return;
            }

            var dataClient = await _dataListener.AcceptTcpClientAsync(cancellationToken);

            using (dataClient)
            using (var dataStream = dataClient.GetStream())
            using (var writer = new StreamWriter(dataStream))
            {
                var files = Directory.GetFiles(_uploadDirectory);
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var fileSize = fileInfo.Length;
                    var fileName = Path.GetFileName(file);
                    var modTime = fileInfo.LastWriteTime.ToString("MMM dd HH:mm");

                    await writer.WriteLineAsync($"-rw-r--r-- 1 owner group {fileSize:D10} {modTime} {fileName}");
                }

                await writer.FlushAsync();
            }

            await SendResponseAsync("226 Transfer complete\r\n");
        }
        catch
        {
            await SendResponseAsync("426 Connection closed\r\n");
        }
    }

    private async Task SendResponseAsync(string message)
    {
        try
        {
            if (_commandStream != null)
            {
                using (var writer = new StreamWriter(_commandStream, leaveOpen: true))
                {
                    await writer.WriteAsync(message);
                    await writer.FlushAsync();
                }
            }
        }
        catch
        {
        }
    }

    public void Dispose()
    {
        _commandStream?.Dispose();
        _dataListener?.Stop();
        _commandClient?.Dispose();
    }
}

