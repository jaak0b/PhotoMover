namespace PhotoMover.Infrastructure.Services;

/// <summary>
/// Detects when files are fully written and ready for processing.
/// Uses file lock detection to determine completion.
/// </summary>
public sealed class FileUploadCompletionDetector : IDisposable
{
    private readonly string _tempDirectory;
    private readonly Func<string, string, Task> _onFileCompleted;
    private FileSystemWatcher? _fileWatcher;
    private readonly Dictionary<string, FileInfo> _monitoredFiles = new();
    private Timer? _completionCheckTimer;
    private readonly object _lockObject = new();

    public FileUploadCompletionDetector(string tempDirectory, Func<string, string, Task> onFileCompleted)
    {
        if (string.IsNullOrWhiteSpace(tempDirectory))
        {
            throw new ArgumentException("Temp directory cannot be null or empty", nameof(tempDirectory));
        }

        _tempDirectory = tempDirectory ?? throw new ArgumentNullException(nameof(tempDirectory));
        _onFileCompleted = onFileCompleted ?? throw new ArgumentNullException(nameof(onFileCompleted));
    }

    /// <summary>
    /// Starts monitoring the temp directory for uploaded files.
    /// </summary>
    public void Start()
    {
        _fileWatcher = new FileSystemWatcher(_tempDirectory)
        {
            EnableRaisingEvents = true,
            Filter = "*.*",
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite
        };

        _fileWatcher.Created += OnFileCreated;
        _fileWatcher.Changed += OnFileChanged;
        _fileWatcher.Error += OnWatcherError;

        _completionCheckTimer = new Timer(CheckForCompletedFiles, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Stops monitoring and cleans up resources.
    /// </summary>
    public void Stop()
    {
        _completionCheckTimer?.Dispose();
        _completionCheckTimer = null;

        _fileWatcher?.Dispose();
        _fileWatcher = null;

        lock (_lockObject)
        {
            _monitoredFiles.Clear();
        }
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Name))
        {
            return;
        }

        lock (_lockObject)
        {
            if (!_monitoredFiles.ContainsKey(e.Name))
            {
                _monitoredFiles[e.Name] = new FileInfo(e.FullPath);
            }
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Name))
        {
            return;
        }

        lock (_lockObject)
        {
            if (File.Exists(e.FullPath))
            {
                _monitoredFiles[e.Name] = new FileInfo(e.FullPath);
            }
        }
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        // Log or handle watcher errors if needed
    }

    private void CheckForCompletedFiles(object? state)
    {
        List<(string name, string fullPath)> completedFiles = new();

        lock (_lockObject)
        {
            var filesToCheck = _monitoredFiles.ToList();

            foreach (var (fileName, fileInfo) in filesToCheck)
            {
                if (!File.Exists(fileInfo.FullName))
                {
                    _monitoredFiles.Remove(fileName);
                    continue;
                }

                if (IsFileComplete(fileInfo.FullName))
                {
                    completedFiles.Add((fileName, fileInfo.FullName));
                    _monitoredFiles.Remove(fileName);
                }
            }
        }

        foreach (var (fileName, fullPath) in completedFiles)
        {
            _ = _onFileCompleted(fileName, fullPath);
        }
    }

    /// <summary>
    /// Determines if a file is completely written and ready for processing.
    /// A file is considered complete when it's no longer locked by the writer.
    /// </summary>
    private static bool IsFileComplete(string filePath)
    {
        try
        {
            // If we can open the file for exclusive access, it's complete
            using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                fileStream.Close();
            }
            return true;
        }
        catch (IOException)
        {
            // File is still being written
            return false;
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
