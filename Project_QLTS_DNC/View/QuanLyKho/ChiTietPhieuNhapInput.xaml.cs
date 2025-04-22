using Project_QLTS_DNC.Models.PhieuNhapKho;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Models;
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
using Project_QLTS_DNC.Services;
using System.Collections.ObjectModel;

namespace Project_QLTS_DNC.View.QuanLyKho
{
    public partial class ChiTietPhieuNhapInput : Window
    {
        private Supabase.Client _client;
        private readonly int _maPhieuNhap;
        private readonly int _maNCC;
        private List<ChiTietPhieuNhap> _danhSachTam = new();

        private async void ChiTietPhieuNhapInput_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();           
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

        private static async Task<int> SinhMaChiTietPhieuNhapAsync(Supabase.Client client)
        {
            try
            {
                var danhSachCTPN = await client.From<ChiTietPhieuNhap>().Get();

                if (danhSachCTPN?.Models == null || danhSachCTPN.Models.Count == 0)
                    return 1;

                int maxMa = danhSachCTPN.Models.Max(ct => ct.MaChiTietPN);
                return maxMa + 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi sinh mã chi tiết phiếu nhập: {ex.Message}");
                throw;
            }
        }
       
        private async Task LoadNhomTaiSanAsync()
        {
            try
            {
                var result = await _client.From<NhomTaiSan>().Get();
                cboNhomTaiSan.ItemsSource = result.Models;
                cboNhomTaiSan.DisplayMemberPath = "TenNhom";
                cboNhomTaiSan.SelectedValuePath = "MaNhomTS";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load nhóm tài sản: " + ex.Message);
            }
        }
       
        public ChiTietPhieuNhapInput(int maPhieuNhap, int maNCC)
        {
            InitializeComponent();
            _maPhieuNhap = maPhieuNhap;
            _maNCC = maNCC;
            Loaded += ChiTietPhieuNhapInput_Load;
        }
        private void LoadPhieuNhap()
        {
            txtMaPhieuNhap.Text = _maPhieuNhap.ToString();
        }

        private async void ChiTietPhieuNhapInput_Load(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            LoadPhieuNhap(); // 👈 Load mã phiếu
            await LoadNhomTaiSanAsync();           
        }

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            if (cboNhomTaiSan.SelectedItem is not NhomTaiSan nhom)
            {
                MessageBox.Show("Vui lòng chọn nhóm tài sản trước khi thêm.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSoLuong.Text) || !int.TryParse(txtSoLuong.Text.Trim(), out int soLuong) || soLuong <= 0)
            {
                MessageBox.Show("Vui lòng nhập số lượng hợp lệ (số nguyên dương).", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDonGia.Text) || !decimal.TryParse(txtDonGia.Text.Trim(), out decimal donGia) || donGia <= 0)
            {
                MessageBox.Show("Vui lòng nhập đơn giá hợp lệ (số thực dương).", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtThoiGianBaoHanh.Text) || !int.TryParse(txtThoiGianBaoHanh.Text.Trim(), out int thoiGianBaoHanh) || thoiGianBaoHanh < 0)
            {
                MessageBox.Show("Vui lòng nhập thời gian bảo hành hợp lệ (số nguyên >= 0).", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Kiểm tra trùng nhóm tài sản
            if (_danhSachTam.Any(x => x.MaNhomTS == nhom.MaNhomTS))
            {
                MessageBox.Show("Nhóm tài sản này đã được thêm trước đó.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ✅ Thêm vào danh sách tạm
            var chiTiet = new ChiTietPhieuNhap
            {
                MaPhieuNhap = int.Parse(txtMaPhieuNhap.Text), // Lấy mã phiếu nhập đang mở form
                MaNhomTS = nhom.MaNhomTS,
                TenTaiSan = nhom.TenNhom, // Gán tên nhóm tài sản vào
                SoLuong = soLuong,
                DonGia = donGia,
                ThoiGianBaoHanh = thoiGianBaoHanh,
                CanQuanLyRieng = chkQuanLyRieng.IsChecked == true
            };

            _danhSachTam.Add(chiTiet);

            // Cập nhật DataGrid
            gridTaiSan.ItemsSource = null;
            gridTaiSan.ItemsSource = _danhSachTam;

            // Reset form
            cboNhomTaiSan.SelectedIndex = -1;
            txtSoLuong.Clear();
            txtDonGia.Clear();
            txtThoiGianBaoHanh.Clear();
            chkQuanLyRieng.IsChecked = false;
        }

        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (_danhSachTam.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất một tài sản trước khi lưu.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (var item in _danhSachTam)
            {
                var chiTietInsert = new ChiTietPhieuNhapInsert
                {
                    MaPhieuNhap = item.MaPhieuNhap,
                    MaNhomTS = item.MaNhomTS,
                    TenTaiSan = item.TenTaiSan,
                    SoLuong = item.SoLuong,
                    DonGia = item.DonGia,
                    ThoiGianBaoHanh = item.ThoiGianBaoHanh,
                    CanQuanLyRieng = item.CanQuanLyRieng
                };

                await _client.From<ChiTietPhieuNhapInsert>().Insert(chiTietInsert);
            }

            MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }





        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
