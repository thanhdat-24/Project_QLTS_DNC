using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Services;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class QuanLyTaiSan : UserControl
    {
        // Thêm biến tham chiếu tới MainWindow
        private MainWindow _mainWindow;

        // Thêm các collection để lưu trữ dữ liệu
        private ObservableCollection<LoaiTaiSan> _dsLoaiTaiSan;
        private ObservableCollection<NhomTaiSan> _dsNhomTaiSan;

        public QuanLyTaiSan()
        {
            InitializeComponent();

            // Tìm MainWindow từ cây phần tử giao diện
            _mainWindow = Application.Current.MainWindow as MainWindow;

            // Kiểm tra nếu tìm thấy MainWindow
            if (_mainWindow == null)
            {
                MessageBox.Show("Không thể tìm thấy cửa sổ chính (MainWindow)", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Load dữ liệu khi khởi tạo
            LoadDuLieuAsync();
        }

        /// <summary>
        /// Phương thức bất đồng bộ để tải dữ liệu từ Supabase
        /// </summary>
        private async void LoadDuLieuAsync()
        {
            try
            {
                // Hiển thị trạng thái đang tải (nếu cần)
                // progress.Visibility = Visibility.Visible;

                // Tải dữ liệu từ Supabase sử dụng các service riêng biệt
                _dsLoaiTaiSan = await LoaiTaiSanService.LayDanhSachLoaiTaiSanAsync();
                _dsNhomTaiSan = await NhomTaiSanService.LayDanhSachNhomTaiSanAsync();

                // Kết hợp dữ liệu Loại Tài Sản và Nhóm Tài Sản
                NhomTaiSanService.KetHopDuLieu(_dsLoaiTaiSan, _dsNhomTaiSan);

                // Cập nhật dữ liệu cho các control con
                if (loaiTaiSanControl != null)
                {
                    loaiTaiSanControl.DsLoaiTaiSan = _dsLoaiTaiSan;
                    loaiTaiSanControl.HienThiDuLieu();
                }

                if (nhomTaiSanControl != null)
                {
                    nhomTaiSanControl.DsLoaiTaiSan = _dsLoaiTaiSan;
                    nhomTaiSanControl.DsNhomTaiSan = _dsNhomTaiSan;
                    nhomTaiSanControl.HienThiDuLieu();
                }

                // Hiển thị thông báo thành công (nếu cần)
                // MessageBox.Show("Đã tải dữ liệu thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Ẩn trạng thái đang tải (nếu cần)
                // progress.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Tìm kiếm
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string tuKhoa = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(tuKhoa))
            {
                // Nếu từ khóa trống, hiển thị tất cả
                if (loaiTaiSanControl != null)
                {
                    loaiTaiSanControl.DsLoaiTaiSan = _dsLoaiTaiSan;
                    loaiTaiSanControl.HienThiDuLieu();
                }

                if (nhomTaiSanControl != null)
                {
                    nhomTaiSanControl.DsNhomTaiSan = _dsNhomTaiSan;
                    nhomTaiSanControl.HienThiDuLieu();
                }
                return;
            }

            // Tìm kiếm trong Loại Tài Sản
            var dsLoaiTaiSanLoc = new ObservableCollection<LoaiTaiSan>(
                _dsLoaiTaiSan.Where(l =>
                    l.MaLoaiTaiSan.ToString().Contains(tuKhoa) ||
                    l.TenLoaiTaiSan.ToLower().Contains(tuKhoa) ||
                    (l.MoTa != null && l.MoTa.ToLower().Contains(tuKhoa))
                )
            );

            // Tìm kiếm trong Nhóm Tài Sản
            var dsNhomTaiSanLoc = new ObservableCollection<NhomTaiSan>(
                _dsNhomTaiSan.Where(n =>
                    n.MaNhomTS.ToString().Contains(tuKhoa) ||
                    n.TenNhom.ToLower().Contains(tuKhoa) ||
                    (n.MoTa != null && n.MoTa.ToLower().Contains(tuKhoa)) ||
                    (n.LoaiTaiSan != null && n.LoaiTaiSan.TenLoaiTaiSan.ToLower().Contains(tuKhoa))
                )
            );

            // Cập nhật dữ liệu cho các control con
            if (loaiTaiSanControl != null)
            {
                loaiTaiSanControl.DsLoaiTaiSan = dsLoaiTaiSanLoc;
                loaiTaiSanControl.HienThiDuLieu();
            }

            if (nhomTaiSanControl != null)
            {
                nhomTaiSanControl.DsNhomTaiSan = dsNhomTaiSanLoc;
                nhomTaiSanControl.HienThiDuLieu();
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Load dữ liệu
        /// </summary>
        private void btnLoadDuLieu_Click(object sender, RoutedEventArgs e)
        {
            LoadDuLieuAsync();
        }
    }
}