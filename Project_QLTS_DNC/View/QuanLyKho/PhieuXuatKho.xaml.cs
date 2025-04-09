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
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for PhieuXuatKho.xaml
    /// </summary>
    public partial class PhieuXuatKho : UserControl
    {
        public PhieuXuatKho()
        {
            InitializeComponent();
        }
        private void btnHuy_Click_1(object sender, RoutedEventArgs e)
        {
            // If the UserControl is hosted in a Window, close the Window
            Window.GetWindow(this)?.Close();
        }
    }
}
