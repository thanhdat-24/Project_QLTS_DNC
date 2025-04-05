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

namespace Project_QLTS_DNC.View.QuanLyToanNha
{
    /// <summary>
    /// Interaction logic for frmTang.xaml
    /// </summary>
    public partial class frmTang : UserControl
    {
        public frmTang()
        {
            InitializeComponent();
        }

        private void btnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            frmThemTang frmThemTang = new frmThemTang();
            frmThemTang.ShowDialog();
        }

       
    }
}
