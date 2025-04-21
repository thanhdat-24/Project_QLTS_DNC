using Project_QLTS_DNC.Models.DuyetPhieu;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.NhaCungCap;
using Project_QLTS_DNC.Models.TonKho;
using Project_QLTS_DNC.Models.QLLoaiTS;
using Project_QLTS_DNC.Models.QLNhomTS;
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
    public partial class frmXemChiTietNhap : UserControl
    {
        private ObservableCollection<ChiTietPhieuHienThi> danhSachChiTiet = new();
        private ChiTietPhieuHienThi thongTinPhieu = new();
        private long maPhieuNhapHienTai;

        public event Action OnPhieuDuyetThanhCong;

        public frmXemChiTietNhap() => InitializeComponent();

        public class ChiTietPhieuHienThi
        {
            public long MaPhieu { get; set; }
            public long MaKho { get; set; }
            public string TenKho { get; set; }
            public string TenNV { get; set; }
            public long MaChiTietPN { get; set; }
            public long MaNhomTS { get; set; }
            public string TenNhomTS { get; set; }
            public string TenNCC { get; set; }
            public string TenTS { get; set; }
            public DateTime? NgayNhap { get; set; }
            public int SoLuong { get; set; }
            public decimal DonGia { get; set; }
            public bool CanQLRieng { get; set; }
            public decimal TongTien { get; set; }
            public string TrangThai { get; set; } // hiển thị
        }

        public async void LoadTheoMaPhieu(long maPhieuNhap)
        {
            maPhieuNhapHienTai = maPhieuNhap;

            try
            {
                var client = await SupabaseService.GetClientAsync();

                var listPhieu = await client.From<PhieuNhapKho>().Get();
                var listChiTiet = await client.From<ChiTietPN>().Get();
                var listKho = await client.From<Kho>().Get();
                var listNV = await client.From<NhanVienModel>().Get();
                var listNCC = await client.From<NhaCungCapClass>().Get();
                var listNhomTS = await client.From<NhomTaiSan>().Get();

                var result = (from pn in listPhieu.Models
                              join ct in listChiTiet.Models on pn.MaPhieuNhap equals ct.MaPhieuNhap
                              join kho in listKho.Models on pn.MaKho equals kho.MaKho
                              join nv in listNV.Models on pn.MaNV equals nv.MaNV
                              join ncc in listNCC.Models on pn.MaNCC equals ncc.MaNCC
                              join nhom in listNhomTS.Models on ct.MaNhomTS equals nhom.MaNhomTS
                              where pn.MaPhieuNhap == maPhieuNhap
                              select new ChiTietPhieuHienThi
                              {
                                  MaPhieu = pn.MaPhieuNhap,
                                  MaKho = pn.MaKho,
                                  TenKho = kho.TenKho,
                                  TenNV = nv.TenNV,
                                  MaChiTietPN = ct.MaChiTietPN ?? 0,
                                  MaNhomTS = ct.MaNhomTS,
                                  TenNhomTS = nhom.TenNhom,
                                  TenNCC = ncc.TenNCC,
                                  TenTS = ct.TenTaiSan,
                                  NgayNhap = pn.NgayNhap,
                                  SoLuong = ct.SoLuong,
                                  DonGia = ct.DonGia ?? 0,
                                  CanQLRieng = ct.CanQuanLyRieng ?? false,
                                  TongTien = ct.TongTien ?? ((ct.DonGia ?? 0) * ct.SoLuong),
                                  TrangThai = pn.TrangThai == true ? "Đã duyệt" :
                                              pn.TrangThai == false ? "Từ chối duyệt" : "Chưa duyệt"
                              }).ToList();

                danhSachChiTiet = new ObservableCollection<ChiTietPhieuHienThi>(result);
                dgChiTietPhieuNhap.ItemsSource = danhSachChiTiet;
                txtStatus.Text = $"Tổng số dòng chi tiết: {danhSachChiTiet.Count}";

                // Thiết lập thông tin chung của phiếu để hiển thị trong các TextBlock
                if (result.Count > 0)
                {
                    thongTinPhieu = result.First();
                    SetupPhieuInfo(thongTinPhieu);
                }

                bool chuaDuyet = result.Any() && result.First().TrangThai == "Chưa duyệt";
                btnDuyet.IsEnabled = chuaDuyet;
                btnTuChoi.IsEnabled = chuaDuyet;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load chi tiết phiếu nhập: " + ex.Message);
            }
        }

        private void SetupPhieuInfo(ChiTietPhieuHienThi phieu)
        {
            // Cập nhật các thông tin chi tiết phiếu lên giao diện
            txtMaPhieu.Text = "PN" + phieu.MaPhieu.ToString();
            txtTenKho.Text = phieu.TenKho;
            txtTenNV.Text = phieu.TenNV;
            txtNgayNhap.Text = phieu.NgayNhap?.ToString("dd/MM/yyyy");
            txtTenNCC.Text = phieu.TenNCC;
            txtTrangThai.Text = phieu.TrangThai;

            // Tính tổng tiền từ tất cả các chi tiết
            decimal tongTien = danhSachChiTiet.Sum(ct => ct.TongTien);
            txtTongTien.Text = string.Format("{0:N0} VNĐ", tongTien);
        }

        private async void btnDuyet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Hiển thị hộp thoại xác nhận
                var result = MessageBox.Show(
                    "Bạn có chắc chắn muốn duyệt phiếu nhập này không?",
                    "Xác nhận duyệt phiếu",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                var client = await SupabaseService.GetClientAsync();

                // Hiển thị thông báo đang xử lý
                btnDuyet.IsEnabled = false;
                btnTuChoi.IsEnabled = false;
                btnHuyBo.IsEnabled = false;

                foreach (var ct in danhSachChiTiet)
                {
                    // ✅ Bỏ qua dòng nếu MaNhomTS = 0
                    if (ct.MaNhomTS == 0)
                        continue;

                    // ✅ Kiểm tra tồn tại trong tồn kho
                    var existing = await client
                        .From<TonKho>()
                        .Filter("ma_kho", Operator.Equals, ct.MaKho)
                        .Filter("ma_nhom_ts", Operator.Equals, ct.MaNhomTS)
                        .Get();

                    if (existing.Models.Any())
                    {
                        var ton = existing.Models.First();
                        ton.SoLuongNhap += ct.SoLuong;
                        ton.NgayCapNhat = DateTime.Now;
                        await client.From<TonKho>().Update(ton);
                    }
                    else
                    {
                        var tonMoi = new TonKho
                        {
                            MaKho = ct.MaKho,
                            MaNhomTS = ct.MaNhomTS,
                            SoLuongNhap = ct.SoLuong,
                            SoLuongXuat = 0,
                            NgayCapNhat = DateTime.Now
                        };
                        await client.From<TonKho>().Insert(tonMoi);
                    }
                }

                // ✅ Cập nhật trạng thái phiếu
                var phieu = await client
                    .From<PhieuNhapKho>()
                    .Filter("ma_phieu_nhap", Operator.Equals, maPhieuNhapHienTai)
                    .Get();

                if (phieu.Models.Any())
                {
                    var p = phieu.Models.First();
                    p.TrangThai = true;
                    await client
                        .From<PhieuNhapKho>()
                        .Where(x => x.MaPhieuNhap == maPhieuNhapHienTai)
                        .Update(p);

                    // Cập nhật UI sau khi duyệt
                    txtTrangThai.Text = "Đã duyệt";
                }

                MessageBox.Show("✅ Duyệt phiếu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                OnPhieuDuyetThanhCong?.Invoke();
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi duyệt phiếu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                btnDuyet.IsEnabled = true;
                btnTuChoi.IsEnabled = true;
                btnHuyBo.IsEnabled = true;
            }
        }

        private async void btnTuChoi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Hiển thị hộp thoại xác nhận
                var result = MessageBox.Show(
                    "Bạn có chắc chắn muốn từ chối phiếu nhập này không?",
                    "Xác nhận từ chối phiếu",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                var client = await SupabaseService.GetClientAsync();

                // Hiển thị thông báo đang xử lý
                btnDuyet.IsEnabled = false;
                btnTuChoi.IsEnabled = false;
                btnHuyBo.IsEnabled = false;

                var phieu = await client
                    .From<PhieuNhapKho>()
                    .Filter("ma_phieu_nhap", Operator.Equals, maPhieuNhapHienTai)
                    .Get();

                if (phieu.Models.Any())
                {
                    var p = phieu.Models.First();
                    p.TrangThai = false; // ❌ cập nhật thành "Từ chối duyệt"
                    await client
                        .From<PhieuNhapKho>()
                        .Where(x => x.MaPhieuNhap == maPhieuNhapHienTai)
                        .Update(p);

                    // Cập nhật UI sau khi từ chối
                    txtTrangThai.Text = "Từ chối duyệt";
                }

                MessageBox.Show("⛔ Phiếu đã bị từ chối!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                OnPhieuDuyetThanhCong?.Invoke();
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi từ chối phiếu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                btnDuyet.IsEnabled = true;
                btnTuChoi.IsEnabled = true;
                btnHuyBo.IsEnabled = true;
            }
        }

        private void btnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            // Hiển thị hộp thoại xác nhận nếu cần
            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn đóng cửa sổ này không?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Window.GetWindow(this)?.Close();
            }
        }
    }
}