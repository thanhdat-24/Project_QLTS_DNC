using Project_QLTS_DNC.Helpers;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Models.ThongBao;
using Project_QLTS_DNC.Services.ThongBao;
using Supabase;
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
using System.Windows.Shapes;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for CTMuaMoi.xaml
    /// </summary>
    public partial class CTMuaMoi : Window
    {
        private int _maPhieu;
        private Supabase.Client _client;
        private List<ChiTietDeNghiMua> _danhSachTam = new();
        public CTMuaMoi(int maPhieuMoi)
        {

            InitializeComponent();
            Loaded += PhieuCTMuaMoiTS_Loaded;
            _maPhieu = maPhieuMoi;
        }
        private async void PhieuCTMuaMoiTS_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            LoadChiTietPhieu();
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
        private void LoadChiTietPhieu()
        {
            txtMaPhieu.Text = _maPhieu.ToString();
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            Close();   
        }

        private async void btnCapNhat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra xem danh sách tạm có tài sản nào không
                if (_danhSachTam == null || _danhSachTam.Count == 0)
                {
                    MessageBox.Show("Vui lòng thêm ít nhất một tài sản vào danh sách.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Lưu thông tin phiếu
                bool allSuccess = true;
                int successCount = 0;
                List<string> errorMessages = new List<string>();

                // Lưu từng tài sản trong danh sách vào database
                foreach (var chiTiet in _danhSachTam)
                {
                    // Đảm bảo mã phiếu đề nghị được gán
                    chiTiet.MaPhieuDeNghi = _maPhieu;

                    var result = await _client
                        .From<ChiTietDeNghiMua>()
                        .Insert(chiTiet);

                    if (result != null && result.Models != null && result.Models.Count > 0)
                    {
                        successCount++;
                    }
                    else
                    {
                        allSuccess = false;
                        errorMessages.Add($"Không thể lưu tài sản: {chiTiet.TenTaiSan}");
                    }
                }

                // Hiển thị thông báo kết quả
                if (allSuccess)
                {
                    MessageBox.Show($"Đã lưu thành công {successCount} tài sản!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else if (successCount > 0)
                {
                    string message = $"Đã lưu thành công {successCount}/{_danhSachTam.Count} tài sản.\n\nCác tài sản sau không thể lưu:\n" + string.Join("\n", errorMessages);
                    MessageBox.Show(message, "Lưu một phần", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("Không thể lưu bất kỳ tài sản nào. Vui lòng kiểm tra lại dữ liệu hoặc kết nối.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra tên tài sản
            if (string.IsNullOrWhiteSpace(txtTenTS.Text))
            {
                MessageBox.Show("Vui lòng nhập tên tài sản.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Kiểm tra số lượng hợp lệ
            if (string.IsNullOrWhiteSpace(txtSoLuong.Text) || !int.TryParse(txtSoLuong.Text.Trim(), out int soLuong) || soLuong <= 0)
            {
                MessageBox.Show("Vui lòng nhập số lượng hợp lệ (số nguyên dương).", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Kiểm tra đơn giá hợp lệ
            if (string.IsNullOrWhiteSpace(txtGia.Text) || !decimal.TryParse(txtGia.Text.Trim(), out decimal gia) || gia <= 0)
            {
                MessageBox.Show("Vui lòng nhập đơn giá hợp lệ (số thực dương).", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Kiểm tra mô tả hợp lệ (có thể để trống)
            string moTa = txtMoTa.Text.Trim();

            // Tạo đối tượng chi tiết phiếu nhập
            var chiTiet = new ChiTietDeNghiMua
            {
                MaPhieuDeNghi = int.Parse(txtMaPhieu.Text), // Mã phiếu từ form
                TenTaiSan = txtTenTS.Text, // Tên tài sản từ form
                SoLuong = soLuong,
                DuKienGia = (int)gia, // Đơn giá từ form
                DonViTinh = txtDonViTinh.Text, // Đơn vị tính từ form
                MoTa = moTa,           };

            // Kiểm tra trùng tài sản trong danh sách tạm
            if (_danhSachTam.Any(x => x.TenTaiSan == chiTiet.TenTaiSan))
            {
                MessageBox.Show("Tài sản này đã được thêm trước đó.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ✅ Thêm vào danh sách tạm
            _danhSachTam.Add(chiTiet);

            // Cập nhật DataGrid hiển thị danh sách tài sản
            gridTaiSan.ItemsSource = null;
            gridTaiSan.ItemsSource = _danhSachTam;

            // Reset form sau khi thêm
            txtTenTS.Clear();
            txtSoLuong.Clear();
            txtGia.Clear();
            txtDonViTinh.Clear();
            txtMoTa.Clear();


        }

        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
