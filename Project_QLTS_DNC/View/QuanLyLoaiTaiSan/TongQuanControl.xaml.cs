using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models.QLLoaiTS;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Services.QLTaiSanService;

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

            // Đăng ký sự kiện Loaded để cập nhật thống kê khi control được tải
            this.Loaded += (s, e) => {
                CapNhatThongKe();
            };
        }

        /// <summary>
        /// Cập nhật thông tin thống kê từ CSDL Supabase
        /// </summary>
        public async void CapNhatThongKe()
        {
            try
            {
                // Hiển thị thông báo đang tải
                txtTongLoaiTaiSan.Text = "...";
                txtTongNhomTaiSan.Text = "...";
                txtTongTaiSan.Text = "...";

                // 1. Lấy dữ liệu Loại Tài Sản từ Supabase
                if (DsLoaiTaiSan == null || DsLoaiTaiSan.Count == 0)
                {
                    DsLoaiTaiSan = await LoaiTaiSanService.LayDanhSachLoaiTaiSanAsync();
                }

                // 2. Lấy dữ liệu Nhóm Tài Sản từ Supabase
                if (DsNhomTaiSan == null || DsNhomTaiSan.Count == 0)
                {
                    DsNhomTaiSan = await NhomTaiSanService.LayDanhSachNhomTaiSanAsync();
                }

                // 3. Đếm số lượng Tài Sản
                int soLuongTaiSan = await TaiSanService.DemSoLuongTaiSanAsync();

                // Cập nhật UI với các giá trị thống kê
                txtTongLoaiTaiSan.Text = DsLoaiTaiSan.Count.ToString();
                txtTongNhomTaiSan.Text = DsNhomTaiSan.Count.ToString();
                txtTongTaiSan.Text = soLuongTaiSan.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật thống kê: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);

                // Đặt giá trị mặc định nếu xảy ra lỗi
                txtTongLoaiTaiSan.Text = "0";
                txtTongNhomTaiSan.Text = "0";
                txtTongTaiSan.Text = "0";
            }
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