namespace PhotoMover.Infrastructure.Services;

using PhotoMover.Core.Services;
using PhotoMover.Core.Models;

/// <summary>
/// Implementation of the import pipeline orchestration.
/// </summary>
public sealed class ImportPipeline : IImportPipeline
{
    private readonly IMetadataExtractor _metadataExtractor;
    private readonly IGroupingRuleEngine _groupingRuleEngine;
    private readonly IFileSystem _fileSystem;

    public ImportPipeline(
        IMetadataExtractor metadataExtractor,
        IGroupingRuleEngine groupingRuleEngine,
        IFileSystem fileSystem)
    {
        _metadataExtractor = metadataExtractor ?? throw new ArgumentNullException(nameof(metadataExtractor));
        _groupingRuleEngine = groupingRuleEngine ?? throw new ArgumentNullException(nameof(groupingRuleEngine));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task<ImportResult> ProcessPhotoAsync(
        string sourcePath,
        GroupingRule groupingRule,
        string destinationRootPath,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            progress?.Report($"Extracting metadata: {Path.GetFileName(sourcePath)}");

            var metadata = await _metadataExtractor.ExtractMetadataAsync(sourcePath, cancellationToken);
            if (metadata == null)
            {
                return new ImportResult
                {
                    SourcePath = sourcePath,
                    DestinationPath = string.Empty,
                    Success = false,
                    ErrorMessage = "Failed to extract metadata",
                    Metadata = null,
                    ProcessedAt = DateTime.Now
                };
            }

            progress?.Report("Evaluating grouping rule");

            var relativePath = _groupingRuleEngine.EvaluatePattern(groupingRule.PathPattern, metadata);
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return new ImportResult
                {
                    SourcePath = sourcePath,
                    DestinationPath = string.Empty,
                    Success = false,
                    ErrorMessage = "Failed to evaluate grouping rule",
                    Metadata = metadata,
                    ProcessedAt = DateTime.Now
                };
            }

            var destinationDirectory = Path.Combine(destinationRootPath, relativePath);
            _fileSystem.CreateDirectory(destinationDirectory);

            var uniqueFilename = _fileSystem.GetUniqueFilename(destinationDirectory, metadata.FileName);
            var destinationPath = Path.Combine(destinationDirectory, uniqueFilename);

            progress?.Report($"Moving to: {relativePath}/{uniqueFilename}");

            await _fileSystem.MoveFileAsync(sourcePath, destinationPath);

            return new ImportResult
            {
                SourcePath = sourcePath,
                DestinationPath = destinationPath,
                Success = true,
                ErrorMessage = null,
                Metadata = metadata,
                ProcessedAt = DateTime.Now
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new ImportResult
            {
                SourcePath = sourcePath,
                DestinationPath = string.Empty,
                Success = false,
                ErrorMessage = ex.Message,
                Metadata = null,
                ProcessedAt = DateTime.Now
            };
        }
    }

    public async Task<IReadOnlyCollection<ImportResult>> ProcessPhotosAsync(
        IReadOnlyCollection<string> sourceFiles,
        GroupingRule groupingRule,
        string destinationRootPath,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ImportResult>();

        int processedCount = 0;
        foreach (var sourceFile in sourceFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await ProcessPhotoAsync(
                sourceFile,
                groupingRule,
                destinationRootPath,
                progress,
                cancellationToken);

            results.Add(result);

            processedCount++;
            progress?.Report($"Processed {processedCount}/{sourceFiles.Count}");
        }

        return results;
    }
}
