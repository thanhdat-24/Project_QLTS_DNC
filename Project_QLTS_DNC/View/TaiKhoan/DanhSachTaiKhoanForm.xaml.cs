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
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.ViewModel.TaiKhoan;
namespace Project_QLTS_DNC.View.TaiKhoan
{
    /// <summary>
    /// Interaction logic for DanhSachTaiKhoanForm.xaml
    /// </summary>
    public partial class DanhSachTaiKhoanForm : UserControl
    {
        public DanhSachTaiKhoanForm()
        {
            InitializeComponent();
            Loaded += DanhSachTaiKhoanForm_Loaded;

        }

        private async void DanhSachTaiKhoanForm_Loaded(object sender, RoutedEventArgs e)
        {
            var client = await SupabaseService.GetClientAsync();
            var taiKhoanService = new TaiKhoanService();
            DataContext = new DanhSachTaiKhoanViewModel(taiKhoanService);
        }

        private void btnTaoTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            var taoTaiKhoanForm = new ThemTaiKhoanForm();
            taoTaiKhoanForm.ShowDialog();
        }
    }
}
