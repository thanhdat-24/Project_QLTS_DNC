using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Services.QLTaiSanService;

namespace Project_QLTS_DNC.View.ThongSoKyThuat
{
    public partial class CapNhatThongSoKyThuatWindow : Window
    {
        // Thông tin nhóm tài sản
        public NhomTaiSan NhomTaiSan { get; private set; }

        // Thông tin thông số cần cập nhật
        private ThongSoKyThuatDTO _thongSoGoc;

        // Thông tin thông số đã cập nhật
        public ThongSoKyThuatDTO ThongSoDaCapNhat { get; private set; }

        public CapNhatThongSoKyThuatWindow(ThongSoKyThuatDTO thongSo, NhomTaiSan nhomTaiSan)
        {
            InitializeComponent();

            // Lưu thông tin nhóm tài sản và thông số gốc
            NhomTaiSan = nhomTaiSan;
            _thongSoGoc = thongSo;

            // Hiển thị thông tin
            txtTenNhom.Text = nhomTaiSan.TenNhom;
            txtMaThongSo.Text = thongSo.MaThongSo.ToString();
            txtTenThongSo.Text = thongSo.TenThongSo;
            txtChiTietThongSo.Text = thongSo.ChiTietThongSo;
            txtSoLuong.Text = thongSo.SoLuong?.ToString() ?? "";
            txtBaoHanh.Text = thongSo.BaoHanh?.ToString() ?? "";
        }

        /// <summary>
        /// Xác thực nhập liệu chỉ cho phép số
        /// </summary>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Hủy
        /// </summary>
        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            // Đóng cửa sổ và trả về kết quả false
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Lưu
        /// </summary>
        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra dữ liệu nhập
            if (!KiemTraDuLieu())
                return;

            try
            {
                // Hiển thị thông báo đang xử lý
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.Cursor = System.Windows.Input.Cursors.Wait;
                }

                // Lấy thông tin từ các trường nhập liệu
                string chiTietThongSo = txtChiTietThongSo.Text?.Trim();

                int? soLuong = null;
                if (!string.IsNullOrWhiteSpace(txtSoLuong.Text) && int.TryParse(txtSoLuong.Text, out int sl))
                {
                    soLuong = sl;
                }

                int? baoHanh = null;
                if (!string.IsNullOrWhiteSpace(txtBaoHanh.Text) && int.TryParse(txtBaoHanh.Text, out int bh))
                {
                    baoHanh = bh;
                }

                // Tạo đối tượng thông số đã cập nhật
                var thongSoDTO = new ThongSoKyThuatDTO
                {
                    MaThongSo = _thongSoGoc.MaThongSo,
                    MaNhomTS = NhomTaiSan.MaNhomTS,
                    TenThongSo = txtTenThongSo.Text.Trim(),
                    ChiTietThongSo = chiTietThongSo,
                    SoLuong = soLuong,
                    BaoHanh = baoHanh
                };

                // Cập nhật vào cơ sở dữ liệu qua service
                ThongSoDaCapNhat = await ThongSoKyThuatService.CapNhatThongSoAsync(thongSoDTO);

                // Khôi phục con trỏ
                if (window != null)
                {
                    window.Cursor = null;
                }

                // Đóng cửa sổ và trả về kết quả true
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                // Khôi phục con trỏ
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.Cursor = null;
                }

                MessageBox.Show($"Lỗi khi cập nhật thông số kỹ thuật: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Kiểm tra dữ liệu nhập
        /// </summary>
        /// <returns>True nếu dữ liệu hợp lệ, False nếu không hợp lệ</returns>
        private bool KiemTraDuLieu()
        {
            bool isValid = true;

            // Kiểm tra tên thông số
            if (string.IsNullOrWhiteSpace(txtTenThongSo.Text))
            {
                txtErrorTenThongSo.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                txtErrorTenThongSo.Visibility = Visibility.Collapsed;
            }

            // Kiểm tra số lượng
            if (!string.IsNullOrWhiteSpace(txtSoLuong.Text))
            {
                if (!int.TryParse(txtSoLuong.Text, out int sl) || sl < 0)
                {
                    txtErrorSoLuong.Visibility = Visibility.Visible;
                    isValid = false;
                }
                else
                {
                    txtErrorSoLuong.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                txtErrorSoLuong.Visibility = Visibility.Collapsed;
            }

            // Kiểm tra bảo hành
            if (!string.IsNullOrWhiteSpace(txtBaoHanh.Text))
            {
                if (!int.TryParse(txtBaoHanh.Text, out int bh) || bh < 0)
                {
                    txtErrorBaoHanh.Visibility = Visibility.Visible;
                    isValid = false;
                }
                else
                {
                    txtErrorBaoHanh.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                txtErrorBaoHanh.Visibility = Visibility.Collapsed;
            }

            return isValid;
        }
    }
}