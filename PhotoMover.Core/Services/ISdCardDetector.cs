namespace PhotoMover.Core.Services;

using PhotoMover.Core.Models;

/// <summary>
/// Service for detecting and scanning SD/microSD cards.
/// </summary>
public interface ISdCardDetector
{
    /// <summary>
    /// Detects all connected SD/microSD cards.
    /// </summary>
    /// <returns>Collection of detected SD card import sources.</returns>
    Task<IReadOnlyCollection<ImportSource>> DetectSdCardsAsync();

    /// <summary>
    /// Scans an SD card for supported image files.
    /// </summary>
    /// <param name="sdCardPath">Root path of the SD card.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Collection of image file paths found.</returns>
    Task<IReadOnlyCollection<string>> ScanSdCardAsync(string sdCardPath, CancellationToken cancellationToken = default);
}
