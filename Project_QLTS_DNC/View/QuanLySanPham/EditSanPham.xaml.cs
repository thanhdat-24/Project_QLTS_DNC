using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Services.QLTaiSanService;
using Project_QLTS_DNC.View.QuanLySanPham;

namespace Project_QLTS_DNC.View.QuanLySanPham
{
    public partial class EditSanPham : Window
    {
        private DanhSachSanPham _parentWindow;
        private List<PhongFilter> _phongList;
        private TaiSanModel _taiSan;

        public EditSanPham(DanhSachSanPham parentWindow, TaiSanModel taiSan, List<PhongFilter> phongList)
        {
            InitializeComponent();
            _parentWindow = parentWindow;
            _taiSan = taiSan;
            _phongList = phongList;

            LoadTaiSanData();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoadTaiSanData()
        {
            try
            {
                // Hiển thị mã tài sản
                txtMaTaiSan.Text = _taiSan.MaTaiSan.ToString();

                // Hiển thị mã chi tiết phiếu nhập nếu có
                if (_taiSan.MaChiTietPN.HasValue)
                {
                    txtMaChiTietPN.Text = _taiSan.MaChiTietPN.Value.ToString();
                }

                // Load thông tin tài sản lên form
                txtTenTaiSan.Text = _taiSan.TenTaiSan;
                txtSoSeri.Text = _taiSan.SoSeri;
                txtMaQR.Text = _taiSan.MaQR;
                dpNgaySuDung.SelectedDate = _taiSan.NgaySuDung;
                dpHanBH.SelectedDate = _taiSan.HanBH;

                // Đặt trạng thái cho ComboBox tình trạng dựa vào giá trị hiện tại
                SetTinhTrangComboBox(_taiSan.TinhTrangSP);

                txtGhiChu.Text = _taiSan.GhiChu;

                // Load danh sách phòng vào combobox
                cboPhong.ItemsSource = _phongList;

                // Chọn phòng hiện tại của tài sản
                if (_taiSan.MaPhong.HasValue)
                {
                    cboPhong.SelectedItem = _phongList.FirstOrDefault(p => p.MaPhong == _taiSan.MaPhong);
                }
                else
                {
                    // Chọn "Tất cả" hoặc phòng đầu tiên
                    cboPhong.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu tài sản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Phương thức mới để đặt giá trị cho ComboBox tình trạng
        private void SetTinhTrangComboBox(string tinhTrangValue)
        {
            // Mặc định là "Không xác định" nếu không tìm thấy
            int selectedIndex = 4;

            if (!string.IsNullOrEmpty(tinhTrangValue))
            {
                // Tìm giá trị tương ứng trong ComboBox
                switch (tinhTrangValue.Trim())
                {
                    case "Mới":
                        selectedIndex = 0;
                        break;
                    case "Đang hoạt động":
                        selectedIndex = 1;
                        break;
                    case "Cần kiểm tra":
                        selectedIndex = 2;
                        break;
                    case "Hư hỏng":
                        selectedIndex = 3;
                        break;
                    case "Không xác định":
                        selectedIndex = 4;
                        break;
                }
            }

            cboTinhTrang.SelectedIndex = selectedIndex;
        }

        private void BtnCapNhat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Cập nhật thông tin tài sản từ form
                UpdateTaiSan();

                // Lưu vào database
                UpdateTaiSanInDatabase();

                // Đóng form sau khi cập nhật
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật tài sản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTaiSan()
        {
            // Lấy dữ liệu từ form và cập nhật vào đối tượng tài sản
            _taiSan.TenTaiSan = txtTenTaiSan.Text.Trim();
            _taiSan.SoSeri = txtSoSeri.Text.Trim();
            _taiSan.MaQR = txtMaQR.Text.Trim();
            _taiSan.NgaySuDung = dpNgaySuDung.SelectedDate;
            _taiSan.HanBH = dpHanBH.SelectedDate;

            // Lấy tình trạng từ ComboBox thay vì TextBox
            if (cboTinhTrang.SelectedItem != null)
            {
                ComboBoxItem selectedItem = cboTinhTrang.SelectedItem as ComboBoxItem;
                _taiSan.TinhTrangSP = selectedItem?.Content.ToString();
            }
            else
            {
                _taiSan.TinhTrangSP = "Không xác định";
            }

            _taiSan.GhiChu = txtGhiChu.Text.Trim();

            // Không cập nhật mã phòng vì control đã bị vô hiệu hóa
            // Giữ nguyên giá trị cũ của _taiSan.MaPhong
        }

        private async void UpdateTaiSanInDatabase()
        {
            try
            {
                // Gọi service để cập nhật tài sản
                var updatedTaiSan = await TaiSanService.CapNhatTaiSanAsync(_taiSan);

                if (updatedTaiSan != null)
                {
                    MessageBox.Show("Cập nhật tài sản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Làm mới danh sách tài sản ở form chính
                    if (_parentWindow != null)
                    {
                        // Gọi phương thức RefreshData của form cha để refresh dữ liệu
                        _parentWindow.RefreshData();
                    }
                }
                else
                {
                    MessageBox.Show("Không thể cập nhật tài sản.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật tài sản trong cơ sở dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}