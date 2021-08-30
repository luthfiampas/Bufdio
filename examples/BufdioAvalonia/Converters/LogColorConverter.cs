using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Bufdio.Players;

namespace BufdioAvalonia.Converters
{
    public class LogColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not AudioPlayerLog log)
            {
                return null;
            }

            return log.Type switch
            {
                AudioPlayerLogType.Error => Brush.Parse("#fc5b5b"),
                AudioPlayerLogType.Warning => Brush.Parse("#f5d664"),
                _ => Brush.Parse("#ffffff")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
