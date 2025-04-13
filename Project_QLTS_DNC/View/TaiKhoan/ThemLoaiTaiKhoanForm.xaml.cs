using Project_QLTS_DNC.Models.TaiKhoan;
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
    /// Interaction logic for ThemLoaiTaiKhoanForm.xaml
    /// </summary>
    public partial class ThemLoaiTaiKhoanForm : Window
    {
        public ThemLoaiTaiKhoanForm()
        {
            InitializeComponent();
        }

        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            string tenLoaiTk = txtTenLoaiTaiKhoan.Text.Trim();
            string moTa = txtMoTa.Text.Trim();

            if (string.IsNullOrEmpty(tenLoaiTk))
            {
                MessageBox.Show("Vui lòng nhập tên loại tài khoản!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var loaiTk = new LoaiTaiKhoanModel
            {
                TenLoaiTk = tenLoaiTk,
                MoTa = moTa
            };

            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<LoaiTaiKhoanModel>().Insert(loaiTk);

                MessageBox.Show("Thêm loại tài khoản thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm loại tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
