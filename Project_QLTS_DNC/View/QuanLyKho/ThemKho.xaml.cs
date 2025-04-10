using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Supabase;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Services;

namespace Project_QLTS_DNC.View.QuanLyKho
{
    public partial class ThemKho : Window
    {
        private Supabase.Client _client = null!;

        public ThemKho()
        {
            InitializeComponent();
            _ = Init();
        }

        private async Task Init()
        {
            await InitializeSupabaseAsync();
            await LoadToaNhaAsync();

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

        private async Task LoadToaNhaAsync()
        {
            try
            {
                // Làm trống các mục hiện có của ComboBox trước khi gán ItemsSource
                cboToaNha.Items.Clear();

                // Lấy dữ liệu các tòa nhà từ Supabase
                var result = await _client.From<ToaNha>().Get();

                // Kiểm tra nếu có dữ liệu từ Supabase
                if (result.Models.Any())
                {
                    // Tạo danh sách ComboBoxItem từ kết quả
                    var list = result.Models.Select(x => new ComboBoxItem
                    {
                        Content = x.TenToaNha,  // Tên tòa nhà
                        Tag = x.MaToaNha       // Mã tòa nhà (lưu trong Tag để dễ dàng truy xuất)
                    }).ToList();

                    // Đưa dữ liệu vào ComboBox
                    cboToaNha.ItemsSource = list;
                    cboToaNha.DisplayMemberPath = "Content";  // Để ComboBox hiển thị tên tòa nhà
                    cboToaNha.SelectedValuePath = "Tag";     // Để ComboBox lưu giá trị mã tòa nhà
                }
                else
                {
                    MessageBox.Show("Không có dữ liệu tòa nhà.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu tòa nhà: " + ex.Message);
            }
        }
        // Hàm Sinh Mã Kho Mới
        private static async Task<int> SinhMaKhoAsync()
        {
            try
            {
                // Lấy danh sách kho hiện có
                var danhSachKho = await LayDanhSachKhoAsync();

                // Nếu không có dữ liệu, bắt đầu từ 1
                if (danhSachKho == null || danhSachKho.Count == 0)
                    return 1;

                // Tìm mã kho lớn nhất trong danh sách
                int maxMaKho = 0;
                foreach (var kho in danhSachKho)
                {
                    if (kho.MaKho > maxMaKho)
                        maxMaKho = kho.MaKho;
                }

                // Trả về mã kho mới = mã kho lớn nhất + 1
                return maxMaKho + 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi sinh mã kho: {ex.Message}");
                throw;
            }
        }

        // Hàm Lấy Danh Sách Kho Từ Supabase
        private static async Task<List<Kho>> LayDanhSachKhoAsync()
        {
            var client = await SupabaseService.GetClientAsync();
            var response = await client.From<Kho>().Get();

            if (response.Models != null)
            {
                return response.Models.ToList();
            }

            return new List<Kho>();
        }

        // Hàm Thêm Kho
        private async void btnLuuKho_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenKho.Text) || cboToaNha.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin.");
                return;
            }

            // Sinh mã kho mới
            int maKhoMoi = await SinhMaKhoAsync();

            // Lấy mã tòa nhà từ Tag của ComboBoxItem
            if (cboToaNha.SelectedItem is ComboBoxItem selectedItem &&
                int.TryParse(selectedItem.Tag?.ToString(), out int maToaNha))
            {
                var newKho = new Kho
                {
                    MaKho = maKhoMoi, // Sinh mã kho mới
                    TenKho = txtTenKho.Text,
                    MoTa = txtMoTa.Text,
                    MaToaNha = maToaNha // Gán mã tòa nhà vào thuộc tính MaToaNha
                };

                try
                {
                    // Thêm kho vào Supabase
                    var client = await SupabaseService.GetClientAsync();
                    var response = await client.From<Kho>().Insert(newKho);

                    if (response.Models.Count > 0)
                    {
                        MessageBox.Show("Lưu kho thành công!");
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Không thể lưu kho!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi lưu kho: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn tòa nhà hợp lệ.");
            }
        }






    }
}
