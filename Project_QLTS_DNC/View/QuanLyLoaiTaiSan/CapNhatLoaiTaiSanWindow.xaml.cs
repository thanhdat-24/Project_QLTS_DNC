using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Project_QLTS_DNC.Models;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class CapNhatLoaiTaiSanWindow : Window
    {
        public LoaiTaiSan LoaiTaiSanSua { get; private set; }

        public CapNhatLoaiTaiSanWindow(LoaiTaiSan loaiTaiSan)
        {
            InitializeComponent();

            // Điền thông tin vào các trường
            txtMaLoaiTaiSan.Text = loaiTaiSan.MaLoaiTaiSan.ToString();
            txtTenLoaiTaiSan.Text = loaiTaiSan.TenLoaiTaiSan;
            txtMoTa.Text = loaiTaiSan.MoTa;

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

            // Tạo đối tượng LoaiTaiSan được sửa
            LoaiTaiSanSua = new LoaiTaiSan
            {
                MaLoaiTaiSan = int.Parse(txtMaLoaiTaiSan.Text),
                TenLoaiTaiSan = txtTenLoaiTaiSan.Text.Trim(),
                MoTa = txtMoTa.Text.Trim()
            };

            // Đóng window và trả về DialogResult là True
            DialogResult = true;
            Close();
        }
    }
}