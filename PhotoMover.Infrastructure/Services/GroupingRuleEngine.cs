namespace PhotoMover.Infrastructure.Services;

using PhotoMover.Core.Services;
using PhotoMover.Core.Models;
using System.Text.RegularExpressions;

/// <summary>
/// Implementation of the grouping rule engine for pattern evaluation.
/// Supports both named placeholders ({CameraModel}, {DateTaken}) and tag IDs ({0x0110}, {272})
/// </summary>
public sealed partial class GroupingRuleEngine : IGroupingRuleEngine
{
    [GeneratedRegex(@"\{([^}:]+)(?::([^}]+))?\}", RegexOptions.Compiled)]
    private static partial Regex PlaceholderRegex();

    private static readonly IReadOnlyDictionary<string, Func<PhotoMetadata, string?>> PlaceholderFunctions =
        new Dictionary<string, Func<PhotoMetadata, string?>>(StringComparer.OrdinalIgnoreCase)
        {
            ["DateTaken"] = m => m.DateTaken.ToString("yyyy-MM-dd"),
            ["CameraModel"] = m => m.CameraModel ?? "Unknown",
            ["LensModel"] = m => m.LensModel ?? "Unknown",
            ["FileName"] = m => Path.GetFileNameWithoutExtension(m.FileName),
            ["FileExtension"] = m => Path.GetExtension(m.FileName)
        };

    public string? EvaluatePattern(string pattern, PhotoMetadata metadata)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return null;

        try
        {
            var result = PlaceholderRegex().Replace(pattern, match =>
            {
                var placeholder = match.Groups[1].Value;
                var format = match.Groups[2].Value;

                // Check if it's a tag ID (hex like 0x0110 or decimal like 272)
                if (IsTagId(placeholder))
                {
                    return EvaluateTagId(placeholder, format, metadata);
                }

                // Otherwise use named placeholder
                if (!PlaceholderFunctions.TryGetValue(placeholder, out var func))
                    return match.Value;

                var value = func(metadata);
                if (value == null)
                    return match.Value;

                if (!string.IsNullOrEmpty(format) && placeholder == "DateTaken")
                {
                    try
                    {
                        return metadata.DateTaken.ToString(format);
                    }
                    catch
                    {
                        return value;
                    }
                }

                return value;
            });

            // Sanitize path
            var invalidChars = Path.GetInvalidPathChars();
            foreach (var ch in invalidChars)
            {
                result = result.Replace(ch.ToString(), "_");
            }

            return result;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Determines if a placeholder is a tag ID (hex or decimal)
    /// </summary>
    private static bool IsTagId(string placeholder)
    {
        if (placeholder.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return true;

        return int.TryParse(placeholder, out _);
    }

    /// <summary>
    /// Evaluates a tag ID placeholder, applying special formatting for dates
    /// </summary>
    private string EvaluateTagId(string tagId, string format, PhotoMetadata metadata)
    {
        if (!metadata.TagIdMetadata.TryGetValue(tagId, out var value))
            return string.Empty;

        // Special case: if format is "yymmdd", this is a date field
        // Format as YYMMDD in a single folder (no slashes)
        if (format.Equals("yymmdd", StringComparison.OrdinalIgnoreCase))
        {
            // Try to parse the value as a date and format as YYMMDD
            if (DateTime.TryParseExact(value, "yyyy:MM:dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out var dateValue))
            {
                return dateValue.ToString("yyMMdd");
            }
        }

        // If format is specified, apply it to the value if it's a date-like string
        if (!string.IsNullOrEmpty(format))
        {
            if (DateTime.TryParseExact(value, "yyyy:MM:dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out var dateValue))
            {
                try
                {
                    return dateValue.ToString(format);
                }
                catch
                {
                    // Format string invalid, return raw value
                }
            }
        }

        return value;
    }

    public IReadOnlyCollection<string> GetAvailablePlaceholders()
    {
        return PlaceholderFunctions.Keys.ToList();
    }

    public bool ValidatePattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return false;

        var matches = PlaceholderRegex().Matches(pattern);
        foreach (Match match in matches)
        {
            var placeholder = match.Groups[1].Value;

            // Tag IDs are always valid
            if (IsTagId(placeholder))
                continue;

            // Check named placeholders
            if (!PlaceholderFunctions.ContainsKey(placeholder))
                return false;
        }

        return true;
    }
}
