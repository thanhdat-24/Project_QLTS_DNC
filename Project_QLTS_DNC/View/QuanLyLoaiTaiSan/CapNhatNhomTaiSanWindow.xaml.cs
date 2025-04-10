using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Project_QLTS_DNC.Services;
using System.Linq;
using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models.QLLoaiTS;
using Project_QLTS_DNC.Models.QLNhomTS;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class CapNhatNhomTaiSanWindow : Window
    {
        public NhomTaiSan NhomTaiSanSua { get; private set; }
        private ObservableCollection<LoaiTaiSan> _dsLoaiTaiSan;
        private bool _isProcessing = false;
        private int _maNhomTSGoc;

        public CapNhatNhomTaiSanWindow(NhomTaiSanDTO nhomTaiSanDTO, ObservableCollection<LoaiTaiSan> dsLoaiTaiSan)
        {
            InitializeComponent();

            if (nhomTaiSanDTO == null)
            {
                throw new ArgumentNullException(nameof(nhomTaiSanDTO), "Nhóm tài sản không được null");
            }

            // Khởi tạo NhomTaiSanSua từ DTO
            NhomTaiSanSua = nhomTaiSanDTO.ToEntity();
            _maNhomTSGoc = nhomTaiSanDTO.MaNhomTS;
            _dsLoaiTaiSan = dsLoaiTaiSan;

            // Gán danh sách loại tài sản cho ComboBox
            cboLoaiTaiSan.ItemsSource = _dsLoaiTaiSan;
            cboLoaiTaiSan.DisplayMemberPath = "TenLoaiTaiSan";
            cboLoaiTaiSan.SelectedValuePath = "MaLoaiTaiSan";

            // Chọn loại tài sản tương ứng
            cboLoaiTaiSan.SelectedValue = nhomTaiSanDTO.MaLoaiTaiSan;

            // Hiển thị thông tin nhóm tài sản cần sửa
            txtTenNhom.Text = nhomTaiSanDTO.TenNhom;
            txtMoTa.Text = nhomTaiSanDTO.MoTa;

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

                // Cập nhật thông tin cho đối tượng NhomTaiSan
                NhomTaiSanSua.MaLoaiTaiSan = loaiTaiSan.MaLoaiTaiSan;
                NhomTaiSanSua.TenNhom = txtTenNhom.Text.Trim();
                NhomTaiSanSua.MoTa = txtMoTa.Text?.Trim();

                // Đánh dấu thành công và đóng cửa sổ
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
            }
            finally
            {
                _isProcessing = false;
            }
        }
    }
}