using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Models;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class QuanLyLoaiTaiSanControl : UserControl
    {
        // Public property để truy cập dữ liệu từ giao diện chính
        public ObservableCollection<LoaiTaiSan> DsLoaiTaiSan { get; set; }
        public ObservableCollection<NhomTaiSan> DsNhomTaiSan { get; set; }

        // Event để thông báo sự thay đổi dữ liệu
        public event Action OnDataChanged;

        public QuanLyLoaiTaiSanControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Cập nhật hiển thị dữ liệu
        /// </summary>
        public void HienThiDuLieu()
        {
            // Hiển thị danh sách Loại Tài Sản
            dgLoaiTaiSan.ItemsSource = null;
            dgLoaiTaiSan.ItemsSource = DsLoaiTaiSan;
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Thêm mới Loại Tài Sản
        /// Giữ lại phương thức để tránh lỗi, nhưng không thực hiện hành động
        /// </summary>
        private void btnThemMoiLoaiTaiSan_Click(object sender, RoutedEventArgs e)
        {
            ThemLoaiTaiSanWindow themLoaiTaiSanWindow = new ThemLoaiTaiSanWindow();
            themLoaiTaiSanWindow.Owner = Window.GetWindow(this);

            // Hiển thị cửa sổ và kiểm tra kết quả
            bool? result = themLoaiTaiSanWindow.ShowDialog();

            if (result == true && themLoaiTaiSanWindow.LoaiTaiSanMoi != null)
            {
                // Thêm loại tài sản mới vào danh sách
                DsLoaiTaiSan.Add(themLoaiTaiSanWindow.LoaiTaiSanMoi);

                // Hiển thị lại dữ liệu
                HienThiDuLieu();

                // Thông báo dữ liệu đã thay đổi
                OnDataChanged?.Invoke();
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Sửa trên DataGrid
        /// Giữ lại phương thức để tránh lỗi, nhưng không thực hiện hành động
        /// </summary>
        private void SuaLoaiTaiSan_Click(object sender, RoutedEventArgs e)
        {
            // Không thực hiện hành động gì
            MessageBox.Show("Chức năng chỉnh sửa đã bị vô hiệu hóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Xóa trên DataGrid
        /// Giữ lại phương thức để tránh lỗi, nhưng không thực hiện hành động
        /// </summary>
        private void XoaLoaiTaiSan_Click(object sender, RoutedEventArgs e)
        {
            // Không thực hiện hành động gì
            MessageBox.Show("Chức năng xóa đã bị vô hiệu hóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}