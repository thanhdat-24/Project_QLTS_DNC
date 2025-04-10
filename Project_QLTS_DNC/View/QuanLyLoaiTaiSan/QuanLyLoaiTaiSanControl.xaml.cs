using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Models.QLLoaiTS;
using Project_QLTS_DNC.Models.QLNhomTS;

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
        /// </summary>
        private async void SuaLoaiTaiSan_Click(object sender, RoutedEventArgs e)
        {
            // Lấy LoaiTaiSan được chọn từ DataGrid
            var loaiTaiSanDuocChon = dgLoaiTaiSan.SelectedItem as LoaiTaiSan;

            if (loaiTaiSanDuocChon == null)
            {
                MessageBox.Show("Vui lòng chọn một loại tài sản để cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Kiểm tra xem loại tài sản này có nhóm tài sản nào không
            if (DsNhomTaiSan != null && DsNhomTaiSan.Any(n => n.MaLoaiTaiSan == loaiTaiSanDuocChon.MaLoaiTaiSan))
            {
                MessageBox.Show("Không thể sửa loại tài sản đã có nhóm tài sản liên kết.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Tạo bản sao của loại tài sản để cập nhật
                var loaiTaiSanCapNhat = new LoaiTaiSan
                {
                    MaLoaiTaiSan = loaiTaiSanDuocChon.MaLoaiTaiSan,
                    TenLoaiTaiSan = loaiTaiSanDuocChon.TenLoaiTaiSan,
                    MoTa = loaiTaiSanDuocChon.MoTa
                };

                // Hiển thị dialog để nhập thông tin mới
                var capNhatWindow = new CapNhatLoaiTaiSanWindow(loaiTaiSanCapNhat);
                capNhatWindow.Owner = Window.GetWindow(this);

                bool? result = capNhatWindow.ShowDialog();

                if (result == true && capNhatWindow.LoaiTaiSanCapNhat != null)
                {
                    // Cập nhật vào cơ sở dữ liệu
                    var loaiTaiSanDaCapNhat = await Services.LoaiTaiSanService.CapNhatLoaiTaiSanAsync(capNhatWindow.LoaiTaiSanCapNhat);

                    // Cập nhật lại dữ liệu trong collection
                    var index = DsLoaiTaiSan.IndexOf(loaiTaiSanDuocChon);
                    if (index >= 0)
                    {
                        DsLoaiTaiSan[index] = loaiTaiSanDaCapNhat;
                    }

                    // Cập nhật lại hiển thị
                    HienThiDuLieu();

                    // Thông báo dữ liệu đã thay đổi
                    OnDataChanged?.Invoke();

                    MessageBox.Show("Cập nhật loại tài sản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật loại tài sản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Xóa trên DataGrid
        /// </summary>
        private async void XoaLoaiTaiSan_Click(object sender, RoutedEventArgs e)
        {
            // Lấy LoaiTaiSan được chọn từ DataGrid
            var loaiTaiSanDuocChon = dgLoaiTaiSan.SelectedItem as LoaiTaiSan;

            if (loaiTaiSanDuocChon == null)
            {
                MessageBox.Show("Vui lòng chọn một loại tài sản để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Kiểm tra xem loại tài sản này có nhóm tài sản nào không
            if (DsNhomTaiSan != null && DsNhomTaiSan.Any(n => n.MaLoaiTaiSan == loaiTaiSanDuocChon.MaLoaiTaiSan))
            {
                MessageBox.Show("Không thể xóa loại tài sản đã có nhóm tài sản liên kết.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Xác nhận xóa
            MessageBoxResult xacNhan = MessageBox.Show($"Bạn có chắc chắn muốn xóa loại tài sản '{loaiTaiSanDuocChon.TenLoaiTaiSan}'?",
                                                      "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (xacNhan != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                // Thực hiện xóa dữ liệu
                bool ketQuaXoa = await Services.LoaiTaiSanService.XoaLoaiTaiSanAsync(loaiTaiSanDuocChon.MaLoaiTaiSan);

                if (ketQuaXoa)
                {
                    // Xóa khỏi collection
                    DsLoaiTaiSan.Remove(loaiTaiSanDuocChon);

                    // Cập nhật lại hiển thị
                    HienThiDuLieu();

                    // Thông báo dữ liệu đã thay đổi
                    OnDataChanged?.Invoke();

                    MessageBox.Show("Xóa loại tài sản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Không thể xóa loại tài sản. Vui lòng thử lại sau.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa loại tài sản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}