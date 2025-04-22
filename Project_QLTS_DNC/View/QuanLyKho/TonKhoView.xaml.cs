using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Project_QLTS_DNC.Models.Kho;
using Project_QLTS_DNC.Models.QLNhomTS;
using Supabase;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Models.PhieuXuatKho;
using Project_QLTS_DNC.Models.BanGiaoTaiSan;
using Project_QLTS_DNC.Models.PhieuNhapKho;

namespace Project_QLTS_DNC.View.QuanLyKho
{
    /// <summary>
    /// Interaction logic for TonKhoView.xaml
    /// </summary>
    public partial class TonKhoView : UserControl
    {
        private Supabase.Client _client;
        private Dictionary<int, string> _khoLookup = new();
        private Dictionary<int, string> _nhomLookup = new();
        private List<TonKho> DanhSachTonKhoGoc = new(); 


        public TonKhoView()
        {
            InitializeComponent();
            Loaded += TonKhoView_Loaded;
        }

        private async Task InitializeSupabaseAsync()
        {
            string supabaseUrl = "https://hoybfwnugefnpctgghha.supabase.co";
            string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhveWJmd251Z2VmbnBjdGdnaGhhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDQxMDQ4OTEsImV4cCI6MjA1OTY4MDg5MX0.KxNfiOUFXHGgqZf3b3xOk6BR4sllMZG_-W-y_OPUwCI";

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false,
                AutoRefreshToken = false
            };

            _client = new Supabase.Client(supabaseUrl, supabaseKey, options);
            await _client.InitializeAsync();
        }

        private async Task LoadTonKhoAsync()
        {
            var tonKhoResult = await _client.From<TonKho>().Get();
            var khoResult = await _client.From<Kho>().Get();
            var nhomResult = await _client.From<NhomTaiSan>().Get();

            var listTonKho = tonKhoResult.Models;
            _khoLookup = khoResult.Models.ToDictionary(k => k.MaKho, k => k.TenKho);
            _nhomLookup = nhomResult.Models.ToDictionary(n => n.MaNhomTS, n => n.TenNhom);

            foreach (var item in listTonKho)
            {
                item.TenKho = _khoLookup.TryGetValue(item.MaKho, out var tenKho) ? tenKho : "";
                item.TenNhomTS = _nhomLookup.TryGetValue(item.MaNhomTS, out var tenNhom) ? tenNhom : "";
            }

            DanhSachTonKhoGoc = listTonKho;
            dgTonKho.ItemsSource = listTonKho;

            cboTenKho.ItemsSource = new List<object> { new { MaKho = (int?)null, TenKho = "Tất cả" } }
                .Concat(_khoLookup.Select(k => new { MaKho = (int?)k.Key, TenKho = k.Value }))
                .ToList();

            cboTenNhomTS.ItemsSource = new List<object> { new { MaNhomTS = (int?)null, TenNhomTS = "Tất cả" } }
                .Concat(_nhomLookup.Select(n => new { MaNhomTS = (int?)n.Key, TenNhomTS = n.Value }))
                .ToList();

        }

        private void LocDuLieu()
        {
            string tuKhoa = txtSearch.Text.Trim().ToLower();
            int? maKho = cboTenKho.SelectedValue as int?;
            int? maNhomTS = cboTenNhomTS.SelectedValue as int?;

            var ketQua = DanhSachTonKhoGoc.AsEnumerable();

            if (!string.IsNullOrEmpty(tuKhoa))
            {
                ketQua = ketQua.Where(x =>
                    (x.TenKho?.ToLower().Contains(tuKhoa) ?? false) ||
                    (x.TenNhomTS?.ToLower().Contains(tuKhoa) ?? false) ||
                    x.MaTonKho.ToString().Contains(tuKhoa)
                );
            }

            if (maKho.HasValue)
                ketQua = ketQua.Where(x => x.MaKho == maKho.Value);

            if (maNhomTS.HasValue)
                ketQua = ketQua.Where(x => x.MaNhomTS == maNhomTS.Value);

            dgTonKho.ItemsSource = ketQua.ToList();
        }


        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) => LocDuLieu();
        private void btnSearch_Click(object sender, RoutedEventArgs e) => LocDuLieu();
        private void cboTenKho_SelectionChanged(object sender, SelectionChangedEventArgs e) => LocDuLieu();
        private void cboTenNhomTS_SelectionChanged(object sender, SelectionChangedEventArgs e) => LocDuLieu();



        private async Task CapNhatSoLuongXuatTonKhoAsync()
        {
            try
            {
                string sql = @"
               WITH 
                    bg AS (
                        -- Tổng số lượng bàn giao theo nhóm tài sản (dựa theo trạng thái phiếu bàn giao)
                        SELECT 
                            ctpn.ma_nhom_ts,
                            COUNT(*) AS tong_so_luong_ban_giao
                        FROM 
                            chitietbangiao ctbg
                        INNER JOIN 
                            bangiaotaisan bgt ON ctbg.ma_bang_giao_ts = bgt.ma_bang_giao_ts
                        INNER JOIN 
                            taisan ts ON ctbg.ma_tai_san = ts.ma_tai_san
                        INNER JOIN 
                            chitietphieunhap ctpn ON ts.ma_chi_tiet_pn = ctpn.ma_chi_tiet_pn
                        WHERE 
                            (bgt.trang_thai = TRUE OR bgt.trang_thai IS NULL) -- ✅ chỉ tính khi phiếu bàn giao được duyệt hoặc chưa duyệt
                        GROUP BY 
                            ctpn.ma_nhom_ts
                    ),
                    xk AS (
                        -- Tổng số lượng xuất kho theo nhóm tài sản (dựa theo trạng thái phiếu xuất kho)
                        SELECT 
                            ctpn.ma_nhom_ts,
                            COUNT(*) AS tong_so_luong_xuat
                        FROM 
                            chitietxuatkho ctxk
                        INNER JOIN 
                            xuatkho xk ON ctxk.ma_phieu_xuat = xk.ma_phieu_xuat
                        INNER JOIN 
                            taisan ts ON ctxk.ma_tai_san = ts.ma_tai_san
                        INNER JOIN 
                            chitietphieunhap ctpn ON ts.ma_chi_tiet_pn = ctpn.ma_chi_tiet_pn
                        WHERE 
                            (xk.trang_thai = TRUE OR xk.trang_thai IS NULL) -- ✅ chỉ tính khi phiếu xuất kho được duyệt hoặc chưa duyệt
                        GROUP BY 
                            ctpn.ma_nhom_ts
                    ),
                    gop AS (
                        SELECT 
                            COALESCE(bg.ma_nhom_ts, xk.ma_nhom_ts) AS ma_nhom_ts,
                            COALESCE(bg.tong_so_luong_ban_giao, 0) + COALESCE(xk.tong_so_luong_xuat, 0) AS tong_so_luong_xuat_gop
                        FROM 
                            bg
                        FULL JOIN 
                            xk ON bg.ma_nhom_ts = xk.ma_nhom_ts
                    )

                    UPDATE tonkho tk
                    SET 
                        so_luong_xuat = gop.tong_so_luong_xuat_gop,
                        ngay_cap_nhat = CURRENT_TIMESTAMP
                    FROM gop
                    WHERE tk.ma_nhom_ts = gop.ma_nhom_ts;
                        ";

                var parameters = new Dictionary<string, object> { { "query", sql } };
                var response = await _client.Rpc("rpc_execute_sql", parameters);
                

                // Sau đó tự xử lý kết quả `response`
                // rồi update từng dòng TonKho (MaNhomTS) theo SoLuongXuat mới
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi truy vấn tồn kho: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void TonKhoView_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            await CapNhatSoLuongXuatTonKhoAsync(); // ⚡ Cập nhật luôn
            await LoadTonKhoAsync();               // ⚡ Load DataGrid
        }
    }
}
