namespace PhotoMover.Converters;

using System.Globalization;
using System.Windows.Data;

/// <summary>
/// Converts a boolean value to a string using the ConverterParameter.
/// Parameter format: "FalseValue|TrueValue"
/// Example: "New Rule|Edit Rule" → false="New Rule", true="Edit Rule"
/// </summary>
public sealed class BooleanToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue || parameter is not string paramString)
        {
            return string.Empty;
        }

        var parts = paramString.Split('|');
        if (parts.Length != 2)
        {
            return string.Empty;
        }

        return boolValue ? parts[1] : parts[0];
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
