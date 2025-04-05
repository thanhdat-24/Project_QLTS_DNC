using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Model;
using static Project_QLTS_DNC.Model.SanPham;

namespace Project_QLTS_DNC.View.QuanLySanPham
{
    public partial class ThemSanPham : Window
    {
        private DanhSachSanPham _parentWindow;

        public ThemSanPham(DanhSachSanPham parentWindow, List<PhongFilter> phongList, List<NhomTSFilter> nhomTSList)
        {
            InitializeComponent();
            _parentWindow = parentWindow;

            // Thiết lập ngày mặc định
            dpNgaySuDung.SelectedDate = DateTime.Today;
            dpHanBH.SelectedDate = DateTime.Today.AddYears(2); // Mặc định BH 2 năm

            // Load dữ liệu cho combobox
            cboMaPhong.ItemsSource = phongList;
            if (phongList.Count > 0) cboMaPhong.SelectedIndex = 0;

            cboMaNhomTS.ItemsSource = nhomTSList;
            if (nhomTSList.Count > 0) cboMaNhomTS.SelectedIndex = 0;

            cboTinhTrang.SelectedIndex = 0; // Mặc định là "Tốt"
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra dữ liệu nhập vào
            if (string.IsNullOrWhiteSpace(txtTenSanPham.Text))
            {
                MessageBox.Show("Vui lòng nhập tên sản phẩm", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTenSanPham.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSoSeri.Text))
            {
                MessageBox.Show("Vui lòng nhập số seri", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtSoSeri.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtGiaTri.Text) || !decimal.TryParse(txtGiaTri.Text, out decimal giaTri))
            {
                MessageBox.Show("Vui lòng nhập giá trị sản phẩm hợp lệ", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtGiaTri.Focus();
                return;
            }

            // Tạo đối tượng sản phẩm mới
            SanPham sanPhamMoi = new SanPham
            {
                MaSP = _parentWindow.GetNewMaSP(),
                TenSanPham = txtTenSanPham.Text.Trim(),
                SoSeri = txtSoSeri.Text.Trim(),
                MaPhong = ((dynamic)cboMaPhong.SelectedItem).MaPhong,
                MaNhomTS = ((dynamic)cboMaNhomTS.SelectedItem).MaNhomTS,
                NgaySuDung = dpNgaySuDung.SelectedDate ?? DateTime.Today,
                HanBH = dpHanBH.SelectedDate ?? DateTime.Today.AddYears(2),
                GiaTriSP = giaTri,
                TinhTrangSP = ((ComboBoxItem)cboTinhTrang.SelectedItem).Content.ToString(),
                GhiChu = txtGhiChu.Text.Trim()
            };

            // Gọi phương thức thêm sản phẩm của cửa sổ cha
            _parentWindow.AddSanPham(sanPhamMoi);

            MessageBox.Show("Thêm sản phẩm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }
    }
}