using ClosedXML.Excel;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Project_QLTS_DNC.Helpers;
using Project_QLTS_DNC.Models.NhaCungCap;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.PhieuNhapKho;
using Project_QLTS_DNC.Services;
using Supabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using Project_QLTS_DNC.Models.ThongTinCongTy;

namespace Project_QLTS_DNC.View.QuanLyKho
{
    public partial class PhieuNhapKhoView : UserControl
    {
        private Supabase.Client _client;
        private Dictionary<int, string> _khoLookup = new();
        private Dictionary<int, string> _nccLookup = new();
        private Dictionary<int, string> _nvLookup = new();
        private ThongTinCongTy _thongTinCongTy;

        private string _keyword = "";
        private int? _filterMaKho = null;
        private int? _filterMaNhom = null;
        private string _filterTrangThai = null;

        public PhieuNhapKhoView()
        {
            InitializeComponent();
            _ = Init();
            LoadThongTinCongTy();
        }

        private async Task Init()
        {
            await InitializeSupabaseAsync();
            await LoadKhoLookupAsync();
            await LoadNhaCungCapLookupAsync();
            await LoadNhanVienLookupAsync();
            await LoadPhieuNhapAsync();
        }

        private async Task InitializeSupabaseAsync()
        {
            var supabaseUrl = "https://hoybfwnugefnpctgghha.supabase.co";
            var supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhveWJmd251Z2VmbnBjdGdnaGhhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDQxMDQ4OTEsImV4cCI6MjA1OTY4MDg5MX0.KxNfiOUFXHGgqZf3b3xOk6BR4sllMZG_-W-y_OPUwCI";

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false,
                AutoRefreshToken = false
            };

            _client = new Supabase.Client(supabaseUrl, supabaseKey, options);
            await _client.InitializeAsync();
        }

        private async Task LoadKhoLookupAsync()
        {
            var result = await _client.From<Kho>().Get();
            _khoLookup = result.Models.ToDictionary(k => k.MaKho, k => k.TenKho);

            cboTenKho.ItemsSource = _khoLookup.Select(k => new ComboBoxItem
            {
                Content = k.Value,
                Tag = k.Key
            });
        }

        private async Task LoadNhaCungCapLookupAsync()
        {
            var result = await _client.From<NhaCungCapClass>().Get();
            _nccLookup = result.Models.ToDictionary(n => n.MaNCC, n => n.TenNCC);
        }

        private async Task LoadNhanVienLookupAsync()
        {
            var result = await _client.From<NhanVienModel>().Get();
            _nvLookup = result.Models.ToDictionary(nv => nv.MaNV, nv => nv.TenNV);
        }

        private void LoadThongTinCongTy()
        {
            string jsonPath = "thongtincongty.json";
            if (File.Exists(jsonPath))
            {
                string json = File.ReadAllText(jsonPath);
                _thongTinCongTy = JsonSerializer.Deserialize<ThongTinCongTy>(json);
            }
            else
            {
                _thongTinCongTy = new ThongTinCongTy();
            }
        }

        private async Task LoadPhieuNhapAsync()
        {
            try
            {
                var result = await _client.From<PhieuNhap>().Get();

                if (result.Models.Any())
                {
                    var displayList = result.Models
                        .Where(p =>
                            // Lọc theo kho
                            (_filterMaKho == null || p.MaKho == _filterMaKho) &&

                            // Lọc theo từ khóa tìm kiếm
                            (string.IsNullOrEmpty(_keyword) ||
                                p.MaPhieuNhap.ToString().Contains(_keyword, StringComparison.OrdinalIgnoreCase) ||
                                (_khoLookup.ContainsKey(p.MaKho) && _khoLookup[p.MaKho].Contains(_keyword, StringComparison.OrdinalIgnoreCase)) ||
                                (_nccLookup.ContainsKey(p.MaNCC) && _nccLookup[p.MaNCC].Contains(_keyword, StringComparison.OrdinalIgnoreCase)) ||
                                (_nvLookup.ContainsKey(p.MaNV) && _nvLookup[p.MaNV].Contains(_keyword, StringComparison.OrdinalIgnoreCase)) ||
                                (p.TrangThai == null && "Chờ duyệt".Contains(_keyword, StringComparison.OrdinalIgnoreCase)) ||
                                (p.TrangThai == true && "Đã duyệt".Contains(_keyword, StringComparison.OrdinalIgnoreCase)) ||
                                (p.TrangThai == false && "Từ chối duyệt".Contains(_keyword, StringComparison.OrdinalIgnoreCase)))
                        &&
                            // Lọc theo trạng thái
                            (string.IsNullOrEmpty(_filterTrangThai) ||
                                (_filterTrangThai == "Chờ duyệt" && p.TrangThai == null) ||
                                (_filterTrangThai == "Đã duyệt" && p.TrangThai == true) ||
                                (_filterTrangThai == "Từ chối duyệt" && p.TrangThai == false))
                        )
                        .Select(p => new
                        {
                            MaPhieuNhap = p.MaPhieuNhap,
                            TenKho = _khoLookup.ContainsKey(p.MaKho) ? _khoLookup[p.MaKho] : $"#{p.MaKho}",
                            TenNhanVien = _nvLookup.ContainsKey(p.MaNV) ? _nvLookup[p.MaNV] : $"#{p.MaNV}",
                            TenNCC = _nccLookup.ContainsKey(p.MaNCC) ? _nccLookup[p.MaNCC] : $"#{p.MaNCC}",
                            NgayNhap = p.NgayNhap,
                            TongTien = p.TongTien,
                            TrangThai = p.TrangThai switch
                            {
                                null => "Chờ duyệt",
                                true => "Đã duyệt",
                                false => "Từ chối duyệt"
                            }
                        })
                        .OrderByDescending(p => p.MaPhieuNhap)
                        .ToList();

                    dgSanPham.ItemsSource = displayList;
                }
                else
                {
                    dgSanPham.ItemsSource = null;
                    MessageBox.Show("Không có dữ liệu phiếu nhập.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu phiếu nhập: {ex.Message}");
            }
        }

        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _keyword = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(_keyword))
            {
                popupSuggest.IsOpen = false;
                return;
            }

            var result = await _client
                .From<ChiTietPhieuNhap>()
                .Select("ten_tai_san")
                .Filter("ten_tai_san", Supabase.Postgrest.Constants.Operator.ILike, $"%{_keyword}%")
                .Limit(10)
                .Get();

            var suggestions = result.Models
                .Select(x => x.TenTaiSan)
                .Distinct()
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
                _ = LoadPhieuNhapAsync();
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

            _ = LoadPhieuNhapAsync();
        }

        private void cboTenKho_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboTenKho.SelectedItem is ComboBoxItem item && int.TryParse(item.Tag?.ToString(), out int maKho))
                _filterMaKho = maKho;
            else
                _filterMaKho = null;

            _ = LoadPhieuNhapAsync();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _keyword = txtSearch.Text.Trim();
            _ = LoadPhieuNhapAsync();
        }

        private void btnThemKho_click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnNhapKho", "them"))
            {
                MessageBox.Show("Bạn không có quyền thêm nhập kho!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var form = new PhieuNhapKhoInput();
            form.ShowDialog();
            _ = LoadPhieuNhapAsync();
        }

        // Phương thức để mở form ThemKho và truyền dữ liệu kho cần sửa
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnNhapKho", "sua"))
            {
                MessageBox.Show("Bạn không có quyền sửa nhập kho!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Lấy kho được chọn từ DataContext
            Button button = sender as Button;
            Kho selectedKho = button.DataContext as Kho;

            if (selectedKho != null)
            {
                // Mở form ThemKho và truyền dữ liệu kho cần sửa
                ThemKho themKhoForm = new ThemKho(selectedKho);  // Truyền kho hiện tại vào form ThemKho
                themKhoForm.ShowDialog();  // Hiển thị form ThemKho
            }
        }
        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnNhapKho", "xoa"))
            {
                MessageBox.Show("Bạn không có quyền xóa nhập kho!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int maPhieuNhap))
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa phiếu nhập có mã '{maPhieuNhap}'?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var client = await SupabaseService.GetClientAsync();

                        // Truy vấn lại đối tượng PhieuNhap từ mã
                        var getResult = await client
                            .From<PhieuNhap>()
                            .Filter("ma_phieu_nhap", Supabase.Postgrest.Constants.Operator.Equals, maPhieuNhap)
                            .Get();

                        var phieuNhapToDelete = getResult.Models.FirstOrDefault();
                        if (phieuNhapToDelete != null)
                        {
                            await client.From<PhieuNhap>().Delete(phieuNhapToDelete);

                            MessageBox.Show("Đã xóa phiếu nhập thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadPhieuNhapAsync(); // Làm mới danh sách
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy phiếu nhập cần xóa.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa phiếu nhập: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Không có phiếu nhập được chọn để xóa.");
            }
        }

        private void btnChiTiet_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnNhapKho", "xem"))
            {
                MessageBox.Show("Bạn không có quyền xem chi tiết nhập kho!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (sender is Button btn && btn.Tag is int maPhieuNhap)
            {
                var form = new ChiTietPhieuNhapView(maPhieuNhap);
                form.ShowDialog();
            }
            else
            {
                MessageBox.Show("Không thể lấy thông tin phiếu nhập.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgSanPham.ItemsSource == null)
                {
                    MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Lưu file Excel",
                    FileName = $"DanhSachPhieuNhap_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var thongTinCongTy = ThongTinCongTyService.DocThongTinCongTy(); // Lấy thông tin công ty

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Danh sách phiếu nhập");
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
                        worksheet.Cell(currentRow, 1).Value = "DANH SÁCH PHIẾU NHẬP KHO";
                        worksheet.Range(currentRow, 1, currentRow, 7).Merge();
                        worksheet.Row(currentRow).Style.Font.Bold = true;
                        worksheet.Row(currentRow).Style.Font.FontSize = 16;
                        worksheet.Row(currentRow).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        currentRow += 2;

                        // Header
                        string[] headers = { "Mã phiếu nhập", "Tên kho", "Người lập phiếu", "Nhà cung cấp", "Ngày nhập", "Tổng tiền", "Trạng thái" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cell(currentRow, i + 1).Value = headers[i];
                            worksheet.Cell(currentRow, i + 1).Style.Font.Bold = true;
                            worksheet.Cell(currentRow, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                            worksheet.Cell(currentRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(currentRow, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        currentRow++;
                        int dataStartRow = currentRow;

                        foreach (dynamic item in dgSanPham.ItemsSource)
                        {
                            worksheet.Cell(currentRow, 1).Value = item.MaPhieuNhap;
                            worksheet.Cell(currentRow, 2).Value = item.TenKho;
                            worksheet.Cell(currentRow, 3).Value = item.TenNhanVien;
                            worksheet.Cell(currentRow, 4).Value = item.TenNCC;
                            worksheet.Cell(currentRow, 5).Value = item.NgayNhap.ToString("dd/MM/yyyy");
                            worksheet.Cell(currentRow, 6).Value = item.TongTien;
                            worksheet.Cell(currentRow, 7).Value = item.TrangThai;

                            for (int i = 1; i <= 7; i++)
                            {
                                worksheet.Cell(currentRow, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(currentRow, i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                worksheet.Cell(currentRow, i).Style.Alignment.Horizontal = (i == 6) ? XLAlignmentHorizontalValues.Right : XLAlignmentHorizontalValues.Center;
                            }

                            worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0 VNĐ";
                            currentRow++;
                        }

                        // Viền bảng
                        var dataRange = worksheet.Range(dataStartRow - 1, 1, currentRow - 1, 7);
                        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        // Ngày xuất
                        worksheet.Cell(currentRow + 2, 6).Value = "Ngày xuất:";
                        worksheet.Cell(currentRow + 2, 7).Value = DateTime.Now;
                        worksheet.Cell(currentRow + 2, 7).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
                        worksheet.Range(currentRow + 2, 6, currentRow + 2, 7).Style.Font.Italic = true;

                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(saveFileDialog.FileName);

                        MessageBox.Show("Xuất file Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

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
                MessageBox.Show("Lỗi khi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void btnExportPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgSanPham.ItemsSource == null)
                {
                    MessageBox.Show("Không có dữ liệu để in.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    FlowDocument flowDocument = new FlowDocument
                    {
                        FontFamily = new FontFamily("Times New Roman"), // <- Font chữ
                        FontSize = 12,                                  // <- Cỡ chữ
                        PagePadding = new Thickness(50),
                        ColumnWidth = double.PositiveInfinity
                    };


                    // ===== HEADER: Logo + Thông tin công ty (giống hình mẫu) =====
                    if (_thongTinCongTy != null)
                    {
                        Grid headerGrid = new Grid
                        {
                            Margin = new Thickness(0, 0, 0, 20)
                        };
                        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) }); // Logo
                        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });     // Thông tin

                        // Logo
                        if (!string.IsNullOrEmpty(_thongTinCongTy.LogoPath) && File.Exists(_thongTinCongTy.LogoPath))
                        {
                            Image logo = new Image
                            {
                                Source = new BitmapImage(new Uri(_thongTinCongTy.LogoPath)),
                                Width = 100,
                                Height = 100,
                                Stretch = Stretch.UniformToFill,
                                Margin = new Thickness(0, 0, 10, 0)
                            };
                            Grid.SetColumn(logo, 0);
                            headerGrid.Children.Add(logo);
                        }

                        // Thông tin công ty
                        StackPanel infoPanel = new StackPanel
                        {
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        infoPanel.Children.Add(new TextBlock
                        {
                            Text = _thongTinCongTy.Ten,
                            FontSize = 16,
                            FontWeight = FontWeights.Bold
                        });

                        infoPanel.Children.Add(new TextBlock
                        {
                            Text = "Địa chỉ: " + _thongTinCongTy.DiaChi,
                            FontSize = 12
                        });

                        infoPanel.Children.Add(new TextBlock
                        {
                            Text = $"SĐT: {_thongTinCongTy.SoDienThoai} - Email: {_thongTinCongTy.Email}",
                            FontSize = 12
                        });

                        infoPanel.Children.Add(new TextBlock
                        {
                            Text = "Mã số thuế: " + _thongTinCongTy.MaSoThue,
                            FontSize = 12
                        });

                      

                        Grid.SetColumn(infoPanel, 1);
                        headerGrid.Children.Add(infoPanel);

                        flowDocument.Blocks.Add(new BlockUIContainer(headerGrid));

                      
                    }


                    //// ===== Dòng kẻ ngang =====
                    //flowDocument.Blocks.Add(new Paragraph(new Run(new string('-', 100))) { Margin = new Thickness(0, 0, 0, 10) });

                    // ===== Tiêu đề =====
                    Paragraph title = new Paragraph(new Run("DANH SÁCH PHIẾU NHẬP"))
                    {
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 20)
                    };
                    flowDocument.Blocks.Add(title);


                    // Thêm ngày in ở trên cùng
                    System.Windows.Documents.Paragraph datePara = new System.Windows.Documents.Paragraph();
                    datePara.TextAlignment = System.Windows.TextAlignment.Right;
                    datePara.Margin = new System.Windows.Thickness(0, 0, 0, 20);
                    System.Windows.Documents.Run dateRun = new System.Windows.Documents.Run("Ngày in: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    dateRun.FontStyle = System.Windows.FontStyles.Italic;
                    dateRun.FontSize = 10;
                    datePara.Inlines.Add(dateRun);
                    flowDocument.Blocks.Add(datePara);

                    // ===== Bảng dữ liệu =====
                    Table table = new Table
                    {
                        CellSpacing = 0,
                        BorderThickness = new Thickness(1),
                        BorderBrush = Brushes.Black
                    };

                    string[] headers = { "Mã PN", "Tên kho", "Người lập", "Nhà cung cấp", "Ngày nhập", "Tổng tiền", "Trạng thái" };
                    double[] columnWidths = { 60, 100, 100, 150, 80, 90, 80 };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        table.Columns.Add(new TableColumn { Width = new GridLength(columnWidths[i]) });
                    }

                    TableRow headerRow = new TableRow { Background = Brushes.LightBlue };
                    foreach (string h in headers)
                    {
                        TableCell cell = new TableCell(new Paragraph(new Run(h))
                        {
                            FontWeight = FontWeights.Bold,
                            TextAlignment = TextAlignment.Center
                        })
                        {
                            BorderThickness = new Thickness(1),
                            BorderBrush = Brushes.Black
                        };
                        headerRow.Cells.Add(cell);
                    }

                    table.RowGroups.Add(new TableRowGroup());
                    table.RowGroups[0].Rows.Add(headerRow);

                    // Dòng dữ liệu
                    foreach (dynamic item in dgSanPham.ItemsSource)
                    {
                        TableRow row = new TableRow();
                        string[] data = {
                    item.MaPhieuNhap.ToString(),
                    item.TenKho,
                    item.TenNhanVien,
                    item.TenNCC,
                    ((DateTime)item.NgayNhap).ToString("dd/MM/yyyy"),
                    string.Format("{0:N0} VNĐ", item.TongTien),
                    item.TrangThai.ToString()
                };

                        for (int i = 0; i < data.Length; i++)
                        {
                            TableCell cell = new TableCell(new Paragraph(new Run(data[i]))
                            {
                                TextAlignment = i == 5 ? TextAlignment.Right : TextAlignment.Left
                            })
                            {
                                BorderThickness = new Thickness(1),
                                BorderBrush = Brushes.Black
                            };
                            row.Cells.Add(cell);
                        }

                        table.RowGroups[0].Rows.Add(row);
                    }

                    flowDocument.Blocks.Add(table);


                    // ===== FOOTER: Người lập phiếu - Xác nhận quản lý =====
                    Grid footerGrid = new Grid
                    {
                        Margin = new Thickness(0, 40, 0, 0) // cách xa phần nội dung
                    };
                    footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    // Cột: Người lập phiếu
                    StackPanel nguoiLapPhieuPanel = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    nguoiLapPhieuPanel.Children.Add(new TextBlock
                    {
                        Text = "Người lập phiếu",
                        FontWeight = FontWeights.Bold,
                        TextAlignment = TextAlignment.Center
                    });

                    nguoiLapPhieuPanel.Children.Add(new TextBlock
                    {
                        Text = "(Ký và ghi rõ họ tên)",
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(0, 20, 0, 0),
                        TextAlignment = TextAlignment.Center
                    });
                    Grid.SetColumn(nguoiLapPhieuPanel, 0);
                    footerGrid.Children.Add(nguoiLapPhieuPanel);

                    // Cột: Xác nhận của quản lý
                    StackPanel quanLyPanel = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    quanLyPanel.Children.Add(new TextBlock
                    {
                        Text = "Xác nhận của quản lý",
                        FontWeight = FontWeights.Bold,
                        TextAlignment = TextAlignment.Center
                    });

                    quanLyPanel.Children.Add(new TextBlock
                    {
                        Text = "(Ký và ghi rõ họ tên)",
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(0, 20, 0, 0),
                        TextAlignment = TextAlignment.Center
                    });
                    Grid.SetColumn(quanLyPanel, 1);
                    footerGrid.Children.Add(quanLyPanel);

                    // Thêm footer vào FlowDocument
                    flowDocument.Blocks.Add(new BlockUIContainer(footerGrid));

                    // In tài liệu
                    DocumentPaginator paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                    printDialog.PrintDocument(paginator, "Danh sách phiếu nhập");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi in: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
