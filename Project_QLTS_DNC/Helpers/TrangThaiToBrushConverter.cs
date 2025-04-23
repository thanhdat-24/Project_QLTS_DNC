using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Project_QLTS_DNC.Helpers
{
    public class TrangThaiToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                "Chờ duyệt" => Brushes.Orange,
                "Đã duyệt" => Brushes.Green,
                "Từ chối" => Brushes.Red,
                _ => Brushes.Gray,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
