namespace PhotoMover.ViewModels;

/// <summary>
/// Simple application-wide state used to coordinate long-running operations
/// between view models (importing and FTP server running).
/// </summary>
public static class AppState
{
    private static bool _isImporting;
    private static bool _isFtpRunning;

    public static event EventHandler? ImportingChanged;
    public static event EventHandler? FtpRunningChanged;

    public static bool IsImporting
    {
        get => _isImporting;
        set
        {
            if (_isImporting == value) return;
            _isImporting = value;
            ImportingChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    public static bool IsFtpRunning
    {
        get => _isFtpRunning;
        set
        {
            if (_isFtpRunning == value) return;
            _isFtpRunning = value;
            FtpRunningChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
