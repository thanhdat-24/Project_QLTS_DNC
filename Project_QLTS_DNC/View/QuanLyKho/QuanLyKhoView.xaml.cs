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
using System.Windows.Navigation;
using System.Windows.Shapes;

using Supabase;
using Project_QLTS_DNC.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Project_QLTS_DNC.View.QuanLyKho
{
    public partial class QuanLyKhoView : UserControl
    {
        private Supabase.Client _client;

        public QuanLyKhoView()
        {
            InitializeComponent();
            _ = Init(); // Gọi phương thức Init để thiết lập kết nối Supabase và tải dữ liệu
        }

        private async Task Init()
        {
            await InitializeSupabaseAsync();
            await LoadKhoDataAsync(); // Gọi hàm tải dữ liệu kho
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

        private async Task LoadKhoDataAsync()
        {
            try
            {
                // Truy vấn dữ liệu kho từ Supabase
                var result = await _client.From<Kho>().Get();

                // Kiểm tra nếu có dữ liệu trả về
                if (result.Models.Any())
                {
                    // Gán dữ liệu vào DataGrid
                    dgKho.ItemsSource = result.Models;
                }
                else
                {
                    MessageBox.Show("Không có dữ liệu kho.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu kho: {ex.Message}");
            }
        }

        private void btnViewDetail_Click(object sender, RoutedEventArgs e)
        {
            ThemKho themKhoForm = new ThemKho();
            themKhoForm.ShowDialog(); // Mở form Thêm Kho
        }
    }
}

