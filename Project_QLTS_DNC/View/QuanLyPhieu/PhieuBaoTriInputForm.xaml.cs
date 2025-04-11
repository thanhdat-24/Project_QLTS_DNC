using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Data;
using Project_QLTS_DNC.Models.BaoTri;

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
        private ObservableCollection<string> _danhSachNhanVien; // Danh sách nhân viên cho ComboBox
        private ObservableCollection<TaiSanCanBaoTri> _danhSachTaiSan; // Danh sách tài sản cho ComboBox

        public PhieuBaoTri PhieuBaoTri => _phieuBaoTri;

        public PhieuBaoTriInputForm(PhieuBaoTri phieuBaoTri)
        {
            InitializeComponent();

            // Đăng ký sự kiện
            btnLuu.Click += BtnLuu_Click;
            btnHuy.Click += BtnHuy_Click;
            txtChiPhiDuKien.PreviewTextInput += TxtChiPhiDuKien_PreviewTextInput;
            cboMaTaiSan.SelectionChanged += CboMaTaiSan_SelectionChanged;

            // Khởi tạo danh sách tài sản được chọn
            _danhSachTaiSanDuocChon = new ObservableCollection<TaiSanCanBaoTri>();

            // Khởi tạo danh sách nhân viên
            KhoiTaoDanhSachNhanVien();

            // Khởi tạo danh sách tài sản
            KhoiTaoDanhSachTaiSan();

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

        private void KhoiTaoDanhSachNhanVien()
        {
            // Trong thực tế, bạn sẽ lấy danh sách nhân viên từ database
            // Đây chỉ là danh sách mẫu
            _danhSachNhanVien = new ObservableCollection<string>
            {
                "Nguyễn Văn A",
                "Trần Thị B",
                "Lê Văn C",
                "Phạm Thị D",
                "Hoàng Văn E"
            };

            // Thiết lập nguồn dữ liệu cho ComboBox người phụ trách
            cboNguoiPhuTrach.ItemsSource = _danhSachNhanVien;
        }

        private void KhoiTaoDanhSachTaiSan()
        {
            // Trong thực tế, bạn sẽ lấy danh sách tài sản từ database
            // Đây chỉ là danh sách mẫu
            _danhSachTaiSan = new ObservableCollection<TaiSanCanBaoTri>
            {
                new TaiSanCanBaoTri { MaTaiSan = "TS001", TenTaiSan = "Máy tính xách tay Dell XPS 13", TinhTrangPhanTram = 80, GhiChu = "Pin yếu" },
                new TaiSanCanBaoTri { MaTaiSan = "TS002", TenTaiSan = "Máy chiếu Epson EB-X05", TinhTrangPhanTram = 75, GhiChu = "Bóng đèn mờ" },
                new TaiSanCanBaoTri { MaTaiSan = "TS003", TenTaiSan = "Máy in HP LaserJet Pro M404dn", TinhTrangPhanTram = 90, GhiChu = "Kẹt giấy thường xuyên" },
                new TaiSanCanBaoTri { MaTaiSan = "TS004", TenTaiSan = "Tủ lạnh Panasonic NR-BL26AVPVN", TinhTrangPhanTram = 85, GhiChu = "Làm lạnh chậm" },
                new TaiSanCanBaoTri { MaTaiSan = "TS005", TenTaiSan = "Điều hòa Daikin FTKC25UAVMV", TinhTrangPhanTram = 70, GhiChu = "Làm mát kém hiệu quả" }
            };

            // Thiết lập nguồn dữ liệu cho ComboBox mã tài sản
            cboMaTaiSan.ItemsSource = _danhSachTaiSan;
            cboMaTaiSan.DisplayMemberPath = "MaTaiSan";
            cboMaTaiSan.SelectedValuePath = "MaTaiSan";
        }

        private void CboMaTaiSan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboMaTaiSan.SelectedItem != null && cboMaTaiSan.SelectedItem is TaiSanCanBaoTri taiSan)
            {
                // Cập nhật tên tài sản và các thông tin khác
                txtTenTaiSan.Text = taiSan.TenTaiSan;

                // Tạo nội dung bảo trì mặc định dựa trên ghi chú tình trạng
                if (!string.IsNullOrEmpty(taiSan.GhiChu))
                {
                    string noiDungMacDinh = $"Bảo trì tài sản (hiện trạng: {taiSan.TinhTrangPhanTram}%): {taiSan.GhiChu}";
                    txtNoiDungBaoTri.Text = noiDungMacDinh;
                }
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

            if (cboTrangThai.Items.Count > 0)
                cboTrangThai.SelectedIndex = 0;

            // Chọn giá trị mặc định cho người phụ trách nếu có
            if (_danhSachNhanVien.Count > 0)
                cboNguoiPhuTrach.SelectedIndex = 0;

            // Chọn giá trị mặc định cho mã tài sản nếu có
            if (_danhSachTaiSan.Count > 0)
                cboMaTaiSan.SelectedIndex = 0;
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

            // Tìm và chọn mã tài sản trong combobox
            foreach (TaiSanCanBaoTri item in _danhSachTaiSan)
            {
                if (item.MaTaiSan == phieu.MaTaiSan)
                {
                    cboMaTaiSan.SelectedItem = item;
                    break;
                }
            }

            // Nếu không tìm thấy trong danh sách, thêm vào danh sách và chọn
            if (cboMaTaiSan.SelectedItem == null && !string.IsNullOrEmpty(phieu.MaTaiSan))
            {
                var taiSanMoi = new TaiSanCanBaoTri
                {
                    MaTaiSan = phieu.MaTaiSan,
                    TenTaiSan = phieu.TenTaiSan
                };
                _danhSachTaiSan.Add(taiSanMoi);
                cboMaTaiSan.SelectedItem = taiSanMoi;
            }

            txtTenTaiSan.Text = phieu.TenTaiSan;

            // Thiết lập giá trị cho ComboBox người phụ trách
            if (!string.IsNullOrEmpty(phieu.NguoiPhuTrach))
            {
                if (_danhSachNhanVien.Contains(phieu.NguoiPhuTrach))
                {
                    cboNguoiPhuTrach.SelectedItem = phieu.NguoiPhuTrach;
                }
                else
                {
                    // Trường hợp người phụ trách không có trong danh sách, thêm vào
                    _danhSachNhanVien.Add(phieu.NguoiPhuTrach);
                    cboNguoiPhuTrach.SelectedItem = phieu.NguoiPhuTrach;
                }
            }

            txtChiPhiDuKien.Text = phieu.ChiPhiDuKien.ToString("N0");
            txtNoiDungBaoTri.Text = phieu.NoiDungBaoTri;

            dtpNgayBaoTri.SelectedDate = phieu.NgayBaoTri;
            dtpNgayHoanThanh.SelectedDate = phieu.NgayHoanThanh;

            // Tìm và chọn giá trị tương ứng trong ComboBox
            SelectComboBoxItem(cboLoaiBaoTri, phieu.LoaiBaoTri);
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

                // Tìm và chọn mã tài sản trong combobox
                foreach (TaiSanCanBaoTri item in _danhSachTaiSan)
                {
                    if (item.MaTaiSan == taiSan.MaTaiSan)
                    {
                        cboMaTaiSan.SelectedItem = item;
                        break;
                    }
                }

                // Nếu không tìm thấy trong danh sách, thêm vào danh sách và chọn
                if (cboMaTaiSan.SelectedItem == null)
                {
                    _danhSachTaiSan.Add(taiSan);
                    cboMaTaiSan.SelectedItem = taiSan;
                }

                // Thiết lập thông tin tài sản vào form
                txtTenTaiSan.Text = taiSan.TenTaiSan;
                txtTenTaiSan.IsReadOnly = true;

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
                cboMaTaiSan.Text = "Multiple";
                txtTenTaiSan.Text = $"{_danhSachTaiSanDuocChon.Count} tài sản được chọn";
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
                // Lấy mã tài sản từ ComboBox thay vì TextBox
                if (cboMaTaiSan.SelectedItem is TaiSanCanBaoTri taiSan)
                {
                    _phieuBaoTri.MaTaiSan = taiSan.MaTaiSan;
                    _phieuBaoTri.TenTaiSan = taiSan.TenTaiSan;
                }
                else if (!string.IsNullOrEmpty(cboMaTaiSan.Text))
                {
                    _phieuBaoTri.MaTaiSan = cboMaTaiSan.Text;
                    _phieuBaoTri.TenTaiSan = txtTenTaiSan.Text;
                }
            }

            _phieuBaoTri.LoaiBaoTri = cboLoaiBaoTri.SelectedItem != null ? ((ComboBoxItem)cboLoaiBaoTri.SelectedItem).Content.ToString() : string.Empty;
            _phieuBaoTri.NgayBaoTri = dtpNgayBaoTri.SelectedDate ?? DateTime.Now;
            _phieuBaoTri.NgayHoanThanh = dtpNgayHoanThanh.SelectedDate;

            // Lấy giá trị người phụ trách từ ComboBox
            _phieuBaoTri.NguoiPhuTrach = cboNguoiPhuTrach.Text;

            _phieuBaoTri.TrangThai = cboTrangThai.SelectedItem != null ? ((ComboBoxItem)cboTrangThai.SelectedItem).Content.ToString() : string.Empty;
            _phieuBaoTri.ChiPhiDuKien = decimal.TryParse(txtChiPhiDuKien.Text.Replace(",", ""), out decimal chiPhi) ? chiPhi : 0;
            _phieuBaoTri.NoiDungBaoTri = txtNoiDungBaoTri.Text;

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
                if (string.IsNullOrWhiteSpace(cboMaTaiSan.Text))
                {
                    MessageBox.Show("Vui lòng chọn hoặc nhập Mã tài sản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    cboMaTaiSan.Focus();
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

            if (string.IsNullOrWhiteSpace(cboNguoiPhuTrach.Text))
            {
                MessageBox.Show("Vui lòng chọn hoặc nhập Người phụ trách!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                cboNguoiPhuTrach.Focus();
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