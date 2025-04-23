using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Project_QLTS_DNC.Helpers
{
    public class BoolToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool daDoc)
            {
                return daDoc ? FontWeights.Normal : FontWeights.Bold;
            }
            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
