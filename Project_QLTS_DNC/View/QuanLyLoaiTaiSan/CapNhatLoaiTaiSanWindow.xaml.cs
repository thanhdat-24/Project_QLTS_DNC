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
        }
    }
}