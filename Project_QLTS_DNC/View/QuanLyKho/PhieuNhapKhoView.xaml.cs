using MaterialDesignThemes.Wpf;
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
using Project_QLTS_DNC.Models;
using ClosedXML.Excel;
using Microsoft.Win32;
using System.ComponentModel;
using System.Data;
using Supabase;


namespace Project_QLTS_DNC.View.QuanLyKho
{
    public partial class PhieuNhapKhoView : UserControl
    {
        private Supabase.Client _client;
        private Dictionary<int, string> _khoLookup = new();
        private Dictionary<int, string> _nccLookup = new();
        private Dictionary<int, string> _nvLookup = new();

        private string _keyword = "";
        private int? _filterMaKho = null;
        private int? _filterMaNhom = null;

        public PhieuNhapKhoView()
        {
            InitializeComponent();
            _ = Init();
        }

        private async Task Init()
        {
            await InitializeSupabaseAsync();
            await LoadKhoLookupAsync();
            await LoadNhaCungCapLookupAsync();
            await LoadNhanVienLookupAsync();
            await LoadPhieuNhapAsync();
        }

        private async Task InitializeSupabaseAsync()
        {
            var supabaseUrl = "https://hoybfwnugefnpctgghha.supabase.co";
            var supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhveWJmd251Z2VmbnBjdGdnaGhhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDQxMDQ4OTEsImV4cCI6MjA1OTY4MDg5MX0.KxNfiOUFXHGgqZf3b3xOk6BR4sllMZG_-W-y_OPUwCI";

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false,
                AutoRefreshToken = false
            };

            _client = new Supabase.Client(supabaseUrl, supabaseKey, options);
            await _client.InitializeAsync();
        }

        private async Task LoadKhoLookupAsync()
        {
            var result = await _client.From<Kho>().Get();
            _khoLookup = result.Models.ToDictionary(k => k.MaKho, k => k.TenKho);

            cboPhong.ItemsSource = _khoLookup.Select(k => new ComboBoxItem
            {
                Content = k.Value,
                Tag = k.Key
            });
        }

        private async Task LoadNhaCungCapLookupAsync()
        {
            var result = await _client.From<NhaCungCapClass>().Get();
            _nccLookup = result.Models.ToDictionary(n => n.MaNCC, n => n.TenNCC);
        }

        private async Task LoadNhanVienLookupAsync()
        {
            var result = await _client.From<NhanVienClass>().Get();
            _nvLookup = result.Models.ToDictionary(nv => nv.MaNV, nv => nv.TenNV);
        }



        private async Task LoadPhieuNhapAsync()
        {
            try
            {
                // Truy vấn dữ liệu kho từ Supabase
                var result = await _client.From<PhieuNhap>().Get();

                // Kiểm tra nếu có dữ liệu trả về
                if (result.Models.Any())
                {
                    // Gán dữ liệu vào DataGrid
                    dgSanPham.ItemsSource = result.Models;
                }
                else
                {
                    MessageBox.Show("Không có dữ liệu kho.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu kho: {ex.Message}");
            }

        }


        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _keyword = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(_keyword))
            {
                popupSuggest.IsOpen = false;
                return;
            }

            var result = await _client
                .From<ChiTietPhieuNhap>()
                .Select("ten_tai_san")
                .Filter("ten_tai_san", Supabase.Postgrest.Constants.Operator.ILike, $"%{_keyword}%")
                .Limit(10)
                .Get();

            var suggestions = result.Models
                .Select(x => x.TenTaiSan)
                .Distinct()
                .ToList();

            lstSuggest.ItemsSource = suggestions;
            popupSuggest.IsOpen = suggestions.Any();

        }

        private void lstSuggest_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (lstSuggest.SelectedItem != null)
            {
                txtSearch.Text = lstSuggest.SelectedItem.ToString();
                _keyword = txtSearch.Text;
                popupSuggest.IsOpen = false;
                _ = LoadPhieuNhapAsync();
            }
        }

        private void cboPhong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboPhong.SelectedItem is ComboBoxItem item && int.TryParse(item.Tag?.ToString(), out int maKho))
                _filterMaKho = maKho;
            else
                _filterMaKho = null;

            _ = LoadPhieuNhapAsync();
        }

        private void cboNhomTS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboNhomTS.SelectedItem is ComboBoxItem item && int.TryParse(item.Tag?.ToString(), out int maNhom))
                _filterMaNhom = maNhom;
            else
                _filterMaNhom = null;

            _ = LoadPhieuNhapAsync();
        }

        private void btnThemKho_click(object sender, RoutedEventArgs e)
        {
            var form = new PhieuNhapKhoInput();
            form.ShowDialog();
            _ = LoadPhieuNhapAsync();
        }


       

    }
}
