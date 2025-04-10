using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Project_QLTS_DNC.Models;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class CapNhatNhomTaiSanWindow : Window
    {
        public NhomTaiSan NhomTaiSanSua { get; private set; }
        private ObservableCollection<LoaiTaiSan> _dsLoaiTaiSan;

        public CapNhatNhomTaiSanWindow(NhomTaiSan nhomTaiSan, ObservableCollection<LoaiTaiSan> dsLoaiTaiSan)
        {
            InitializeComponent();

            // Lưu trữ thông tin
            NhomTaiSanSua = nhomTaiSan;
            _dsLoaiTaiSan = dsLoaiTaiSan;

            // Thiết lập dữ liệu cho ComboBox Loại Tài Sản
            cboLoaiTaiSan.ItemsSource = _dsLoaiTaiSan;
            cboLoaiTaiSan.DisplayMemberPath = "TenLoaiTaiSan";
            cboLoaiTaiSan.SelectedValuePath = "MaLoaiTaiSan";

            // Hiển thị thông tin lên form
            cboLoaiTaiSan.SelectedValue = nhomTaiSan.MaLoaiTaiSan;
            txtTenNhom.Text = nhomTaiSan.TenNhom;
            txtMoTa.Text = nhomTaiSan.MoTa;

            // Đặt tiêu đề cửa sổ
            Title = $"Cập nhật nhóm tài sản: {nhomTaiSan.TenNhom}";
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
            if (cboLoaiTaiSan.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn loại tài sản!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                cboLoaiTaiSan.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTenNhom.Text))
            {
                MessageBox.Show("Vui lòng nhập tên nhóm tài sản!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTenNhom.Focus();
                return;
            }

            try
            {
                // Lấy mã loại tài sản đã chọn
                int maLoaiTaiSan = (int)cboLoaiTaiSan.SelectedValue;

                // Cập nhật thông tin cho nhóm tài sản
                NhomTaiSanSua.MaLoaiTaiSan = maLoaiTaiSan;
                NhomTaiSanSua.TenNhom = txtTenNhom.Text.Trim();
                NhomTaiSanSua.MoTa = txtMoTa.Text?.Trim();

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