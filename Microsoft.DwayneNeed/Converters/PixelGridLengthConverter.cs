﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.DwayneNeed.Converters
{
    [ValueConversion(typeof(double), typeof(GridLength))]
    public class PixelGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new GridLength((double) value, GridUnitType.Pixel);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((GridLength) value).Value;
        }
    }
}