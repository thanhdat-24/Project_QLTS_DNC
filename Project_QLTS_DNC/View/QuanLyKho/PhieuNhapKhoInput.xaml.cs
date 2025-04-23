using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Supabase;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.PhieuNhapKho;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.NhaCungCap;
using Project_QLTS_DNC.Services;

namespace Project_QLTS_DNC.View.QuanLyKho
{
    public partial class PhieuNhapKhoInput : Window
    {
        private Supabase.Client _client;

        public PhieuNhapKhoInput()
        {
            InitializeComponent();
            Loaded += PhieuNhapKhoInput_Loaded;
        }

        private async void PhieuNhapKhoInput_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            await LoadKhoAsync();
            await LoadNhaCungCapAsync();
            await LoadNhanVienAsync();
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

        private async Task LoadKhoAsync()
        {
            var result = await _client.From<Kho>().Get();
            cboMaKho.ItemsSource = result.Models;
            cboMaKho.DisplayMemberPath = "TenKho";
            cboMaKho.SelectedValuePath = "MaKho";
        }

        private async Task LoadNhaCungCapAsync()
        {
            var result = await _client.From<NhaCungCapClass>().Get();
            cboNhaCungCap.ItemsSource = result.Models; // 👉 Không Convert
            cboNhaCungCap.DisplayMemberPath = "TenNCC"; // 👈 Bind property
            cboNhaCungCap.SelectedValuePath = "MaNCC";
        }

        private async Task LoadNhanVienAsync()
        {
            var result = await _client.From<NhanVienModel>().Get();
            cboNguoiLapPhieu.ItemsSource = result.Models;
            cboNguoiLapPhieu.DisplayMemberPath = "TenNV";
            cboNguoiLapPhieu.SelectedValuePath = "MaNV";
        }


        private static async Task<int> SinhMaPhieuNhapAsync(Supabase.Client client)
        {
            try
            {
                var danhSach = await client.From<PhieuNhap>().Get();

                if (danhSach?.Models == null || danhSach.Models.Count == 0)
                    return 1;

                int maxMa = danhSach.Models.Max(p => p.MaPhieuNhap);
                return maxMa + 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi sinh mã phiếu nhập: {ex.Message}");
                throw;
            }
        }

        private async Task<decimal> TinhTongTienPhieuNhapAsync(int maPhieuNhap)
        {
            var result = await _client.From<ChiTietPhieuNhap>()
                                      .Where(x => x.MaPhieuNhap == maPhieuNhap)
                                      .Get();

            if (result.Models == null) return 0;

            return result.Models.Sum(x => (x.SoLuong ?? 0) * (x.DonGia ?? 0));

        }


        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnCapNhat_Click(object sender, RoutedEventArgs e)
        {
            if (cboMaKho.SelectedItem is not Kho selectedKho)
            {
                MessageBox.Show("Vui lòng chọn kho.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cboNhaCungCap.SelectedItem is not NhaCungCapClass selectedNCC ||
                cboNguoiLapPhieu.SelectedItem is not NhanVienModel selectedNV)
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp và người lập phiếu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Kiểm tra ngày nhập
            if (dpNgayNhap.SelectedDate is not DateTime ngayNhap)
            {
                MessageBox.Show("Vui lòng chọn ngày nhập hợp lệ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpNgayNhap.Focus();
                return;
            }

            if (ngayNhap.Date < DateTime.Now.Date)
            {
                MessageBox.Show("Ngày nhập không được nhỏ hơn ngày hiện tại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpNgayNhap.Focus();
                return;
            }


            decimal tongTien = decimal.TryParse(txtTongTien.Text.Replace(",", ""), out decimal tt) ? tt : 0;

            // 👉 Lấy mã đã chọn
            int selectedMaNCC = selectedNCC.MaNCC;
            int selectedMaNV = selectedNV.MaNV;

            // 👉 Sinh mã phiếu nhập mới
            int maPhieuMoi = await SinhMaPhieuNhapAsync(_client);

            // 👉 Tạo đối tượng phiếu nhập
            var phieuNhap = new PhieuNhap
            {
                MaPhieuNhap = maPhieuMoi,
                MaKho = selectedKho.MaKho,
                MaNCC = selectedMaNCC,
                MaNV = selectedMaNV,
                NgayNhap = ngayNhap,
                TongTien = tongTien,
                TrangThai = null
            };

            // 👉 Lưu vào Supabase
            var response = await _client.From<PhieuNhap>().Insert(phieuNhap);

            if (response.Model != null)
            {
                

                // 👉 Mở form nhập chi tiết
                var chiTietForm = new ChiTietPhieuNhapInput(maPhieuMoi, selectedMaNCC);
                chiTietForm.ShowDialog();

                // 👉 Sau khi nhập chi tiết, tính tổng tiền mới
                decimal tongTienCapNhat = await TinhTongTienPhieuNhapAsync(maPhieuMoi);
                txtTongTien.Text = tongTienCapNhat.ToString("N0");

                // 👉 Cập nhật lại tổng tiền
                phieuNhap.TongTien = tongTienCapNhat;
                await _client.From<PhieuNhap>().Update(phieuNhap);

                this.Close();
            }
            else
            {
                MessageBox.Show("Lưu phiếu nhập thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




    }
}