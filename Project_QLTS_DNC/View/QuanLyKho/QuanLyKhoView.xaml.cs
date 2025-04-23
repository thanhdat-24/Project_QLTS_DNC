using Project_QLTS_DNC.Helpers;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.Kho;
using Project_QLTS_DNC.Models.ToaNha;
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
        private ObservableCollection<KhoViewModel> _allKho = new();




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
                var toaNhaResult = await _client.From<ToaNha>().Get();

                var toaNhaDict = toaNhaResult.Models.ToDictionary(t => t.MaToaNha, t => t.TenToaNha);

                // Chuyển sang ViewModel để hiển thị tên tòa nhà
                var viewModels = result.Models.Select(k => new KhoViewModel
                {
                    MaKho = k.MaKho,
                    TenKho = k.TenKho,
                    MoTa = k.MoTa,
                    MaToaNha = k.MaToaNha,
                    TenToaNha = toaNhaDict.TryGetValue(k.MaToaNha, out var tenToaNha) ? tenToaNha : "---"
                }).ToList();

                _allKho = new ObservableCollection<KhoViewModel>(viewModels);
                dgKho.ItemsSource = _allKho;
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

        private async void btnViewDetail_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnDanhSachKho", "them"))
            {
                MessageBox.Show("Bạn không có quyền thêm kho!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ThemKho themKhoForm = new ThemKho();
            themKhoForm.ShowDialog(); 
            await LoadKhoDataAsync();
        }
        // Phương thức để mở form chỉnh sửa kho
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnDanhSachKho", "sua"))
            {
                MessageBox.Show("Bạn không có quyền sửa danh sách kho!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Button button = sender as Button;
            KhoViewModel selectedKhoVM = button?.DataContext as KhoViewModel;

            if (selectedKhoVM != null)
            {
             
                Kho khoToEdit = new Kho
                {
                    MaKho = selectedKhoVM.MaKho,
                    TenKho = selectedKhoVM.TenKho,
                    MoTa = selectedKhoVM.MoTa,
                    MaToaNha = selectedKhoVM.MaToaNha
                };

                ThemKho editKhoForm = new ThemKho(khoToEdit);  // Form chỉnh sửa kho
                editKhoForm.ShowDialog();

                
                _ = LoadKhoDataAsync(); 
            }
            else
            {
                MessageBox.Show("Không có kho được chọn để chỉnh sửa.");
            }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnDanhSachKho", "xoa"))
            {
                MessageBox.Show("Bạn không có quyền xóa danh sách kho!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Button button = sender as Button;
            KhoViewModel selectedKho = button?.DataContext as KhoViewModel;

            if (selectedKho != null)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa kho '{selectedKho.TenKho}'?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var client = await SupabaseService.GetClientAsync(); // 🔧 khai báo đúng client

                        // Lấy lại kho theo mã từ Supabase để thực hiện xóa
                        var khoResult = await client.From<Kho>().Get();
                        var khoToDelete = khoResult.Models.FirstOrDefault(k => k.MaKho == selectedKho.MaKho); // 🔧 khai báo khoToDelete

                        if (khoToDelete != null)
                        {
                            await client.From<Kho>().Delete(khoToDelete); // ✅ xóa
                            await LoadKhoDataAsync(); // 🔄 tải lại sau khi xóa

                            MessageBox.Show("Đã xóa kho thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy kho cần xóa.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa kho: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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

