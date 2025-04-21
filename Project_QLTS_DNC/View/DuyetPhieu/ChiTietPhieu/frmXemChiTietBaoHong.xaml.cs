using Project_QLTS_DNC.Models.Phieu;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Supabase.Postgrest.Constants;

namespace Project_QLTS_DNC.View.DuyetPhieu.ChiTietPhieu
{
    public partial class frmXemChiTietBaoHong : UserControl
    {
        private long maPhieuBaoHongHienTai;
        private ObservableCollection<BaoHongHienThi> danhSachChiTiet = new();
        private BaoHongHienThi thongTinPhieu = new();

        public event Action OnPhieuDuyetThanhCong;

        public frmXemChiTietBaoHong()
        {
            InitializeComponent();
        }

        public class BaoHongHienThi
        {
            public long MaPhieuBaoHong { get; set; }
            public string TenNV { get; set; }
            public string TenPhong { get; set; }
            public string TenTaiSan { get; set; }
            public string HinhThucGhiNhan { get; set; }
            public string MoTa { get; set; }
            public DateTime NgayBaoHong { get; set; }
            public bool? TrangThaiBool { get; set; }

            public string TrangThai => TrangThaiBool == true ? "Đã duyệt"
                                     : TrangThaiBool == false ? "Từ chối duyệt"
                                     : "Chưa duyệt";
        }

        public async void LoadTheoMaPhieu(long maPhieuBaoHong)
        {
            maPhieuBaoHongHienTai = maPhieuBaoHong;

            try
            {
                var client = await SupabaseService.GetClientAsync();

                var dsPhieu = await client.From<BaoHong>()
                    .Filter("ma_phieu_bao_hong", Operator.Equals, maPhieuBaoHong)
                    .Get();

                var dsNV = await client.From<NhanVienModel>().Get();
                var dsPhong = await client.From<Phong>().Get();
                var dsTS = await client.From<TaiSanModel>().Get();

                var result = dsPhieu.Models.Select(p =>
                {
                    var nv = dsNV.Models.FirstOrDefault(x => x.MaNV == p.MaNV);
                    var phong = dsPhong.Models.FirstOrDefault(x => x.MaPhong == p.MaPhong);
                    var ts = dsTS.Models.FirstOrDefault(x => x.MaTaiSan == p.MaTaiSan);


                    return new BaoHongHienThi
                    {
                        MaPhieuBaoHong = p.MaPhieuBaoHong,
                        TenNV = nv?.TenNV ?? "(Không rõ)",
                        TenPhong = phong?.TenPhong ?? "(Không rõ)",
                        TenTaiSan = ts?.TenTaiSan ?? "(Không rõ)",
                        HinhThucGhiNhan = p.HinhThucGhiNhan,
                        MoTa = p.MoTa,
                        NgayBaoHong = p.NgayBaoHong,
                        TrangThaiBool = p.TrangThai
                    };
                }).ToList();

                danhSachChiTiet = new ObservableCollection<BaoHongHienThi>(result);
                dgChiTietBaoHong.ItemsSource = danhSachChiTiet;
                txtStatus.Text = $"Tổng số dòng chi tiết: {danhSachChiTiet.Count}";

                if (danhSachChiTiet.Count > 0)
                {
                    thongTinPhieu = danhSachChiTiet.First();
                    SetupPhieuInfo(thongTinPhieu);
                }

                bool chuaDuyet = result.Any() && result.First().TrangThai == "Chưa duyệt";
                btnDuyet.IsEnabled = chuaDuyet;
                btnTuChoi.IsEnabled = chuaDuyet;
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi load phiếu báo hỏng: " + ex.Message);
            }
        }

        private void SetupPhieuInfo(BaoHongHienThi phieu)
        {
            txtMaPhieu.Text = "PBH" + phieu.MaPhieuBaoHong;
            txtTenNV.Text = phieu.TenNV;
            txtTenNCC.Text = phieu.TenPhong;
            txtNgayBaoHong.Text = phieu.NgayBaoHong.ToString("dd/MM/yyyy");
            txtMoTa.Text = phieu.MoTa;
            txtTrangThai.Text = phieu.TrangThai;
        }

        private async void btnDuyet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                var phieu = await client
                    .From<BaoHong>()
                    .Filter("ma_phieu_bao_hong", Operator.Equals, maPhieuBaoHongHienTai)
                    .Get();

                if (phieu.Models.Any())
                {
                    var p = phieu.Models.First();
                    p.TrangThai = true;
                    await client
                        .From<BaoHong>()
                        .Where(x => x.MaPhieuBaoHong == maPhieuBaoHongHienTai)
                        .Update(p);
                }

                MessageBox.Show("✅ Phiếu đã được duyệt!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                OnPhieuDuyetThanhCong?.Invoke();
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi duyệt phiếu: " + ex.Message);
            }
        }

        private async void btnTuChoi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                var phieu = await client
                    .From<BaoHong>()
                    .Filter("ma_phieu_bao_hong", Operator.Equals, maPhieuBaoHongHienTai)
                    .Get();

                if (phieu.Models.Any())
                {
                    var p = phieu.Models.First();
                    p.TrangThai = false;
                    await client
                        .From<BaoHong>()
                        .Where(x => x.MaPhieuBaoHong == maPhieuBaoHongHienTai)
                        .Update(p);
                }

                MessageBox.Show("⛔ Phiếu đã bị từ chối!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                OnPhieuDuyetThanhCong?.Invoke();
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi từ chối phiếu: " + ex.Message);
            }
        }

        private void btnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}
