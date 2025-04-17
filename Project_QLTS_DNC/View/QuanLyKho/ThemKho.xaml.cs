using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Supabase;
using System.Threading.Tasks;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Models.ToaNha;

namespace Project_QLTS_DNC.View.QuanLyKho
{
    public partial class ThemKho : Window
    {
        private Supabase.Client _client = null!;
        private Kho _selectedKho;

        public ThemKho()
        {
            InitializeComponent();
            _ = Init();

           
        }

        public ThemKho(Kho selectedKho) : this()
        {
            _selectedKho = selectedKho;  // Lưu kho được chọn vào biến

            // Điền dữ liệu kho vào các trường nhập liệu để chỉnh sửa
            txtTenKho.Text = selectedKho.TenKho;
            txtMoTa.Text = selectedKho.MoTa;
            cboToaNha.SelectedValue = selectedKho.MaToaNha;
            btnLuuKho.Content = "Cập nhật kho"; // Đổi chữ trên nút Lưu thành "Cập nhật kho"
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
                var result = await _client.From<ToaNha>().Get();
                cboToaNha.ItemsSource = result.Models; // 👉 Gán thẳng Model
                cboToaNha.DisplayMemberPath = "TenToaNha"; // 👈 Hiển thị theo tên
                cboToaNha.SelectedValuePath = "MaToaNha"; // 👈 Lưu mã
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu tòa nhà: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int maKhoMoi = _selectedKho != null ? _selectedKho.MaKho : await SinhMaKhoAsync();

            if (cboToaNha.SelectedItem is not ToaNha selectedToaNha)
            {
                MessageBox.Show("Vui lòng chọn tòa nhà hợp lệ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newKho = new Kho
            {
                MaKho = maKhoMoi,
                TenKho = txtTenKho.Text,
                MoTa = txtMoTa.Text,
                MaToaNha = selectedToaNha.MaToaNha
            };

            try
            {
                var client = await SupabaseService.GetClientAsync();

                if (_selectedKho == null)
                {
                    // 👉 Nếu thêm mới
                    var response = await client.From<Kho>().Insert(newKho);

                    if (response.Models.Count > 0)
                    {
                        MessageBox.Show("Lưu kho thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Không thể lưu kho!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // 👉 Nếu đang cập nhật kho cũ
                    var result = await client.From<Kho>().Get();

                    if (result.Models.Any())
                    {
                        var existingKho = result.Models.FirstOrDefault(k => k.MaKho == _selectedKho.MaKho);

                        if (existingKho != null)
                        {
                            existingKho.TenKho = newKho.TenKho;
                            existingKho.MoTa = newKho.MoTa;
                            existingKho.MaToaNha = newKho.MaToaNha;

                            var updateResponse = await client.From<Kho>().Update(existingKho);

                            if (updateResponse.Models.Count > 0)
                            {
                                MessageBox.Show("Cập nhật kho thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Không thể cập nhật kho!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Kho không tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không có dữ liệu kho.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu kho: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
