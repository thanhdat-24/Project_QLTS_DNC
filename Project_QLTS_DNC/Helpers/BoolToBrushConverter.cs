using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Project_QLTS_DNC.Helpers
{
    public class BoolToBrushConverter : IValueConverter
    {
        public Brush ReadBrush { get; set; } = Brushes.White;
        public Brush UnreadBrush { get; set; } = Brushes.LightYellow;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool daDoc)
            {
                return daDoc ? ReadBrush : UnreadBrush;
            }
            return ReadBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
