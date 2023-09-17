using System;
using System.Windows.Data;

namespace SpaceProfiler;

public class TrimmingValueConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is string content)
        {
            if (content.Length <= Constants.MaxNodeNameLength)
                return content;
            return content.Substring(0, Constants.MaxNodeNameLength) + "...";
        }

        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}