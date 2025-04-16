using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.PhieuXuatKho;
using Supabase;

namespace Project_QLTS_DNC.View.QuanLyKho
{
    public partial class PhieuXuatKho : Window
    {
        private Supabase.Client _client;

        public PhieuXuatKho()
        {
            InitializeComponent();
            Loaded += PhieuXuatKho_Loaded;
        }

        private async void PhieuXuatKho_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            await LoadKhoAsync();
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

            cboMaKhoXuat.ItemsSource = result.Models;
            cboMaKhoXuat.DisplayMemberPath = "TenKho";
            cboMaKhoXuat.SelectedValuePath = "MaKho";

            cboMaKhoNhan.ItemsSource = result.Models;
            cboMaKhoNhan.DisplayMemberPath = "TenKho";
            cboMaKhoNhan.SelectedValuePath = "MaKho";
        }

        private async Task LoadNhanVienAsync()
        {
            var result = await _client.From<NhanVienModel>().Get();

            if (result.Models != null)
            {
                cboNguoiLapPhieu.ItemsSource = result.Models;
                cboNguoiLapPhieu.DisplayMemberPath = "TenNV";
                cboNguoiLapPhieu.SelectedValuePath = "MaNV";
            }
            else
            {
                cboNguoiLapPhieu.ItemsSource = null;
            }
        }

        private static async Task<long> SinhMaPhieuXuatAsync(Supabase.Client client)
        {
            var danhSach = await client.From<PhieuXuat>().Get();

            if (danhSach?.Models == null || danhSach.Models.Count == 0)
                return 1;

            return danhSach.Models.Max(p => p.MaPhieuXuat) + 1;
        }
        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc muốn huỷ bỏ?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        private async void btnCapNhat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboMaKhoXuat.SelectedItem is not Kho selectedKhoXuat)
                {
                    MessageBox.Show("Vui lòng chọn kho xuất.");
                    return;
                }

                if (cboMaKhoNhan.SelectedItem is not Kho selectedKhoNhan)
                {
                    MessageBox.Show("Vui lòng chọn kho nhận.");
                    return;
                }

                if (dpNgayXuat.SelectedDate is not DateTime ngayXuat)
                {
                    MessageBox.Show("Vui lòng chọn ngày xuất.");
                    return;
                }

                if (cboNguoiLapPhieu.SelectedItem is not NhanVienModel selectedNV)
                {
                    MessageBox.Show("Vui lòng chọn người lập phiếu.");
                    return;
                }

                // ✅ Tự sinh mã
                long maPhieuXuatMoi = await SinhMaPhieuXuatAsync(_client);

                var phieuXuat = new PhieuXuat
                {
                    MaPhieuXuat = maPhieuXuatMoi,
                    MaKhoXuat = selectedKhoXuat.MaKho,
                    MaKhoNhan = selectedKhoNhan.MaKho,
                    MaNV = selectedNV.MaNV,
                    NgayXuat = ngayXuat,
                    GhiChu = txtGhiChu.Text,
                    TrangThai = txtTrangThai.Text
                };

                var result = await _client.From<PhieuXuat>().Insert(phieuXuat);

                if (result.Model == null)
                {
                    MessageBox.Show("Không thể tạo phiếu xuất.");
                    return;
                }

                var chiTietForm = new ChiTietPhieuXuat(maPhieuXuatMoi);
                chiTietForm.ShowDialog();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tạo phiếu xuất: " + ex.Message);
            }
        }

    }
}
