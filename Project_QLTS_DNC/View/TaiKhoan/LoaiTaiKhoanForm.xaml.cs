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
using DocumentFormat.OpenXml.Bibliography;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Services.TaiKhoan;
using Project_QLTS_DNC.ViewModels.TaiKhoan;

namespace Project_QLTS_DNC.View.TaiKhoan
{
    /// <summary>
    /// Interaction logic for LoaiTaiKhoanForm.xaml
    /// </summary>
    public partial class LoaiTaiKhoanForm : UserControl
    {
        private readonly LoaiTaiKhoanService _loaiTaiKhoanService = new();
        public LoaiTaiKhoanForm()
        {
            InitializeComponent();
            // Ensure the namespace and class name are correct
            _ = LoadDanhSachLoaiTaiKhoan();
        }

        private async Task LoadDanhSachLoaiTaiKhoan()
        {
            List<LoaiTaiKhoanModel> danhSach = await _loaiTaiKhoanService.LayDSLoaiTK();
            dgLoaiTaiKhoan.ItemsSource = danhSach;
        }

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            var themLoaiTaiKhoanWindow = new ThemLoaiTaiKhoanForm();
            themLoaiTaiKhoanWindow.ShowDialog();
        }
    }
}
