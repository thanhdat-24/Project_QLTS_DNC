using Project_QLTS_DNC.Models.Phieu;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Supabase.Postgrest.Constants;
using Project_QLTS_DNC.Models.DuyetPhieu;

namespace Project_QLTS_DNC.View.DuyetPhieu.ChiTietPhieu
{
    public partial class frmXemChiTietDeNghiMua : UserControl
    {
        private ObservableCollection<ChiTietDeNghiMuaHienThi> danhSachChiTiet = new();
        private long _maPhieuDeNghi;

        public event Action? OnPhieuDuyetThanhCong;

        public frmXemChiTietDeNghiMua()
        {
            InitializeComponent();
        }

        public class ChiTietDeNghiMuaHienThi
        {
            public long MaPhieuDeNghi { get; set; }
            public long MaCTPhieuDeNghi { get; set; }
            public string TenNV { get; set; }
            public string DonViDeNghi { get; set; }
            public string TenTaiSan { get; set; }
            public int SoLuong { get; set; }
            public string DonViTinh { get; set; }
            public int? DuKienGia { get; set; }
            public string LyDo { get; set; }
            public string MoTa { get; set; }
            public string TrangThai { get; set; }
        }

        public void LoadTheoMaPhieu(long maPhieu)
        {
            _maPhieuDeNghi = maPhieu;
            _ = LoadChiTietPhieuTheoMa(maPhieu);
        }

        private async Task LoadChiTietPhieuTheoMa(long maPhieu)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var phieuRes = await client
                    .From<denghimua>()
                    .Filter("ma_phieu_de_nghi", Operator.Equals, maPhieu)
                    .Get();

                var listChiTiet = await client.From<CTdenghimua>().Get();
                var listNV = await client.From<NhanVienModel>().Get();

                var phieu = phieuRes.Models.FirstOrDefault();
                if (phieu == null)
                {
                    MessageBox.Show("Không tìm thấy phiếu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var nhanVien = listNV.Models.FirstOrDefault(x => x.MaNV == phieu.MaNV);

                // Gán thông tin phiếu
                txtMaPhieu.Text = "PDN" + phieu.MaPhieuDeNghi;
                txtNgayNhap.Text = phieu.NgayDeNghiMua?.ToString("dd/MM/yyyy") ?? "";
                txtDonVi.Text = phieu.DonViDeNghi;
                txtTenNV.Text = nhanVien?.TenNV ?? "(Không rõ)";
                txtTenNCC.Text = phieu.GhiChu;
                txtTrangThai.Text = phieu.TrangThai == true ? "Đã duyệt" :
                                    phieu.TrangThai == false ? "Từ chối duyệt" : "Chưa duyệt";

                // Danh sách chi tiết
                var result = (from ct in listChiTiet.Models
                              where ct.MaPhieuDeNghi == maPhieu
                              select new ChiTietDeNghiMuaHienThi
                              {
                                  MaPhieuDeNghi = phieu.MaPhieuDeNghi,
                                  MaCTPhieuDeNghi = ct.MaChiTietDNM,
                                  TenNV = nhanVien?.TenNV ?? "(Không rõ)",
                                  DonViDeNghi = phieu.DonViDeNghi,
                                  TenTaiSan = ct.TenTaiSan,
                                  SoLuong = ct.SoLuong,
                                  DonViTinh = ct.DonViTinh,
                                  DuKienGia = ct.DuKienGia,
                                  LyDo = phieu.LyDo,
                                  MoTa = ct.MoTa,
                                  TrangThai = phieu.TrangThai == true ? "Đã duyệt" :
                                              phieu.TrangThai == false ? "Từ chối duyệt" : "Chưa duyệt"
                              }).ToList();

                danhSachChiTiet = new ObservableCollection<ChiTietDeNghiMuaHienThi>(result);
                dgChiTietPhieuNhap.ItemsSource = danhSachChiTiet;
                txtStatus.Text = $"Tổng số dòng chi tiết: {danhSachChiTiet.Count}";

                bool chuaDuyet = phieu.TrangThai == null;
                btnDuyet.IsEnabled = chuaDuyet;
                btnTuChoi.IsEnabled = chuaDuyet;
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi load phiếu đề nghị mua: " + ex.Message);
            }
        }

        private async void btnDuyet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var res = await client
                    .From<denghimua>()
                    .Filter("ma_phieu_de_nghi", Operator.Equals, _maPhieuDeNghi)
                    .Get();

                if (res.Models.Any())
                {
                    var phieu = res.Models.First();
                    phieu.TrangThai = true;
                    await client.From<denghimua>().Update(phieu);

                    txtTrangThai.Text = "Đã duyệt";
                    MessageBox.Show("✅ Phiếu đã được duyệt!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    OnPhieuDuyetThanhCong?.Invoke();
                    Window.GetWindow(this)?.Close();
                }
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
                var res = await client
                    .From<denghimua>()
                    .Filter("ma_phieu_de_nghi", Operator.Equals, _maPhieuDeNghi)
                    .Get();

                if (res.Models.Any())
                {
                    var phieu = res.Models.First();
                    phieu.TrangThai = false;
                    await client.From<denghimua>().Update(phieu);

                    txtTrangThai.Text = "Từ chối duyệt";
                    MessageBox.Show("⛔ Phiếu đã bị từ chối!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    OnPhieuDuyetThanhCong?.Invoke();
                    Window.GetWindow(this)?.Close();
                }
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
