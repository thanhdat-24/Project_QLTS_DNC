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

namespace Project_QLTS_DNC.View.QuanLyKho
{
    /// <summary>
    /// Interaction logic for QuanLyKhoView.xaml
    /// </summary>
    public partial class QuanLyKhoView : UserControl
    {
        public QuanLyKhoView()
        {
            InitializeComponent();
        }
        private void btnViewDetail_Click(object sender, RoutedEventArgs e)
        {
            ThemKho themKhoForm = new ThemKho();
            themKhoForm.ShowDialog(); // hoặc .Show() nếu không muốn modal
        }

    }
}
