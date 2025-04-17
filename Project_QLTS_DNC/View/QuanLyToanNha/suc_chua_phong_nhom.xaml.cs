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
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Models.QLNhomTS;
using Supabase;

namespace Project_QLTS_DNC.View.QuanLyToanNha
{
    /// <summary>
    /// Interaction logic for suc_chua_phong_nhom.xaml
    /// </summary>
    public partial class suc_chua_phong_nhom : Window
    {
        private List<NhomTaiSan> _dsNhomTaiSan = new();
        private Supabase.Client _client;
        public suc_chua_phong_nhom()
        {
            InitializeComponent();
        }

        private int _maPhong;

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

      

        private async void ThemSucChuaPhongNhom_Loaded(object sender, RoutedEventArgs e)
        {
            txtMaPhong.Text = _maPhong.ToString();
            await InitializeSupabaseAsync();
            await LoadNhomTaiSanAsync();
        }

        public suc_chua_phong_nhom(int maPhong)
        {
            InitializeComponent();
            _maPhong = maPhong;
            Loaded += ThemSucChuaPhongNhom_Loaded;
        }
        private async Task LoadNhomTaiSanAsync()
        {
            try
            {
                var result = await _client.From<NhomTaiSan>().Get();
                _dsNhomTaiSan = result.Models ?? new List<NhomTaiSan>();

                cboNhomTS.ItemsSource = _dsNhomTaiSan;
                cboNhomTS.DisplayMemberPath = "TenNhom";  // Hiển thị tên nhóm tài sản
                cboNhomTS.SelectedValuePath = "MaNhomTS";   // Lưu giá trị là mã nhóm
                cboNhomTS.SelectedIndex = -1; // Không chọn mặc định
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải nhóm tài sản: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnLuu_Click(object sender, RoutedEventArgs e)

        {
            MessageBox.Show($"DEBUG: _maPhong = {_maPhong}");

            if (cboNhomTS.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn nhóm tài sản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtSucChua.Text, out int sucChua) || sucChua <= 0)
            {
                MessageBox.Show("Sức chứa phải là số nguyên dương!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var model = new PhongNhomTS
            {
                MaPhong = _maPhong, // được truyền từ form trước
                MaNhomTS = (int)cboNhomTS.SelectedValue,
                SucChua = sucChua
            };

            MessageBox.Show($"MODEL gửi lên: MaPhong = {model.MaPhong}, MaNhomTS = {model.MaNhomTS}, SucChua = {model.SucChua}");

            try
            {
                var response = await _client.From<PhongNhomTS>().Insert(model);
                MessageBox.Show("Đã lưu sức chứa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
