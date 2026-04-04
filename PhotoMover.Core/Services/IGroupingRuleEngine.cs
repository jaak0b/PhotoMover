namespace PhotoMover.Core.Services;

using PhotoMover.Core.Models;

/// <summary>
/// Service for evaluating grouping rules and generating destination paths.
/// </summary>
public interface IGroupingRuleEngine
{
    /// <summary>
    /// Evaluates a grouping rule pattern against photo metadata.
    /// </summary>
    /// <param name="pattern">Pattern string like "{CameraModel}/{DateTaken:yyyy}"</param>
    /// <param name="metadata">Photo metadata to use for substitution.</param>
    /// <returns>Evaluated path string, or null if evaluation fails.</returns>
    string? EvaluatePattern(string pattern, PhotoMetadata metadata);

    /// <summary>
    /// Gets all available placeholder names for patterns.
    /// </summary>
    /// <returns>Collection of available placeholders.</returns>
    IReadOnlyCollection<string> GetAvailablePlaceholders();

    /// <summary>
    /// Validates a grouping rule pattern for correctness.
    /// </summary>
    /// <param name="pattern">Pattern string to validate.</param>
    /// <returns>True if valid; otherwise false.</returns>
    bool ValidatePattern(string pattern);
}
