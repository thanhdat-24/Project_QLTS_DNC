using Project_QLTS_DNC.Helpers;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Services;
using Supabase;
using System;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace Project_QLTS_DNC.View.QuanLyKho
{
    public partial class QuanLyKhoView : UserControl
    {
        private Supabase.Client _client;
        private Kho _selectedKho;
        private ObservableCollection<Kho> _listKho;
        private ObservableCollection<Kho> _allKho = new();



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
                if (!QuyenNguoiDungHelper.HasPermission("btnDanhSachKho", "xem"))
                {
                    MessageBox.Show("Bạn không có quyền xem danh sách kho!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var result = await _client.From<Kho>().Get();

                if (result.Models.Any())
                {
                    _allKho = new ObservableCollection<Kho>(result.Models);
                    dgKho.ItemsSource = _allKho;
                }
                else
                {
                    dgKho.ItemsSource = null;
                    MessageBox.Show("Không có dữ liệu kho.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu kho: {ex.Message}");
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                dgKho.ItemsSource = _allKho;
                return;
            }

            var filteredList = _allKho
                .Where(k => !string.IsNullOrEmpty(k.TenKho) && k.TenKho.ToLower().Contains(keyword))
                .ToList();

            dgKho.ItemsSource = filteredList;
        }

        private void btnViewDetail_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnDanhSachKho", "them"))
            {
                MessageBox.Show("Bạn không có quyền thêm kho!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ThemKho themKhoForm = new ThemKho();
            themKhoForm.ShowDialog(); // Mở form Thêm Kho
        }
        // Phương thức để mở form chỉnh sửa kho
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnDanhSachKho", "sua"))
            {
                MessageBox.Show("Bạn không có quyền sửa danh sách kho!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Lấy kho được chọn từ DataContext
            Button button = sender as Button;
            Kho selectedKho = button.DataContext as Kho;

            if (selectedKho != null)
            {
                // Mở form chỉnh sửa kho và truyền dữ liệu cần chỉnh sửa
                ThemKho editKhoForm = new ThemKho(selectedKho);  // Giả sử EditKho là form để chỉnh sửa kho
                editKhoForm.ShowDialog();  // Hiển thị form chỉnh sửa
            }
        }
        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnDanhSachKho", "xoa"))
            {
                MessageBox.Show("Bạn không có quyền xóa danh sách kho!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Lấy kho được chọn từ DataContext của nút
            Button button = sender as Button;
            Kho selectedKho = button.DataContext as Kho;

            if (selectedKho != null)
            {
                // Xác nhận trước khi xóa
                MessageBoxResult result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa kho '{selectedKho.TenKho}'?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var client = await SupabaseService.GetClientAsync();

                        // Lấy tất cả danh sách kho từ Supabase
                        var resultKho = await client.From<Kho>().Get();

                        // Tìm kho cần xóa trong danh sách kho
                        var khoToDelete = resultKho.Models.FirstOrDefault(k => k.MaKho == selectedKho.MaKho);

                        if (khoToDelete != null)
                        {
                            // Xóa kho khỏi Supabase, không sử dụng `Where`, `Eq`, `Match`, `Filter`
                            var deleteResponse = await client.From<Kho>().Delete(khoToDelete);

                            
                        }
                        else
                        {
                            MessageBox.Show("Kho không tồn tại.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa kho: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Không có kho được chọn để xóa.");
            }
        }


    }
}

