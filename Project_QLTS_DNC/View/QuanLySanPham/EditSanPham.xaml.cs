using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Model;
using static Project_QLTS_DNC.Model.SanPham;

namespace Project_QLTS_DNC.View.QuanLySanPham
{
    public partial class EditSanPham : Window
    {
        private DanhSachSanPham _parentWindow;
        private SanPham _sanPham;

        public EditSanPham(DanhSachSanPham parentWindow, SanPham sanPham, List<PhongFilter> phongList, List<NhomTSFilter> nhomTSList)
        {
            InitializeComponent();
            _parentWindow = parentWindow;
            _sanPham = sanPham;

            // Load dữ liệu cho combobox
            cboMaPhong.ItemsSource = phongList;
            cboMaNhomTS.ItemsSource = nhomTSList;

            // Hiển thị thông tin sản phẩm hiện tại
            LoadSanPhamData();
        }

        private void LoadSanPhamData()
        {
            // Hiển thị thông tin sản phẩm lên form
            txtMaSP.Text = _sanPham.MaSP.ToString();
            txtTenSanPham.Text = _sanPham.TenSanPham;
            txtSoSeri.Text = _sanPham.SoSeri;
            txtGiaTri.Text = _sanPham.GiaTriSP.ToString();
            txtGhiChu.Text = _sanPham.GhiChu;

            // Chọn phòng
            var phongItem = cboMaPhong.Items.Cast<dynamic>().FirstOrDefault(p => p.MaPhong == _sanPham.MaPhong);
            if (phongItem != null)
            {
                cboMaPhong.SelectedItem = phongItem;
            }

            // Chọn nhóm tài sản
            var nhomItem = cboMaNhomTS.Items.Cast<dynamic>().FirstOrDefault(n => n.MaNhomTS == _sanPham.MaNhomTS);
            if (nhomItem != null)
            {
                cboMaNhomTS.SelectedItem = nhomItem;
            }

            // Chọn tình trạng
            foreach (ComboBoxItem item in cboTinhTrang.Items)
            {
                if (item.Content.ToString() == _sanPham.TinhTrangSP)
                {
                    cboTinhTrang.SelectedItem = item;
                    break;
                }
            }

            // Chọn ngày
            dpNgaySuDung.SelectedDate = _sanPham.NgaySuDung;
            dpHanBH.SelectedDate = _sanPham.HanBH;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnCapNhat_Click(object sender, RoutedEventArgs e)
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

            // Cập nhật dữ liệu sản phẩm
            _sanPham.TenSanPham = txtTenSanPham.Text.Trim();
            _sanPham.SoSeri = txtSoSeri.Text.Trim();
            _sanPham.MaPhong = ((dynamic)cboMaPhong.SelectedItem).MaPhong;
            _sanPham.MaNhomTS = ((dynamic)cboMaNhomTS.SelectedItem).MaNhomTS;
            _sanPham.NgaySuDung = dpNgaySuDung.SelectedDate ?? DateTime.Today;
            _sanPham.HanBH = dpHanBH.SelectedDate ?? DateTime.Today.AddYears(2);
            _sanPham.GiaTriSP = giaTri;
            _sanPham.TinhTrangSP = ((ComboBoxItem)cboTinhTrang.SelectedItem).Content.ToString();
            _sanPham.GhiChu = txtGhiChu.Text.Trim();

            // Gọi phương thức cập nhật trong cửa sổ cha
            _parentWindow.UpdateSanPham(_sanPham);

            MessageBox.Show("Cập nhật sản phẩm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }
    }
}