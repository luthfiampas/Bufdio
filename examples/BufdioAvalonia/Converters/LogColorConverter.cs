using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using BufdioAvalonia.Common;

namespace BufdioAvalonia.Converters
{
    public class LogColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Log log)
            {
                return null;
            }

            return log.Type switch
            {
                Log.LogType.Error => Brush.Parse("#fc5b5b"),
                Log.LogType.Warning => Brush.Parse("#f5d664"),
                _ => Brush.Parse("#ffffff")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
