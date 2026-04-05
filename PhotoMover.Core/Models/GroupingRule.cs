namespace PhotoMover.Core.Models;

/// <summary>
/// Represents a user-defined grouping rule for organizing photos.
/// </summary>
public sealed record GroupingRule
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string PathPattern { get; init; }
    public required bool IsActive { get; init; }
    public required int Priority { get; init; }
    public required Dictionary<string, string> Metadata { get; init; }

    /// <summary>
    /// Example: "{CameraModel}/{DateTaken:yyyy}/{DateTaken:MM}"
    /// </summary>
    public GroupingRule()
    {
        Metadata = new Dictionary<string, string>();
    }
}
