using System;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using Project_QLTS_DNC.Models;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for TaiSanCanBaoTriEditForm.xaml
    /// </summary>
    public partial class TaiSanCanBaoTriEditForm : Window
    {
        public TaiSanCanBaoTri TaiSan { get; private set; }
        private bool _isInitializing = true;

        public TaiSanCanBaoTriEditForm(TaiSanCanBaoTri taiSan)
        {
            InitializeComponent();

            TaiSan = taiSan ?? new TaiSanCanBaoTri();

            // Hiển thị thông tin tài sản
            HienThiThongTinTaiSan();

            _isInitializing = false;
        }

        private void HienThiThongTinTaiSan()
        {
            txtMaTaiSan.Text = TaiSan.MaTaiSan;
            txtTenTaiSan.Text = TaiSan.TenTaiSan;
            txtViTri.Text = TaiSan.ViTri;
            sldTinhTrang.Value = TaiSan.TinhTrangPhanTram;
            txtTinhTrang.Text = TaiSan.TinhTrangPhanTram.ToString();
            txtGhiChu.Text = TaiSan.GhiChu;

            // Chọn giá trị trong ComboBox
            ChonGiaTriComboBox(cboNhomTaiSan, TaiSan.NhomTaiSan);
            ChonGiaTriComboBox(cboPhong, TaiSan.TenPhong);
        }

        private void ChonGiaTriComboBox(ComboBox comboBox, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString() == value)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void SldTinhTrang_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isInitializing) return;

            // Cập nhật giá trị vào TextBox
            txtTinhTrang.Text = ((int)sldTinhTrang.Value).ToString();
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!KiemTraDuLieu())
            {
                return;
            }

            // Cập nhật thông tin từ form vào đối tượng tài sản
            TaiSan.TenTaiSan = txtTenTaiSan.Text;
            TaiSan.NhomTaiSan = cboNhomTaiSan.SelectedItem != null ? ((ComboBoxItem)cboNhomTaiSan.SelectedItem).Content.ToString() : string.Empty;
            TaiSan.ViTri = txtViTri.Text;
            TaiSan.TenPhong = cboPhong.SelectedItem != null ? ((ComboBoxItem)cboPhong.SelectedItem).Content.ToString() : string.Empty;
            TaiSan.TinhTrangPhanTram = (int)sldTinhTrang.Value;
            TaiSan.GhiChu = txtGhiChu.Text;

            // Trong trường hợp thực tế, cập nhật dữ liệu vào CSDL

            // Đóng form và trả về kết quả thành công
            this.DialogResult = true;
            this.Close();
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            // Đóng form và trả về kết quả hủy
            this.DialogResult = false;
            this.Close();
        }

        private bool KiemTraDuLieu()
        {
            if (string.IsNullOrWhiteSpace(txtTenTaiSan.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên tài sản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                txtTenTaiSan.Focus();
                return false;
            }

            if (cboNhomTaiSan.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn Nhóm tài sản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                cboNhomTaiSan.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtViTri.Text))
            {
                MessageBox.Show("Vui lòng nhập Vị trí!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                txtViTri.Focus();
                return false;
            }

            if (cboPhong.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn Phòng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                cboPhong.Focus();
                return false;
            }

            return true;
        }
    }
}