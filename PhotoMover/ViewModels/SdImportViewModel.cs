namespace PhotoMover.ViewModels;

using PhotoMover.Core.Services;
using PhotoMover.Core.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.IO;
using System.Windows.Forms;

/// <summary>
/// View model for SD/microSD card import functionality.
/// </summary>
public sealed class SdImportViewModel : ViewModelBase
{
    private readonly ISdCardDetector _sdCardDetector;
    private readonly IImportPipeline _importPipeline;
    private readonly IRuleRepository _ruleRepository;

    private ObservableCollection<ImportSource> _detectedCards = new();
    private ImportSource? _selectedCard;
    private bool _isScanning;
    private bool _isImporting;
    private string _status = "Ready";
    private double _progress;
    private ObservableCollection<ImportResult> _results = new();
    private string _destinationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PhotoMover");
    private string _estimatedTimeRemaining = "";
    private int _filesProcessed;
    private int _totalFiles;
    private DateTime _importStartTime;
    private CancellationTokenSource? _importCancellationTokenSource;

    public ObservableCollection<ImportSource> DetectedCards
    {
        get => _detectedCards;
        private set => SetProperty(ref _detectedCards, value);
    }

    public ImportSource? SelectedCard
    {
        get => _selectedCard;
        set => SetProperty(ref _selectedCard, value);
    }

    public bool IsScanning
    {
        get => _isScanning;
        private set => SetProperty(ref _isScanning, value);
    }

    public bool IsImporting
    {
        get => _isImporting;
        private set => SetProperty(ref _isImporting, value);
    }

    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public double Progress
    {
        get => _progress;
        private set => SetProperty(ref _progress, value);
    }

    public string EstimatedTimeRemaining
    {
        get => _estimatedTimeRemaining;
        private set => SetProperty(ref _estimatedTimeRemaining, value);
    }

    public int FilesProcessed
    {
        get => _filesProcessed;
        private set => SetProperty(ref _filesProcessed, value);
    }

    public int TotalFiles
    {
        get => _totalFiles;
        private set => SetProperty(ref _totalFiles, value);
    }

    public ObservableCollection<ImportResult> Results
    {
        get => _results;
        private set => SetProperty(ref _results, value);
    }

    public string DestinationPath
    {
        get => _destinationPath;
        set => SetProperty(ref _destinationPath, value);
    }

    public ICommand DetectCardsCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand CancelImportCommand { get; }
    public ICommand BrowseFolderCommand { get; }

    public SdImportViewModel(
        ISdCardDetector sdCardDetector,
        IImportPipeline importPipeline,
        IRuleRepository ruleRepository)
    {
        _sdCardDetector = sdCardDetector ?? throw new ArgumentNullException(nameof(sdCardDetector));
        _importPipeline = importPipeline ?? throw new ArgumentNullException(nameof(importPipeline));
        _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));

        DetectCardsCommand = new RelayCommandAsync(_ => DetectCardsAsync(), _ => !IsScanning);
        ImportCommand = new RelayCommandAsync(_ => ImportFromCardAsync(), _ => !IsImporting && SelectedCard is not null);
        CancelImportCommand = new RelayCommand(_ => CancelImport(), _ => IsImporting);
        BrowseFolderCommand = new RelayCommand(_ => BrowseFolder(), _ => !IsImporting);

        // react to app state changes so UI can update
        AppState.ImportingChanged += (_, _) => OnPropertyChanged(nameof(CanEditConfiguration));
        AppState.FtpRunningChanged += (_, _) => OnPropertyChanged(nameof(CanEditConfiguration));
    }

    public async Task DetectCardsAsync()
    {
        IsScanning = true;
        Status = "Detecting removable drives...";
        try
        {
            var cards = await _sdCardDetector.DetectSdCardsAsync();
            DetectedCards.Clear();

            foreach (var card in cards)
            {
                DetectedCards.Add(card);
            }

            if (cards.Count == 0)
            {
                Status = "No removable drives found. Ensure your SD card/USB drive is properly connected and detected by Windows.";
            }
            else
            {
                Status = $"Found {cards.Count} removable drive(s)";
            }
        }
        catch (Exception ex)
        {
            Status = $"Detection failed: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
        }
    }

    public async Task ImportFromCardAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedCard is null)
        {
            Status = "Please select an SD card";
            return;
        }

        var activeRule = await _ruleRepository.GetActiveRuleAsync();
        if (activeRule == null)
        {
            Status = "Please configure a grouping rule first";
            return;
        }

        if (!CanEditConfiguration)
        {
            Status = "Cannot start import while FTP server is running or another import is active";
            return;
        }

        IsImporting = true;
        AppState.IsImporting = true;
        Results.Clear();
        Progress = 0;
        FilesProcessed = 0;
        TotalFiles = 0;
        EstimatedTimeRemaining = "";
        _importStartTime = DateTime.Now;

        // Create a new cancellation token source for this import operation
        _importCancellationTokenSource = new CancellationTokenSource();

        try
        {
            Status = "Scanning SD card...";
            var files = await _sdCardDetector.ScanSdCardAsync(SelectedCard.RootPath, cancellationToken);

            if (files.Count == 0)
            {
                Status = "No image files found";
                return;
            }

            TotalFiles = files.Count;
            Status = $"Importing {files.Count} files...";

            // Create progress reporter that updates UI with file count and ETA
            var progress = new Progress<string>(msg =>
            {
                // Parse progress message to extract processed count if available
                UpdateProgress(msg);
            });

            var results = await _importPipeline.ProcessPhotosAsync(
                files,
                activeRule,
                DestinationPath,
                progress,
                _importCancellationTokenSource.Token);

            foreach (var result in results)
            {
                Results.Add(result);
            }

            var successCount = results.Count(r => r.Success);
            Status = $"Completed: {successCount}/{results.Count} files imported successfully";
            Progress = 100;
            EstimatedTimeRemaining = "";
        }
        catch (OperationCanceledException)
        {
            Status = "Import cancelled by user";
            Progress = FilesProcessed > 0 ? (FilesProcessed * 100.0 / TotalFiles) : 0;
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
        finally
        {
            IsImporting = false;
            AppState.IsImporting = false;
            _importCancellationTokenSource?.Dispose();
            _importCancellationTokenSource = null;
        }
    }

    public bool CanEditConfiguration => !AppState.IsImporting && !AppState.IsFtpRunning;

    /// <summary>
    /// Cancels the ongoing import operation.
    /// </summary>
    public void CancelImport()
    {
        if (_importCancellationTokenSource is not null)
        {
            _importCancellationTokenSource.Cancel();
        }
    }

    /// <summary>
    /// Opens a folder selection dialog and updates the destination path.
    /// </summary>
    public void BrowseFolder()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select the destination folder for imported photos",
            SelectedPath = DestinationPath,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            DestinationPath = dialog.SelectedPath;
        }
    }

    /// <summary>
    /// Updates progress based on processed files and calculates ETA.
    /// </summary>
    private void UpdateProgress(string statusMessage)
    {
        Status = statusMessage;

        if (TotalFiles <= 0)
            return;

        // Extract processed count from status message like "Processed 3/10"
        if (statusMessage.StartsWith("Processed "))
        {
            var parts = statusMessage.Replace("Processed ", "").Split('/');
            if (parts.Length == 2 && 
                int.TryParse(parts[0], out int processed) &&
                int.TryParse(parts[1], out int total))
            {
                FilesProcessed = processed;
                if (TotalFiles != total)
                {
                    TotalFiles = total;
                }
            }
        }
        else
        {
            // Fallback: increment based on other status messages
            FilesProcessed = Math.Min(FilesProcessed + 1, TotalFiles);
        }

        // Calculate progress percentage
        Progress = (FilesProcessed * 100.0) / TotalFiles;

        // Calculate estimated time remaining
        CalculateEstimatedTimeRemaining();
    }

    /// <summary>
    /// Calculates and updates the estimated time remaining for the import.
    /// </summary>
    private void CalculateEstimatedTimeRemaining()
    {
        if (FilesProcessed <= 0 || TotalFiles <= 0)
        {
            EstimatedTimeRemaining = "";
            return;
        }

        var elapsedTime = DateTime.Now - _importStartTime;
        if (elapsedTime.TotalSeconds < 1)
        {
            EstimatedTimeRemaining = "calculating...";
            return;
        }

        // Calculate average time per file
        var secondsPerFile = elapsedTime.TotalSeconds / FilesProcessed;
        var remainingFiles = TotalFiles - FilesProcessed;
        var estimatedRemainingSeconds = secondsPerFile * remainingFiles;

        var estimatedTime = TimeSpan.FromSeconds(estimatedRemainingSeconds);

        // Format as "X min Y sec" or just "X min"
        if (estimatedTime.TotalMinutes < 1)
        {
            EstimatedTimeRemaining = $"{(int)estimatedTime.TotalSeconds}s remaining";
        }
        else
        {
            var minutes = (int)estimatedTime.TotalMinutes;
            var seconds = estimatedTime.Seconds;
            EstimatedTimeRemaining = seconds > 0 
                ? $"{minutes}m {seconds}s remaining" 
                : $"{minutes}m remaining";
        }
    }
}
