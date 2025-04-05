using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Project_QLTS_DNC.Models; // Thêm namespace cho Models

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class QuanLyTaiSan : UserControl
    {
        // Danh sách lưu trữ dữ liệu, sử dụng từ Models
        private List<LoaiTaiSan> _dsLoaiTaiSan;
        private List<NhomTaiSan> _dsNhomTaiSan;

        public QuanLyTaiSan()
        {
            InitializeComponent();
            LoadDuLieuMau();
        }

        /// <summary>
        /// Tạo dữ liệu mẫu để hiển thị UI
        /// </summary>
        private void LoadDuLieuMau()
        {
            // Tạo dữ liệu mẫu cho Loại Tài Sản
            _dsLoaiTaiSan = new List<LoaiTaiSan>
            {
                new LoaiTaiSan { MaLoaiTaiSan = 1, TenLoaiTaiSan = "Máy tính", MoTa = "Các thiết bị máy tính để bàn, laptop" },
                new LoaiTaiSan { MaLoaiTaiSan = 2, TenLoaiTaiSan = "Thiết bị mạng", MoTa = "Router, Switch, Modem" },
                new LoaiTaiSan { MaLoaiTaiSan = 3, TenLoaiTaiSan = "Thiết bị văn phòng", MoTa = "Bàn, ghế, tủ, kệ" },
                new LoaiTaiSan { MaLoaiTaiSan = 4, TenLoaiTaiSan = "Thiết bị điện tử", MoTa = "Máy chiếu, máy in, máy scan" },
                new LoaiTaiSan { MaLoaiTaiSan = 5, TenLoaiTaiSan = "Phương tiện vận chuyển", MoTa = "Xe cộ, phương tiện di chuyển" },
            };

            // Tạo dữ liệu mẫu cho Nhóm Tài Sản, và thiết lập liên kết với LoaiTaiSan
            _dsNhomTaiSan = new List<NhomTaiSan>
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

            // Hiển thị dữ liệu
            HienThiDuLieu();

            // Cập nhật thông tin tổng quan
            CapNhatThongKe();
        }

        /// <summary>
        /// Hiển thị dữ liệu lên giao diện
        /// </summary>
        private void HienThiDuLieu()
        {
            // Hiển thị danh sách Loại Tài Sản
            dgLoaiTaiSan.ItemsSource = _dsLoaiTaiSan;

            // Hiển thị danh sách Nhóm Tài Sản với thông tin Loại Tài Sản tích hợp
            var nhomTaiSanVm = _dsNhomTaiSan.Select(nhom => new
            {
                nhom.MaNhomTS,
                nhom.ma_loai_ts,
                TenLoaiTaiSan = nhom.LoaiTaiSan?.TenLoaiTaiSan ?? "",
                nhom.TenNhom,
                SoLuongTS = nhom.SoLuong ?? 0,
                nhom.MoTa
            }).ToList();

            dgNhomTaiSan.ItemsSource = nhomTaiSanVm;
        }

        /// <summary>
        /// Cập nhật thông tin thống kê tổng quan
        /// </summary>
        private void CapNhatThongKe()
        {
            // Cập nhật số lượng loại tài sản
            txtTongLoaiTaiSan.Text = _dsLoaiTaiSan.Count.ToString();

            // Cập nhật số lượng nhóm tài sản
            txtTongNhomTaiSan.Text = _dsNhomTaiSan.Count.ToString();

            // Cập nhật tổng số tài sản
            int tongTaiSan = _dsNhomTaiSan.Sum(x => x.SoLuong ?? 0);
            txtTongTaiSan.Text = tongTaiSan.ToString();
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Thêm mới
        /// </summary>
        private void btnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            // Lấy TabItem hiện tại đang được chọn
            var selectedTab = ((TabControl)this.FindName("tabMain"))?.SelectedItem as TabItem;
            string tabHeader = selectedTab?.Header.ToString() ?? "";

            if (tabHeader == "LOẠI TÀI SẢN")
            {
                // Mở form thêm mới loại tài sản
                MessageBox.Show("Mở form thêm mới loại tài sản", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (tabHeader == "NHÓM TÀI SẢN")
            {
                // Mở form thêm mới nhóm tài sản
                MessageBox.Show("Mở form thêm mới nhóm tài sản", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Mặc định mở form thêm mới tài sản
                MessageBox.Show("Mở form thêm mới tài sản", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
                HienThiDuLieu();
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

            // Ánh xạ kết quả nhóm tài sản thành ViewModel
            var nhomTaiSanVm = ketQuaNhomTS.Select(nhom => new
            {
                nhom.MaNhomTS,
                nhom.ma_loai_ts,
                TenLoaiTaiSan = nhom.LoaiTaiSan?.TenLoaiTaiSan ?? "",
                nhom.TenNhom,
                SoLuongTS = nhom.SoLuong ?? 0,
                nhom.MoTa
            }).ToList();

            // Hiển thị kết quả lọc
            dgLoaiTaiSan.ItemsSource = ketQuaLoaiTS;
            dgNhomTaiSan.ItemsSource = nhomTaiSanVm;
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Load dữ liệu
        /// </summary>
        private void btnLoadDuLieu_Click(object sender, RoutedEventArgs e)
        {
            // Reset tìm kiếm và hiển thị lại toàn bộ dữ liệu
            txtSearch.Text = string.Empty;
            HienThiDuLieu();
        }
    }
}