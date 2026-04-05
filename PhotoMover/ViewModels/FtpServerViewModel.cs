namespace PhotoMover.ViewModels;

using System.IO;
using System.Windows.Input;
using System.Collections.ObjectModel;
using PhotoMover.Core.Services;

/// <summary>
/// Represents an uploaded file in the FTP server.
/// </summary>
public sealed class UploadedFile
{
    public required string FileName { get; init; }
    public required long FileSize { get; init; }
    public required DateTime UploadedAt { get; init; }

    public string FileSizeDisplay => FormatBytes(FileSize);

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

/// <summary>
/// View model for FTP server management.
/// </summary>
public sealed class FtpServerViewModel : ViewModelBase
{
    private readonly IFtpServer _ftpServer;
    private readonly IRuleRepository _ruleRepository;
    private const int MinPort = 1;
    private const int MaxPort = 65535;

    private bool _isRunning;
    private int _port = 21;
    private string _status = "Stopped";
    private ObservableCollection<UploadedFile> _uploadedFiles = new();

    public bool IsRunning
    {
        get => _isRunning;
        private set => SetProperty(ref _isRunning, value);
    }

    public int Port
    {
        get => _port;
        set
        {
            if (value >= MinPort && value <= MaxPort)
            {
                SetProperty(ref _port, value);
            }
        }
    }

    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public ObservableCollection<UploadedFile> UploadedFiles
    {
        get => _uploadedFiles;
        private set => SetProperty(ref _uploadedFiles, value);
    }

    public ICommand StartServerCommand { get; }
    public ICommand StopServerCommand { get; }
    public ICommand ClearFilesCommand { get; }

    public FtpServerViewModel(IFtpServer ftpServer, IRuleRepository ruleRepository)
    {
        _ftpServer = ftpServer ?? throw new ArgumentNullException(nameof(ftpServer));
        _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
        // Listen to global app state changes so we can update command availability
        AppState.ImportingChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CanEditConfiguration));
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        };
        AppState.FtpRunningChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CanEditConfiguration));
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        };

        StartServerCommand = new RelayCommandAsync(_ => StartServerAsync(), _ => !IsRunning && CanEditConfiguration);
        // Allow stopping the server while it's running regardless of CanEditConfiguration
        StopServerCommand = new RelayCommandAsync(_ => StopServerAsync(), _ => IsRunning);
        ClearFilesCommand = new RelayCommand(_ => ClearUploadedFiles(), _ => UploadedFiles.Count > 0);

        // Subscribe to file uploaded events from the FTP server
        _ftpServer.FileUploaded += FtpServer_FileUploaded;
    }

    public async Task StartServerAsync()
    {
        if (IsRunning)
            return;

        try
        {
            // Prevent starting when there is no active grouping rule configured
            var activeRule = await _ruleRepository.GetActiveRuleAsync();
            if (activeRule == null)
            {
                Status = "Cannot start FTP server: no active grouping rule configured";
                return;
            }

            if (!Directory.Exists(activeRule.DestinationPath))
            {
                Directory.CreateDirectory(activeRule.DestinationPath);
            }

            await _ftpServer.StartAsync(Port, activeRule.DestinationPath);
            IsRunning = true;
            Status = $"Running on port {Port}";

            AppState.IsFtpRunning = true;
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
        catch (Exception ex)
        {
            // Improve guidance for common failures (port in use or permissions)
            var msg = ex.Message;
            if (ex.InnerException is System.Net.Sockets.SocketException || msg.Contains("permission", StringComparison.OrdinalIgnoreCase) || msg.Contains("access", StringComparison.OrdinalIgnoreCase))
            {
                msg += " - try a non-privileged port (>1024) or run the app with elevated privileges.";
            }

            Status = $"Error: {msg}";
        }
    }

    public async Task StopServerAsync()
    {
        if (!IsRunning)
            return;

        try
        {
            await _ftpServer.StopAsync();
            IsRunning = false;
            Status = "Stopped";

            AppState.IsFtpRunning = false;
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
    }

    public bool CanEditConfiguration => !AppState.IsImporting && !AppState.IsFtpRunning;

    private void ClearUploadedFiles()
    {
        UploadedFiles.Clear();
    }

    private void FtpServer_FileUploaded(object? sender, FtpFileUploadedEventArgs e)
    {
        // Add uploaded file to the list
        // Check if file already exists in list to avoid duplicates
        var existingFile = UploadedFiles.FirstOrDefault(f => f.FileName == e.FileName && f.FileSize == e.FileSize);
        if (existingFile == null)
        {
            UploadedFiles.Insert(0, new UploadedFile
            {
                FileName = e.FileName,
                FileSize = e.FileSize,
                UploadedAt = e.UploadedAt
            });
        }
    }
}
