using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project_QLTS_DNC.Models; // Thêm namespace chứa class PhieuBaoTri

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for PhieuBaoTriInputForm.xaml
    /// </summary>
    public partial class PhieuBaoTriInputForm : Window
    {
        private PhieuBaoTri _phieuBaoTri;
        private bool _isEditMode = false;

        public PhieuBaoTri PhieuBaoTri => _phieuBaoTri;

        public PhieuBaoTriInputForm(PhieuBaoTri phieuBaoTri)
        {
            InitializeComponent();

            // Đăng ký sự kiện
            btnLuu.Click += BtnLuu_Click;
            btnHuy.Click += BtnHuy_Click;
            txtChiPhiDuKien.PreviewTextInput += TxtChiPhiDuKien_PreviewTextInput;

            if (phieuBaoTri != null)
            {
                // Chế độ chỉnh sửa
                _isEditMode = true;
                _phieuBaoTri = phieuBaoTri;
                this.Title = "Chỉnh sửa phiếu bảo trì";

                // Hiển thị thông tin phiếu
                HienThiThongTinPhieu(phieuBaoTri);
            }
            else
            {
                // Chế độ thêm mới
                _isEditMode = false;
                _phieuBaoTri = new PhieuBaoTri();
                this.Title = "Thêm phiếu bảo trì mới";

                // Khởi tạo giá trị mặc định
                KhoiTaoGiaTriMacDinh();
            }
        }

        private void KhoiTaoGiaTriMacDinh()
        {
            // Tạo mã phiếu mới
            txtMaPhieu.Text = TaoMaPhieuMoi();

            // Thiết lập các giá trị mặc định
            dtpNgayBaoTri.SelectedDate = DateTime.Now;
            dtpNgayHoanThanh.SelectedDate = DateTime.Now.AddDays(7);
            txtChiPhiDuKien.Text = "0";

            // Chọn giá trị mặc định cho các combobox
            if (cboLoaiBaoTri.Items.Count > 0)
                cboLoaiBaoTri.SelectedIndex = 0;

            if (cboDonViBaoTri.Items.Count > 0)
                cboDonViBaoTri.SelectedIndex = 0;

            if (cboTrangThai.Items.Count > 0)
                cboTrangThai.SelectedIndex = 0;
        }

        private string TaoMaPhieuMoi()
        {
            // Trong thực tế sẽ lấy mã phiếu từ DB
            // Chỉ là giá trị mẫu cho demo
            return $"BT{DateTime.Now.ToString("yyyyMMddHHmmss")}";
        }

        private void HienThiThongTinPhieu(PhieuBaoTri phieu)
        {
            if (phieu == null) return;

            txtMaPhieu.Text = phieu.MaPhieu;
            txtMaTaiSan.Text = phieu.MaTaiSan;
            txtTenTaiSan.Text = phieu.TenTaiSan;
            txtNguoiPhuTrach.Text = phieu.NguoiPhuTrach;
            txtChiPhiDuKien.Text = phieu.ChiPhiDuKien.ToString("N0");
            txtNoiDungBaoTri.Text = phieu.NoiDungBaoTri;

            dtpNgayBaoTri.SelectedDate = phieu.NgayBaoTri;
            dtpNgayHoanThanh.SelectedDate = phieu.NgayHoanThanh;

            // Tìm và chọn giá trị tương ứng trong ComboBox
            SelectComboBoxItem(cboLoaiBaoTri, phieu.LoaiBaoTri);
            SelectComboBoxItem(cboTrangThai, phieu.TrangThai);
            SelectComboBoxItem(cboDonViBaoTri, phieu.DonViBaoTri ?? "Nội bộ");
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
            _phieuBaoTri.MaPhieu = txtMaPhieu.Text;
            _phieuBaoTri.MaTaiSan = txtMaTaiSan.Text;
            _phieuBaoTri.TenTaiSan = txtTenTaiSan.Text;
            _phieuBaoTri.LoaiBaoTri = cboLoaiBaoTri.SelectedItem != null ? ((ComboBoxItem)cboLoaiBaoTri.SelectedItem).Content.ToString() : string.Empty;
            _phieuBaoTri.NgayBaoTri = dtpNgayBaoTri.SelectedDate ?? DateTime.Now;
            _phieuBaoTri.NgayHoanThanh = dtpNgayHoanThanh.SelectedDate;
            _phieuBaoTri.NguoiPhuTrach = txtNguoiPhuTrach.Text;
            _phieuBaoTri.TrangThai = cboTrangThai.SelectedItem != null ? ((ComboBoxItem)cboTrangThai.SelectedItem).Content.ToString() : string.Empty;
            _phieuBaoTri.ChiPhiDuKien = decimal.TryParse(txtChiPhiDuKien.Text.Replace(",", ""), out decimal chiPhi) ? chiPhi : 0;
            _phieuBaoTri.NoiDungBaoTri = txtNoiDungBaoTri.Text;
            _phieuBaoTri.DonViBaoTri = cboDonViBaoTri.SelectedItem != null ? ((ComboBoxItem)cboDonViBaoTri.SelectedItem).Content.ToString() : string.Empty;

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

        private void TxtChiPhiDuKien_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Chỉ cho phép nhập số
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
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

            if (string.IsNullOrWhiteSpace(txtNguoiPhuTrach.Text))
            {
                MessageBox.Show("Vui lòng nhập Người phụ trách!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                txtNguoiPhuTrach.Focus();
                return false;
            }

            if (dtpNgayBaoTri.SelectedDate == null)
            {
                MessageBox.Show("Vui lòng chọn Ngày bảo trì!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                dtpNgayBaoTri.Focus();
                return false;
            }

            if (dtpNgayHoanThanh.SelectedDate != null && dtpNgayHoanThanh.SelectedDate < dtpNgayBaoTri.SelectedDate)
            {
                MessageBox.Show("Ngày hoàn thành không thể trước Ngày bảo trì!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                dtpNgayHoanThanh.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtNoiDungBaoTri.Text))
            {
                MessageBox.Show("Vui lòng nhập Nội dung bảo trì!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                txtNoiDungBaoTri.Focus();
                return false;
            }

            return true;
        }
    }
}