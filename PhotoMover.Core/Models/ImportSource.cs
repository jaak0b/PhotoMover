namespace PhotoMover.Core.Models;

/// <summary>
/// Represents the source of photo imports.
/// </summary>
public enum ImportSourceType
{
    FtpServer,
    SdCard,
    LocalFolder
}

public sealed record ImportSource
{
    public required ImportSourceType Type { get; init; }
    public required string Name { get; init; }
    public required string RootPath { get; init; }
    public required DateTime DetectedAt { get; init; }
}
