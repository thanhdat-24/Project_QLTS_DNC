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
                nhom.MaLoaiTaiSan,
                TenLoaiTaiSan = nhom.LoaiTaiSan?.TenLoaiTaiSan ?? "",
                nhom.TenNhom,
                nhom.MoTa
            }).ToList();

            dgNhomTaiSan.ItemsSource = null;
            dgNhomTaiSan.ItemsSource = nhomTaiSanVm;
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Thêm mới Nhóm Tài Sản
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
                themNhomTaiSanWindow.NhomTaiSanMoi.LoaiTaiSan = DsLoaiTaiSan.FirstOrDefault(l => l.MaLoaiTaiSan == themNhomTaiSanWindow.NhomTaiSanMoi.MaLoaiTaiSan);

                // Hiển thị lại dữ liệu
                HienThiDuLieu();

                // Thông báo dữ liệu đã thay đổi
                OnDataChanged?.Invoke();
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Sửa trên DataGrid
        /// </summary>
        private async void SuaNhomTaiSan_Click(object sender, RoutedEventArgs e)
        {
            // Xác định nhóm tài sản được chọn
            dynamic nhomTaiSanVM = dgNhomTaiSan.SelectedItem;
            if (nhomTaiSanVM == null)
            {
                MessageBox.Show("Vui lòng chọn một nhóm tài sản để sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Tìm nhóm tài sản tương ứng trong danh sách
            var nhomTaiSan = DsNhomTaiSan.FirstOrDefault(n => n.MaNhomTS == nhomTaiSanVM.MaNhomTS);
            if (nhomTaiSan == null)
            {
                MessageBox.Show("Không tìm thấy thông tin nhóm tài sản.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Mở cửa sổ cập nhật nhóm tài sản
                var capNhatWindow = new CapNhatNhomTaiSanWindow(nhomTaiSan, DsLoaiTaiSan);
                capNhatWindow.Owner = Window.GetWindow(this);

                bool? result = capNhatWindow.ShowDialog();

                if (result == true && capNhatWindow.NhomTaiSanSua != null)
                {
                    // Cập nhật vào cơ sở dữ liệu
                    var nhomTaiSanDaCapNhat = await Services.NhomTaiSanService.CapNhatNhomTaiSanAsync(capNhatWindow.NhomTaiSanSua);

                    // Cập nhật lại đối tượng trong collection
                    var index = DsNhomTaiSan.IndexOf(nhomTaiSan);
                    if (index >= 0)
                    {
                        DsNhomTaiSan[index] = nhomTaiSanDaCapNhat;

                        // Cập nhật tham chiếu đến Loại Tài Sản
                        nhomTaiSanDaCapNhat.LoaiTaiSan = DsLoaiTaiSan.FirstOrDefault(l => l.MaLoaiTaiSan == nhomTaiSanDaCapNhat.MaLoaiTaiSan);
                    }

                    // Cập nhật lại hiển thị
                    HienThiDuLieu();

                    // Thông báo dữ liệu đã thay đổi
                    OnDataChanged?.Invoke();

                    MessageBox.Show("Cập nhật nhóm tài sản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật nhóm tài sản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Xóa trên DataGrid
        /// </summary>
        private async void XoaNhomTaiSan_Click(object sender, RoutedEventArgs e)
        {
            // Xác định nhóm tài sản được chọn
            dynamic nhomTaiSanVM = dgNhomTaiSan.SelectedItem;
            if (nhomTaiSanVM == null)
            {
                MessageBox.Show("Vui lòng chọn một nhóm tài sản để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Tìm nhóm tài sản tương ứng trong danh sách
            var nhomTaiSan = DsNhomTaiSan.FirstOrDefault(n => n.MaNhomTS == nhomTaiSanVM.MaNhomTS);
            if (nhomTaiSan == null)
            {
                MessageBox.Show("Không tìm thấy thông tin nhóm tài sản.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Xác nhận xóa
            MessageBoxResult xacNhan = MessageBox.Show($"Bạn có chắc chắn muốn xóa nhóm tài sản '{nhomTaiSan.TenNhom}'?",
                                                      "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (xacNhan != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                // Hiển thị thông báo đang xử lý
                var window = Window.GetWindow(this);
                window.Cursor = System.Windows.Input.Cursors.Wait;

                // Thực hiện xóa dữ liệu
                bool ketQuaXoa = await Services.NhomTaiSanService.XoaNhomTaiSanAsync(nhomTaiSan.MaNhomTS);

                // Khôi phục con trỏ
                window.Cursor = null;

                if (ketQuaXoa)
                {
                    // Xóa khỏi collection
                    DsNhomTaiSan.Remove(nhomTaiSan);

                    // Cập nhật lại hiển thị
                    HienThiDuLieu();

                    // Thông báo dữ liệu đã thay đổi
                    OnDataChanged?.Invoke();

                    MessageBox.Show("Xóa nhóm tài sản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Không thể xóa nhóm tài sản. Vui lòng thử lại sau.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa nhóm tài sản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}