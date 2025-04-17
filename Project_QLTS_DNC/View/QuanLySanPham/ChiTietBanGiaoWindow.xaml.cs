using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
using Project_QLTS_DNC.Services.BanGiaoTaiSanService;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class ChiTietBanGiaoWindow : Window
    {
        private int _maBanGiao;
        private BanGiaoTaiSanDTO _thongTinBanGiao;
        private ObservableCollection<ChiTietBanGiaoDTO> _dsChiTiet;

        public ChiTietBanGiaoWindow(int maBanGiao)
        {
            InitializeComponent();
            _maBanGiao = maBanGiao;

            // Load dữ liệu
            Loaded += ChiTietBanGiaoWindow_Loaded;
        }

        private async void ChiTietBanGiaoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                ShowLoading(true);

                // Lấy thông tin phiếu bàn giao
                var dsBanGiao = await BanGiaoTaiSanService.LayDanhSachPhieuBanGiaoAsync();
                _thongTinBanGiao = dsBanGiao.FirstOrDefault(p => p.MaBanGiaoTS == _maBanGiao);

                if (_thongTinBanGiao == null)
                {
                    MessageBox.Show($"Không tìm thấy thông tin phiếu bàn giao với mã {_maBanGiao}.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                // Hiển thị thông tin phiếu
                txtMaPhieu.Text = _thongTinBanGiao.MaBanGiaoTS.ToString();
                txtNgayBanGiao.Text = _thongTinBanGiao.NgayBanGiao.ToString("dd/MM/yyyy HH:mm");
                txtNguoiLap.Text = _thongTinBanGiao.TenNV;
                txtPhong.Text = _thongTinBanGiao.TenPhong;
                txtToaNha.Text = _thongTinBanGiao.TenToaNha;
                txtNoiDung.Text = _thongTinBanGiao.NoiDung;

                // Định dạng trạng thái
                txtTrangThai.Text = _thongTinBanGiao.TrangThaiText;
                switch (_thongTinBanGiao.TrangThaiText)
                {
                    case "Chờ duyệt":
                        txtTrangThai.Foreground = new SolidColorBrush(Colors.Orange);
                        break;
                    case "Đã duyệt":
                        txtTrangThai.Foreground = new SolidColorBrush(Colors.Green);
                        break;
                    case "Từ chối duyệt":
                        txtTrangThai.Foreground = new SolidColorBrush(Colors.Red);
                        break;
                }

                // Lấy chi tiết bàn giao
                _dsChiTiet = await BanGiaoTaiSanService.LayDanhSachChiTietBanGiaoAsync(_maBanGiao);
                dgChiTietBanGiao.ItemsSource = _dsChiTiet;

                // Cập nhật tiêu đề cửa sổ
                this.Title = $"Chi Tiết Phiếu Bàn Giao - Mã phiếu: {_maBanGiao}";
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

        private void btnDong_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShowLoading(bool isShow)
        {
            LoadingOverlay.Visibility = isShow ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}