namespace PhotoMover.Converters;

using System.Globalization;
using System.Windows;
using System.Windows.Data;
using static System.Globalization.NumberStyles;

/// Converter: boolean -> Thickness. Parameter format: "FalseThickness|TrueThickness".
/// Supports: single-value, two-value (horizontal,vertical), four-value formats.
public sealed class BooleanToThicknessConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Validate inputs
        if (value is not bool boolValue)
        {
            return new Thickness(0);
        }

        if (parameter is not string paramString || string.IsNullOrWhiteSpace(paramString))
        {
            return new Thickness(0);
        }

        // Split parameter into false and true thickness values
        var parts = paramString.Split('|');
        if (parts.Length != 2)
        {
            return new Thickness(0);
        }

        // Select the appropriate thickness string based on boolean value
        var thicknessString = boolValue ? parts[1].Trim() : parts[0].Trim();

        // Parse the thickness string
        return ParseThickness(thicknessString);
    }

    /// <summary>
    /// Parses a thickness string into a Thickness object.
    /// Supports formats: "2" (uniform), "1,2" (horizontal,vertical), "1,2,3,4" (left,top,right,bottom)
    /// </summary>
    private static Thickness ParseThickness(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new Thickness(0);
        }

        var parts = value.Split(',');

        // Try to parse each part as a double
        var doubles = new double[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!double.TryParse(parts[i].Trim(), Any, CultureInfo.InvariantCulture, out doubles[i]))
            {
                return new Thickness(0);
            }
        }

        // Apply thickness based on number of values provided
        return doubles.Length switch
        {
            1 => new Thickness(doubles[0]),                                          // Uniform thickness
            2 => new Thickness(doubles[0], doubles[1], doubles[0], doubles[1]),      // Horizontal, Vertical
            4 => new Thickness(doubles[0], doubles[1], doubles[2], doubles[3]),      // Left, Top, Right, Bottom
            _ => new Thickness(0)                                                    // Invalid format
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

