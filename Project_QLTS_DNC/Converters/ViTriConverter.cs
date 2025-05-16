using System;
using System.Globalization;
using System.Windows.Data;

namespace Project_QLTS_DNC.Converters
{
    public class ViTriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue && intValue <= 0)
                return string.Empty;

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value as string))
                return 0;

            if (int.TryParse(value as string, out int result))
                return result;

            return 0;
        }
    }
}