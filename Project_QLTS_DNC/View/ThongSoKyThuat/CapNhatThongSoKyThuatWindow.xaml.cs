using System;
using System.Windows;
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

                // Tạo đối tượng thông số đã cập nhật
                var thongSoDTO = new ThongSoKyThuatDTO
                {
                    MaThongSo = _thongSoGoc.MaThongSo,
                    MaNhomTS = NhomTaiSan.MaNhomTS,
                    TenThongSo = txtTenThongSo.Text.Trim()
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

            return isValid;
        }
    }
}