using System;
using System.Windows;
using System.Windows.Data;

namespace SpaceProfiler;

public class TrimmedTextBlockVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is string content)
        {
            if (content.Length <= Constants.MaxNodeNameLength)
                return Visibility.Hidden;
            return Visibility.Visible;
        }

        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}