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
                // Lấy dữ liệu từ Supabase
                var result = await _client.From<ToaNha>().Get();

                if (result.Models != null && result.Models.Any())
                {
                    // Gán trực tiếp danh sách ToaNha vào ComboBox
                    cboToaNha.ItemsSource = result.Models;
                    cboToaNha.DisplayMemberPath = "TenToaNha";    // Hiển thị tên tòa
                    cboToaNha.SelectedValuePath = "MaToaNha";     // Lưu mã tòa khi chọn
                    cboToaNha.SelectedIndex = -1;                 // Không chọn mặc định
                }
                else
                {
                    MessageBox.Show("Không có dữ liệu tòa nhà.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
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
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin.");
                return;
            }

            // Nếu đang sửa kho, giữ nguyên mã kho cũ
            int maKhoMoi = _selectedKho != null ? _selectedKho.MaKho : await SinhMaKhoAsync();

            // Lấy mã tòa nhà từ Tag của ComboBoxItem
            if (cboToaNha.SelectedItem is ComboBoxItem selectedItem &&
                int.TryParse(selectedItem.Tag?.ToString(), out int maToaNha))
            {
                var newKho = new Kho
                {
                    MaKho = maKhoMoi, // Giữ mã kho cũ khi sửa
                    TenKho = txtTenKho.Text,
                    MoTa = txtMoTa.Text,
                    MaToaNha = maToaNha // Gán mã tòa nhà vào thuộc tính MaToaNha
                };

                try
                {
                    var client = await SupabaseService.GetClientAsync();

                    if (_selectedKho == null)
                    {
                        // Nếu là thêm kho mới, sử dụng Insert
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
                    else
                    {
                        // Nếu là sửa kho cũ, đầu tiên cần lấy toàn bộ danh sách kho
                        var result = await client.From<Kho>().Get();

                        if (result.Models.Any())
                        {
                            // Tìm kho cần sửa trong bộ nhớ
                            var existingKho = result.Models.FirstOrDefault(k => k.MaKho == _selectedKho.MaKho);

                            if (existingKho != null)
                            {
                                // Cập nhật các trường dữ liệu
                                existingKho.TenKho = newKho.TenKho;
                                existingKho.MoTa = newKho.MoTa;
                                existingKho.MaToaNha = newKho.MaToaNha;

                                // Cập nhật kho vào Supabase
                                var updateResponse = await client.From<Kho>().Update(existingKho);

                                if (updateResponse.Models.Count > 0)
                                {
                                    MessageBox.Show("Cập nhật kho thành công!");
                                    this.Close();
                                }
                                else
                                {
                                    MessageBox.Show("Không thể cập nhật kho!");
                                }
                            }
                            else
                            {
                                MessageBox.Show("Kho không tồn tại.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Không có dữ liệu kho.");
                        }
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
