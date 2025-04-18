﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models.QLLoaiTS;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Services.QLTaiSanService;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class QuanLyTaiSan : UserControl
    {
        // Thêm biến tham chiếu tới MainWindow
        private MainWindow _mainWindow;

        // Thêm các collection để lưu trữ dữ liệu
        private ObservableCollection<LoaiTaiSan> _dsLoaiTaiSan;
        private ObservableCollection<NhomTaiSan> _dsNhomTaiSan;

        // Thêm collection DTO cho hiển thị
        private ObservableCollection<NhomTaiSanDTO> _dsNhomTaiSanDTO;

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

                // Tạo các DTO cho hiển thị
                _dsNhomTaiSanDTO = NhomTaiSanService.TaoDanhSachDTO(_dsNhomTaiSan, _dsLoaiTaiSan);

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
                if (tongQuanControl != null)
                {
                    tongQuanControl.CapNhatThongKe();
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
        /// Chuyển đổi chuỗi tiếng Việt có dấu thành chuỗi không dấu
        /// </summary>
        private string ChuyenChuoiKhongDau(string chuoi)
        {
            if (string.IsNullOrEmpty(chuoi))
                return string.Empty;

            var normalizedString = chuoi.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            string result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            // Xử lý thêm các ký tự đặc biệt tiếng Việt
            result = result.Replace('Đ', 'D').Replace('đ', 'd');
            return result;
        }

        /// <summary>
        /// Kiểm tra xem một chuỗi có chứa chuỗi khác không (không phân biệt dấu)
        /// </summary>
        private bool ChuoiChuaChuoiKhac(string chuoiGoc, string tuKhoa)
        {
            if (string.IsNullOrEmpty(chuoiGoc))
                return false;

            // Chuyển đổi cả hai chuỗi sang dạng không dấu và chữ thường
            string chuoiGocKhongDau = ChuyenChuoiKhongDau(chuoiGoc).ToLower();
            string tuKhoaKhongDau = ChuyenChuoiKhongDau(tuKhoa).ToLower();

            // Kiểm tra chuỗi gốc có chứa từ khóa không
            return chuoiGocKhongDau.Contains(tuKhoaKhongDau);
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Tìm kiếm
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string tuKhoa = txtSearch.Text.Trim();

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

            // Tìm kiếm trong Loại Tài Sản sử dụng hàm tìm kiếm không dấu
            var dsLoaiTaiSanLoc = new ObservableCollection<LoaiTaiSan>(
                _dsLoaiTaiSan.Where(l =>
                    l.MaLoaiTaiSan.ToString().Contains(tuKhoa) ||
                    ChuoiChuaChuoiKhac(l.TenLoaiTaiSan, tuKhoa) ||
                    (l.MoTa != null && ChuoiChuaChuoiKhac(l.MoTa, tuKhoa))
                )
            );

            // Tìm kiếm trong Nhóm Tài Sản - sử dụng DTO để tìm kiếm với hàm tìm kiếm không dấu
            var dsNhomTaiSanDTOLoc = new ObservableCollection<NhomTaiSanDTO>(
                _dsNhomTaiSanDTO.Where(dto =>
                    dto.MaNhomTS.ToString().Contains(tuKhoa) ||
                    ChuoiChuaChuoiKhac(dto.TenNhom, tuKhoa) ||
                    (dto.MoTa != null && ChuoiChuaChuoiKhac(dto.MoTa, tuKhoa)) ||
                    (dto.TenLoaiTaiSan != null && ChuoiChuaChuoiKhac(dto.TenLoaiTaiSan, tuKhoa))
                )
            );

            // Tạo danh sách nhóm tài sản entity (không phải DTO) từ kết quả lọc
            var dsNhomTaiSanLoc = new ObservableCollection<NhomTaiSan>(
                dsNhomTaiSanDTOLoc.Select(dto => _dsNhomTaiSan.FirstOrDefault(n => n.MaNhomTS == dto.MaNhomTS)).Where(n => n != null) // Loại bỏ các kết quả null
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