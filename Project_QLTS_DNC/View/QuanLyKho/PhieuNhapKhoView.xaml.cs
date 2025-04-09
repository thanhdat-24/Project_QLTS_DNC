using MaterialDesignThemes.Wpf;
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
using Microsoft.Win32; // Add this namespace for SaveFileDialog

namespace Project_QLTS_DNC.View.QuanLyKho
{
    /// <summary>
    /// Interaction logic for PhieuNhapKhoView.xaml
    /// </summary>
    public partial class PhieuNhapKhoView : UserControl
    {
        public PhieuNhapKhoView()
        {
            InitializeComponent();
        }
        private void btnThemKho_click(object sender, RoutedEventArgs e)
        {
            PhieuNhapKhoInput phieuNhapKho = new PhieuNhapKhoInput();
            phieuNhapKho.ShowDialog();// hoặc .Show() nếu không muốn modal
        }

        
    }
}
