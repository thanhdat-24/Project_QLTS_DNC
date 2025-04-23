using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Project_QLTS_DNC.Helpers
{
    public class SoSanhLonHon0VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int so && so > 0)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
