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
    public partial class frmTuChoiNhap : Window
    {
        public event Action OnPhieuDuyetSuccess;

        private ObservableCollection<ChiTietPhieuHienThi> danhSachChiTiet;

        public frmTuChoiNhap()
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
            public decimal? TongTien => DonGia * SoLuong;
            public bool? TrangThai { get; set; }
            public string TrangThaiHienThi => TrangThai == true
                ? "Đã duyệt"
                : TrangThai == false
                    ? "Từ chối duyệt"
                    : "Chưa duyệt";
            public long MaKho { get; set; }
            public bool IsSelected { get; set; } = false;
        }

        private async void LoadDanhSachPhieuNhap()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var dsChiTiet = await client.From<ChiTietPN>().Get();
                var dsPhieu = await client.From<PhieuNhapKho>().Get();

                var result = (from ct in dsChiTiet.Models
                              join pn in dsPhieu.Models on ct.MaPhieuNhap equals pn.MaPhieuNhap
                              where pn.TrangThai == null
                              select new ChiTietPhieuHienThi
                              {
                                  MaChiTietPN = ct.MaChiTietPN,
                                  MaPhieuNhap = ct.MaPhieuNhap,
                                  MaNhomTS = ct.MaNhomTS,
                                  TenTaiSan = ct.TenTaiSan,
                                  SoLuong = ct.SoLuong,
                                  DonGia = ct.DonGia,
                                  TrangThai = pn.TrangThai,
                                  MaKho = pn.MaKho
                              }).ToList();

                danhSachChiTiet = new ObservableCollection<ChiTietPhieuHienThi>(result);
                dgCTPhieuTuChoi.ItemsSource = danhSachChiTiet;
                UpdateStatusText();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi load dữ liệu: {ex.Message}");
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
                    MessageBox.Show("Vui lòng chọn ít nhất một dòng để từ chối.");
                    return;
                }

                var nhomPhieu = selected.GroupBy(p => p.MaPhieuNhap);

                foreach (var group in nhomPhieu)
                {
                    var phieu = await client
                        .From<PhieuNhapKho>()
                        .Filter("ma_phieu_nhap", Operator.Equals, group.Key)
                        .Get();

                    if (phieu.Models.Any())
                    {
                        var p = phieu.Models.First();
                        p.TrangThai = false; // Gán từ chối

                        await client
                            .From<PhieuNhapKho>()
                            .Where(x => x.MaPhieuNhap == group.Key)
                            .Update(p);
                    }
                }

                MessageBox.Show($"✅ Đã từ chối {selected.Count} dòng.");
                LoadDanhSachPhieuNhap();
                OnPhieuDuyetSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi từ chối:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) => this.Close();

        private void CheckBox_Changed(object sender, RoutedEventArgs e) => UpdateStatusText();

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in danhSachChiTiet) item.IsSelected = true;
            dgCTPhieuTuChoi.Items.Refresh();
            UpdateStatusText();
        }

        private void btnUnselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in danhSachChiTiet) item.IsSelected = false;
            dgCTPhieuTuChoi.Items.Refresh();
            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            int count = danhSachChiTiet.Count(p => p.IsSelected);
            txtStatus.Text = $"Đã chọn: {count} / {danhSachChiTiet.Count} dòng";
        }

        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtSearchSeri.Text.Trim().ToLower();
            dgCTPhieuTuChoi.ItemsSource = string.IsNullOrEmpty(keyword)
                ? danhSachChiTiet
                : new ObservableCollection<ChiTietPhieuHienThi>(
                    danhSachChiTiet.Where(p =>
                        (p.MaChiTietPN?.ToString().Contains(keyword) ?? false) ||
                        p.MaPhieuNhap.ToString().Contains(keyword) ||
                        (p.TenTaiSan?.ToLower().Contains(keyword) ?? false) ||
                        p.TrangThaiHienThi.ToLower().Contains(keyword)
                    )
                );
            UpdateStatusText();
        }
    }
}
