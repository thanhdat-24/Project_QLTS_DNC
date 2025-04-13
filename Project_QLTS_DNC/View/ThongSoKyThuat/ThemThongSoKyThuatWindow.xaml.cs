using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Services.QLTaiSanService;

namespace Project_QLTS_DNC.View.ThongSoKyThuat
{
    public partial class ThemThongSoKyThuatWindow : Window
    {
        // Thông tin nhóm tài sản
        public NhomTaiSan NhomTaiSan { get; private set; }

        // Danh sách thông số kỹ thuật mới được tạo
        public ObservableCollection<ThongSoKyThuatDTO> DanhSachThongSoMoi { get; private set; }

        // Danh sách để hiển thị trên DataGrid
        private ObservableCollection<ThongSoKyThuatDTO> _danhSachPreview;

        // Thông tin thông số mới được tạo (giữ lại để tương thích với code cũ)
        public ThongSoKyThuatDTO ThongSoMoi { get; private set; }

        public ThemThongSoKyThuatWindow(NhomTaiSan nhomTaiSan)
        {
            InitializeComponent();

            // Lưu thông tin nhóm tài sản
            NhomTaiSan = nhomTaiSan;

            // Hiển thị tên nhóm tài sản
            txtTenNhom.Text = nhomTaiSan.TenNhom;

            // Khởi tạo danh sách
            DanhSachThongSoMoi = new ObservableCollection<ThongSoKyThuatDTO>();
            _danhSachPreview = new ObservableCollection<ThongSoKyThuatDTO>();

            // Gán nguồn dữ liệu cho DataGrid
            dgThongSoPreview.ItemsSource = _danhSachPreview;
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
        /// Thêm một thông số vào danh sách preview
        /// </summary>
        private void btnThemThongSo_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra dữ liệu nhập
            if (string.IsNullOrWhiteSpace(txtTenThongSo.Text))
            {
                txtErrorThongSo.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                txtErrorThongSo.Visibility = Visibility.Collapsed;
            }

            // Lấy thông tin từ các trường nhập liệu
            string tenThongSo = txtTenThongSo.Text.Trim();
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

            // Tạo đối tượng thông số mới
            var thongSo = new ThongSoKyThuatDTO
            {
                MaNhomTS = NhomTaiSan.MaNhomTS,
                TenThongSo = tenThongSo,
                ChiTietThongSo = chiTietThongSo,
                SoLuong = soLuong,
                BaoHanh = baoHanh
            };

            // Thêm vào danh sách preview
            _danhSachPreview.Add(thongSo);

            // Reset các trường nhập liệu
            txtTenThongSo.Text = string.Empty;
            txtSoLuong.Text = "1";
            txtBaoHanh.Text = "12";
            // Giữ nguyên trường Chi tiết để dễ nhập các thông số có cùng chi tiết

            // Focus vào trường Tên thông số để tiếp tục nhập
            txtTenThongSo.Focus();
        }

        /// <summary>
        /// Xóa một thông số khỏi danh sách preview
        /// </summary>
        private void XoaThongSoPreview_Click(object sender, RoutedEventArgs e)
        {
            // Lấy thông số được chọn từ button xóa
            Button btn = sender as Button;
            ThongSoKyThuatDTO thongSo = btn.DataContext as ThongSoKyThuatDTO;

            if (thongSo != null)
            {
                _danhSachPreview.Remove(thongSo);
            }
        }

        /// <summary>
        /// Xóa tất cả thông số khỏi danh sách preview
        /// </summary>
        private void btnXoaTatCa_Click(object sender, RoutedEventArgs e)
        {
            if (_danhSachPreview.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Bạn có chắc chắn muốn xóa tất cả thông số đã thêm?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _danhSachPreview.Clear();
                }
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Hủy
        /// </summary>
        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            // Nếu có thông số đã thêm, hiển thị thông báo xác nhận
            if (_danhSachPreview.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Bạn có các thông số chưa được lưu. Bạn có chắc chắn muốn hủy bỏ?",
                    "Xác nhận hủy",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            // Đóng cửa sổ và trả về kết quả false
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Lưu tất cả
        /// </summary>
        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra danh sách có thông số nào không
            if (_danhSachPreview.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất một thông số kỹ thuật.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Hiển thị thông báo đang xử lý
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.Cursor = System.Windows.Input.Cursors.Wait;
                }

                // Thêm từng thông số vào cơ sở dữ liệu
                DanhSachThongSoMoi.Clear();

                foreach (var thongSoDTO in _danhSachPreview)
                {
                    var thongSoDaThem = await ThongSoKyThuatService.ThemThongSoAsync(thongSoDTO);
                    DanhSachThongSoMoi.Add(thongSoDaThem);

                    // Gán thông số đầu tiên cho ThongSoMoi để tương thích với code cũ
                    if (ThongSoMoi == null)
                    {
                        ThongSoMoi = thongSoDaThem;
                    }
                }

                // Khôi phục con trỏ
                if (window != null)
                {
                    window.Cursor = null;
                }

                // Hiển thị thông báo thành công
                MessageBox.Show($"Đã thêm thành công {DanhSachThongSoMoi.Count} thông số kỹ thuật.",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

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

                MessageBox.Show($"Lỗi khi thêm thông số kỹ thuật: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}