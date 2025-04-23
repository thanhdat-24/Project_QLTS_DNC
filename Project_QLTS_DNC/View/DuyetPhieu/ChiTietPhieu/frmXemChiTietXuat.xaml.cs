using Project_QLTS_DNC.Models.Phieu;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.TonKho;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Supabase.Postgrest.Constants;
using Project_QLTS_DNC.Models.PhieuXuatKho;
using Project_QLTS_DNC.Models.QLTaiSan;

namespace Project_QLTS_DNC.View.DuyetPhieu.ChiTietPhieu
{
    public partial class frmXemChiTietXuat : UserControl
    {
        private long maPhieuXuatHienTai;
        private ObservableCollection<ChiTietPhieuXuatHienThi> danhSachChiTiet = new();

        public event Action? OnPhieuDuyetThanhCong;

        public frmXemChiTietXuat() => InitializeComponent();

        public class ChiTietPhieuXuatHienThi
        {
            public long MaPhieuXuat { get; set; }
            public long MaChiTiet { get; set; }
            public long MaTaiSan { get; set; }
            public string TenTaiSan { get; set; }
            public int SoLuong { get; set; }
            public string TenKhoXuat { get; set; }
            public string TenKhoNhan { get; set; }
            public string GhiChu { get; set; }
            public long MaKhoXuat { get; set; }
        }

        public async void LoadTheoMaPhieu(long maPhieuXuat)
        {
            maPhieuXuatHienTai = maPhieuXuat;
            try
            {
                var client = await SupabaseService.GetClientAsync();

                var dsPhieu = await client.From<PhieuXuat>().Get();
                var dsChiTiet = await client.From<ChiTietPhieuXuatModel>().Get();
                var dsKho = await client.From<Kho>().Get();
                var dsNV = await client.From<NhanVienModel>().Get();
                var dsTaiSan = await client.From<TaiSanModel>().Get();

                var phieu = dsPhieu.Models.FirstOrDefault(p => p.MaPhieuXuat == maPhieuXuat);
                if (phieu == null)
                {
                    MessageBox.Show("Không tìm thấy phiếu xuất!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Gán thông tin chung
                txtMaPhieu.Text = "PX" + phieu.MaPhieuXuat;
                txtNgayXuat.Text = phieu.NgayXuat.ToString("dd/MM/yyyy");
                txtTrangThai.Text = phieu.TrangThai == true ? "Đã duyệt"
                                      : phieu.TrangThai == false ? "Từ chối duyệt"
                                      : "Chưa duyệt";
                txtTenKhoXuat.Text = dsKho.Models.FirstOrDefault(k => k.MaKho == phieu.MaKhoXuat)?.TenKho ?? "(Không rõ)";
                txtTenKhoNhan.Text = dsKho.Models.FirstOrDefault(k => k.MaKho == phieu.MaKhoNhan)?.TenKho ?? "(Không rõ)";
                txtTenNV.Text = dsNV.Models.FirstOrDefault(n => n.MaNV == phieu.MaNV)?.TenNV ?? "(Không rõ)";

                // Load chi tiết
                var chiTiet = (from ct in dsChiTiet.Models
                               where ct.MaPhieuXuat == maPhieuXuat
                               let ts = dsTaiSan.Models.FirstOrDefault(t => t.MaTaiSan == ct.MaTaiSan)
                               select new ChiTietPhieuXuatHienThi
                               {
                                   MaPhieuXuat = ct.MaPhieuXuat,
                                   MaChiTiet = ct.MaChiTietXK,
                                   MaTaiSan = ct.MaTaiSan,
                                   TenTaiSan = ts?.TenTaiSan ?? "(Không rõ)",
                                   SoLuong = ct.SoLuong,
                                   TenKhoXuat = txtTenKhoXuat.Text,
                                   TenKhoNhan = txtTenKhoNhan.Text,
                                   GhiChu = phieu.GhiChu,
                                   MaKhoXuat = phieu.MaKhoXuat
                               }).ToList();

                danhSachChiTiet = new ObservableCollection<ChiTietPhieuXuatHienThi>(chiTiet);
                dgChiTietPhieuXuat.ItemsSource = danhSachChiTiet;
                txtStatus.Text = $"Tổng số dòng chi tiết: {danhSachChiTiet.Count}";

                bool chuaDuyet = phieu.TrangThai == null;
                bool coChiTiet = danhSachChiTiet.Any();

                btnDuyet.IsEnabled = chuaDuyet && coChiTiet;
                btnTuChoi.IsEnabled = chuaDuyet && coChiTiet;
                btnHuyBo.IsEnabled = true;

                // Nếu không có chi tiết, cảnh báo
                if (!coChiTiet)
                {
                    MessageBox.Show("Phiếu này không có thông tin chi tiết để duyệt!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi load phiếu xuất: " + ex.Message);
            }
        }


        private async void btnDuyet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // ✅ Kiểm tra tồn kho trước
                foreach (var ct in danhSachChiTiet)
                {
                    var ton = await client
                        .From<TonKho>()
                        .Filter("ma_kho", Operator.Equals, ct.MaKhoXuat)
                        .Filter("ma_nhom_ts", Operator.Equals, ct.MaTaiSan)
                        .Get();

                    var tonKho = ton.Models.FirstOrDefault();

                    if (tonKho == null)
                    {
                        MessageBox.Show($"❌ Khu vực \"{ct.TenKhoXuat}\" không có tài sản \"{ct.TenTaiSan}\".", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // ✅ Tính tồn kho bằng cách lấy nhập - xuất
                    int soLuongTon = tonKho.SoLuongNhap - tonKho.SoLuongXuat;

                    if (ct.SoLuong > soLuongTon)
                    {
                        MessageBox.Show(
                            $"❌ Tài sản \"{ct.TenTaiSan}\" tại kho \"{ct.TenKhoXuat}\" không đủ tồn kho.\n" +
                            $"✔️ Hiện còn: {soLuongTon} | ❌ Cần xuất: {ct.SoLuong}",
                            "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // ✅ Tiến hành cập nhật tồn kho
                foreach (var ct in danhSachChiTiet)
                {
                    var ton = await client
                        .From<TonKho>()
                        .Filter("ma_kho", Operator.Equals, ct.MaKhoXuat)
                        .Filter("ma_nhom_ts", Operator.Equals, ct.MaTaiSan)
                        .Get();

                    var tonKho = ton.Models.First();

                    tonKho.SoLuongXuat += ct.SoLuong;
                    tonKho.NgayCapNhat = DateTime.Now;

                    await client.From<TonKho>().Update(tonKho);
                }

                // ✅ Cập nhật trạng thái phiếu xuất
                var res = await client
                    .From<PhieuXuat>()
                    .Filter("ma_phieu_xuat", Operator.Equals, maPhieuXuatHienTai)
                    .Get();

                var phieu = res.Models.First();
                phieu.TrangThai = true;

                await client.From<PhieuXuat>().Update(phieu);

                MessageBox.Show("✅ Duyệt phiếu xuất thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
                var res = await client
                    .From<PhieuXuat>()
                    .Filter("ma_phieu_xuat", Operator.Equals, maPhieuXuatHienTai)
                    .Get();

                var phieu = res.Models.First();
                phieu.TrangThai = false;
                await client.From<PhieuXuat>().Update(phieu);

                MessageBox.Show("Phiếu đã bị từ chối!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                OnPhieuDuyetThanhCong?.Invoke();
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi từ chối phiếu: " + ex.Message);
            }
        }

        private void btnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}