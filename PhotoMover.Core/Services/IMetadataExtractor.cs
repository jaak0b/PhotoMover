namespace PhotoMover.Core.Services;

using PhotoMover.Core.Models;

/// <summary>
/// Service for extracting EXIF metadata from photo files.
/// </summary>
public interface IMetadataExtractor
{
    /// <summary>
    /// Extracts metadata from a photo file.
    /// </summary>
    /// <param name="filePath">Full path to the photo file.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Extracted metadata or null if extraction fails.</returns>
    Task<PhotoMetadata?> ExtractMetadataAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of available EXIF metadata field names.
    /// </summary>
    /// <returns>Collection of EXIF field names.</returns>
    IReadOnlyCollection<string> GetAvailableMetadataFields();
}
