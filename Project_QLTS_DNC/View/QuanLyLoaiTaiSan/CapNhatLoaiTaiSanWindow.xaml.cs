﻿using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Project_QLTS_DNC.Models.QLLoaiTS;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class CapNhatLoaiTaiSanWindow : Window
    {
        public LoaiTaiSan LoaiTaiSanSua { get; private set; }
        public LoaiTaiSan LoaiTaiSanCapNhat { get; private set; }

        public CapNhatLoaiTaiSanWindow(LoaiTaiSan loaiTaiSan)
        {
            InitializeComponent();

            // Khởi tạo đối tượng cần cập nhật
            LoaiTaiSanSua = loaiTaiSan;

            // Hiển thị thông tin lên form
            txtTenLoaiTaiSan.Text = loaiTaiSan.TenLoaiTaiSan;
            txtMoTa.Text = loaiTaiSan.MoTa;

            // Đặt tiêu đề cửa sổ
            Title = $"Cập nhật loại tài sản: {loaiTaiSan.TenLoaiTaiSan}";
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
            // Kiểm tra dữ liệu nhập vào
            if (string.IsNullOrWhiteSpace(txtTenLoaiTaiSan.Text))
            {
                MessageBox.Show("Vui lòng nhập tên loại tài sản!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTenLoaiTaiSan.Focus();
                return;
            }

            try
            {
                // Cập nhật thông tin mới vào đối tượng
                LoaiTaiSanCapNhat = new LoaiTaiSan
                {
                    MaLoaiTaiSan = LoaiTaiSanSua.MaLoaiTaiSan,
                    TenLoaiTaiSan = txtTenLoaiTaiSan.Text.Trim(),
                    MoTa = txtMoTa.Text?.Trim()
                };

                // Đặt DialogResult để trả về kết quả
                DialogResult = true;

                // Đóng cửa sổ
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}