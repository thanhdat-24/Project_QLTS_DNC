using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Models;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class TongQuanControl : UserControl
    {
        // Public property để truy cập dữ liệu từ giao diện chính
        public ObservableCollection<LoaiTaiSan> DsLoaiTaiSan { get; set; }
        public ObservableCollection<NhomTaiSan> DsNhomTaiSan { get; set; }

        // Tham chiếu tới MainWindow
        private MainWindow _mainWindow;

        public TongQuanControl()
        {
            InitializeComponent();

            // Tìm MainWindow từ cây phần tử giao diện
            _mainWindow = Application.Current.MainWindow as MainWindow;

            // Kiểm tra nếu tìm thấy MainWindow
            if (_mainWindow == null)
            {
                MessageBox.Show("Không thể tìm thấy cửa sổ chính (MainWindow)", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Cập nhật thông tin thống kê
        /// </summary>
        public void CapNhatThongKe()
        {
            // Cập nhật số lượng loại tài sản
            txtTongLoaiTaiSan.Text = DsLoaiTaiSan.Count.ToString();

            // Cập nhật số lượng nhóm tài sản
            txtTongNhomTaiSan.Text = DsNhomTaiSan.Count.ToString();

            // Cập nhật tổng số tài sản
            int tongTaiSan = DsNhomTaiSan.Sum(x => x.SoLuong ?? 0);
            txtTongTaiSan.Text = tongTaiSan.ToString();
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn vào "Xem chi tiết" trên card Tổng Tài Sản
        /// </summary>
        private void btnXemChiTietTongTaiSan_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem MainWindow đã được khởi tạo chưa
            if (_mainWindow != null)
            {
                // Sử dụng phương thức ChuyenDenTraCuuTaiSan để chuyển trang
                _mainWindow.ChuyenDenTraCuuTaiSan();

                // Thông báo để kiểm tra sự kiện đã được kích hoạt
                MessageBox.Show("Đang chuyển đến trang Tra cứu tài sản", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Không thể tìm thấy cửa sổ chính (MainWindow)", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}