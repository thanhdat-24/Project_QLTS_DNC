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
        /// </summary>
        private void btnThemMoiLoaiTaiSan_Click(object sender, RoutedEventArgs e)
        {
            // Tạo và hiển thị popup thêm mới Loại Tài Sản
            ThemLoaiTaiSanWindow themLoaiTaiSanWindow = new ThemLoaiTaiSanWindow();

            // Hiệu ứng làm mờ nền
            var currentWindow = Window.GetWindow(this);
            themLoaiTaiSanWindow.Owner = currentWindow;
            themLoaiTaiSanWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Hiển thị popup như một dialog
            bool? result = themLoaiTaiSanWindow.ShowDialog();

            // Xử lý kết quả từ popup
            if (result == true && themLoaiTaiSanWindow.LoaiTaiSanMoi != null)
            {
                // Thêm Loại Tài Sản mới vào danh sách
                // Tạo mã Loại Tài Sản tự động
                int maxMaLoaiTaiSan = DsLoaiTaiSan.Count > 0 ? DsLoaiTaiSan.Max(x => x.MaLoaiTaiSan) : 0;
                themLoaiTaiSanWindow.LoaiTaiSanMoi.MaLoaiTaiSan = maxMaLoaiTaiSan + 1;

                // Thêm vào danh sách
                DsLoaiTaiSan.Add(themLoaiTaiSanWindow.LoaiTaiSanMoi);

                // Kích hoạt sự kiện thay đổi dữ liệu
                OnDataChanged?.Invoke();

                // Cập nhật lại giao diện
                HienThiDuLieu();

                // Thông báo thành công
                MessageBox.Show("Thêm mới Loại Tài Sản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Sửa trên DataGrid
        /// </summary>
        private void SuaLoaiTaiSan_Click(object sender, RoutedEventArgs e)
        {
            // Lấy đối tượng LoaiTaiSan được chọn
            var button = sender as Button;
            if (button == null) return;

            var loaiTaiSan = button.DataContext as LoaiTaiSan;
            if (loaiTaiSan == null) return;

            // Mở cửa sổ chỉnh sửa
            CapNhatLoaiTaiSanWindow suaLoaiTaiSanWindow = new CapNhatLoaiTaiSanWindow(loaiTaiSan);

            // Hiệu ứng làm mờ nền
            var currentWindow = Window.GetWindow(this);
            suaLoaiTaiSanWindow.Owner = currentWindow;
            suaLoaiTaiSanWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Hiển thị popup như một dialog
            bool? result = suaLoaiTaiSanWindow.ShowDialog();

            // Xử lý kết quả từ popup
            if (result == true && suaLoaiTaiSanWindow.LoaiTaiSanSua != null)
            {
                // Tìm và cập nhật Loại Tài Sản trong danh sách
                var indexToUpdate = DsLoaiTaiSan.IndexOf(loaiTaiSan);
                if (indexToUpdate != -1)
                {
                    DsLoaiTaiSan[indexToUpdate] = suaLoaiTaiSanWindow.LoaiTaiSanSua;

                    // Kích hoạt sự kiện thay đổi dữ liệu
                    OnDataChanged?.Invoke();

                    // Cập nhật lại giao diện
                    HienThiDuLieu();

                    // Thông báo thành công
                    MessageBox.Show("Chỉnh sửa Loại Tài Sản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Xóa trên DataGrid
        /// </summary>
        private void XoaLoaiTaiSan_Click(object sender, RoutedEventArgs e)
        {
            // Lấy đối tượng LoaiTaiSan được chọn
            var button = sender as Button;
            if (button == null) return;

            var loaiTaiSan = button.DataContext as LoaiTaiSan;
            if (loaiTaiSan == null) return;

            // Kiểm tra xem Loại Tài Sản có tồn tại Nhóm Tài Sản không
            var nhomTaiSanLienQuan = DsNhomTaiSan.Where(x => x.ma_loai_ts == loaiTaiSan.MaLoaiTaiSan).ToList();

            if (nhomTaiSanLienQuan.Any())
            {
                MessageBox.Show("Không thể xóa Loại Tài Sản đã có Nhóm Tài Sản!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Hiển thị hộp thoại xác nhận
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa Loại Tài Sản '{loaiTaiSan.TenLoaiTaiSan}'?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Xóa Loại Tài Sản khỏi danh sách
                DsLoaiTaiSan.Remove(loaiTaiSan);

                // Kích hoạt sự kiện thay đổi dữ liệu
                OnDataChanged?.Invoke();

                // Cập nhật lại giao diện
                HienThiDuLieu();

                // Thông báo thành công
                MessageBox.Show("Xóa Loại Tài Sản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}