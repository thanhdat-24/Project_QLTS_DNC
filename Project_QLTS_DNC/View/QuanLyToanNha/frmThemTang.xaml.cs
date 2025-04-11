using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Project_QLTS_DNC.View.QuanLyToanNha
{
    public partial class frmThemTang : Window
    {
        public Tang TangMoi { get; set; }
        private List<ToaNha> DanhSachToaNha = new();

        public frmThemTang()
        {
            InitializeComponent();
            Loaded += frmThemTang_Loaded;
        }

        private async void frmThemTang_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DanhSachToaNha = (await ToaNhaService.LayDanhSachToaNhaAsync()).ToList();
                cboTenToa.ItemsSource = DanhSachToaNha;
                cboTenToa.DisplayMemberPath = "TenToaNha";     // tên hiển thị
                cboTenToa.SelectedValuePath = "MaToaNha";      // giá trị thực lưu
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách tòa nhà: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (cboTenToa.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn tòa nhà!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTenTang.Text))
            {
                MessageBox.Show("Vui lòng nhập tên tầng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TangMoi = new Tang
            {
                MaToa = (int)cboTenToa.SelectedValue,
                TenTang = txtTenTang.Text.Trim(),
                MoTa = txtMota.Text.Trim()
            };

            try
            {
                TangMoi = await TangService.ThemTangAsync(TangMoi);
                MessageBox.Show("Thêm tầng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm tầng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
