using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models.BanGiaoTaiSan;
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
            txtcbTiepNhan.Text = _phieu.CbTiepNhan;
            txtNgayBanGiao.Text = _phieu.NgayBanGiao.ToString("dd/MM/yyyy");
            txtNoiDung.Text = _phieu.NoiDung;
            txtTrangThai.Text = _phieu.TrangThai == true ? "Đã duyệt" : "Chưa duyệt";

            // Load chi tiết phiếu bàn giao
            _dsChiTiet = await BanGiaoTaiSanService.LayDanhSachChiTietBanGiaoAsync(_maPhieuBanGiao);
            dgChiTietPhieuNhap.ItemsSource = _dsChiTiet;
            txtStatus.Text = $"Tổng số dòng chi tiết: {_dsChiTiet.Count}";

            // Nếu không có chi tiết → cảnh báo và disable nút duyệt/từ chối
            if (_dsChiTiet.Count == 0)
            {
                MessageBox.Show("Phiếu bàn giao này không có thông tin chi tiết để duyệt!!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                btnDuyet.IsEnabled = false;
                btnTuChoi.IsEnabled = false;
                return;
            }

            btnDuyet.IsEnabled = _phieu.TrangThai != true;
            btnTuChoi.IsEnabled = _phieu.TrangThai != true;
        }

        private async void btnDuyet_Click(object sender, RoutedEventArgs e)
        {
            await DuyetPhieuAsync();
        }

        private async Task DuyetPhieuAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // ✅ 1. Lấy danh sách nhóm TS & kiểm tra trước
                var dsNhomTS = _dsChiTiet.Select(ct => ct.MaNhomTS!.Value).Distinct().ToList();
                var dsViTriDaDung = await BanGiaoTaiSanService.LayDanhSachViTriDaSuDungAsync(_phieu.MaPhong!.Value, dsNhomTS);

                // ✅ 2. Kiểm tra trùng vị trí - nếu có lỗi thì thoát NGAY
                foreach (var ct in _dsChiTiet)
                {
                    if (dsViTriDaDung.TryGetValue(ct.MaNhomTS!.Value, out var dsViTri) && dsViTri.Contains(ct.ViTriTS))
                    {
                        MessageBox.Show($"❌ Vị trí {ct.ViTriTS} trong phòng {_phieu.MaPhong} đã được dùng cho nhóm tài sản {ct.MaNhomTS}", "Lỗi khi duyệt", MessageBoxButton.OK, MessageBoxImage.Error);
                        return; // ⛔ STOP hoàn toàn, chưa cập nhật gì
                    }
                }

                // ✅ 3. CHỈ khi hợp lệ mới thực hiện cập nhật mã phòng
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
                        await client.From<TaiSanModel>().Update(taiSan);
                    }
                }

                // ✅ 4. Cập nhật trạng thái phiếu bàn giao
                var phieuResp = await client
                    .From<BanGiaoTaiSanModel>()
                    .Filter("ma_bang_giao_ts", Operator.Equals, _maPhieuBanGiao)
                    .Get();

                var phieu = phieuResp.Models.FirstOrDefault();
                if (phieu != null)
                {
                    phieu.TrangThai = true;
                    await client.From<BanGiaoTaiSanModel>().Update(phieu);
                }

                MessageBox.Show("✅ Duyệt phiếu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                OnPhieuDuyetThanhCong?.Invoke();
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi duyệt: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
