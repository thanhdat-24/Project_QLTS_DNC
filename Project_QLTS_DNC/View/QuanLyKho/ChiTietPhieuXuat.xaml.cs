using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.PhieuXuatKho;
using Project_QLTS_DNC.Models.QLNhomTS;
using Supabase;

namespace Project_QLTS_DNC.View.QuanLyKho
{
    /// <summary>
    /// Interaction logic for ChiTietPhieuXuat.xaml
    /// </summary>
    public partial class ChiTietPhieuXuat : Window
    {
        private Supabase.Client _client;
        private long _maPhieuXuat;
        private List<ChiTietPhieuXuatInsert> _danhSachTam = new List<ChiTietPhieuXuatInsert>();

        public ChiTietPhieuXuat(long maPhieuXuatMoi)
        {
            InitializeComponent();
            _maPhieuXuat = maPhieuXuatMoi; // ← Gán mã phiếu
            Loaded += ChiTietPhieuXuat_Load;
        }

        private async void ChiTietPhieuXuat_Load(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            LoadPhieuXuat();
            await LoadNhomTaiSanAsync();
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

        private void LoadPhieuXuat()
        {
            txtMaPhieuXuat.Text = _maPhieuXuat.ToString();
        }

        private async Task LoadNhomTaiSanAsync()
        {
            try
            {
                var result = await _client.From<NhomTaiSan>().Get();
                if (result.Models == null || !result.Models.Any())
                {
                    MessageBox.Show("Không có nhóm tài sản nào.");
                    return;
                }

                cboNhomTaiSan.ItemsSource = result.Models;
                cboNhomTaiSan.DisplayMemberPath = "TenNhom";
                cboNhomTaiSan.SelectedValuePath = "MaNhomTS";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load nhóm tài sản: " + ex.Message);
            }
        }

        private async Task<int> SinhMaChiTietPhieuXuatAsync()
        {
            try
            {
                var danhSachCTPX = await _client.From<ChiTietPhieuXuatModel>().Get();
                if (danhSachCTPX?.Models == null || danhSachCTPX.Models.Count == 0)
                    return 1;

                int maxMa = danhSachCTPX.Models.Max(ct => ct.MaChiTietXK);
                return maxMa + 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi sinh mã chi tiết phiếu xuất: {ex.Message}");
                throw;
            }
        }

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            if (cboNhomTaiSan.SelectedItem is not NhomTaiSan nhom)
            {
                MessageBox.Show("Vui lòng chọn nhóm tài sản trước khi thêm.");
                return;
            }

            if (!int.TryParse(txtSoLuong.Text, out int soLuong) || soLuong <= 0)
            {
                MessageBox.Show("Số lượng không hợp lệ. Phải là số nguyên dương.");
                return;
            }

            try
            {
                // Truy cập trực tiếp thuộc tính MaNhomTS của đối tượng nhom
                int maNhomTS = nhom.MaNhomTS;

                var item = new ChiTietPhieuXuatInsert
                {
                    MaPhieuXuat = _maPhieuXuat,
                    MaTaiSan = maNhomTS,
                    SoLuong = soLuong,
                };

                _danhSachTam.Add(item);

                gridTaiSan.ItemsSource = null;
                gridTaiSan.ItemsSource = _danhSachTam;

                txtSoLuong.Clear();

                // Hiển thị thông báo thành công (tùy chọn)
                // MessageBox.Show($"Đã thêm nhóm tài sản: {nhom.TenNhomTS}, số lượng: {soLuong}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm nhóm tài sản: {ex.Message}\n\nThông tin chi tiết: {ex}");
            }
        }

        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (_danhSachTam.Count == 0)
            {
                MessageBox.Show("Chưa có tài sản nào để lưu.");
                return;
            }

            try
            {
                foreach (var item in _danhSachTam)
                {
                    var insertItem = new ChiTietPhieuXuatInsert
                    {
                        MaPhieuXuat = item.MaPhieuXuat,
                        MaTaiSan = item.MaTaiSan,
                        SoLuong = item.SoLuong
                    };

                    var response = await _client.From<ChiTietPhieuXuatInsert>().Insert(insertItem);

                    // Kiểm tra lỗi từ Supabas
                    // Kiểm tra nếu không có dữ liệu trả về
                    if (response.Models == null || response.Models.Count == 0)
                        throw new Exception("Không thể lưu chi tiết: " + item.MaTaiSan);
                }

                MessageBox.Show("Đã lưu toàn bộ chi tiết thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
