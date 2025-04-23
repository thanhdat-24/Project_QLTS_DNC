using Project_QLTS_DNC.Helpers;
using Project_QLTS_DNC.Models.NhanVien;
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
        private Supabase.Client _client;
        public CTMuaMoi()
        {

            InitializeComponent();
            InitializeComponent();
            Loaded += PhieuCTMuaMoiTS_Loaded;
        }
        private async void PhieuCTMuaMoiTS_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            await LoadPhieuMuaMoiAsync();
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
        private async Task LoadPhieuMuaMoiAsync()
        {
            var result = await _client.From<MuaMoiTS>().Get();

            if (result.Models != null)
            {
                cboMaPhieuDeNghi.ItemsSource = result.Models;
                cboMaPhieuDeNghi.DisplayMemberPath = "MaPhieuDeNghi";
                cboMaPhieuDeNghi.SelectedValuePath = "MaPhieuDeNghi";
            }
            else
            {
                cboMaPhieuDeNghi.ItemsSource = null;
            }
        }
        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            Close();   
        }

        private async void btnCapNhat_Click(object sender, RoutedEventArgs e)
        {
            var model = new ChiTietDeNghiMua
            {
                MaPhieuDeNghi = int.Parse(cboMaPhieuDeNghi.SelectedValue?.ToString() ?? "0"),
                TenTaiSan = txtTenTS.Text,
                SoLuong = int.TryParse(txtSoLuong.Text, out var sl) ? sl : 0,
                DonViTinh = txtDonViTinh.Text,
                MoTa = txtMoTa.Text,
                DuKienGia = int.TryParse(txtGia.Text, out var dg) ? dg : 0
            };

            var result = await _client
                .From<ChiTietDeNghiMua>()
                .Insert(model);

            if (result != null && result.Models != null)
            {
               

                MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close(); // đóng form
            }
            else
            {
                MessageBox.Show("Lưu thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
