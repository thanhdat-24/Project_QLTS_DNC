using Project_QLTS_DNC.ViewModel.Baotri;
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

        // Constructor gốc
        public LichSuBaoTriWindow()
        {
            InitializeComponent();
            _viewModel = new LichSuBaoTriViewModel();
            DataContext = _viewModel;
            Loaded += LichSuBaoTriWindow_Loaded;
        }

        // Constructor mới để hỗ trợ lọc theo mã tài sản
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
                await _viewModel.LoadDataAsync();

                // Thêm delay ngắn để đảm bảo DataGrid đã render xong
                await Task.Delay(100);

                // Cập nhật STT cho DataGrid
                UpdateSTTColumn();

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
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        // Các phương thức khác giữ nguyên như cũ
        private void UpdateSTTColumn()
        {
            try
            {
                if (dgLichSu.Items.Count > 0)
                {
                    // Đảm bảo DataGrid đã render
                    dgLichSu.UpdateLayout();

                    for (int i = 0; i < dgLichSu.Items.Count; i++)
                    {
                        // Lấy DataGridRow
                        if (dgLichSu.ItemContainerGenerator.ContainerFromIndex(i) is DataGridRow row)
                        {
                            if (dgLichSu.Columns.Count > 0 && dgLichSu.Columns[0] != null)
                            {
                                // Lấy cell của cột STT
                                if (dgLichSu.Columns[0].GetCellContent(row) is TextBlock cell)
                                {
                                    cell.Text = (i + 1).ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật STT: {ex.Message}");
            }
        }

        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtTimKiem.Text?.Trim().ToLower() ?? "";

            if (string.IsNullOrEmpty(keyword))
            {
                // Hiển thị tất cả
                dgLichSu.ItemsSource = _viewModel.DanhSachLichSu;
            }
            else
            {
                // Lọc theo từ khóa
                var filteredList = _viewModel.DanhSachLichSu.Where(item =>
                    (item.LichSu.TenTaiSan?.ToLower().Contains(keyword) ?? false) ||
                    (item.LichSu.SoSeri?.ToLower().Contains(keyword) ?? false) ||
                    (item.LichSu.TenNguoiThucHien?.ToLower().Contains(keyword) ?? false) ||
                    (item.LichSu.GhiChu?.ToLower().Contains(keyword) ?? false)
                ).ToList();

                dgLichSu.ItemsSource = filteredList;
            }

            // Cập nhật lại STT
            UpdateRowNumbers();
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
                dgLichSu.ItemsSource = _viewModel.DanhSachLichSu;
            }
            else
            {
                var filteredList = _viewModel.DanhSachLichSu.Where(item =>
                    item.LichSu.LoaiHoatDong == selectedValue
                ).ToList();

                dgLichSu.ItemsSource = filteredList;
            }

            // Cập nhật lại STT
            UpdateRowNumbers();
        }

        private void UpdateRowNumbers()
        {
            for (int i = 0; i < dgLichSu.Items.Count; i++)
            {
                var row = (DataGridRow)dgLichSu.ItemContainerGenerator.ContainerFromIndex(i);
                if (row != null)
                {
                    var cell = dgLichSu.Columns[0].GetCellContent(row) as TextBlock;
                    if (cell != null)
                    {
                        cell.Text = (i + 1).ToString();
                    }
                }
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
                            worksheet.Cell("A2").Value = $"Ngày xuất báo cáo: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}";
                            worksheet.Range("A2:G2").Merge();
                            worksheet.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            // Thêm header cho bảng
                            worksheet.Cell("A4").Value = "STT";
                            worksheet.Cell("B4").Value = "Ngày thực hiện";
                            worksheet.Cell("C4").Value = "Loại hoạt động";
                            worksheet.Cell("D4").Value = "Thông tin tài sản";
                            worksheet.Cell("E4").Value = "Số lượng tài sản";
                            worksheet.Cell("F4").Value = "Người thực hiện";
                            worksheet.Cell("G4").Value = "Ghi chú";

                            // Format header
                            var headerRange = worksheet.Range("A4:G4");
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            // Điền dữ liệu
                            var currentItems = dgLichSu.Items.Cast<LichSuBaoTriItem>().ToList();
                            for (int i = 0; i < currentItems.Count; i++)
                            {
                                var item = currentItems[i];
                                int row = i + 5; // Bắt đầu từ dòng 5

                                worksheet.Cell(row, 1).Value = i + 1; // STT
                                worksheet.Cell(row, 2).Value = item.LichSu.NgayThucHien.ToString("dd/MM/yyyy HH:mm:ss");
                                worksheet.Cell(row, 3).Value = item.LichSu.LoaiHoatDong;
                                worksheet.Cell(row, 4).Value = $"{item.LichSu.TenTaiSan} (SN: {item.LichSu.SoSeri})";
                                worksheet.Cell(row, 5).Value = item.SoLuongTaiSan;
                                worksheet.Cell(row, 6).Value = item.LichSu.TenNguoiThucHien;
                                worksheet.Cell(row, 7).Value = item.LichSu.GhiChu;
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