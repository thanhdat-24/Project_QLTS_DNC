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
using ClosedXML.Excel;
using Microsoft.Win32;
using System.ComponentModel;
using System.Data;
using Supabase;
using Project_QLTS_DNC.Models.PhieuNhapKho;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.NhaCungCap;
using Project_QLTS_DNC.Services;


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
        private string _filterTrangThai = null;


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

            cboTenKho.ItemsSource = _khoLookup.Select(k => new ComboBoxItem
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
            var result = await _client.From<NhanVienModel>().Get();
            _nvLookup = result.Models.ToDictionary(nv => nv.MaNV, nv => nv.TenNV);
        }



        private async Task LoadPhieuNhapAsync()
        {
            try
            {
                var result = await _client.From<PhieuNhap>().Get();

                if (result.Models.Any())
                {
                    var displayList = result.Models
                        .Where(p =>
                            // Lọc theo kho
                            (_filterMaKho == null || p.MaKho == _filterMaKho) &&

                            // Lọc theo từ khóa tìm kiếm
                            (string.IsNullOrEmpty(_keyword) ||
                                p.MaPhieuNhap.ToString().Contains(_keyword, StringComparison.OrdinalIgnoreCase) ||
                                (_khoLookup.ContainsKey(p.MaKho) && _khoLookup[p.MaKho].Contains(_keyword, StringComparison.OrdinalIgnoreCase)) ||
                                (_nccLookup.ContainsKey(p.MaNCC) && _nccLookup[p.MaNCC].Contains(_keyword, StringComparison.OrdinalIgnoreCase)) ||
                                (_nvLookup.ContainsKey(p.MaNV) && _nvLookup[p.MaNV].Contains(_keyword, StringComparison.OrdinalIgnoreCase)) ||
                                (!string.IsNullOrEmpty(p.TrangThai) && p.TrangThai.Contains(_keyword, StringComparison.OrdinalIgnoreCase))
                            ) &&

                            // Lọc theo trạng thái
                            (string.IsNullOrEmpty(_filterTrangThai) ||
                                 (_filterTrangThai == "Chờ duyệt" && string.IsNullOrEmpty(p.TrangThai)) ||
                                 (_filterTrangThai == "Đã duyệt" && p.TrangThai == "Đã duyệt") ||
                                 (_filterTrangThai == "Từ chối duyệt" && p.TrangThai == "Từ chối duyệt"))

                        )
                        .Select(p => new
                        {
                            MaPhieuNhap = p.MaPhieuNhap,
                            TenKho = _khoLookup.ContainsKey(p.MaKho) ? _khoLookup[p.MaKho] : $"#{p.MaKho}",
                            TenNhanVien = _nvLookup.ContainsKey(p.MaNV) ? _nvLookup[p.MaNV] : $"#{p.MaNV}",
                            TenNCC = _nccLookup.ContainsKey(p.MaNCC) ? _nccLookup[p.MaNCC] : $"#{p.MaNCC}",
                            NgayNhap = p.NgayNhap,
                            TongTien = p.TongTien,
                            TrangThai = string.IsNullOrEmpty(p.TrangThai)
                                    ? "Chờ duyệt"
                                    : (p.TrangThai == "DaDuyet" ? "Đã duyệt" :
                                       (p.TrangThai == "TuChoiDuyet" ? "Từ chối duyệt" : p.TrangThai))

                        })
                        .OrderByDescending(p => p.NgayNhap)
                        .ToList();

                    dgSanPham.ItemsSource = displayList;
                }
                else
                {
                    dgSanPham.ItemsSource = null;
                    MessageBox.Show("Không có dữ liệu phiếu nhập.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu phiếu nhập: {ex.Message}");
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
        private void cboTrangThai_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboTrangThai.SelectedItem is ComboBoxItem item && item.Content != null)
            {
                string selected = item.Content.ToString();
                _filterTrangThai = selected == "Tất cả" ? null : selected;
            }
            else
            {
                _filterTrangThai = null;
            }

            _ = LoadPhieuNhapAsync();
        }

        private void cboTenKho_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboTenKho.SelectedItem is ComboBoxItem item && int.TryParse(item.Tag?.ToString(), out int maKho))
                _filterMaKho = maKho;
            else
                _filterMaKho = null;

            _ = LoadPhieuNhapAsync();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _keyword = txtSearch.Text.Trim();
            _ = LoadPhieuNhapAsync();
        }

        private void btnThemKho_click(object sender, RoutedEventArgs e)
        {
            var form = new PhieuNhapKhoInput();
            form.ShowDialog();
            _ = LoadPhieuNhapAsync();
        }

        // Phương thức để mở form ThemKho và truyền dữ liệu kho cần sửa
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Lấy kho được chọn từ DataContext
            Button button = sender as Button;
            Kho selectedKho = button.DataContext as Kho;

            if (selectedKho != null)
            {
                // Mở form ThemKho và truyền dữ liệu kho cần sửa
                ThemKho themKhoForm = new ThemKho(selectedKho);  // Truyền kho hiện tại vào form ThemKho
                themKhoForm.ShowDialog();  // Hiển thị form ThemKho
            }
        }
        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int maPhieuNhap))
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa phiếu nhập có mã '{maPhieuNhap}'?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var client = await SupabaseService.GetClientAsync();

                        // Truy vấn lại đối tượng PhieuNhap từ mã
                        var getResult = await client
                            .From<PhieuNhap>()
                            .Filter("ma_phieu_nhap", Supabase.Postgrest.Constants.Operator.Equals, maPhieuNhap)
                            .Get();

                        var phieuNhapToDelete = getResult.Models.FirstOrDefault();
                        if (phieuNhapToDelete != null)
                        {
                            await client.From<PhieuNhap>().Delete(phieuNhapToDelete);

                            MessageBox.Show("Đã xóa phiếu nhập thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadPhieuNhapAsync(); // Làm mới danh sách
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy phiếu nhập cần xóa.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa phiếu nhập: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Không có phiếu nhập được chọn để xóa.");
            }
        }

    }
}
