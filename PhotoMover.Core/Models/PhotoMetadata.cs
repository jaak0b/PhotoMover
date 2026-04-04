namespace PhotoMover.Core.Models;

/// <summary>
/// Represents extracted EXIF metadata from a photo file.
/// Supports both named fields and tag ID-based access to EXIF data.
/// </summary>
public sealed record PhotoMetadata
{
    public required string FilePath { get; init; }
    public required string FileName { get; init; }
    public required DateTime DateTaken { get; init; }
    public required string? CameraModel { get; init; }
    public required string? LensModel { get; init; }
    public required int? Orientation { get; init; }

    /// <summary>
    /// All metadata fields by name (e.g., "Model" -> "Canon EOS R5")
    /// </summary>
    public required IReadOnlyDictionary<string, string> AllMetadata { get; init; }

    /// <summary>
    /// EXIF tag ID to value mapping (e.g., 0x0110 -> "Canon EOS R5")
    /// Key is hex string like "0x0110" or decimal like "272"
    /// </summary>
    public required IReadOnlyDictionary<string, string> TagIdMetadata { get; init; }
}
