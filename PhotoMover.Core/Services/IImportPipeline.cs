namespace PhotoMover.Core.Services;

using PhotoMover.Core.Models;

/// <summary>
/// Service for orchestrating the complete photo import pipeline.
/// </summary>
public interface IImportPipeline
{
    /// <summary>
    /// Processes a single photo through the complete import pipeline.
    /// </summary>
    /// <param name="sourcePath">Path to the source photo file.</param>
    /// <param name="groupingRule">Grouping rule to apply.</param>
    /// <param name="destinationRootPath">Root destination folder.</param>
    /// <param name="progress">Progress reporter for operation status.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result of the import operation.</returns>
    Task<ImportResult> ProcessPhotoAsync(
        string sourcePath,
        GroupingRule groupingRule,
        string destinationRootPath,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch processes multiple photos.
    /// </summary>
    /// <param name="sourceFiles">Collection of source file paths.</param>
    /// <param name="groupingRule">Grouping rule to apply to all files.</param>
    /// <param name="destinationRootPath">Root destination folder.</param>
    /// <param name="progress">Progress reporter for operation status.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Collection of import results.</returns>
    Task<IReadOnlyCollection<ImportResult>> ProcessPhotosAsync(
        IReadOnlyCollection<string> sourceFiles,
        GroupingRule groupingRule,
        string destinationRootPath,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);
}
