using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Models;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class QuanLyTaiSan : UserControl
    {
        // Danh sách lưu trữ dữ liệu, sử dụng từ Models
        private ObservableCollection<LoaiTaiSan> _dsLoaiTaiSan;
        private ObservableCollection<NhomTaiSan> _dsNhomTaiSan;

        // Thêm biến tham chiếu tới MainWindow
        private MainWindow _mainWindow;

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

            // Tải dữ liệu mẫu
            LoadDuLieuMau();

            // Ủy quyền xử lý sự kiện thay đổi dữ liệu từ các UserControl con
            loaiTaiSanControl.OnDataChanged += CapNhatGiaoDienSauThayDoi;
            nhomTaiSanControl.OnDataChanged += CapNhatGiaoDienSauThayDoi;
        }

        /// <summary>
        /// Tạo dữ liệu mẫu để hiển thị UI
        /// </summary>
        private void LoadDuLieuMau()
        {
            // Tạo dữ liệu mẫu cho Loại Tài Sản
            _dsLoaiTaiSan = new ObservableCollection<LoaiTaiSan>
            {
                new LoaiTaiSan { MaLoaiTaiSan = 1, TenLoaiTaiSan = "Máy tính", MoTa = "Các thiết bị máy tính để bàn, laptop" },
                new LoaiTaiSan { MaLoaiTaiSan = 2, TenLoaiTaiSan = "Thiết bị mạng", MoTa = "Router, Switch, Modem" },
                new LoaiTaiSan { MaLoaiTaiSan = 3, TenLoaiTaiSan = "Thiết bị văn phòng", MoTa = "Bàn, ghế, tủ, kệ" },
                new LoaiTaiSan { MaLoaiTaiSan = 4, TenLoaiTaiSan = "Thiết bị điện tử", MoTa = "Máy chiếu, máy in, máy scan" },
                new LoaiTaiSan { MaLoaiTaiSan = 5, TenLoaiTaiSan = "Phương tiện vận chuyển", MoTa = "Xe cộ, phương tiện di chuyển" },
            };

            // Tạo dữ liệu mẫu cho Nhóm Tài Sản, và thiết lập liên kết với LoaiTaiSan
            _dsNhomTaiSan = new ObservableCollection<NhomTaiSan>
            {
                new NhomTaiSan { MaNhomTS = 1, ma_loai_ts = 1, TenNhom = "Máy tính để bàn", SoLuong = 25, MoTa = "PC văn phòng" },
                new NhomTaiSan { MaNhomTS = 2, ma_loai_ts = 1, TenNhom = "Laptop", SoLuong = 35, MoTa = "Laptop cho nhân viên" },
                new NhomTaiSan { MaNhomTS = 3, ma_loai_ts = 1, TenNhom = "Máy chủ", SoLuong = 5, MoTa = "Server phòng IT" },
                new NhomTaiSan { MaNhomTS = 4, ma_loai_ts = 2, TenNhom = "Router", SoLuong = 10, MoTa = "Thiết bị phát wifi" },
                new NhomTaiSan { MaNhomTS = 5, ma_loai_ts = 2, TenNhom = "Switch", SoLuong = 8, MoTa = "Switch mạng LAN" },
                new NhomTaiSan { MaNhomTS = 6, ma_loai_ts = 3, TenNhom = "Bàn làm việc", SoLuong = 50, MoTa = "Bàn cho nhân viên" },
                new NhomTaiSan { MaNhomTS = 7, ma_loai_ts = 3, TenNhom = "Ghế văn phòng", SoLuong = 60, MoTa = "Ghế ergonomic" },
                new NhomTaiSan { MaNhomTS = 8, ma_loai_ts = 4, TenNhom = "Máy chiếu", SoLuong = 5, MoTa = "Máy chiếu phòng họp" },
            };

            // Thiết lập các liên kết Navigation Property giữa các model
            foreach (var nhomTS in _dsNhomTaiSan)
            {
                nhomTS.LoaiTaiSan = _dsLoaiTaiSan.FirstOrDefault(x => x.MaLoaiTaiSan == nhomTS.ma_loai_ts);
            }

            foreach (var loaiTS in _dsLoaiTaiSan)
            {
                loaiTS.NhomTaiSans = _dsNhomTaiSan.Where(x => x.ma_loai_ts == loaiTS.MaLoaiTaiSan).ToList();
            }

            // Truyền dữ liệu vào các UserControl
            loaiTaiSanControl.DsLoaiTaiSan = _dsLoaiTaiSan;
            loaiTaiSanControl.DsNhomTaiSan = _dsNhomTaiSan;
            loaiTaiSanControl.HienThiDuLieu();

            nhomTaiSanControl.DsLoaiTaiSan = _dsLoaiTaiSan;
            nhomTaiSanControl.DsNhomTaiSan = _dsNhomTaiSan;
            nhomTaiSanControl.HienThiDuLieu();

            tongQuanControl.DsLoaiTaiSan = _dsLoaiTaiSan;
            tongQuanControl.DsNhomTaiSan = _dsNhomTaiSan;
            tongQuanControl.CapNhatThongKe();
        }

        /// <summary>
        /// Xử lý sự kiện khi có dữ liệu thay đổi từ các UserControl con
        /// </summary>
        private void CapNhatGiaoDienSauThayDoi()
        {
            // Cập nhật lại giao diện của các UserControl
            loaiTaiSanControl.HienThiDuLieu();
            nhomTaiSanControl.HienThiDuLieu();
            tongQuanControl.CapNhatThongKe();
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Tìm kiếm
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string tuKhoa = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(tuKhoa))
            {
                // Hiển thị lại toàn bộ dữ liệu
                loaiTaiSanControl.HienThiDuLieu();
                nhomTaiSanControl.HienThiDuLieu();
                return;
            }

            // Lọc danh sách Loại Tài Sản
            var ketQuaLoaiTS = _dsLoaiTaiSan.Where(x =>
                x.MaLoaiTaiSan.ToString().Contains(tuKhoa) ||
                (x.TenLoaiTaiSan?.ToLower() ?? "").Contains(tuKhoa) ||
                (x.MoTa?.ToLower() ?? "").Contains(tuKhoa)
            ).ToList();

            // Lọc danh sách Nhóm Tài Sản
            var ketQuaNhomTS = _dsNhomTaiSan.Where(x =>
                x.MaNhomTS.ToString().Contains(tuKhoa) ||
                (x.TenNhom?.ToLower() ?? "").Contains(tuKhoa) ||
                (x.LoaiTaiSan?.TenLoaiTaiSan?.ToLower() ?? "").Contains(tuKhoa) ||
                (x.MoTa?.ToLower() ?? "").Contains(tuKhoa)
            ).ToList();

            // Hiển thị kết quả lọc
            // Áp dụng bộ lọc tìm kiếm vào các UserControl
            loaiTaiSanControl.DsLoaiTaiSan = new ObservableCollection<LoaiTaiSan>(ketQuaLoaiTS);
            loaiTaiSanControl.HienThiDuLieu();

            // Tạo một danh sách tạm để tránh thay đổi dữ liệu gốc
            var tempNhomTaiSan = new ObservableCollection<NhomTaiSan>(ketQuaNhomTS);
            nhomTaiSanControl.DsNhomTaiSan = tempNhomTaiSan;
            nhomTaiSanControl.HienThiDuLieu();
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Load dữ liệu
        /// </summary>
        private void btnLoadDuLieu_Click(object sender, RoutedEventArgs e)
        {
            // Reset tìm kiếm và hiển thị lại toàn bộ dữ liệu
            txtSearch.Text = string.Empty;

            // Khôi phục dữ liệu gốc
            loaiTaiSanControl.DsLoaiTaiSan = _dsLoaiTaiSan;
            loaiTaiSanControl.HienThiDuLieu();

            nhomTaiSanControl.DsNhomTaiSan = _dsNhomTaiSan;
            nhomTaiSanControl.HienThiDuLieu();
        }
    }
}