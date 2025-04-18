using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for MuaMoiOption.xaml
    /// </summary>
    public partial class MuaMoiOption : Window
    {
        public MuaMoiOption()
        {
            InitializeComponent();
        }

        private void btnMuaMoi_Click(object sender, RoutedEventArgs e)
        {
            var optionWindow = new CTMuaMoi(); // Tên class của window mới
            optionWindow.ShowDialog(); // Hiển thị cửa sổ dưới dạng modal
        }

        private void btnMuaBoSung_Click(object sender, RoutedEventArgs e)
        {
            var optionWindow = new CTMuaBoSung(); // Tên class của window mới
            optionWindow.ShowDialog(); // Hiển thị cửa sổ dưới dạng modal
        }
    }
}
