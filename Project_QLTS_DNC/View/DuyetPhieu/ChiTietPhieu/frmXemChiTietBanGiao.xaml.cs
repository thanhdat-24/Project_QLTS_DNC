using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Services.BanGiaoTaiSanService;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Supabase.Postgrest.Constants;

namespace Project_QLTS_DNC.View.DuyetPhieu.ChiTietPhieu
{
    public partial class frmXemChiTietBanGiao : UserControl
    {
        public event Action OnPhieuDuyetThanhCong;
        private int _maPhieuBanGiao;
        private BanGiaoTaiSanDTO _phieu;
        private ObservableCollection<ChiTietBanGiaoDTO> _dsChiTiet = new();

        public frmXemChiTietBanGiao(int maPhieuBanGiao)
        {
            InitializeComponent();
            _maPhieuBanGiao = maPhieuBanGiao;
            Loaded += frmXemChiTietBanGiao_Loaded;
        }

        private async void frmXemChiTietBanGiao_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDuLieuPhieu();
        }

        private async Task LoadDuLieuPhieu()
        {
            var danhSachPhieu = await BanGiaoTaiSanService.LayDanhSachPhieuBanGiaoAsync();
            _phieu = danhSachPhieu.FirstOrDefault(p => p.MaBanGiaoTS == _maPhieuBanGiao);

            if (_phieu == null)
            {
                MessageBox.Show("Không tìm thấy phiếu bàn giao!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Gán dữ liệu vào các TextBlock
            txtMaPhieu.Text = $"BG{_phieu.MaBanGiaoTS}";
            txtPhong.Text = _phieu.TenPhong;
            txtKho.Text = _phieu.TenToaNha;
            txtTenNVBanGiao.Text = _phieu.TenNV;
            txtTenNVTiepNhan.Text = _phieu.TenNV;
            txtNgayBanGiao.Text = _phieu.NgayBanGiao.ToString("dd/MM/yyyy");
            txtNoiDung.Text = _phieu.NoiDung;
            txtTrangThai.Text = _phieu.TrangThai == true ? "Đã duyệt" : "Chưa duyệt";

            btnDuyet.IsEnabled = _phieu.TrangThai != true;
            btnTuChoi.IsEnabled = _phieu.TrangThai != true;

            // Load chi tiết
            _dsChiTiet = await BanGiaoTaiSanService.LayDanhSachChiTietBanGiaoAsync(_maPhieuBanGiao);
            dgChiTietPhieuNhap.ItemsSource = _dsChiTiet;
            txtStatus.Text = $"Tổng số dòng chi tiết: {_dsChiTiet.Count}";
        }

        private async void btnDuyet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Cập nhật mã phòng cho từng tài sản trong chi tiết bàn giao
                foreach (var ct in _dsChiTiet)
                {
                    var response = await client
                        .From<TaiSanModel>()
                        .Filter("ma_tai_san", Operator.Equals, ct.MaTaiSan)
                        .Get();

                    var taiSan = response.Models.FirstOrDefault();
                    if (taiSan != null)
                    {
                        taiSan.MaPhong = _phieu.MaPhong;

                        // Update lại tài sản
                        await client.From<TaiSanModel>().Update(taiSan);
                    }
                }

                // Cập nhật trạng thái duyệt phiếu bàn giao
                var ketQua = await BanGiaoTaiSanService.DuyetPhieuBanGiaoAsync(_maPhieuBanGiao, true);

                if (ketQua)
                {
                    MessageBox.Show("✅ Duyệt phiếu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    OnPhieuDuyetThanhCong?.Invoke();
                    Window.GetWindow(this)?.Close();
                }
                else
                {
                    MessageBox.Show("❌ Không thể duyệt phiếu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi duyệt: " + ex.Message);
            }
        }



        private async void btnTuChoi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ketQua = await BanGiaoTaiSanService.DuyetPhieuBanGiaoAsync(_maPhieuBanGiao, false);
                if (ketQua)
                {
                    MessageBox.Show("❌ Phiếu đã bị từ chối.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDuLieuPhieu();
                }
                else
                {
                    MessageBox.Show("Không thể từ chối phiếu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi từ chối phiếu: " + ex.Message);
            }
        }

        private void btnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            parentWindow?.Close(); // Hoặc collapse nếu bạn dùng trong tab
        }
    }
}
