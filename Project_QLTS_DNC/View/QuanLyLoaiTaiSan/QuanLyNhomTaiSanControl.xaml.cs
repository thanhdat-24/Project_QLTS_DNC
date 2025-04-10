using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Models;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class QuanLyNhomTaiSanControl : UserControl
    {
        // Public property để truy cập dữ liệu từ giao diện chính
        public ObservableCollection<LoaiTaiSan> DsLoaiTaiSan { get; set; }
        public ObservableCollection<NhomTaiSan> DsNhomTaiSan { get; set; }

        // Event để thông báo sự thay đổi dữ liệu
        public event Action OnDataChanged;

        public QuanLyNhomTaiSanControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Cập nhật hiển thị dữ liệu
        /// </summary>
        public void HienThiDuLieu()
        {
            // Hiển thị danh sách Nhóm Tài Sản với thông tin Loại Tài Sản tích hợp
            var nhomTaiSanVm = DsNhomTaiSan.Select(nhom => new
            {
                nhom.MaNhomTS,
                nhom.ma_loai_ts,
                TenLoaiTaiSan = nhom.LoaiTaiSan?.TenLoaiTaiSan ?? "",
                nhom.TenNhom,
                nhom.MoTa
            }).ToList();

            dgNhomTaiSan.ItemsSource = null;
            dgNhomTaiSan.ItemsSource = nhomTaiSanVm;
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Thêm mới Nhóm Tài Sản
        /// Giữ lại phương thức để tránh lỗi, nhưng không thực hiện hành động
        /// </summary>
        private void btnThemMoiNhomTaiSan_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem đã có danh sách loại tài sản chưa
            if (DsLoaiTaiSan == null || DsLoaiTaiSan.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu loại tài sản để chọn. Vui lòng tải lại dữ liệu.",
                                "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ThemNhomTaiSanWindow themNhomTaiSanWindow = new ThemNhomTaiSanWindow(DsLoaiTaiSan);
            themNhomTaiSanWindow.Owner = Window.GetWindow(this);

            // Hiển thị cửa sổ và kiểm tra kết quả
            bool? result = themNhomTaiSanWindow.ShowDialog();

            if (result == true && themNhomTaiSanWindow.NhomTaiSanMoi != null)
            {
                // Thêm nhóm tài sản mới vào danh sách
                DsNhomTaiSan.Add(themNhomTaiSanWindow.NhomTaiSanMoi);

                // Cập nhật tham chiếu đến Loại Tài Sản
                themNhomTaiSanWindow.NhomTaiSanMoi.LoaiTaiSan = DsLoaiTaiSan.FirstOrDefault(l => l.MaLoaiTaiSan == themNhomTaiSanWindow.NhomTaiSanMoi.ma_loai_ts);

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
        private void SuaNhomTaiSan_Click(object sender, RoutedEventArgs e)
        {
            // Không thực hiện hành động gì
            MessageBox.Show("Chức năng chỉnh sửa đã bị vô hiệu hóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Xóa trên DataGrid
        /// Giữ lại phương thức để tránh lỗi, nhưng không thực hiện hành động
        /// </summary>
        private void XoaNhomTaiSan_Click(object sender, RoutedEventArgs e)
        {
            // Không thực hiện hành động gì
            MessageBox.Show("Chức năng xóa đã bị vô hiệu hóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}