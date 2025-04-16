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
using Project_QLTS_DNC.Models.KiemKe;
using Project_QLTS_DNC.Models.NhanVien;
using Supabase;

namespace Project_QLTS_DNC.View.KiemKe
{
    /// <summary>
    /// Interaction logic for DotKiemKeInput.xaml
    /// </summary>
    public partial class DotKiemKeInput : Window
    {
        private Supabase.Client _client;
        private List<NhanVienModel> _dsNhanVien;
        public DotKiemKeInput()
        {
            InitializeComponent();
            Loaded += DotKiemKeInput_Loaded;
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

        private async void DotKiemKeInput_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            await LoadNhanVienAsync();
        }


        private async Task LoadNhanVienAsync()
        {
            var result = await _client.From<NhanVienModel>().Get();
            _dsNhanVien = result.Models;
            cboNhanVien.ItemsSource = _dsNhanVien;
        }

        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTenDot.Text))
                {
                    MessageBox.Show("Vui lòng nhập tên đợt kiểm kê.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dot = new DotKiemKe
                {
                    TenDot = txtTenDot.Text.Trim(),
                    NgayBatDau = dpNgayBatDau.SelectedDate,
                    NgayKetThuc = dpNgayKetThuc.SelectedDate,
                    MaNV = cboNhanVien.SelectedValue != null ? (int)cboNhanVien.SelectedValue : 0,
                    GhiChu = txtGhiChu.Text.Trim()
                };

                await _client.From<DotKiemKe>().Insert(dot);

                MessageBox.Show("Đã lưu đợt kiểm kê thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
