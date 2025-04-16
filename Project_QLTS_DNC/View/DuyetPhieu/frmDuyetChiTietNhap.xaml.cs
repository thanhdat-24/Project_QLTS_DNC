using Project_QLTS_DNC.Models.DuyetPhieu;
using Project_QLTS_DNC.Models.TonKho;
using Project_QLTS_DNC.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using static Supabase.Postgrest.Constants;

namespace Project_QLTS_DNC.View.DuyetPhieu
{
    public partial class frmDuyetChiTietNhap : Window
    {
        public event Action OnPhieuDuyetSuccess;
        private ObservableCollection<ChiTietPhieuHienThi> danhSachChiTiet;

        public frmDuyetChiTietNhap()
        {
            InitializeComponent();
            LoadDanhSachPhieuNhap();
        }

        public class ChiTietPhieuHienThi
        {
            public long? MaChiTietPN { get; set; }
            public long MaPhieuNhap { get; set; }
            public long MaNhomTS { get; set; }
            public string TenTaiSan { get; set; }
            public int SoLuong { get; set; }
            public decimal? DonGia { get; set; }
            public decimal TongTien => (DonGia ?? 0) * SoLuong;
            public bool? TrangThaiBool { get; set; }
            public string TrangThai => TrangThaiBool == true ? "Đã duyệt"
                                 : TrangThaiBool == false ? "Từ chối duyệt"
                                 : "Chưa duyệt";
            public long MaKho { get; set; }
            public bool IsSelected { get; set; } = false;
        }

        private async void LoadDanhSachPhieuNhap()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var dsChiTiet = await client.From<ChiTietPhieuNhap>().Get();
                var dsPhieu = await client.From<PhieuNhapKhoInput>().Get();

                var result = (from ct in dsChiTiet.Models
                              join pn in dsPhieu.Models on ct.MaPhieuNhap equals pn.MaPhieuNhap
                              where pn.TrangThai == null // chỉ lấy phiếu chưa duyệt
                              select new ChiTietPhieuHienThi
                              {
                                  MaChiTietPN = ct.MaChiTietPN,
                                  MaPhieuNhap = ct.MaPhieuNhap,
                                  MaNhomTS = ct.MaNhomTS,
                                  TenTaiSan = ct.TenTaiSan,
                                  SoLuong = ct.SoLuong ?? 0,
                                  DonGia = ct.DonGia,
                                  TrangThaiBool = pn.TrangThai,
                                  MaKho = pn.MaKho
                              }).ToList();

                danhSachChiTiet = new ObservableCollection<ChiTietPhieuHienThi>(result);
                dgCTPhieu.ItemsSource = danhSachChiTiet;
                UpdateStatusText();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi load dữ liệu: {ex.Message}");
            }
        }

        private async void btnDuyet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var selected = danhSachChiTiet.Where(p => p.IsSelected).ToList();

                if (!selected.Any())
                {
                    MessageBox.Show("Vui lòng chọn ít nhất một dòng để duyệt.");
                    return;
                }

                var nhomPhieu = selected.GroupBy(p => p.MaPhieuNhap);

                foreach (var group in nhomPhieu)
                {
                    foreach (var ct in group)
                    {
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

                    var phieu = await client
                        .From<PhieuNhapKhoInput>()
                        .Filter("ma_phieu_nhap", Operator.Equals, group.Key)
                        .Get();

                    if (phieu.Models.Any())
                    {
                        var p = phieu.Models.First();
                        p.TrangThai = true; // ✅ duyệt = true
                        await client
                            .From<PhieuNhapKhoInput>()
                            .Where(x => x.MaPhieuNhap == group.Key)
                            .Update(p);
                    }
                }

                MessageBox.Show($"✅ Đã duyệt thành công {selected.Count} dòng.");
                LoadDanhSachPhieuNhap();
                OnPhieuDuyetSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi duyệt: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnTuChoi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var selected = danhSachChiTiet.Where(p => p.IsSelected).ToList();

                if (!selected.Any())
                {
                    MessageBox.Show("Vui lòng chọn dòng cần từ chối.");
                    return;
                }

                var nhomPhieu = selected.GroupBy(p => p.MaPhieuNhap);
                foreach (var group in nhomPhieu)
                {
                    var phieu = await client
                        .From<PhieuNhapKhoInput>()
                        .Filter("ma_phieu_nhap", Operator.Equals, group.Key)
                        .Get();

                    if (phieu.Models.Any())
                    {
                        var p = phieu.Models.First();
                        p.TrangThai = false; // ❌ từ chối
                        await client
                            .From<PhieuNhapKhoInput>()
                            .Where(x => x.MaPhieuNhap == group.Key)
                            .Update(p);
                    }
                }

                MessageBox.Show("⛔ Đã từ chối phiếu.");
                LoadDanhSachPhieuNhap();
                OnPhieuDuyetSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi từ chối: {ex}");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) => this.Hide();

        private void CheckBox_Changed(object sender, RoutedEventArgs e) => UpdateStatusText();

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in danhSachChiTiet) item.IsSelected = true;
            dgCTPhieu.Items.Refresh();
            UpdateStatusText();
        }

        private void btnUnselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in danhSachChiTiet) item.IsSelected = false;
            dgCTPhieu.Items.Refresh();
            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            int count = danhSachChiTiet.Count(p => p.IsSelected);
            txtStatus.Text = $"Đã chọn: {count} / {danhSachChiTiet.Count} dòng";
        }

        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(keyword))
            {
                dgCTPhieu.ItemsSource = danhSachChiTiet;
                UpdateStatusText();
                return;
            }

            var ketQua = danhSachChiTiet.Where(p =>
                (p.MaChiTietPN.HasValue && p.MaChiTietPN.Value.ToString().Contains(keyword)) ||
                p.MaPhieuNhap.ToString().Contains(keyword) ||
                (!string.IsNullOrEmpty(p.TenTaiSan) && p.TenTaiSan.ToLower().Contains(keyword)) ||
                (!string.IsNullOrEmpty(p.TrangThai) && p.TrangThai.ToLower().Contains(keyword))
            ).ToList();

            dgCTPhieu.ItemsSource = new ObservableCollection<ChiTietPhieuHienThi>(ketQua);
            UpdateStatusText();
        }
    }
}
