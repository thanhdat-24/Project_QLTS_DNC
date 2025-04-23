using Project_QLTS_DNC.Models.LichSu;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Supabase.Postgrest.Constants;

namespace Project_QLTS_DNC.View.DuyetPhieu.ChiTietPhieu
{
    public partial class frmXemChiTietLichSuDiChuyen : UserControl
    {
        private long maLichSuHienTai;
        private LichSuHienThi thongTinPhieu = new();

        public event Action OnPhieuDuyetThanhCong;

        public frmXemChiTietLichSuDiChuyen()
        {
            InitializeComponent();
        }

        public class LichSuHienThi
        {
            public long MaLichSu { get; set; }
            public string TenNhanVien { get; set; }
            public string TenPhongCu { get; set; }
            public string TenPhongMoi { get; set; }
            public DateTime? NgayBanGiao { get; set; }
            public string GhiChu { get; set; }
            public bool? TrangThai { get; set; }

            public string TrangThaiText => TrangThai == true ? "Đã duyệt"
                                            : TrangThai == false ? "Từ chối duyệt"
                                            : "Chưa duyệt";
        }

        public class ChiTietLichSuHienThi
        {
            public string MaLichSu { get; set; }
            public string TenNhanVien { get; set; }
            public string TenTaiSan { get; set; }
            public string GhiChu { get; set; }
            public string NgayBanGiao { get; set; }
        }

        public async void LoadTheoMaPhieu(long maLichSu)
        {
            maLichSuHienTai = maLichSu;

            try
            {
                var client = await SupabaseService.GetClientAsync();

                var ds = await client.From<LichSuDiChuyenTaiSan>()
                                     .Filter("ma_lich_su", Operator.Equals, maLichSu)
                                     .Get();

                var dsNhanVien = await client.From<NhanVienModel>().Get();
                var dsPhong = await client.From<Phong>().Get();
                var dsTS = await client.From<TaiSanModel>().Get();

                var record = ds.Models.FirstOrDefault();
                if (record == null)
                {
                    MessageBox.Show("Không tìm thấy phiếu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var nv = dsNhanVien.Models.FirstOrDefault(x => x.MaNV == record.MaNhanVien);
                var phongCu = dsPhong.Models.FirstOrDefault(x => x.MaPhong == record.MaPhongCu);
                var phongMoi = dsPhong.Models.FirstOrDefault(x => x.MaPhong == record.MaPhongMoi);

                thongTinPhieu = new LichSuHienThi
                {
                    MaLichSu = record.MaLichSu,
                    TenNhanVien = nv?.TenNV ?? "(Không rõ)",
                    TenPhongCu = phongCu?.TenPhong ?? "(Không rõ)",
                    TenPhongMoi = phongMoi?.TenPhong ?? "(Không rõ)",
                    NgayBanGiao = record.NgayBanGiao,
                    GhiChu = record.GhiChu,
                    TrangThai = record.TrangThai
                };

                SetupPhieuInfo(thongTinPhieu);

                var ts = dsTS.Models.FirstOrDefault(x => x.MaTaiSan == record.MaTaiSan);
                var chiTietList = new List<ChiTietLichSuHienThi>
                {
                    new ChiTietLichSuHienThi
                    {
                        MaLichSu = "LS" + record.MaLichSu,
                        TenNhanVien = nv?.TenNV ?? "(Không rõ)",
                        TenTaiSan = ts?.TenTaiSan ?? "(Không rõ)",
                        GhiChu = record.GhiChu,
                        NgayBanGiao = record.NgayBanGiao?.ToString("dd/MM/yyyy")
                    }
                };

                dgChiTietBaoHong.ItemsSource = chiTietList;
                txtStatus.Text = $"Tổng số dòng chi tiết: {chiTietList.Count}";

                bool chuaDuyet = thongTinPhieu.TrangThai == null;
                btnDuyet.IsEnabled = chuaDuyet;
                btnTuChoi.IsEnabled = chuaDuyet;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load phiếu: " + ex.Message);
            }
        }

        private void SetupPhieuInfo(LichSuHienThi phieu)
        {
            txtMaLichSu.Text = "LS" + phieu.MaLichSu;
            txtTenNhanVien.Text = phieu.TenNhanVien;
            txtTenPhongCu.Text = phieu.TenPhongCu;
            txtTenPhongMoi.Text = phieu.TenPhongMoi;
            txtNgayBanGiao.Text = phieu.NgayBanGiao?.ToString("dd/MM/yyyy");
            txtTrangThai.Text = phieu.TrangThaiText;
        }

        private async void btnDuyet_Click(object sender, RoutedEventArgs e)
        {
            await CapNhatTrangThai(true, "✅ Phiếu đã được duyệt!");
        }

        private async void btnTuChoi_Click(object sender, RoutedEventArgs e)
        {
            await CapNhatTrangThai(false, "⛔ Phiếu đã bị từ chối!");
        }

        private async Task CapNhatTrangThai(bool trangThai, string message)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                var ds = await client.From<LichSuDiChuyenTaiSan>()
                                     .Filter("ma_lich_su", Operator.Equals, maLichSuHienTai)
                                     .Get();

                if (ds.Models.Any())
                {
                    var p = ds.Models.First();
                    p.TrangThai = trangThai;

                    await client.From<LichSuDiChuyenTaiSan>()
                                .Where(x => x.MaLichSu == maLichSuHienTai)
                                .Update(p);

                    if (trangThai == true && p.MaTaiSan.HasValue && p.MaPhongMoi.HasValue)
                    {
                        var dsTS = await client.From<TaiSanModel>()
                                              .Filter("ma_tai_san", Operator.Equals, p.MaTaiSan.Value)
                                              .Get();

                        var tsUpdate = dsTS.Models.FirstOrDefault();
                        if (tsUpdate != null)
                        {
                            tsUpdate.MaPhong = (int?)p.MaPhongMoi;
                            await client.From<TaiSanModel>()
                                        .Where(x => x.MaTaiSan == tsUpdate.MaTaiSan)
                                        .Update(tsUpdate);
                        }
                    }

                    MessageBox.Show(message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    OnPhieuDuyetThanhCong?.Invoke();
                    Window.GetWindow(this)?.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi cập nhật trạng thái: " + ex.Message);
            }
        }

        private void btnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}
