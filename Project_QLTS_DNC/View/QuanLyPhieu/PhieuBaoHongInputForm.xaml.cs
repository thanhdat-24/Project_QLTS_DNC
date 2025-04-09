using System;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Models;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for PhieuBaoHongInputForm.xaml
    /// </summary>
    public partial class PhieuBaoHongInputForm : Window
    {
        private PhieuBaoHong _phieuBaoHong;
        private bool _isEditMode = false;

        public PhieuBaoHong PhieuBaoHong => _phieuBaoHong;

        public PhieuBaoHongInputForm(PhieuBaoHong phieuBaoHong)
        {
            InitializeComponent();

            // Đăng ký sự kiện
            btnLuu.Click += BtnLuu_Click;
            btnHuy.Click += BtnHuy_Click;

            if (phieuBaoHong != null)
            {
                // Chế độ chỉnh sửa
                _isEditMode = true;
                _phieuBaoHong = phieuBaoHong;
                this.Title = "Chỉnh sửa phiếu báo hỏng";

                // Hiển thị thông tin phiếu
                HienThiThongTinPhieu(phieuBaoHong);
            }
            else
            {
                // Chế độ thêm mới
                _isEditMode = false;
                _phieuBaoHong = new PhieuBaoHong();
                this.Title = "Thêm phiếu báo hỏng mới";

                // Khởi tạo giá trị mặc định
                KhoiTaoGiaTriMacDinh();
            }
        }

        private void KhoiTaoGiaTriMacDinh()
        {
            // Tạo mã phiếu mới
            txtMaPhieu.Text = TaoMaPhieuMoi();

            // Thiết lập các giá trị mặc định
            dtpNgayLap.SelectedDate = DateTime.Now;
            txtNguoiLap.Text = "Người dùng hiện tại"; // Trong thực tế sẽ lấy từ hệ thống

            // Chọn giá trị mặc định cho các combobox
            if (cboMucDoHong.Items.Count > 0)
                cboMucDoHong.SelectedIndex = 0;

            if (cboTrangThai.Items.Count > 0)
                cboTrangThai.SelectedIndex = 0;
        }

        private string TaoMaPhieuMoi()
        {
            // Trong thực tế sẽ lấy mã phiếu từ DB
            // Chỉ là giá trị mẫu cho demo
            return $"PBH{DateTime.Now.ToString("yyyyMMddHHmmss")}";
        }

        private void HienThiThongTinPhieu(PhieuBaoHong phieu)
        {
            if (phieu == null) return;

            txtMaPhieu.Text = phieu.MaPhieu;
            txtMaTaiSan.Text = phieu.MaTaiSan;
            txtTenTaiSan.Text = phieu.TenTaiSan;
            txtNguoiLap.Text = phieu.NguoiLap;
            txtBoPhanQuanLy.Text = phieu.BoPhanQuanLy;
            txtMoTa.Text = phieu.MoTa;

            dtpNgayLap.SelectedDate = phieu.NgayLap;

            // Tìm và chọn giá trị tương ứng trong ComboBox
            SelectComboBoxItem(cboMucDoHong, phieu.MucDoHong);
            SelectComboBoxItem(cboTrangThai, phieu.TrangThai);
        }

        private void SelectComboBoxItem(ComboBox comboBox, string value)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString() == value)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!KiemTraDuLieu())
            {
                return;
            }

            // Cập nhật dữ liệu từ form vào đối tượng
            _phieuBaoHong.MaPhieu = txtMaPhieu.Text;
            _phieuBaoHong.MaTaiSan = txtMaTaiSan.Text;
            _phieuBaoHong.TenTaiSan = txtTenTaiSan.Text;
            _phieuBaoHong.MucDoHong = cboMucDoHong.SelectedItem != null ? ((ComboBoxItem)cboMucDoHong.SelectedItem).Content.ToString() : string.Empty;
            _phieuBaoHong.NgayLap = dtpNgayLap.SelectedDate ?? DateTime.Now;
            _phieuBaoHong.NguoiLap = txtNguoiLap.Text;
            _phieuBaoHong.TrangThai = cboTrangThai.SelectedItem != null ? ((ComboBoxItem)cboTrangThai.SelectedItem).Content.ToString() : string.Empty;
            _phieuBaoHong.BoPhanQuanLy = txtBoPhanQuanLy.Text;
            _phieuBaoHong.MoTa = txtMoTa.Text;

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
            if (string.IsNullOrWhiteSpace(txtMaPhieu.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã phiếu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                txtMaPhieu.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtMaTaiSan.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã tài sản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                txtMaTaiSan.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtNguoiLap.Text))
            {
                MessageBox.Show("Vui lòng nhập Người lập!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                txtNguoiLap.Focus();
                return false;
            }

            if (dtpNgayLap.SelectedDate == null)
            {
                MessageBox.Show("Vui lòng chọn Ngày lập!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                dtpNgayLap.Focus();
                return false;
            }

            if (cboMucDoHong.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn Mức độ hỏng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                cboMucDoHong.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtMoTa.Text))
            {
                MessageBox.Show("Vui lòng nhập Mô tả chi tiết!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                txtMoTa.Focus();
                return false;
            }

            return true;
        }
    }
}