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

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class LapPhieuBanGiaoWindow : Window
    {
        private ObservableCollection<PhongBanGiaoFilter> _dsPhong;
        private ObservableCollection<KhoBanGiaoFilter> _dsKho;
        private ObservableCollection<NhanVienModel> _dsNhanVien;
        private ObservableCollection<TaiSanKhoBanGiaoDTO> _dsTaiSanKho;

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

        private void txtViTri_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Chỉ cho phép nhập số
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
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
                var ngayBanGiao = dateBanGiao.SelectedDate ?? DateTime.Now;
                var noiDung = txtNoiDung.Text.Trim();

                // Tạo phiếu bàn giao
                var phieuBanGiao = new BanGiaoTaiSanModel
                {
                    NgayBanGiao = ngayBanGiao,
                    MaNV = selectedNhanVien.MaNV,
                    MaPhong = selectedPhong.MaPhong,
                    NoiDung = noiDung,
                    TrangThai = null // Chờ duyệt
                };

                // Lưu phiếu bàn giao
                var phieuBanGiaoResult = await BanGiaoTaiSanService.ThemPhieuBanGiaoAsync(phieuBanGiao);

                // Lấy danh sách tài sản đã chọn
                var dsTaiSanDaChon = _dsTaiSanKho.Where(t => t.IsSelected).ToList();

                if (dsTaiSanDaChon.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn ít nhất một tài sản để bàn giao.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ShowLoading(false);
                    return;
                }

                // Tạo danh sách chi tiết bàn giao
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

                // Lưu chi tiết bàn giao
                var dsChiTietResult = await BanGiaoTaiSanService.ThemChiTietBanGiaoAsync(dsChiTietBanGiao);

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
            foreach (var taiSan in dsTaiSanDaChon)
            {
                if (taiSan.ViTriTS <= 0)
                {
                    MessageBox.Show($"Vị trí của tài sản '{taiSan.TenTaiSan}' phải là số dương.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
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