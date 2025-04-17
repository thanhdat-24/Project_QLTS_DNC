using Project_QLTS_DNC.Helpers;
using Project_QLTS_DNC.Models.PhanQuyen;
using Project_QLTS_DNC.ViewModels.TaiKhoan;
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

namespace Project_QLTS_DNC.View.TaiKhoan
{
    /// <summary>
    /// Interaction logic for PhanQuyenForm.xaml
    /// </summary>
    public partial class PhanQuyenForm : UserControl
    {
        private LoaiTaiKhoanViewModel viewModel = new();
        public PhanQuyenForm()
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += async (_, __) => await viewModel.LoadDataAsync();

        }

        private async void PhanQuyenForm_Loaded(object sender, RoutedEventArgs e)
        {
            await viewModel.LoadDataAsync();
             
        }

        private async void btnLuuPQ_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
