namespace PhotoMover.Core.Models;

/// <summary>
/// Represents the result of importing and organizing a photo.
/// </summary>
public sealed record ImportResult
{
    public required string SourcePath { get; init; }
    public required string DestinationPath { get; init; }
    public required bool Success { get; init; }
    public required string? ErrorMessage { get; init; }
    public required PhotoMetadata? Metadata { get; init; }
    public required DateTime ProcessedAt { get; init; }
}
