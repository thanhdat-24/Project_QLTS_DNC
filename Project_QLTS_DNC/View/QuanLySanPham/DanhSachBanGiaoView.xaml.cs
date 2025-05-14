using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClosedXML.Excel;
using Microsoft.Win32;
using System.ComponentModel;
using System.Data;
using Supabase;
using Project_QLTS_DNC.Models.BanGiaoTaiSan;
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Services.BanGiaoTaiSanService;
using System.Windows.Media;
using Project_QLTS_DNC.Helpers;
using System.IO;
namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class DanhSachBanGiaoView : UserControl
    {
        private ObservableCollection<BanGiaoTaiSanDTO> _dsBanGiao;
        private string _keyword = "";
        private int? _filterMaPhong = null;
        private string _filterTrangThai = null;

        // Converter để kiểm tra bằng nhau và chuyển đổi sang Visibility
        private EqualityToVisibilityConverter _equalityToVisibilityConverter = new EqualityToVisibilityConverter();

        public DanhSachBanGiaoView()
        {
            InitializeComponent();
            Resources.Add("EqualityToVisibilityConverter", _equalityToVisibilityConverter);
            _ = Init();
        }

        private async Task Init()
        {
            await LoadPhongLookupAsync();
            await LoadBanGiaoAsync();
        }

        private async Task LoadPhongLookupAsync()
        {
            try
            {
                // Lấy danh sách phòng để hiển thị trong combobox
                var dsPhong = await BanGiaoTaiSanService.LayDanhSachPhongBanGiaoAsync();

                // Tạo danh sách mới để thêm item "Tất cả"
                var dsPhongFilter = new List<object>
                {
                    new { TenPhong = "Tất cả", MaPhong = (int?)null }
                };

                // Thêm các phòng từ database
                dsPhongFilter.AddRange(dsPhong.Select(p => new { TenPhong = p.TenPhong, MaPhong = (int?)p.MaPhong }));

                // Gán nguồn dữ liệu cho combobox
                cboPhong.ItemsSource = dsPhongFilter;
                cboPhong.SelectedIndex = 0; // Mặc định chọn "Tất cả"
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadBanGiaoAsync()
        {
            try
            {
                if (!QuyenNguoiDungHelper.HasPermission("btnBanGiaoTaiSan", "xem"))
                {
                    MessageBox.Show("Bạn không có xem bàn giao tài sản!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                // Lấy danh sách phiếu bàn giao từ database
                _dsBanGiao = await BanGiaoTaiSanService.LayDanhSachPhieuBanGiaoAsync();

                // Áp dụng bộ lọc
                var displayList = _dsBanGiao
                    .Where(p =>
                        // Lọc theo từ khóa
                        (string.IsNullOrEmpty(_keyword) ||
                         p.MaBanGiaoTS.ToString().Contains(_keyword, StringComparison.OrdinalIgnoreCase) ||
                         (p.TenNV != null && p.TenNV.Contains(_keyword, StringComparison.OrdinalIgnoreCase)) ||
                         (p.TenPhong != null && p.TenPhong.Contains(_keyword, StringComparison.OrdinalIgnoreCase)) ||
                         (p.NoiDung != null && p.NoiDung.Contains(_keyword, StringComparison.OrdinalIgnoreCase)) ||
                         (p.TrangThaiText != null && p.TrangThaiText.Contains(_keyword, StringComparison.OrdinalIgnoreCase)))
                        &&
                        // Lọc theo phòng
                        (_filterMaPhong == null || p.MaPhong == _filterMaPhong)
                        &&
                        // Lọc theo trạng thái
                        (string.IsNullOrEmpty(_filterTrangThai) ||
                         (_filterTrangThai == "Chờ duyệt" && p.TrangThai == null) ||
                         (_filterTrangThai == "Đã duyệt" && p.TrangThai == true) ||
                         (_filterTrangThai == "Từ chối duyệt" && p.TrangThai == false))
                    )
                    .OrderByDescending(p => p.NgayBanGiao)
                    .ToList();

                // Gán nguồn dữ liệu cho DataGrid
                dgBanGiao.ItemsSource = displayList;

                // Cập nhật trạng thái
                txtStatus.Text = $"Tổng số phiếu bàn giao: {displayList.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu phiếu bàn giao: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _keyword = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(_keyword))
            {
                popupSuggest.IsOpen = false;
                return;
            }

            // Tìm kiếm các gợi ý từ danh sách hiện có
            var suggestions = _dsBanGiao
                .Where(p =>
                    p.NoiDung?.Contains(_keyword, StringComparison.OrdinalIgnoreCase) == true ||
                    p.TenNV?.Contains(_keyword, StringComparison.OrdinalIgnoreCase) == true ||
                    p.TenPhong?.Contains(_keyword, StringComparison.OrdinalIgnoreCase) == true)
                .Select(p => p.NoiDung)
                .Distinct()
                .Take(10)
                .ToList();

            lstSuggest.ItemsSource = suggestions;
            popupSuggest.IsOpen = suggestions.Any();
        }

        private void lstSuggest_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (lstSuggest.SelectedItem != null)
            {
                txtSearch.Text = lstSuggest.SelectedItem.ToString();
                _keyword = txtSearch.Text;
                popupSuggest.IsOpen = false;
                _ = LoadBanGiaoAsync();
            }
        }

        private void cboTrangThai_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboTrangThai.SelectedItem is ComboBoxItem item && item.Content != null)
            {
                string selected = item.Content.ToString();
                _filterTrangThai = selected == "Tất cả" ? null : selected;
            }
            else
            {
                _filterTrangThai = null;
            }

            _ = LoadBanGiaoAsync();
        }

        private void cboPhong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboPhong.SelectedItem != null)
            {
                var selected = cboPhong.SelectedItem as dynamic;
                _filterMaPhong = selected.MaPhong;
            }
            else
            {
                _filterMaPhong = null;
            }

            _ = LoadBanGiaoAsync();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _keyword = txtSearch.Text.Trim();
            _ = LoadBanGiaoAsync();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            // Reset các bộ lọc
            _keyword = "";
            txtSearch.Text = "";
            cboPhong.SelectedIndex = 0;
            cboTrangThai.SelectedIndex = 0;

            // Làm mới dữ liệu
            _ = LoadBanGiaoAsync();
        }

        private void btnThemBanGiao_click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnBanGiaoTaiSan", "them"))
            {
                MessageBox.Show("Bạn không có quyền thêm bàn giao tài sản!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var lapPhieuBanGiaoWindow = new LapPhieuBanGiaoWindow();
            lapPhieuBanGiaoWindow.ShowDialog();

            // Làm mới dữ liệu sau khi đóng form
            _ = LoadBanGiaoAsync();
        }

        private void btnChiTiet_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnBanGiaoTaiSan", "sua"))
            {
                MessageBox.Show("Bạn không có quyền sửa bàn giao tài sản!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int maBanGiao))
            {
                var chiTietBanGiaoWindow = new ChiTietBanGiaoWindow(maBanGiao);
                chiTietBanGiaoWindow.ShowDialog();

                // Làm mới dữ liệu sau khi đóng form
                _ = LoadBanGiaoAsync();
            }
        }

        //private async void btnDuyet_Click(object sender, RoutedEventArgs e)
        //{
        //    if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int maBanGiao))
        //    {
        //        MessageBoxResult result = MessageBox.Show(
        //            $"Bạn có chắc chắn muốn duyệt phiếu bàn giao có mã '{maBanGiao}'?",
        //            "Xác nhận duyệt",
        //            MessageBoxButton.YesNo,
        //            MessageBoxImage.Question);

        //        if (result == MessageBoxResult.Yes)
        //        {
        //            try
        //            {
        //                bool success = await BanGiaoTaiSanService.DuyetPhieuBanGiaoAsync(maBanGiao, true);

        //                if (success)
        //                {
        //                    MessageBox.Show("Đã duyệt phiếu bàn giao thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        //                    await LoadBanGiaoAsync(); // Làm mới danh sách
        //                }
        //                else
        //                {
        //                    MessageBox.Show("Không thể duyệt phiếu bàn giao.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show($"Lỗi khi duyệt phiếu bàn giao: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        //            }
        //        }
        //    }
        //}

        //private async void btnTuChoi_Click(object sender, RoutedEventArgs e)
        //{
        //    if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int maBanGiao))
        //    {
        //        MessageBoxResult result = MessageBox.Show(
        //            $"Bạn có chắc chắn muốn từ chối phiếu bàn giao có mã '{maBanGiao}'?",
        //            "Xác nhận từ chối",
        //            MessageBoxButton.YesNo,
        //            MessageBoxImage.Question);

        //        if (result == MessageBoxResult.Yes)
        //        {
        //            try
        //            {
        //                bool success = await BanGiaoTaiSanService.DuyetPhieuBanGiaoAsync(maBanGiao, false);

        //                if (success)
        //                {
        //                    MessageBox.Show("Đã từ chối phiếu bàn giao thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        //                    await LoadBanGiaoAsync(); // Làm mới danh sách
        //                }
        //                else
        //                {
        //                    MessageBox.Show("Không thể từ chối phiếu bàn giao.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show($"Lỗi khi từ chối phiếu bàn giao: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        //            }
        //        }
        //    }
        //}

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnBanGiaoTaiSan", "xoa"))
            {
                MessageBox.Show("Bạn không có quyền xóa bàn giao tài sản!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int maBanGiao))
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa phiếu bàn giao có mã '{maBanGiao}'?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = await BanGiaoTaiSanService.XoaPhieuBanGiaoAsync(maBanGiao);

                        if (success)
                        {
                            MessageBox.Show("Đã xóa phiếu bàn giao thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadBanGiaoAsync(); // Làm mới danh sách
                        }
                        else
                        {
                            MessageBox.Show("Không thể xóa phiếu bàn giao.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa phiếu bàn giao: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgBanGiao.Items.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Lưu danh sách phiếu bàn giao",
                    FileName = $"DanhSachPhieuBanGiao_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var thongTinCongTy = ThongTinCongTyService.DocThongTinCongTy(); // Thông tin công ty

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("PhieuBanGiao");
                        int currentRow = 1;

                        // Logo công ty
                        if (!string.IsNullOrEmpty(thongTinCongTy.LogoPath) && File.Exists(thongTinCongTy.LogoPath))
                        {
                            worksheet.AddPicture(thongTinCongTy.LogoPath)
                                     .MoveTo(worksheet.Cell(currentRow, 1))
                                     .WithSize(140, 60);
                            worksheet.Row(currentRow).Height = 50;
                        }

                        // Thông tin công ty
                        worksheet.Cell(currentRow, 2).Value = thongTinCongTy.Ten;
                        worksheet.Cell(currentRow, 2).Style.Font.Bold = true;
                        worksheet.Cell(currentRow, 2).Style.Font.FontSize = 14;
                        worksheet.Range(currentRow, 2, currentRow, 7).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        currentRow++;

                        worksheet.Cell(currentRow, 2).Value = "Địa chỉ: " + thongTinCongTy.DiaChi;
                        worksheet.Range(currentRow, 2, currentRow, 7).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        currentRow++;

                        worksheet.Cell(currentRow, 2).Value = $"SĐT: {thongTinCongTy.SoDienThoai} - Email: {thongTinCongTy.Email}";
                        worksheet.Range(currentRow, 2, currentRow, 7).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        currentRow++;

                        worksheet.Cell(currentRow, 2).Value = "Mã số thuế: " + thongTinCongTy.MaSoThue;
                        worksheet.Range(currentRow, 2, currentRow, 7).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        currentRow += 2;

                        // Tiêu đề báo cáo
                        worksheet.Cell(currentRow, 1).Value = "DANH SÁCH PHIẾU BÀN GIAO TÀI SẢN";
                        worksheet.Range(currentRow, 1, currentRow, 7).Merge();
                        worksheet.Row(currentRow).Style.Font.Bold = true;
                        worksheet.Row(currentRow).Style.Font.FontSize = 16;
                        worksheet.Row(currentRow).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        currentRow += 2;

                        // Tiêu đề cột
                        string[] headers = { "Mã phiếu", "Ngày bàn giao", "Người lập phiếu", "Phòng", "Tòa nhà", "Nội dung", "Trạng thái" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cell(currentRow, i + 1).Value = headers[i];
                        }

                        var headerRange = worksheet.Range(currentRow, 1, currentRow, 7);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        currentRow++;

                        // Dữ liệu
                        foreach (BanGiaoTaiSanDTO item in dgBanGiao.Items)
                        {
                            worksheet.Cell(currentRow, 1).Value = item.MaBanGiaoTS;
                            worksheet.Cell(currentRow, 2).Value = item.NgayBanGiao;
                            worksheet.Cell(currentRow, 2).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                            worksheet.Cell(currentRow, 3).Value = item.TenNV;
                            worksheet.Cell(currentRow, 4).Value = item.TenPhong;
                            worksheet.Cell(currentRow, 5).Value = item.TenToaNha;
                            worksheet.Cell(currentRow, 6).Value = item.NoiDung;
                            worksheet.Cell(currentRow, 7).Value = item.TrangThaiText;

                            // Màu trạng thái
                            if (item.TrangThaiText == "Chờ duyệt")
                                worksheet.Cell(currentRow, 7).Style.Font.FontColor = XLColor.Orange;
                            else if (item.TrangThaiText == "Đã duyệt")
                                worksheet.Cell(currentRow, 7).Style.Font.FontColor = XLColor.Green;
                            else if (item.TrangThaiText == "Từ chối duyệt")
                                worksheet.Cell(currentRow, 7).Style.Font.FontColor = XLColor.Red;

                            currentRow++;
                        }

                        // Viền bảng
                        var dataRange = worksheet.Range(worksheet.Cell(currentRow - dgBanGiao.Items.Count - 1, 1), worksheet.Cell(currentRow - 1, 7));
                        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        // Ngày xuất
                        worksheet.Cell(currentRow + 2, 6).Value = "Ngày xuất:";
                        worksheet.Cell(currentRow + 2, 7).Value = DateTime.Now;
                        worksheet.Cell(currentRow + 2, 7).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
                        worksheet.Range(currentRow + 2, 6, currentRow + 2, 7).Style.Font.Italic = true;

                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(saveFileDialog.FileName);

                        MessageBox.Show($"Xuất Excel thành công! File được lưu tại:\n{saveFileDialog.FileName}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveFileDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        //private void btnXuatPDF_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Chức năng xuất PDF sẽ được phát triển trong phiên bản sau!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        //}
    }

    public class EqualityToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null && parameter == null)
                return Visibility.Visible;

            if (value != null && parameter != null && value.ToString() == parameter.ToString())
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}