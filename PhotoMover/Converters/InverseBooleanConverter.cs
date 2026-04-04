using System.Globalization;
using System.Windows.Data;

namespace PhotoMover.Converters
{
    /// <summary>
    /// Converter that inverts a boolean value (true becomes false, false becomes true).
    /// </summary>
    public sealed class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }

            return value;
        }
    }
}
