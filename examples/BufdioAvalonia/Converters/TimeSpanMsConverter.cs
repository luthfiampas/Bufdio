﻿using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace BufdioAvalonia.Converters;

public class TimeSpanMsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TimeSpan ts)
        {
            return null;
        }

        return ts.TotalMilliseconds;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double d)
        {
            return null;
        }

        return TimeSpan.FromMilliseconds(d >= 0 ? d : 0);
    }
}
