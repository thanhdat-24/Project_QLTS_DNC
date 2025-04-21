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
            // 1. Load dữ liệu cần thiết
            var tonKhoResult = await _client.From<TonKho>().Get();
            var khoResult = await _client.From<Kho>().Get();
            var nhomResult = await _client.From<NhomTaiSan>().Get();
            var chiTietXuatKhoResult = await _client.From<Project_QLTS_DNC.Models.PhieuXuatKho.ChiTietPhieuXuatInsert>().Get();
            var chiTietBanGiaoResult = await _client.From<Project_QLTS_DNC.Models.BanGiaoTaiSan.ChiTietBanGiaoModel>().Get();

            var listTonKho = tonKhoResult.Models;
            var listChiTietXuatKho = chiTietXuatKhoResult.Models;
            var listChiTietBanGiao = chiTietBanGiaoResult.Models;

            _khoLookup = khoResult.Models.ToDictionary(k => k.MaKho, k => k.TenKho);
            _nhomLookup = nhomResult.Models.ToDictionary(n => n.MaNhomTS, n => n.TenNhom);

            // 2. Xử lý từng dòng tồn kho
            foreach (var item in listTonKho)
            {
                // Gán tên kho, tên nhóm
                item.TenKho = _khoLookup.TryGetValue(item.MaKho, out var tenKho) ? tenKho : "";
                item.TenNhomTS = _nhomLookup.TryGetValue(item.MaNhomTS, out var tenNhom) ? tenNhom : "";

                // 👉 Lúc này MaNhomTS trong TonKho chính là MaTaiSan

                int tongXuatKho = listChiTietXuatKho
                    .Where(x => x.MaTaiSan == item.MaNhomTS) // So sánh trực tiếp
                    .Sum(x => x.SoLuong);

                int tongBanGiao = listChiTietBanGiao
                    .Where(x => x.MaTaiSan == item.MaNhomTS) // So sánh trực tiếp
                    .Count(); // Nếu 1 dòng = 1 tài sản

                item.SoLuongTon = item.SoLuongNhap - (tongXuatKho + tongBanGiao);
            }

            dgTonKho.ItemsSource = listTonKho;
        }

        private async Task<int> TinhTongSoLuongTaiSanAsync()
        {
            // Load dữ liệu
            var chiTietXuatKhoResult = await _client.From<Project_QLTS_DNC.Models.PhieuXuatKho.ChiTietPhieuXuatInsert>().Get();
            var chiTietBanGiaoResult = await _client.From<Project_QLTS_DNC.Models.BanGiaoTaiSan.ChiTietBanGiaoModel>().Get();

            var listChiTietXuatKho = chiTietXuatKhoResult.Models;
            var listChiTietBanGiao = chiTietBanGiaoResult.Models;

            // Tính tổng
            int tongXuatKho = listChiTietXuatKho.Sum(x => x.SoLuong);
            int tongBanGiao = listChiTietBanGiao.Count(); // mỗi dòng = 1 tài sản

            int tongCong = tongXuatKho + tongBanGiao;

            return tongCong;
        }



        private async void TonKhoView_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            await LoadTonKhoAsync();
        }




    }
}
