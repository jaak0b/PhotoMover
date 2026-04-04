namespace PhotoMover.Converters;

using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

/// Converter: boolean -> Brush. Parameter: "FalseColor|TrueColor" (e.g. "#E0E0E0|#27AE60").
public sealed class BooleanToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue || parameter is not string paramString)
        {
            return new SolidColorBrush(Colors.Transparent);
        }

        var parts = paramString.Split('|');
        if (parts.Length != 2)
        {
            return new SolidColorBrush(Colors.Transparent);
        }

        var colorString = boolValue ? parts[1] : parts[0];

        try
        {
            var color = (Color)ColorConverter.ConvertFromString(colorString);
            return new SolidColorBrush(color);
        }
        catch
        {
            return new SolidColorBrush(Colors.Transparent);
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
