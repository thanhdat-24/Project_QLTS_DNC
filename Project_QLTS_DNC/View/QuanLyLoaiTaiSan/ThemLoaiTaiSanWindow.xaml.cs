using System;
using System.Windows;
using System.Windows.Input;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Services;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class ThemLoaiTaiSanWindow : Window
    {
        public LoaiTaiSan LoaiTaiSanMoi { get; private set; }
        private bool _isProcessing = false;

        public ThemLoaiTaiSanWindow()
        {
            InitializeComponent();

            // Gán sự kiện cho nút đóng
            btnDong.Click += btnDong_Click;
        }

        // Cho phép di chuyển cửa sổ
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        // Phương thức xử lý sự kiện khi nhấn nút Hủy
        private void btnDong_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Tránh người dùng nhấn nút nhiều lần
            if (_isProcessing)
                return;

            try
            {
                // Kiểm tra dữ liệu nhập
                if (string.IsNullOrWhiteSpace(txtTenLoaiTaiSan.Text))
                {
                    MessageBox.Show("Vui lòng nhập tên loại tài sản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtTenLoaiTaiSan.Focus();
                    return;
                }

                _isProcessing = true;

                // Hiển thị trạng thái đang xử lý nếu cần
                // loadingIndicator.Visibility = Visibility.Visible;

                // Tạo đối tượng LoaiTaiSan mới
                LoaiTaiSanMoi = new LoaiTaiSan
                {
                    TenLoaiTaiSan = txtTenLoaiTaiSan.Text.Trim(),
                    MoTa = txtMoTa.Text.Trim()
                };

                // Gọi service để lưu vào database
                LoaiTaiSanMoi = await LoaiTaiSanService.ThemLoaiTaiSanAsync(LoaiTaiSanMoi);

                // Đánh dấu thành công và đóng cửa sổ
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
            }
            finally
            {
                _isProcessing = false;
                // loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
    }
}