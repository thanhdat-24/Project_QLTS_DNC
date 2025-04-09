using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
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

            // Lưu danh sách Loại Tài Sản
            _dsLoaiTaiSan = dsLoaiTaiSan;

            // Nạp dữ liệu vào ComboBox Loại Tài Sản
            cboLoaiTaiSan.ItemsSource = _dsLoaiTaiSan;
            cboLoaiTaiSan.DisplayMemberPath = "TenLoaiTaiSan";
            cboLoaiTaiSan.SelectedValuePath = "MaLoaiTaiSan";

            // Điền thông tin vào các trường
            txtMaNhomTS.Text = nhomTaiSan.MaNhomTS.ToString();
            cboLoaiTaiSan.SelectedValue = nhomTaiSan.ma_loai_ts;
            txtTenNhom.Text = nhomTaiSan.TenNhom;
            txtSoLuong.Text = nhomTaiSan.SoLuong?.ToString() ?? "";
            txtMoTa.Text = nhomTaiSan.MoTa;

            // Đóng window khi nhấn nút đóng
            btnDong.Click += (s, e) => Close();

            // Cho phép di chuyển cửa sổ
            MouseLeftButtonDown += Window_MouseLeftButtonDown;
        }

        // Chỉ cho phép nhập số
        private void txtSoLuong_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9]+"); // regex that matches disallowed text
            return !regex.IsMatch(text);
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
            if (cboLoaiTaiSan.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn Loại Tài Sản", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTenNhom.Text))
            {
                MessageBox.Show("Vui lòng nhập tên nhóm tài sản", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int soLuong = 0;
            if (!string.IsNullOrWhiteSpace(txtSoLuong.Text))
            {
                if (!int.TryParse(txtSoLuong.Text, out soLuong))
                {
                    MessageBox.Show("Số lượng phải là số nguyên", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            // Tạo đối tượng NhomTaiSan được sửa
            NhomTaiSanSua = new NhomTaiSan
            {
                MaNhomTS = int.Parse(txtMaNhomTS.Text),
                ma_loai_ts = (int)cboLoaiTaiSan.SelectedValue,
                TenNhom = txtTenNhom.Text.Trim(),
                SoLuong = soLuong,
                MoTa = txtMoTa.Text.Trim()
            };

            // Đóng window và trả về DialogResult là True
            DialogResult = true;
            Close();
        }
    }
}