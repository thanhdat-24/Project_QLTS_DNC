using Project_QLTS_DNC.ViewModel.Baotri;
using Project_QLTS_DNC.Models.BaoTri;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using ClosedXML.Excel;
using System.IO;

namespace Project_QLTS_DNC.Views
{
    public partial class LichSuBaoTriWindow : Window
    {
        private readonly LichSuBaoTriViewModel _viewModel;
        private List<int> _filteredMaTaiSanList;
        private DateTime? _tuNgayFilter;
        private DateTime? _denNgayFilter;

        // Constructor gốc
        public LichSuBaoTriWindow()
        {
            InitializeComponent();
            _viewModel = new LichSuBaoTriViewModel();
            DataContext = _viewModel;
            Loaded += LichSuBaoTriWindow_Loaded;
        }

        // Constructor để hỗ trợ lọc theo mã tài sản
        public LichSuBaoTriWindow(List<int> maTaiSanList)
        {
            InitializeComponent();
            _filteredMaTaiSanList = maTaiSanList ?? new List<int>();
            _viewModel = new LichSuBaoTriViewModel(_filteredMaTaiSanList);
            DataContext = _viewModel;
            Loaded += LichSuBaoTriWindow_Loaded;

            // Cập nhật tiêu đề cửa sổ nếu có lọc
            if (_filteredMaTaiSanList.Count > 0)
            {
                this.Title = $"Lịch sử bảo trì - {_filteredMaTaiSanList.Count} tài sản được chọn";
            }
        }

        private async void LichSuBaoTriWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                // Debug info
                if (_filteredMaTaiSanList != null && _filteredMaTaiSanList.Count > 0)
                {
                    Console.WriteLine($"[DEBUG] LichSuBaoTriWindow_Loaded với {_filteredMaTaiSanList.Count} mã tài sản");
                }

                // Tải dữ liệu
                await _viewModel.LoadDataAsync();

                // Kiểm tra nếu không có dữ liệu
                if (_viewModel.DanhSachLichSu.Count == 0)
                {
                    Console.WriteLine("[DEBUG] Không có dữ liệu để hiển thị!");

                    if (_filteredMaTaiSanList != null && _filteredMaTaiSanList.Count > 0)
                    {
                        lblFilterInfo.Visibility = Visibility.Visible;
                        lblFilterInfo.Text = $"Không có lịch sử bảo trì cho {_filteredMaTaiSanList.Count} tài sản được chọn";
                    }
                }
                else
                {
                    // Hiển thị thông tin về bộ lọc (nếu có)
                    if (_filteredMaTaiSanList != null && _filteredMaTaiSanList.Count > 0)
                    {
                        lblFilterInfo.Visibility = Visibility.Visible;
                        lblFilterInfo.Text = $"Đang xem lịch sử cho {_filteredMaTaiSanList.Count} tài sản được chọn";
                    }
                    else
                    {
                        lblFilterInfo.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] LichSuBaoTriWindow_Loaded Exception: {ex.Message}");
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtTimKiem.Text?.Trim().ToLower() ?? "";

            if (string.IsNullOrEmpty(keyword))
            {
                // Hiển thị tất cả
                _viewModel.ReloadData();
            }
            else
            {
                // Lọc theo từ khóa
                _viewModel.FilterByKeyword(keyword);
            }
        }

        private void txtTimKiem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnTimKiem_Click(sender, e);
            }
        }

        private void cboLoaiHoatDong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || !(sender is ComboBox comboBox)) return;

            string selectedValue = (comboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (selectedValue == "Tất cả hoạt động")
            {
                _viewModel.ReloadData();
            }
            else if (!string.IsNullOrEmpty(selectedValue))
            {
                // Lọc theo từ khóa trong ghi chú
                string keyword = selectedValue.ToLower();
                _viewModel.FilterByKeyword(keyword);
            }
        }

        // Thêm phương thức xử lý sự kiện lọc theo ngày
        private void btnLocTheoNgay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Lấy giá trị từ DatePicker
                _tuNgayFilter = dpTuNgay.SelectedDate;
                _denNgayFilter = dpDenNgay.SelectedDate;

                // Nếu ngày kết thúc được chọn, đảm bảo là cuối ngày (23:59:59)
                if (_denNgayFilter.HasValue)
                {
                    _denNgayFilter = _denNgayFilter.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }

                // Hiển thị thông báo lọc
                UpdateFilterInfoText();

                // Nếu cả hai giá trị đều null, hiển thị tất cả dữ liệu
                if (!_tuNgayFilter.HasValue && !_denNgayFilter.HasValue)
                {
                    _viewModel.ReloadData();
                    lblFilterInfo.Visibility = Visibility.Collapsed;
                    return;
                }

                // Lọc dữ liệu - Sử dụng phương thức trong ViewModel
                _viewModel.FilterByDateRange(_tuNgayFilter, _denNgayFilter);

                // Hiển thị thông báo nếu không có dữ liệu
                if (_viewModel.DanhSachLichSu.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu nào trong khoảng thời gian đã chọn.",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lọc dữ liệu theo ngày: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Phương thức cập nhật thông báo lọc
        private void UpdateFilterInfoText()
        {
            // Định dạng chuỗi thông báo lọc
            string filterText = "";

            if (_tuNgayFilter.HasValue && _denNgayFilter.HasValue)
            {
                filterText = $"Đang lọc từ ngày {_tuNgayFilter.Value:dd/MM/yyyy} đến ngày {_denNgayFilter.Value:dd/MM/yyyy}";
            }
            else if (_tuNgayFilter.HasValue)
            {
                filterText = $"Đang lọc từ ngày {_tuNgayFilter.Value:dd/MM/yyyy} đến nay";
            }
            else if (_denNgayFilter.HasValue)
            {
                filterText = $"Đang lọc đến ngày {_denNgayFilter.Value:dd/MM/yyyy}";
            }

            // Cập nhật và hiển thị thông báo lọc
            if (!string.IsNullOrEmpty(filterText))
            {
                lblFilterInfo.Text = filterText;
                lblFilterInfo.Visibility = Visibility.Visible;
            }
            else
            {
                lblFilterInfo.Visibility = Visibility.Collapsed;
            }
        }

        private void btnXuatBaoCao_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Hiển thị dialog chọn vị trí lưu file
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    DefaultExt = "xlsx",
                    FileName = $"LichSuBaoTri_{DateTime.Now:yyyyMMdd}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    try
                    {
                        // Tạo file Excel
                        using (var workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add("Lịch Sử Bảo Trì");

                            // Thêm tiêu đề
                            worksheet.Cell("A1").Value = "LỊCH SỬ BẢO TRÌ TÀI SẢN";
                            worksheet.Cell("A1").Style.Font.Bold = true;
                            worksheet.Cell("A1").Style.Font.FontSize = 16;
                            worksheet.Range("A1:G1").Merge();
                            worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            // Thêm thông tin ngày xuất báo cáo
                            worksheet.Cell("A2").Value = $"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                            worksheet.Range("A2:G2").Merge();
                            worksheet.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            // Thêm thông tin lọc (nếu có)
                            if (_tuNgayFilter.HasValue || _denNgayFilter.HasValue || (_filteredMaTaiSanList != null && _filteredMaTaiSanList.Count > 0))
                            {
                                string filterInfo = "Bộ lọc: ";

                                if (_tuNgayFilter.HasValue && _denNgayFilter.HasValue)
                                {
                                    filterInfo += $"Từ ngày {_tuNgayFilter.Value:dd/MM/yyyy} đến ngày {_denNgayFilter.Value:dd/MM/yyyy}";
                                }
                                else if (_tuNgayFilter.HasValue)
                                {
                                    filterInfo += $"Từ ngày {_tuNgayFilter.Value:dd/MM/yyyy}";
                                }
                                else if (_denNgayFilter.HasValue)
                                {
                                    filterInfo += $"Đến ngày {_denNgayFilter.Value:dd/MM/yyyy}";
                                }

                                if (_filteredMaTaiSanList != null && _filteredMaTaiSanList.Count > 0)
                                {
                                    if (_tuNgayFilter.HasValue || _denNgayFilter.HasValue)
                                    {
                                        filterInfo += ", ";
                                    }
                                    filterInfo += $"{_filteredMaTaiSanList.Count} tài sản được chọn";
                                }

                                worksheet.Cell("A3").Value = filterInfo;
                                worksheet.Range("A3:G3").Merge();
                                worksheet.Cell("A3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                worksheet.Cell("A3").Style.Font.Italic = true;
                            }

                            // Thêm header cho bảng
                            worksheet.Cell("A4").Value = "STT";
                            worksheet.Cell("B4").Value = "Ngày thực hiện";
                            worksheet.Cell("C4").Value = "Mã tài sản";
                            worksheet.Cell("D4").Value = "Tên tài sản";
                            worksheet.Cell("E4").Value = "Số seri";
                            worksheet.Cell("F4").Value = "Tình trạng tài sản";
                            worksheet.Cell("G4").Value = "Người thực hiện";
                            worksheet.Cell("H4").Value = "Ghi chú";

                            // Format header
                            var headerRange = worksheet.Range("A4:H4");
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            // Điền dữ liệu
                            var currentItems = _viewModel.DanhSachLichSu.ToList();
                            for (int i = 0; i < currentItems.Count; i++)
                            {
                                var item = currentItems[i];
                                int row = i + 5; // Bắt đầu từ dòng 5

                                worksheet.Cell(row, 1).Value = i + 1; // STT
                                worksheet.Cell(row, 2).Value = item.NgayThucHien.ToString("dd/MM/yyyy HH:mm:ss");
                                worksheet.Cell(row, 3).Value = item.MaTaiSan?.ToString() ?? "";
                                worksheet.Cell(row, 4).Value = item.TenTaiSan;
                                worksheet.Cell(row, 5).Value = item.SoSeri;
                                worksheet.Cell(row, 6).Value = item.TinhTrangTaiSan;
                                worksheet.Cell(row, 7).Value = item.TenNguoiThucHien;
                                worksheet.Cell(row, 8).Value = item.GhiChu;
                            }

                            // Auto-fit columns
                            worksheet.Columns().AdjustToContents();

                            // Lưu file
                            workbook.SaveAs(saveFileDialog.FileName);
                        }

                        MessageBox.Show("Xuất báo cáo thành công!", "Thông báo",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất báo cáo: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDong_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}