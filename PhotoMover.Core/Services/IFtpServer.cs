namespace PhotoMover.Core.Services;

/// <summary>
/// Service for hosting an embedded FTP server.
/// </summary>
public interface IFtpServer
{
    /// <summary>
    /// Event raised when a new file is uploaded to the FTP server.
    /// </summary>
    event EventHandler<FtpFileUploadedEventArgs>? FileUploaded;

    /// <summary>
    /// Starts the FTP server.
    /// </summary>
    /// <param name="port">Port to listen on (default 21).</param>
    /// <param name="uploadDirectory">Directory where uploads are stored.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task StartAsync(int port, string uploadDirectory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the FTP server.
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Gets the current running state of the server.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets the port the server is listening on.
    /// </summary>
    int Port { get; }

    /// <summary>
    /// Gets the upload directory for the server.
    /// </summary>
    string? UploadDirectory { get; }
}

/// <summary>
/// Arguments for the FileUploaded event.
/// </summary>
public sealed class FtpFileUploadedEventArgs : EventArgs
{
    public required string FilePath { get; init; }
    public required string FileName { get; init; }
    public required long FileSize { get; init; }
    public required DateTime UploadedAt { get; init; }
}
