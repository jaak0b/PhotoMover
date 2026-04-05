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
    /// Root folder into which photos are moved after grouping.
    /// Defaults to Documents\PhotoMover for rules created before this field was introduced.
    /// </summary>
    public string DestinationPath { get; init; } = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PhotoMover");

    /// <summary>
    /// Example: "{CameraModel}/{DateTaken:yyyy}/{DateTaken:MM}"
    /// </summary>
    public GroupingRule()
    {
        Metadata = new Dictionary<string, string>();
    }
}
