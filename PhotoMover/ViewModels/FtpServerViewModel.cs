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
    private const int MinPort = 1;
    private const int MaxPort = 65535;

    private bool _isRunning;
    private int _port = 21;
    private string _status = "Stopped";
    private string _uploadDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FTP");
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

    public string UploadDirectory
    {
        get => _uploadDirectory;
        set => SetProperty(ref _uploadDirectory, value);
    }

    public ObservableCollection<UploadedFile> UploadedFiles
    {
        get => _uploadedFiles;
        private set => SetProperty(ref _uploadedFiles, value);
    }

    public ICommand StartServerCommand { get; }
    public ICommand StopServerCommand { get; }
    public ICommand ClearFilesCommand { get; }
    public ICommand BrowseFtpDirectoryCommand { get; }

    public FtpServerViewModel(IFtpServer ftpServer)
    {
        _ftpServer = ftpServer ?? throw new ArgumentNullException(nameof(ftpServer));

        StartServerCommand = new RelayCommandAsync(_ => StartServerAsync(), _ => !IsRunning);
        StopServerCommand = new RelayCommandAsync(_ => StopServerAsync(), _ => IsRunning);
        ClearFilesCommand = new RelayCommand(_ => ClearUploadedFiles(), _ => UploadedFiles.Count > 0);
        BrowseFtpDirectoryCommand = new RelayCommand(_ => BrowseFtpDirectory());

        // Subscribe to file uploaded events from the FTP server
        _ftpServer.FileUploaded += FtpServer_FileUploaded;
    }

    public async Task StartServerAsync()
    {
        if (IsRunning)
            return;

        try
        {
            if (!Directory.Exists(UploadDirectory))
            {
                Directory.CreateDirectory(UploadDirectory);
            }

            await _ftpServer.StartAsync(Port, UploadDirectory);
            IsRunning = true;
            Status = $"Running on port {Port}";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
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
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
    }

    private void ClearUploadedFiles()
    {
        UploadedFiles.Clear();
    }

    private void BrowseFtpDirectory()
    {
        var folderDialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select FTP Upload Directory",
            ShowNewFolderButton = true,
            SelectedPath = UploadDirectory
        };

        var result = folderDialog.ShowDialog();
        if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
        {
            UploadDirectory = folderDialog.SelectedPath;
        }
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
