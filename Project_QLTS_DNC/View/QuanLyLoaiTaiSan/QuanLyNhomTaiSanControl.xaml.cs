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
                SoLuongTS = nhom.SoLuong ?? 0,
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
            // Tạo và hiển thị popup thêm mới Nhóm Tài Sản
            ThemNhomTaiSanWindow themNhomTaiSanWindow = new ThemNhomTaiSanWindow(DsLoaiTaiSan);

            // Hiệu ứng làm mờ nền
            var currentWindow = Window.GetWindow(this);
            themNhomTaiSanWindow.Owner = currentWindow;
            themNhomTaiSanWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Hiển thị popup như một dialog
            bool? result = themNhomTaiSanWindow.ShowDialog();

            // Xử lý kết quả từ popup
            if (result == true && themNhomTaiSanWindow.NhomTaiSanMoi != null)
            {
                // Thêm Nhóm Tài Sản mới vào danh sách
                // Tạo mã Nhóm Tài Sản tự động
                int maxMaNhomTS = DsNhomTaiSan.Count > 0 ? DsNhomTaiSan.Max(x => x.MaNhomTS) : 0;
                themNhomTaiSanWindow.NhomTaiSanMoi.MaNhomTS = maxMaNhomTS + 1;

                // Thêm vào danh sách
                DsNhomTaiSan.Add(themNhomTaiSanWindow.NhomTaiSanMoi);

                // Cập nhật liên kết Navigation Property
                var loaiTaiSan = DsLoaiTaiSan.FirstOrDefault(x => x.MaLoaiTaiSan == themNhomTaiSanWindow.NhomTaiSanMoi.ma_loai_ts);
                if (loaiTaiSan != null)
                {
                    themNhomTaiSanWindow.NhomTaiSanMoi.LoaiTaiSan = loaiTaiSan;
                    loaiTaiSan.NhomTaiSans.Add(themNhomTaiSanWindow.NhomTaiSanMoi);
                }

                // Kích hoạt sự kiện thay đổi dữ liệu
                OnDataChanged?.Invoke();

                // Cập nhật lại giao diện
                HienThiDuLieu();

                // Thông báo thành công
                MessageBox.Show("Thêm mới Nhóm Tài Sản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Sửa trên DataGrid
        /// </summary>
        private void SuaNhomTaiSan_Click(object sender, RoutedEventArgs e)
        {
            // Lấy đối tượng được chọn
            var button = sender as Button;
            if (button == null) return;

            var rowData = button.DataContext;
            if (rowData == null) return;

            // Lấy mã nhóm tài sản từ dòng được chọn
            int maNhomTS = (int)rowData.GetType().GetProperty("MaNhomTS").GetValue(rowData);

            // Tìm NhomTaiSan từ danh sách gốc
            var nhomTaiSan = DsNhomTaiSan.FirstOrDefault(x => x.MaNhomTS == maNhomTS);
            if (nhomTaiSan == null) return;

            // Mở cửa sổ chỉnh sửa
            CapNhatNhomTaiSanWindow suaNhomTaiSanWindow = new CapNhatNhomTaiSanWindow(nhomTaiSan, DsLoaiTaiSan);

            // Hiệu ứng làm mờ nền
            var currentWindow = Window.GetWindow(this);
            suaNhomTaiSanWindow.Owner = currentWindow;
            suaNhomTaiSanWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Hiển thị popup như một dialog
            bool? result = suaNhomTaiSanWindow.ShowDialog();

            // Xử lý kết quả từ popup
            if (result == true && suaNhomTaiSanWindow.NhomTaiSanSua != null)
            {
                // Tìm và cập nhật Nhóm Tài Sản trong danh sách
                var indexToUpdate = DsNhomTaiSan.IndexOf(nhomTaiSan);
                if (indexToUpdate != -1)
                {
                    // Xóa liên kết cũ
                    var oldLoaiTaiSan = nhomTaiSan.LoaiTaiSan;
                    if (oldLoaiTaiSan != null)
                    {
                        oldLoaiTaiSan.NhomTaiSans.Remove(nhomTaiSan);
                    }

                    // Cập nhật Nhóm Tài Sản
                    DsNhomTaiSan[indexToUpdate] = suaNhomTaiSanWindow.NhomTaiSanSua;

                    // Cập nhật liên kết Navigation Property
                    var newLoaiTaiSan = DsLoaiTaiSan.FirstOrDefault(x => x.MaLoaiTaiSan == suaNhomTaiSanWindow.NhomTaiSanSua.ma_loai_ts);
                    if (newLoaiTaiSan != null)
                    {
                        suaNhomTaiSanWindow.NhomTaiSanSua.LoaiTaiSan = newLoaiTaiSan;
                        newLoaiTaiSan.NhomTaiSans.Add(suaNhomTaiSanWindow.NhomTaiSanSua);
                    }

                    // Kích hoạt sự kiện thay đổi dữ liệu
                    OnDataChanged?.Invoke();

                    // Cập nhật lại giao diện
                    HienThiDuLieu();

                    // Thông báo thành công
                    MessageBox.Show("Chỉnh sửa Nhóm Tài Sản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Xóa trên DataGrid
        /// </summary>
        private void XoaNhomTaiSan_Click(object sender, RoutedEventArgs e)
        {
            // Lấy đối tượng được chọn
            var button = sender as Button;
            if (button == null) return;

            var rowData = button.DataContext;
            if (rowData == null) return;

            // Lấy mã nhóm tài sản từ dòng được chọn
            int maNhomTS = (int)rowData.GetType().GetProperty("MaNhomTS").GetValue(rowData);

            // Tìm NhomTaiSan từ danh sách gốc
            var nhomTaiSan = DsNhomTaiSan.FirstOrDefault(x => x.MaNhomTS == maNhomTS);
            if (nhomTaiSan == null) return;

            // Hiển thị hộp thoại xác nhận
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa Nhóm Tài Sản '{nhomTaiSan.TenNhom}'?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Xóa liên kết với Loại Tài Sản
                var loaiTaiSan = nhomTaiSan.LoaiTaiSan;
                if (loaiTaiSan != null)
                {
                    loaiTaiSan.NhomTaiSans.Remove(nhomTaiSan);
                }

                // Xóa Nhóm Tài Sản khỏi danh sách
                DsNhomTaiSan.Remove(nhomTaiSan);

                // Kích hoạt sự kiện thay đổi dữ liệu
                OnDataChanged?.Invoke();

                // Cập nhật lại giao diện
                HienThiDuLieu();

                // Thông báo thành công
                MessageBox.Show("Xóa Nhóm Tài Sản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}