using System;
using System.Globalization;
using System.Windows.Data;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public class BooleanToTextConverter : IValueConverter
    {
        public string TrueText { get; set; } = "Có";
        public string FalseText { get; set; } = "Không";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueText : FalseText;
            }
            return FalseText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return stringValue == TrueText;
            }
            return false;
        }
    }
}