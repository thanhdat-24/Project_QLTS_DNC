using Project_QLTS_DNC.Services.TaiKhoan;
using Project_QLTS_DNC.Services;
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

namespace Project_QLTS_DNC.View.TaiKhoan
{
    /// <summary>
    /// Interaction logic for ThemTaiKhoanForm.xaml
    /// </summary>
    public partial class ThemTaiKhoanForm : Window
    {
        private LoaiTaiKhoanService _loaiTaiKhoanService;
        private NhanVienService _nhanVienService;
        public ThemTaiKhoanForm()
        {
            InitializeComponent();
            _loaiTaiKhoanService = new LoaiTaiKhoanService();
            _nhanVienService = new NhanVienService();
        }

        private void cboLoaiTaiKhoan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLoaiTaiKhoan();
            await LoadNhanVien();
        }

        private async Task LoadLoaiTaiKhoan()
        {
            try
            {
                var dsLoaiTK = await _loaiTaiKhoanService.LayDSLoaiTK();
                cboLoaiTaiKhoan.ItemsSource = dsLoaiTK;
                if (dsLoaiTK.Count > 0)
                {
                    cboLoaiTaiKhoan.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách loại tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadNhanVien()
        {
            try
            {
                var dsNhanVien = await _nhanVienService.LayTatCaNhanVienDtoAsync();
                cboNhanVien.ItemsSource = dsNhanVien;
                if (dsNhanVien.Count > 0)
                {
                    cboNhanVien.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách nhân viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tenTaiKhoan = txtTenTaiKhoan.Text.Trim();
                var matKhau = txtMatKhau.Password.Trim();


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
