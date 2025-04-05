using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace Project_QLTS_DNC.View.QuanLyToanNha
{
    /// <summary>
    /// Interaction logic for frmToaNha.xaml
    /// </summary>
    public partial class frmToaNha : UserControl
    {
        public frmToaNha()
        {
            InitializeComponent();
        }

        private void btnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            frmThemToaNha frmThemToaNha = new frmThemToaNha();
            frmThemToaNha.ShowDialog();
        }

       

       
    }
}
