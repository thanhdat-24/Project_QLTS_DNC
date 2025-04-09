using System;
using System.Windows;
using System.Windows.Input;
using Project_QLTS_DNC.Models;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class ThemLoaiTaiSanWindow : Window
    {
        public LoaiTaiSan LoaiTaiSanMoi { get; private set; }

        public ThemLoaiTaiSanWindow()
        {
            InitializeComponent();

            // Đóng window khi nhấn nút đóng
            btnDong.Click += (s, e) => Close();

            // Cho phép di chuyển cửa sổ
            MouseLeftButtonDown += Window_MouseLeftButtonDown;
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
            Close();
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra và xác nhận các trường nhập liệu
            if (string.IsNullOrWhiteSpace(txtTenLoaiTaiSan.Text))
            {
                MessageBox.Show("Vui lòng nhập tên loại tài sản", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Tạo đối tượng LoaiTaiSan mới
            LoaiTaiSanMoi = new LoaiTaiSan
            {
                TenLoaiTaiSan = txtTenLoaiTaiSan.Text.Trim(),
                MoTa = txtMoTa.Text.Trim()
            };

            // Đóng window và trả về DialogResult là True
            DialogResult = true;
            Close();
        }
    }
}