using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Supabase;
using Supabase.Postgrest;
using Project_QLTS_DNC.Models.KiemKe;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Helpers;

namespace Project_QLTS_DNC.View.KiemKe
{
    public partial class DotKiemKeView : UserControl
    {
        // Khai báo đối tượng Supabase client.
        private Supabase.Client _client;

        public DotKiemKeView()
        {
            InitializeComponent();
            // Đăng ký sự kiện Loaded của UserControl để khởi tạo và load dữ liệu.
            Loaded += DotKiemKeView_Loaded;
        }

        private async void DotKiemKeView_Loaded(object sender, RoutedEventArgs e)
        {
            // Khởi tạo kết nối Supabase
            await InitializeSupabaseAsync();
            // Load dữ liệu từ bảng dotkiemke và gán cho DataGrid.
            await LoadDotKiemKeAsync();
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

        private async Task LoadDotKiemKeAsync()
        {
            try
            {
                if (!QuyenNguoiDungHelper.HasPermission("btnBaoCaoKiemKe", "xem"))
                {
                    MessageBox.Show("Bạn không có quyền xem kiểm kê!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                // Sử dụng phương thức From<T>() để truy xuất dữ liệu từ bảng "dotkiemke"
                var response = await _client.From<DotKiemKe>().Get();

                if (response.Models != null)
                {
                    // Gán dữ liệu cho DataGrid đã đặt tên là dgDotKiemKe trong XAML
                    dgDotKiemKe.ItemsSource = response.Models;
                }
                else
                {
                    MessageBox.Show("Không có dữ liệu từ bảng dotkiemke.");
                }
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi nếu có sự cố trong quá trình truy xuất dữ liệu.
                MessageBox.Show("Có lỗi khi tải dữ liệu: " + ex.Message);
            }
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnBaoCaoKiemKe", "them"))
            {
                MessageBox.Show("Bạn không có quyền thêm kiểm kê!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var inputWindow = new DotKiemKeInput();
            inputWindow.ShowDialog();

            // Sau khi đóng form thêm mới, load lại danh sách nếu cần
            _ = LoadDotKiemKeAsync(); // Gọi lại hàm load danh sách
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnBaoCaoKiemKe", "sua"))
            {
                MessageBox.Show("Bạn không có quyền sửa kiểm kê!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Lấy đợt kiểm kê được chọn từ DataContext
            Button button = sender as Button;
            DotKiemKe selectedDot = button?.DataContext as DotKiemKe;

            if (selectedDot != null)
            {
                // Mở form chỉnh sửa và truyền dữ liệu cần sửa
                var editWindow = new DotKiemKeInput(selectedDot);
                editWindow.ShowDialog();

                // Sau khi đóng form, load lại danh sách
                _ = LoadDotKiemKeAsync();
            }
        }


        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnBaoCaoKiemKe", "xoa"))
            {
                MessageBox.Show("Bạn không có quyền xóa kiểm kê!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Lấy đợt kiểm kê được chọn từ DataContext của nút
            Button button = sender as Button;
            DotKiemKe selectedDot = button?.DataContext as DotKiemKe;

            if (selectedDot != null)
            {
                // Hộp thoại xác nhận
                MessageBoxResult result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa đợt kiểm kê '{selectedDot.TenDot}'?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var client = await SupabaseService.GetClientAsync();

                        // Lấy toàn bộ danh sách đợt kiểm kê từ Supabase
                        var resultDot = await client.From<DotKiemKe>().Get();

                        // Tìm đợt cần xóa
                        var dotToDelete = resultDot.Models.FirstOrDefault(d => d.MaDotKiemKe == selectedDot.MaDotKiemKe);

                        if (dotToDelete != null)
                        {
                            // Gọi xóa
                            var deleteResponse = await client.From<DotKiemKe>().Delete(dotToDelete);

                            if (deleteResponse.Models.Count > 0)
                            {
                                MessageBox.Show("Đã xóa đợt kiểm kê thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                                // Load lại danh sách
                                await LoadDotKiemKeAsync();
                            }
                            else
                            {
                                MessageBox.Show("Không thể xóa đợt kiểm kê!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Đợt kiểm kê không tồn tại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa đợt kiểm kê: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Không có đợt kiểm kê được chọn để xóa.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


    }
}
