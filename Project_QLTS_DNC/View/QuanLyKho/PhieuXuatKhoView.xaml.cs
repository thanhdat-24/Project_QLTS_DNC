using Supabase.Postgrest.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Project_QLTS_DNC.Models.PhieuXuatKho;
using Project_QLTS_DNC.Models;
using System.Windows.Controls;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.PhieuNhapKho;
using Project_QLTS_DNC.Services;

namespace Project_QLTS_DNC.View.QuanLyKho
{
    public partial class PhieuXuatKhoView : UserControl
    {
        private Supabase.Client _client;
        private Dictionary<int, string> _khoLookup = new Dictionary<int, string>();
        private Dictionary<int, string> _nvLookup = new Dictionary<int, string>();

        public PhieuXuatKhoView()
        {
            InitializeComponent();
            Loaded += async (s, e) => await Init();

            // Khởi tạo Supabase client
            _client = new Supabase.Client("https://hoybfwnugefnpctgghha.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhveWJmd251Z2VmbnBjdGdnaGhhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDQxMDQ4OTEsImV4cCI6MjA1OTY4MDg5MX0.KxNfiOUFXHGgqZf3b3xOk6BR4sllMZG_-W-y_OPUwCI");
        }

        private async Task Init()
        {
            await LoadKhoLookupAsync();
            await LoadNhanVienLookupAsync();
            await LoadPhieuXuatAsync();
        }

        private async Task LoadKhoLookupAsync()
        {
            try
            {
                // Lấy dữ liệu từ bảng "kho"
                var result = await _client.From<Kho>().Get();

                // Kiểm tra nếu có dữ liệu
                if (result.Models != null && result.Models.Any())
                {
                    // Tạo từ điển MaKho -> TenKho
                    _khoLookup = result.Models.ToDictionary(k => (int)k.MaKho, k => k.TenKho);
                }
                else
                {
                    _khoLookup.Clear();
                    MessageBox.Show("Không có dữ liệu kho.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách kho: {ex.Message}");
            }
        }

        private async Task LoadNhanVienLookupAsync()
        {
            try
            {
                var result = await _client.From<NhanVienModel>().Get();
                if (result.Models != null)
                {
                    _nvLookup = result.Models.ToDictionary(nv => nv.MaNV, nv => nv.TenNV);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách nhân viên: {ex.Message}");
            }
        }

        private async Task LoadPhieuXuatAsync()
        {
            try
            {
                var result = await _client.From<PhieuXuat>().Get();

                if (result.Models.Any())
                {
                    // Lọc các phiếu xuất theo mã phiếu xuất
                    var displayList = result.Models
                        .Where(p =>
                            // Tìm kiếm theo mã phiếu xuất
                            (string.IsNullOrEmpty(txtSearch.Text) || p.MaPhieuXuat.ToString().Contains(txtSearch.Text))
                        )
                        .Select(p => new
                        {
                            MaPhieuXuat = p.MaPhieuXuat,
                            TenKhoXuat = _khoLookup.ContainsKey((int)p.MaKhoXuat) ? _khoLookup[(int)p.MaKhoXuat] : $"#{p.MaKhoXuat}",
                            TenKhoNhan = _khoLookup.ContainsKey((int)p.MaKhoNhan) ? _khoLookup[(int)p.MaKhoNhan] : $"#{p.MaKhoNhan}",
                            NgayXuat = p.NgayXuat.ToString("dd/MM/yyyy"),
                            TenNhanVien = _nvLookup.ContainsKey((int)p.MaNV) ? _nvLookup[(int)p.MaNV] : $"#{p.MaNV}",
                            TrangThai = p.TrangThai ?? "Chờ duyệt",
                            GhiChu = p.GhiChu
                        })
                        .ToList();

                    dgPhieuXuatKho.ItemsSource = displayList;
                }
                else
                {
                    dgPhieuXuatKho.ItemsSource = null;
                    MessageBox.Show("Không có dữ liệu phiếu xuất.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu phiếu xuất: {ex.Message}");
            }
        }


        private async void btnAdd_Click_1(object sender, RoutedEventArgs e)
        {
            // Mở cửa sổ thêm phiếu xuất kho mới
            var window = new PhieuXuatKho();
            if (window.ShowDialog() == true)
            {
                await LoadPhieuXuatAsync();
            }
        }

        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            await LoadPhieuXuatAsync();
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int maPhieuXuat))
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa phiếu xuất có mã '{maPhieuXuat}'?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var client = await SupabaseService.GetClientAsync();

                        // Truy vấn lại đối tượng PhieuXuat từ mã
                        var getResult = await client
                            .From<PhieuXuat>()
                            .Filter("ma_phieu_xuat", Supabase.Postgrest.Constants.Operator.Equals, maPhieuXuat)
                            .Get();

                        var phieuXuatToDelete = getResult.Models.FirstOrDefault();
                        if (phieuXuatToDelete != null)
                        {
                            await client.From<PhieuXuat>().Delete(phieuXuatToDelete);

                            MessageBox.Show("Đã xóa phiếu xuất thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadPhieuXuatAsync(); // Làm mới danh sách
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy phiếu xuất cần xóa.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa phiếu xuất: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Không có phiếu xuất được chọn để xóa.");
            }
        }

        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {

        }

    }

}

