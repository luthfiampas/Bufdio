using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace BufdioAvalonia.Converters;

public class TimeSpanFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TimeSpan ts)
        {
            return "00:00:00";
        }

        var hours = ts.Hours.ToString().PadLeft(2, '0');
        var minutes = ts.Minutes.ToString().PadLeft(2, '0');
        var seconds = ts.Seconds.ToString().PadLeft(2, '0');

        return $"{hours}:{minutes}:{seconds}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
