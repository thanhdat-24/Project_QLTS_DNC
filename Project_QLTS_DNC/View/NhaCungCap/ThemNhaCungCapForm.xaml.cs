using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Project_QLTS_DNC.View.NhaCungCap
{
    public partial class ThemNhaCungCapForm : Window
    {
        public NhaCungCap NhaCungCapMoi { get; set; } // Dữ liệu sẽ được trả về khi nhấn Lưu
        private static int nccCounter = 1;  // Đếm số lượng nhà cung cấp để tạo mã NCC tự động                
        public ThemNhaCungCapForm()
        {
            InitializeComponent();

        }


        private string SinhMaNCC()
        {
            return nccCounter++.ToString("D3");  // Sinh mã theo định dạng 001, 002, 003, ...
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Reset the error messages and highlight colors before validating
            ResetErrorMessages();

            string soDienThoai = txtSoDienThoai.Text.Trim();
            string email = txtEmail.Text.Trim();
            string tenNhaCungCap = txtTenNhaCungCap.Text.Trim();
            string diaChi = txtDiaChi.Text.Trim();

            bool isValid = true;

            // Kiểm tra các trường bắt buộc không được để trống (ngoại trừ mô tả)
            if (string.IsNullOrEmpty(tenNhaCungCap))
            {
                isValid = false;
                ShowError(txtTenNhaCungCap, "Tên Nhà Cung Cấp không được để trống.");
            }

            if (string.IsNullOrEmpty(diaChi))
            {
                isValid = false;
                ShowError(txtDiaChi, "Địa Chỉ không được để trống.");
            }

            if (string.IsNullOrEmpty(soDienThoai))
            {
                isValid = false;
                ShowError(txtSoDienThoai, "Số Điện Thoại không được để trống.");
            }

            if (string.IsNullOrEmpty(email))
            {
                isValid = false;
                ShowError(txtEmail, "Email không được để trống.");
            }

            // Kiểm tra số điện thoại: chỉ số và tối đa 11 ký tự
            if (!Regex.IsMatch(soDienThoai, @"^\d{1,11}$"))
            {
                isValid = false;
                ShowError(txtSoDienThoai, "Số điện thoại chỉ được nhập số và tối đa 11 chữ số.");
            }

            // Kiểm tra định dạng email
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                isValid = false;
                ShowError(txtEmail, "Email không đúng định dạng.");
            }

            // Nếu mọi thứ hợp lệ, tạo đối tượng NhaCungCap mới
            if (isValid)
            {
                NhaCungCapMoi = new NhaCungCap
                {
                    TenNhaCungCap = tenNhaCungCap,
                    DiaChi = diaChi,
                    SoDienThoai = soDienThoai,
                    Email = email,
                    MoTa = txtMoTa.Text,  // Mô Tả có thể để trống
                    MaNCC = SinhMaNCC()  // Sinh mã NCC tự động
                };

                // Đóng cửa sổ và trả kết quả về
                DialogResult = true;
                this.Close();
            }
        }

        // Hàm hiển thị thông báo lỗi trên các TextBox
        private void ShowError(TextBox textBox, string errorMessage)
        {
            textBox.ToolTip = errorMessage;  // Hiển thị thông báo lỗi dưới dạng tooltip
            textBox.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightPink);  // Đổi màu nền thành đỏ nhạt để chỉ ra lỗi
        }

        // Hàm để reset lỗi trước khi kiểm tra lại
        private void ResetErrorMessages()
        {
            txtTenNhaCungCap.Background = System.Windows.Media.Brushes.White;
            txtDiaChi.Background = System.Windows.Media.Brushes.White;
            txtSoDienThoai.Background = System.Windows.Media.Brushes.White;
            txtEmail.Background = System.Windows.Media.Brushes.White;

            txtTenNhaCungCap.ToolTip = null;
            txtDiaChi.ToolTip = null;
            txtSoDienThoai.ToolTip = null;
            txtEmail.ToolTip = null;
        }


        // Ngăn không cho người dùng nhập ký tự không phải số vào textbox số điện thoại
        private void txtSoDienThoai_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d$");
        }
    }
}
