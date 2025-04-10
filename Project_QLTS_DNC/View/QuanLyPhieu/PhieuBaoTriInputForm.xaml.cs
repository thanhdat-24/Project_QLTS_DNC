using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Data;
using Project_QLTS_DNC.Models;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for PhieuBaoTriInputForm.xaml
    /// </summary>
    public partial class PhieuBaoTriInputForm : Window
    {
        private PhieuBaoTri _phieuBaoTri;
        private bool _isEditMode = false;
        private ObservableCollection<TaiSanCanBaoTri> _danhSachTaiSanDuocChon;
        private bool _isMultipleAssets = false;

        public PhieuBaoTri PhieuBaoTri => _phieuBaoTri;

        public PhieuBaoTriInputForm(PhieuBaoTri phieuBaoTri)
        {
            InitializeComponent();

            // Đăng ký sự kiện
            btnLuu.Click += BtnLuu_Click;
            btnHuy.Click += BtnHuy_Click;
            txtChiPhiDuKien.PreviewTextInput += TxtChiPhiDuKien_PreviewTextInput;

            // Khởi tạo danh sách tài sản được chọn
            _danhSachTaiSanDuocChon = new ObservableCollection<TaiSanCanBaoTri>();

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

        /// <summary>
        /// Thiết lập danh sách tài sản đã chọn từ form DanhSachBaoTri
        /// </summary>
        public void ThietLapDanhSachTaiSan(List<TaiSanCanBaoTri> danhSachTaiSan)
        {
            if (danhSachTaiSan == null || danhSachTaiSan.Count == 0) return;

            _danhSachTaiSanDuocChon.Clear();
            foreach (var taiSan in danhSachTaiSan)
            {
                _danhSachTaiSanDuocChon.Add(taiSan);
            }

            _isMultipleAssets = _danhSachTaiSanDuocChon.Count > 1;

            // Nếu chỉ có 1 tài sản, thiết lập thông tin mặc định cho phiếu
            if (!_isMultipleAssets)
            {
                var taiSan = _danhSachTaiSanDuocChon[0];

                // Thiết lập thông tin tài sản vào form
                txtMaTaiSan.Text = taiSan.MaTaiSan;
                txtTenTaiSan.Text = taiSan.TenTaiSan;
                txtMaTaiSan.IsReadOnly = true;

                // Cho phép chỉnh sửa tên tài sản
                txtTenTaiSan.IsReadOnly = false;
                txtTenTaiSan.Background = System.Windows.Media.Brushes.White;

                // Cập nhật thông tin vào đối tượng phiếu
                _phieuBaoTri.MaTaiSan = taiSan.MaTaiSan;
                _phieuBaoTri.TenTaiSan = taiSan.TenTaiSan;

                // Tạo nội dung bảo trì mặc định dựa trên ghi chú tình trạng
                if (!string.IsNullOrEmpty(taiSan.GhiChu))
                {
                    string noiDungMacDinh = $"Bảo trì tài sản (hiện trạng: {taiSan.TinhTrangPhanTram}%): {taiSan.GhiChu}";
                    txtNoiDungBaoTri.Text = noiDungMacDinh;
                    _phieuBaoTri.NoiDungBaoTri = noiDungMacDinh;
                }

                // Ẩn panel danh sách tài sản
                pnlDanhSachTaiSan.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Nếu có nhiều tài sản, hiển thị danh sách tài sản trong form
                HienThiDanhSachTaiSanDuocChon();

                // Đặt thông tin phiếu với nhiều tài sản
                txtMaTaiSan.Text = "Multiple";
                txtTenTaiSan.Text = $"{_danhSachTaiSanDuocChon.Count} tài sản được chọn";
                txtMaTaiSan.IsReadOnly = true;
                txtTenTaiSan.IsReadOnly = true;

                // Cập nhật thông tin vào đối tượng phiếu
                _phieuBaoTri.MaTaiSan = "Multiple";
                _phieuBaoTri.TenTaiSan = $"{_danhSachTaiSanDuocChon.Count} tài sản được chọn";

                // Tạo nội dung bảo trì mặc định
                string noiDungMacDinh = $"Bảo trì {_danhSachTaiSanDuocChon.Count} tài sản được chọn từ danh sách (xem chi tiết trong mục tài sản)";
                txtNoiDungBaoTri.Text = noiDungMacDinh;
                _phieuBaoTri.NoiDungBaoTri = noiDungMacDinh;
            }
        }

        private void HienThiDanhSachTaiSanDuocChon()
        {
            if (_danhSachTaiSanDuocChon.Count <= 0) return;

            // Cập nhật nguồn dữ liệu cho DataGrid
            dgDanhSachTaiSan.ItemsSource = _danhSachTaiSanDuocChon;

            // Hiển thị panel danh sách tài sản
            pnlDanhSachTaiSan.Visibility = Visibility.Visible;
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!KiemTraDuLieu())
            {
                return;
            }

            // Cập nhật dữ liệu từ form vào đối tượng
            _phieuBaoTri.MaPhieu = txtMaPhieu.Text;

            // Chỉ cập nhật mã tài sản và tên tài sản nếu không phải là nhiều tài sản
            if (!_isMultipleAssets)
            {
                _phieuBaoTri.MaTaiSan = txtMaTaiSan.Text;
                _phieuBaoTri.TenTaiSan = txtTenTaiSan.Text;
            }

            _phieuBaoTri.LoaiBaoTri = cboLoaiBaoTri.SelectedItem != null ? ((ComboBoxItem)cboLoaiBaoTri.SelectedItem).Content.ToString() : string.Empty;
            _phieuBaoTri.NgayBaoTri = dtpNgayBaoTri.SelectedDate ?? DateTime.Now;
            _phieuBaoTri.NgayHoanThanh = dtpNgayHoanThanh.SelectedDate;
            _phieuBaoTri.NguoiPhuTrach = txtNguoiPhuTrach.Text;
            _phieuBaoTri.TrangThai = cboTrangThai.SelectedItem != null ? ((ComboBoxItem)cboTrangThai.SelectedItem).Content.ToString() : string.Empty;
            _phieuBaoTri.ChiPhiDuKien = decimal.TryParse(txtChiPhiDuKien.Text.Replace(",", ""), out decimal chiPhi) ? chiPhi : 0;
            _phieuBaoTri.NoiDungBaoTri = txtNoiDungBaoTri.Text;
            _phieuBaoTri.DonViBaoTri = cboDonViBaoTri.SelectedItem != null ? ((ComboBoxItem)cboDonViBaoTri.SelectedItem).Content.ToString() : string.Empty;

            // Nếu là nhiều tài sản, lưu thông tin để xử lý sau
            if (_isMultipleAssets)
            {
                // Trong trường hợp thực tế, lưu danh sách tài sản vào bảng chi tiết phiếu bảo trì
                LuuChiTietPhieuBaoTri();
            }

            // Đóng form và trả về kết quả thành công
            this.DialogResult = true;
            this.Close();
        }

        private void LuuChiTietPhieuBaoTri()
        {
            // Trong trường hợp thực tế, lưu từng tài sản trong danh sách vào bảng chi tiết phiếu bảo trì
            // Mẫu code này chỉ là giả định, cần điều chỉnh theo cấu trúc dữ liệu thực tế

            // Ví dụ:
            /*
            string maPhieu = _phieuBaoTri.MaPhieu;
            
            // Xóa dữ liệu cũ nếu đang cập nhật
            if (_isEditMode)
            {
                // DELETE FROM chitietphieubaotri WHERE maphieu = @maPhieu
            }
            
            // Thêm chi tiết mới
            foreach (var taiSan in _danhSachTaiSanDuocChon)
            {
                // INSERT INTO chitietphieubaotri (maphieu, mataisan, noidung, ghichu) 
                // VALUES (@maPhieu, @maTaiSan, @noiDung, @ghiChu)
            }
            */
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

            // Bỏ qua kiểm tra mã tài sản và tên tài sản nếu đang xử lý nhiều tài sản
            if (!_isMultipleAssets)
            {
                if (string.IsNullOrWhiteSpace(txtMaTaiSan.Text))
                {
                    MessageBox.Show("Vui lòng nhập Mã tài sản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtMaTaiSan.Focus();
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtTenTaiSan.Text))
                {
                    MessageBox.Show("Vui lòng nhập Tên tài sản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtTenTaiSan.Focus();
                    return false;
                }
            }
            else
            {
                // Kiểm tra xem có tài sản nào được chọn không
                if (_danhSachTaiSanDuocChon.Count == 0)
                {
                    MessageBox.Show("Không có tài sản nào được chọn!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
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