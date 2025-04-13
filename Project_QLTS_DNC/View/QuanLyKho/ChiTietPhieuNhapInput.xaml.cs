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
                MessageBox.Show("Vui lòng chọn nhóm tài sản trước khi thêm.");
                return;
            }

            if (!int.TryParse(txtSoLuong.Text, out int soLuong))
            {
                MessageBox.Show("Số lượng không hợp lệ.");
                return;
            }

            if (!decimal.TryParse(txtDonGia.Text, out decimal donGia))
            {
                MessageBox.Show("Đơn giá không hợp lệ.");
                return;
            }

            // 👉 KHÔNG sinh mã ở đây
            var item = new ChiTietPhieuNhap
            {
                MaChiTietPN = 0, // placeholder, sẽ sinh sau khi nhấn "Lưu"
                MaPhieuNhap = _maPhieuNhap,
                MaNhomTS = nhom.MaNhomTS,
                TenTaiSan = nhom.TenNhom,
                SoLuong = soLuong,
                DonGia = donGia,
                CanQuanLyRieng = chkQuanLyRieng.IsChecked == true
            };

            _danhSachTam.Add(item);

            gridTaiSan.ItemsSource = null;
            gridTaiSan.ItemsSource = _danhSachTam;

            txtSoLuong.Clear();
            txtDonGia.Clear();
            chkQuanLyRieng.IsChecked = false;
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
                    // 👉 Sinh mã cho từng dòng
                    int maMoi = (int)await SinhMaChiTietPhieuNhapAsync(_client);
                    item.MaChiTietPN = maMoi;

                    var response = await _client.From<ChiTietPhieuNhap>().Insert(item);
                    if (response.Model == null)
                        throw new Exception("Không thể lưu chi tiết: " + item.TenTaiSan);
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
