﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Linq;
using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models.QLLoaiTS;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Services.QLTaiSanService;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class ThemNhomTaiSanWindow : Window
    {
        public NhomTaiSan NhomTaiSanMoi { get; private set; }
        private ObservableCollection<LoaiTaiSan> _dsLoaiTaiSan;
        private bool _isProcessing = false;

        public ThemNhomTaiSanWindow(ObservableCollection<LoaiTaiSan> dsLoaiTaiSan)
        {
            InitializeComponent();

            _dsLoaiTaiSan = dsLoaiTaiSan;

            // Ẩn cảnh báo khi khởi tạo
            borderCanhBao.Visibility = Visibility.Collapsed;

            // Gán danh sách loại tài sản cho ComboBox
            cboLoaiTaiSan.ItemsSource = _dsLoaiTaiSan;
            cboLoaiTaiSan.DisplayMemberPath = "TenLoaiTaiSan";
            cboLoaiTaiSan.SelectedValuePath = "MaLoaiTaiSan";

            // Chọn loại tài sản đầu tiên nếu có
            if (_dsLoaiTaiSan.Count > 0)
            {
                cboLoaiTaiSan.SelectedIndex = 0;
            }

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

        // Sự kiện xử lý khi thay đổi loại tài sản được chọn
        private void cboLoaiTaiSan_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var loaiTaiSanDuocChon = cboLoaiTaiSan.SelectedItem as LoaiTaiSan;

            if (loaiTaiSanDuocChon != null)
            {
                // Kiểm tra thuộc tính QuanLyRieng
                if (loaiTaiSanDuocChon.QuanLyRieng)
                {
                    // Hiển thị cảnh báo nếu loại tài sản có QuanLyRieng = true
                    borderCanhBao.Visibility = Visibility.Visible;
                }
                else
                {
                    // Ẩn cảnh báo
                    borderCanhBao.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                // Ẩn cảnh báo nếu không có loại tài sản nào được chọn
                borderCanhBao.Visibility = Visibility.Collapsed;
            }
        }

        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Tránh người dùng nhấn nút nhiều lần
            if (_isProcessing)
                return;

            try
            {
                // Kiểm tra dữ liệu nhập
                if (cboLoaiTaiSan.SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn loại tài sản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    cboLoaiTaiSan.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtTenNhom.Text))
                {
                    MessageBox.Show("Vui lòng nhập tên nhóm tài sản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtTenNhom.Focus();
                    return;
                }

                _isProcessing = true;

                // Lấy loại tài sản đã chọn
                var loaiTaiSan = cboLoaiTaiSan.SelectedItem as LoaiTaiSan;

                // Tạo đối tượng NhomTaiSan mới
                NhomTaiSanMoi = new NhomTaiSan
                {
                    MaLoaiTaiSan = loaiTaiSan.MaLoaiTaiSan,
                    TenNhom = txtTenNhom.Text.Trim(),
                    MoTa = txtMoTa.Text?.Trim()
                };

                // Gọi service để lưu vào database
                NhomTaiSanMoi = await NhomTaiSanService.ThemNhomTaiSanAsync(NhomTaiSanMoi);

                // Hiển thị thông báo thành công
                MessageBox.Show($"Thêm mới nhóm tài sản '{NhomTaiSanMoi.TenNhom}' thành công!",
                                "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

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
            }
        }
    }
}