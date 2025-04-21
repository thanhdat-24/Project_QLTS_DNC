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
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.ViewModel.TaiKhoan;
using Project_QLTS_DNC.DTOs;
namespace Project_QLTS_DNC.View.TaiKhoan
{
    /// <summary>
    /// Interaction logic for DanhSachTaiKhoanForm.xaml
    /// </summary>
    public partial class DanhSachTaiKhoanForm : UserControl
    {
        public DanhSachTaiKhoanForm()
        {
            InitializeComponent();
            Loaded += DanhSachTaiKhoanForm_Loaded;

        }

        private async void DanhSachTaiKhoanForm_Loaded(object sender, RoutedEventArgs e)
        {
            var client = await SupabaseService.GetClientAsync();
            var taiKhoanService = new TaiKhoanService();
            DataContext = new DanhSachTaiKhoanViewModel(taiKhoanService);
        }

        private void btnTaoTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            var taoTaiKhoanForm = new ThemTaiKhoanForm();
            taoTaiKhoanForm.ShowDialog();
        }

        private void btnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                var selectedTaiKhoan = dgDanhSachTaiKhoan.SelectedItem as TaiKhoanDTO; // Thay dgTaiKhoan bằng tên thực của DataGrid

                if (selectedTaiKhoan == null)
                {
                    MessageBox.Show("Vui lòng chọn tài khoản cần chỉnh sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                
                var editForm = new ThemTaiKhoanForm();

                
                editForm.LoadTaiKhoanData(selectedTaiKhoan);

                
                editForm.Title = "Chỉnh sửa tài khoản";

                
                editForm.txtTenTaiKhoan.IsEnabled = false;

               
                if (editForm.ShowDialog() == true)
                {
                    
                    LoadDanhSachTaiKhoan(); 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở form chỉnh sửa: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadDanhSachTaiKhoan()
        {
            try
            {
                // Lấy ViewModel từ DataContext
                if (DataContext is DanhSachTaiKhoanViewModel viewModel)
                {
                    // Gọi phương thức load dữ liệu từ ViewModel
                    await viewModel.LoadDanhSachTaiKhoanAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnKhoaTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Lấy tài khoản được chọn từ DataGrid
                var selectedTaiKhoan = dgDanhSachTaiKhoan.SelectedItem as TaiKhoanDTO;

                if (selectedTaiKhoan == null)
                {
                    MessageBox.Show("Vui lòng chọn tài khoản cần khóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Hiển thị hộp thoại xác nhận
                var result = MessageBox.Show($"Bạn có chắc chắn muốn khóa tài khoản '{selectedTaiKhoan.TenTaiKhoan}' không?",
                    "Xác nhận khóa tài khoản",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Khởi tạo service nếu chưa có
                    var taiKhoanService = new TaiKhoanService();

                    // Gọi phương thức khóa tài khoản (đã thêm vào TaiKhoanService)
                    var khoaTaiKhoanResult = await taiKhoanService.KhoaTaiKhoanAsync(selectedTaiKhoan.MaTk);

                    if (khoaTaiKhoanResult)
                    {
                        MessageBox.Show("Tài khoản đã được khóa thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Load lại danh sách tài khoản để cập nhật UI
                        if (DataContext is DanhSachTaiKhoanViewModel viewModel)
                        {
                            await viewModel.LoadDanhSachTaiKhoanAsync();

                            // Thêm lệnh này để buộc refresh UI
                            dgDanhSachTaiKhoan.Items.Refresh();
                        }
                        else
                        {
                            await LoadDanhSachTaiKhoan();
                            dgDanhSachTaiKhoan.Items.Refresh();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi khóa tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnToggleLock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Lấy tài khoản được chọn từ DataGrid
                var selectedTaiKhoan = ((FrameworkElement)sender).DataContext as TaiKhoanDTO;

                if (selectedTaiKhoan == null)
                {
                    MessageBox.Show("Vui lòng chọn tài khoản cần thay đổi trạng thái.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Xác định hành động dựa trên trạng thái hiện tại
                bool isLocking = selectedTaiKhoan.TrangThai; // true = đang hoạt động, sẽ khóa
                string action = isLocking ? "khóa" : "mở khóa";

                // Hiển thị hộp thoại xác nhận
                var result = MessageBox.Show($"Bạn có chắc chắn muốn {action} tài khoản '{selectedTaiKhoan.TenTaiKhoan}' không?",
                    $"Xác nhận {action} tài khoản",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Khởi tạo service nếu chưa có
                    var taiKhoanService = new TaiKhoanService();

                    bool operationResult;

                    // Gọi phương thức tương ứng dựa trên trạng thái hiện tại
                    if (isLocking)
                    {
                        // Đang hoạt động -> Khóa lại
                        operationResult = await taiKhoanService.KhoaTaiKhoanAsync(selectedTaiKhoan.MaTk);
                    }
                    else
                    {
                        // Đang bị khóa -> Mở khóa
                        operationResult = await taiKhoanService.MoKhoaTaiKhoanAsync(selectedTaiKhoan.MaTk);
                    }

                    if (operationResult)
                    {
                        MessageBox.Show($"Tài khoản đã được {action} thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Load lại danh sách tài khoản để cập nhật UI
                        if (DataContext is DanhSachTaiKhoanViewModel viewModel)
                        {
                            await viewModel.LoadDanhSachTaiKhoanAsync();
                            dgDanhSachTaiKhoan.Items.Refresh();
                        }
                        else
                        {
                            await LoadDanhSachTaiKhoan();
                            dgDanhSachTaiKhoan.Items.Refresh();
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Không thể {action} tài khoản.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thay đổi trạng thái tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DanhSachTaiKhoanViewModel viewModel)
            {
                viewModel.TimKiem();
            }
        }

        private void txtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DanhSachTaiKhoanViewModel viewModel)
            {
                
                viewModel.TuKhoa = string.Empty;

                
                viewModel.SelectedLoaiTaiKhoan = viewModel.DanhSachLoaiTaiKhoan.FirstOrDefault(x => x.MaLoaiTk == 0);

                
                viewModel.LoadDanhSachTaiKhoanAsync();
            }
        }
    }
}
