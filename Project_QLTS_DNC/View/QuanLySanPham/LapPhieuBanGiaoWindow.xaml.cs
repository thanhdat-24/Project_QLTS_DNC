using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.BanGiaoTaiSan;
using Project_QLTS_DNC.Services.BanGiaoTaiSanService;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.View.QuanLyToanNha;
using Project_QLTS_DNC.Helpers;
using Project_QLTS_DNC.Services.ThongBao;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class LapPhieuBanGiaoWindow : Window
    {
        private ObservableCollection<PhongBanGiaoFilter> _dsPhong;
        private ObservableCollection<KhoBanGiaoFilter> _dsKho;
        private ObservableCollection<NhanVienModel> _dsNhanVien;
        private ObservableCollection<TaiSanKhoBanGiaoDTO> _dsTaiSanKho;
        private Dictionary<int, List<int>> _viTriDaSuDung = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> _viTriKhaDung = new Dictionary<int, List<int>>();

        private int? _maToaNhaPhong = null;
        private int? _maToaNhaKho = null;

        public LapPhieuBanGiaoWindow()
        {
            InitializeComponent();

            // Khởi tạo ngày hiện tại
            dateBanGiao.SelectedDate = DateTime.Now;

            // Load dữ liệu
            _ = LoadInitialData();
        }

        private async Task LoadInitialData()
        {
            try
            {
                ShowLoading(true);

                // Load phòng
                _dsPhong = await BanGiaoTaiSanService.LayDanhSachPhongBanGiaoAsync();
                cboPhong.ItemsSource = _dsPhong;

                // Load kho
                _dsKho = await BanGiaoTaiSanService.LayDanhSachKhoBanGiaoAsync();
                cboKho.ItemsSource = _dsKho;

                // Load nhân viên
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<NhanVienModel>().Get();
                _dsNhanVien = new ObservableCollection<NhanVienModel>(response.Models);
                cboNhanVien.ItemsSource = _dsNhanVien;

                // Chọn nhân viên đầu tiên nếu có
                if (_dsNhanVien.Count > 0)
                {
                    cboNhanVien.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async void cboPhong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selectedPhong = cboPhong.SelectedItem as PhongBanGiaoFilter;
                if (selectedPhong != null)
                {
                    _maToaNhaPhong = selectedPhong.MaToaNha;
                    await CheckToaNhaCompatibility();
                }
                else
                {
                    _maToaNhaPhong = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chọn phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void cboKho_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selectedKho = cboKho.SelectedItem as KhoBanGiaoFilter;
                if (selectedKho != null)
                {
                    _maToaNhaKho = selectedKho.MaToaNha;

                    // Kiểm tra tòa nhà
                    await CheckToaNhaCompatibility();

                    // Nếu tòa nhà hợp lệ và đã chọn cả phòng và kho, load tài sản trong kho
                    if (_maToaNhaPhong != null && _maToaNhaPhong == _maToaNhaKho)
                    {
                        await LoadTaiSanTrongKho();
                    }
                }
                else
                {
                    _maToaNhaKho = null;
                    dgTaiSanKho.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chọn kho: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CheckToaNhaCompatibility()
        {
            // Hiển thị cảnh báo nếu đã chọn cả phòng và kho nhưng không cùng tòa nhà
            if (_maToaNhaPhong != null && _maToaNhaKho != null)
            {
                if (_maToaNhaPhong != _maToaNhaKho)
                {
                    txtThongBaoToaNha.Visibility = Visibility.Visible;
                    dgTaiSanKho.ItemsSource = null;
                }
                else
                {
                    txtThongBaoToaNha.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                txtThongBaoToaNha.Visibility = Visibility.Collapsed;
            }
        }

        private async Task CapNhatDanhSachViTriAsync()
        {
            try
            {
                if (_dsTaiSanKho == null || _dsTaiSanKho.Count == 0 || !_maToaNhaPhong.HasValue)
                    return;

                var selectedPhong = cboPhong.SelectedItem as PhongBanGiaoFilter;
                if (selectedPhong == null)
                    return;

                // Lấy danh sách nhóm tài sản
                var dsNhomTS = _dsTaiSanKho
                    .Where(ts => ts.MaNhomTS.HasValue)
                    .Select(ts => ts.MaNhomTS.Value)
                    .Distinct()
                    .ToList();

                if (dsNhomTS.Count == 0)
                    return;

                // Lấy danh sách vị trí đã sử dụng
                _viTriDaSuDung = await BanGiaoTaiSanService.LayDanhSachViTriDaSuDungAsync(selectedPhong.MaPhong, dsNhomTS);

                // Lấy danh sách vị trí khả dụng
                _viTriKhaDung = await BanGiaoTaiSanService.TaoDanhSachViTriKhaDungAsync(selectedPhong.MaPhong, dsNhomTS);

                // Cập nhật danh sách vị trí cho mỗi tài sản
                foreach (var taiSan in _dsTaiSanKho)
                {
                    if (taiSan.MaNhomTS.HasValue)
                    {
                        int maNhomTS = taiSan.MaNhomTS.Value;

                        // Xóa danh sách vị trí cũ
                        taiSan.ViTriList.Clear();

                        // Nếu có danh sách vị trí khả dụng cho nhóm này
                        // Nếu có thông tin sức chứa cho nhóm này
                        if (_viTriKhaDung.TryGetValue(maNhomTS, out List<int> dsViTri))
                        {
                            List<int> daDaSuDung = _viTriDaSuDung.TryGetValue(maNhomTS, out List<int> ds) ? ds : new List<int>();

                            // Thêm tất cả vị trí vào danh sách
                            foreach (int viTri in dsViTri)
                            {
                                taiSan.ViTriList.Add(new ViTriTSItem
                                {
                                    ViTri = viTri,
                                    DaDuocSuDung = daDaSuDung.Contains(viTri),
                                    IsConfigurationNeeded = false
                                });
                            }

                            // Add code here to handle default selection if needed
                        }
                        else
                        {
                            // No configured capacity - add a special item
                            taiSan.ViTriList.Add(new ViTriTSItem
                            {
                                ViTri = 0,  // Invalid position that will trigger validation
                                DaDuocSuDung = false,
                                IsConfigurationNeeded = true
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật danh sách vị trí: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadTaiSanTrongKho()
        {
            try
            {
                ShowLoading(true);

                // Lấy danh sách tài sản trong kho thuộc tòa nhà đã chọn
                if (_maToaNhaPhong.HasValue)
                {
                    _dsTaiSanKho = await BanGiaoTaiSanService.LayDanhSachTaiSanTrongKhoAsync(_maToaNhaPhong.Value);
                    dgTaiSanKho.ItemsSource = _dsTaiSanKho;

                    // Cập nhật danh sách vị trí
                    await CapNhatDanhSachViTriAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách tài sản trong kho: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        // Thêm sự kiện cho ComboBox vị trí
        private void cboViTri_Loaded(object sender, RoutedEventArgs e)
        {
            // Xử lý sự kiện khi ComboBox được tải
            ComboBox cbo = sender as ComboBox;
            if (cbo != null && cbo.Items.Count > 0 && cbo.SelectedItem == null)
            {
                // Mặc định chọn vị trí đầu tiên nếu chưa có vị trí được chọn
                cbo.SelectedIndex = 0;
            }
        }

        private async void cboViTri_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbo = sender as ComboBox;
            if (cbo != null && cbo.SelectedItem != null)
            {
                var taiSan = cbo.DataContext as TaiSanKhoBanGiaoDTO;
                if (taiSan != null)
                {
                    var viTriItem = cbo.SelectedItem as ViTriTSItem;
                    if (viTriItem != null)
                    {
                        // Check if configuration is needed
                        if (viTriItem.IsConfigurationNeeded)
                        {
                            // Show configuration dialog
                            MessageBoxResult result = MessageBox.Show(
                                $"Phòng chưa được cấu hình sức chứa cho nhóm tài sản '{taiSan.TenNhomTS}'. Bạn có muốn cấu hình ngay không?",
                                "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                            if (result == MessageBoxResult.Yes)
                            {
                                // Get selected room
                                var selectedPhong = cboPhong.SelectedItem as PhongBanGiaoFilter;
                                if (selectedPhong != null && taiSan.MaNhomTS.HasValue)
                                {
                                    // Open configuration form
                                    var configWindow = new suc_chua_phong_nhom(selectedPhong.MaPhong);
                                    configWindow.ShowDialog();

                                    // Reload position list after configuration
                                    await CapNhatDanhSachViTriAsync();

                                    // Try to select the first available position
                                    if (taiSan.ViTriList.Count > 0)
                                    {
                                        var firstNonConfigItem = taiSan.ViTriList.FirstOrDefault(v => !v.IsConfigurationNeeded);
                                        if (firstNonConfigItem != null)
                                        {
                                            cbo.SelectedItem = firstNonConfigItem;
                                            taiSan.ViTriTS = firstNonConfigItem.ViTri;
                                        }
                                    }
                                }
                            }
                            // Don't update ViTriTS
                            return;
                        }

                        // Update asset position as normal
                        taiSan.ViTriTS = viTriItem.ViTri;
                    }
                }
            }
        }
        private async void dgTaiSanKho_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Nếu có tài sản được chọn/hủy, cập nhật danh sách vị trí
            await CapNhatDanhSachViTriAsync();
        }
        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            // Xác nhận trước khi đóng
            MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn hủy bỏ việc lập phiếu bàn giao?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        private async void btnLapPhieu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (!ValidateInput())
                    return;
                ShowLoading(true);

                // Lấy thông tin từ form
                var selectedPhong = cboPhong.SelectedItem as PhongBanGiaoFilter;
                var selectedNhanVien = cboNhanVien.SelectedItem as NhanVienModel;
                var selectedKho = cboKho.SelectedItem as KhoBanGiaoFilter;

                // Lấy ngày từ DatePicker và thêm giờ hiện tại
                DateTime selectedDate = dateBanGiao.SelectedDate ?? DateTime.Now.Date;
                DateTime localTime = new DateTime(
                    selectedDate.Year,
                    selectedDate.Month,
                    selectedDate.Day,
                    DateTime.Now.Hour,
                    DateTime.Now.Minute,
                    DateTime.Now.Second,
                    DateTimeKind.Local  // Chỉ định rõ ràng đây là giờ địa phương
                );

                // Chuyển đổi sang UTC trước khi lưu vào Supabase
                DateTime utcTime = localTime.ToUniversalTime();

                var noiDung = txtNoiDung.Text.Trim();
                var nguoiTiepNhan = txtNguoiTiepNhan.Text.Trim(); // Lấy thông tin người tiếp nhận

                // Phần còn lại giữ nguyên
                var dsTaiSanDaChon = _dsTaiSanKho.Where(t => t.IsSelected).ToList();
                if (dsTaiSanDaChon.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn ít nhất một tài sản để bàn giao.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ShowLoading(false);
                    return;
                }

                // THÊM MỚI: Kiểm tra và yêu cầu cấu hình sức chứa cho các nhóm tài sản quản lý riêng
                bool sucChuaValid = await BanGiaoTaiSanService.KiemTraVaYeuCauCauHinhSucChuaAsync(
                    selectedPhong.MaPhong,
                    dsTaiSanDaChon);
                if (!sucChuaValid)
                {
                    ShowLoading(false);
                    return; // Dừng nếu không thỏa mãn điều kiện sức chứa
                }

                // Tạo phiếu bàn giao với thời gian UTC
                var phieuBanGiao = new BanGiaoTaiSanModel
                {
                    NgayBanGiao = utcTime,  // Lưu thời gian UTC vào cơ sở dữ liệu
                    MaNV = selectedNhanVien.MaNV,
                    MaPhong = selectedPhong.MaPhong,
                    NoiDung = noiDung,
                    TrangThai = null, // Chờ duyệt
                    CbTiepNhan = nguoiTiepNhan, // Thêm thông tin người tiếp nhận
                    MaKho = selectedKho.MaKho
                };

                // Phần còn lại của hàm giữ nguyên
                var phieuBanGiaoResult = await BanGiaoTaiSanService.ThemPhieuBanGiaoAsync(phieuBanGiao);
                var dsChiTietBanGiao = new List<ChiTietBanGiaoModel>();
                foreach (var taiSan in dsTaiSanDaChon)
                {
                    dsChiTietBanGiao.Add(new ChiTietBanGiaoModel
                    {
                        MaBanGiaoTS = phieuBanGiaoResult.MaBanGiaoTS,
                        MaTaiSan = taiSan.MaTaiSan,
                        ViTriTS = taiSan.ViTriTS,
                        GhiChu = taiSan.GhiChu
                    });
                }
                var dsChiTietResult = await BanGiaoTaiSanService.ThemChiTietBanGiaoAsync(dsChiTietBanGiao);

                // 🔔 Gửi thông báo cập nhật phiếu
                await new ThongBaoService().ThongBaoTaoPhieuAsync(
                    phieuBanGiaoResult.MaBanGiaoTS,
                    "phiếu bàn giao",
                    ThongTinDangNhap.TaiKhoanDangNhap.MaTk
                );


                MessageBox.Show($"Lập phiếu bàn giao thành công! Mã phiếu: {phieuBanGiaoResult.MaBanGiaoTS}",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lập phiếu bàn giao: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private bool ValidateInput()
        {
            // Kiểm tra phòng
            if (cboPhong.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn phòng bàn giao đến.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                cboPhong.Focus();
                return false;
            }

            // Kiểm tra kho
            if (cboKho.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn kho nguồn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                cboKho.Focus();
                return false;
            }

            // Kiểm tra tòa nhà
            if (_maToaNhaPhong != _maToaNhaKho)
            {
                MessageBox.Show("Phòng và kho phải thuộc cùng một tòa nhà!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            // Kiểm tra nhân viên
            if (cboNhanVien.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn người lập phiếu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                cboNhanVien.Focus();
                return false;
            }

            // Kiểm tra ngày
            if (!dateBanGiao.SelectedDate.HasValue)
            {
                MessageBox.Show("Vui lòng chọn ngày bàn giao.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                dateBanGiao.Focus();
                return false;
            }

            // Kiểm tra các tài sản đã chọn
            var dsTaiSanDaChon = _dsTaiSanKho?.Where(t => t.IsSelected).ToList();

            if (dsTaiSanDaChon == null || dsTaiSanDaChon.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một tài sản để bàn giao.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Kiểm tra vị trí
            // Trong phương thức ValidateInput, thay đổi phần kiểm tra vị trí:
            foreach (var taiSan in dsTaiSanDaChon)
            {
                if (taiSan.ViTriTS <= 0)
                {
                    // Kiểm tra xem có phải do chưa cấu hình sức chứa
                    bool needsConfiguration = taiSan.ViTriList.Any(v => v.IsConfigurationNeeded);
                    if (needsConfiguration)
                    {
                        MessageBox.Show($"Vui lòng cấu hình sức chứa cho tài sản '{taiSan.TenTaiSan}' trước khi bàn giao.",
                            "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show($"Vị trí của tài sản '{taiSan.TenTaiSan}' phải là số dương.",
                            "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    return false;
                }
            }

            return true;
        }

        private void ShowLoading(bool isShow)
        {
            LoadingOverlay.Visibility = isShow ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}