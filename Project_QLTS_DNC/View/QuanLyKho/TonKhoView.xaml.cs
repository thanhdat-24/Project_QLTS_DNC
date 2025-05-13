using ClosedXML.Excel;
using Project_QLTS_DNC.Helpers;
using Project_QLTS_DNC.Models.BanGiaoTaiSan;
using Project_QLTS_DNC.Models.Kho;
using Project_QLTS_DNC.Models.PhieuNhapKho;
using Project_QLTS_DNC.Models.PhieuXuatKho;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Services;
using Supabase;
using System;
using System.Collections.Generic;
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
using Project_QLTS_DNC.Models.Kho;
using Project_QLTS_DNC.Models.QLNhomTS;
using Supabase;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Models.PhieuXuatKho;
using Project_QLTS_DNC.Models.BanGiaoTaiSan;
using Project_QLTS_DNC.Models.PhieuNhapKho;
using Project_QLTS_DNC.Helpers;
using System.Text.Json;
using System.IO;
using Project_QLTS_DNC.Models.ThongTinCongTy;

namespace Project_QLTS_DNC.View.QuanLyKho
{
    /// <summary>
    /// Interaction logic for TonKhoView.xaml
    /// </summary>
    public partial class TonKhoView : UserControl
    {
        private Supabase.Client _client;
        private Dictionary<int, string> _khoLookup = new();
        private Dictionary<int, string> _nhomLookup = new();
        private List<TonKho> DanhSachTonKhoGoc = new();
        private ThongTinCongTy _thongTinCongTy;

        public TonKhoView()
        {
            InitializeComponent();
            Loaded += TonKhoView_Loaded;
            this.IsVisibleChanged += TonKhoView_IsVisibleChanged;
            LoadThongTinCongTy();
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

        private async Task InitializeSupabaseAsync()
        {
            string supabaseUrl = "https://hoybfwnugefnpctgghha.supabase.co";
            string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhveWJmd251Z2VmbnBjdGdnaGhhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDQxMDQ4OTEsImV4cCI6MjA1OTY4MDg5MX0.KxNfiOUFXHGgqZf3b3xOk6BR4sllMZG_-W-y_OPUwCI";

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false,
                AutoRefreshToken = false
            };

            _client = new Supabase.Client(supabaseUrl, supabaseKey, options);
            await _client.InitializeAsync();
        }

        private async void TonKhoView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
            {
                if (_client == null)
                    await InitializeSupabaseAsync(); // ✅ đảm bảo khởi tạo Supabase

                await CapNhatSoLuongXuatTonKhoAsync();
                await LoadTonKhoAsync();
            }
        }

        private async Task LoadTonKhoAsync()
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnTonKho", "xem"))
            {
                MessageBox.Show("Bạn không có quyền xem tồn kho!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var tonKhoResult = await _client.From<TonKho>().Get();
            var khoResult = await _client.From<Kho>().Get();
            var nhomResult = await _client.From<NhomTaiSan>().Get();

            var listTonKho = tonKhoResult.Models;
            _khoLookup = khoResult.Models.ToDictionary(k => k.MaKho, k => k.TenKho);
            _nhomLookup = nhomResult.Models.ToDictionary(n => n.MaNhomTS, n => n.TenNhom);

            foreach (var item in listTonKho)
            {
                item.TenKho = _khoLookup.TryGetValue(item.MaKho, out var tenKho) ? tenKho : "";
                item.TenNhomTS = _nhomLookup.TryGetValue(item.MaNhomTS, out var tenNhom) ? tenNhom : "";
            }

            DanhSachTonKhoGoc = listTonKho;
            dgTonKho.ItemsSource = listTonKho;

            cboTenKho.ItemsSource = new List<object> { new { MaKho = (int?)null, TenKho = "Tất cả" } }
                .Concat(_khoLookup.Select(k => new { MaKho = (int?)k.Key, TenKho = k.Value }))
                .ToList();

            cboTenNhomTS.ItemsSource = new List<object> { new { MaNhomTS = (int?)null, TenNhomTS = "Tất cả" } }
                .Concat(_nhomLookup.Select(n => new { MaNhomTS = (int?)n.Key, TenNhomTS = n.Value }))
                .ToList();
        }

        private void LocDuLieu()
        {
            string tuKhoa = txtSearch.Text.Trim().ToLower();
            int? maKho = cboTenKho.SelectedValue as int?;
            int? maNhomTS = cboTenNhomTS.SelectedValue as int?;

            var ketQua = DanhSachTonKhoGoc.AsEnumerable();

            if (!string.IsNullOrEmpty(tuKhoa))
            {
                ketQua = ketQua.Where(x =>
                    (x.TenKho?.ToLower().Contains(tuKhoa) ?? false) ||
                    (x.TenNhomTS?.ToLower().Contains(tuKhoa) ?? false) ||
                    x.MaTonKho.ToString().Contains(tuKhoa)
                );
            }

            if (maKho.HasValue)
                ketQua = ketQua.Where(x => x.MaKho == maKho.Value);

            if (maNhomTS.HasValue)
                ketQua = ketQua.Where(x => x.MaNhomTS == maNhomTS.Value);

            var list = ketQua.ToList();
            dgTonKho.ItemsSource = list;

            // Tính tổng
            int tongNhap = list.Sum(x => x.SoLuongNhap);
            int tongXuat = list.Sum(x => x.SoLuongXuat);
            int tongTon = list.Sum(x => x.SoLuongTon);

            txtTongSoLuong.Text = $"Tổng số lượng nhập: {tongNhap:N0} | xuất: {tongXuat:N0} | tồn: {tongTon:N0}";
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) => LocDuLieu();
        private void btnSearch_Click(object sender, RoutedEventArgs e) => LocDuLieu();
        private void cboTenKho_SelectionChanged(object sender, SelectionChangedEventArgs e) => LocDuLieu();
        private void cboTenNhomTS_SelectionChanged(object sender, SelectionChangedEventArgs e) => LocDuLieu();

        private async Task CapNhatSoLuongXuatTonKhoAsync()
        {
            try
            {
                string sql = @"
           WITH ton_bg AS (
    SELECT 
        ctpn.ma_nhom_ts,
        COUNT(DISTINCT ts.ma_tai_san) AS so_luong
    FROM 
        chitietbangiao ctbg
    JOIN 
        bangiaotaisan bgt ON ctbg.ma_bang_giao_ts = bgt.ma_bang_giao_ts
    JOIN 
        taisan ts ON ctbg.ma_tai_san = ts.ma_tai_san
    JOIN 
        chitietphieunhap ctpn ON ts.ma_chi_tiet_pn = ctpn.ma_chi_tiet_pn
    WHERE 
        bgt.trang_thai IS NULL OR bgt.trang_thai = TRUE
    GROUP BY 
        ctpn.ma_nhom_ts
),
ton_xk AS (
    SELECT 
        ctpn.ma_nhom_ts,
        COUNT(DISTINCT ts.ma_tai_san) AS so_luong
    FROM 
        chitietxuatkho ctxk
    JOIN 
        xuatkho xk ON ctxk.ma_phieu_xuat = xk.ma_phieu_xuat
    JOIN 
        taisan ts ON ctxk.ma_tai_san = ts.ma_tai_san
    JOIN 
        chitietphieunhap ctpn ON ts.ma_chi_tiet_pn = ctpn.ma_chi_tiet_pn
    WHERE 
        xk.trang_thai IS NULL OR xk.trang_thai = TRUE
    GROUP BY 
        ctpn.ma_nhom_ts
),
tong_xuat AS (
    SELECT 
        COALESCE(bg.ma_nhom_ts, xk.ma_nhom_ts) AS ma_nhom_ts,
        COALESCE(bg.so_luong, 0) + COALESCE(xk.so_luong, 0) AS tong
    FROM 
        ton_bg bg
    FULL JOIN 
        ton_xk xk ON bg.ma_nhom_ts = xk.ma_nhom_ts
)

UPDATE tonkho tk
SET 
    so_luong_xuat = tong_xuat.tong,
    ngay_cap_nhat = CURRENT_TIMESTAMP
FROM tong_xuat
WHERE tk.ma_nhom_ts = tong_xuat.ma_nhom_ts;


                        ";

                var parameters = new Dictionary<string, object> { { "query", sql } };
                var response = await _client.Rpc("cap_nhat_so_luong_xuat_tonkho", new Dictionary<string, object>());

                // Sau đó tự xử lý kết quả `response`
                // rồi update từng dòng TonKho (MaNhomTS) theo SoLuongXuat mới
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi truy vấn tồn kho: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void TonKhoView_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            await CapNhatSoLuongXuatTonKhoAsync(); // ⚡ Cập nhật luôn
            await LoadTonKhoAsync();               // ⚡ Load DataGrid
        }

        private void btnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgTonKho.ItemsSource == null)
                {
                    MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    Title = "Lưu file Excel",
                    FileName = "DanhSachTonKho.xlsx"
                };

                if (dialog.ShowDialog() == true)
                {
                    var thongTinCongTy = ThongTinCongTyService.DocThongTinCongTy(); // Gọi thông tin công ty

                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Tồn Kho");
                        int currentRow = 1;

                        // Logo
                        if (!string.IsNullOrEmpty(thongTinCongTy.LogoPath) && File.Exists(thongTinCongTy.LogoPath))
                        {
                            worksheet.AddPicture(thongTinCongTy.LogoPath)
                                     .MoveTo(worksheet.Cell(currentRow, 1))
                                     .WithSize(140, 60);
                            worksheet.Row(currentRow).Height = 50;
                        }

                        // Tên công ty
                        worksheet.Cell(currentRow, 2).Value = thongTinCongTy.Ten;
                        worksheet.Cell(currentRow, 2).Style.Font.Bold = true;
                        worksheet.Cell(currentRow, 2).Style.Font.FontSize = 14;
                        worksheet.Range(currentRow, 2, currentRow, 6).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        currentRow++;

                        // Địa chỉ
                        worksheet.Cell(currentRow, 2).Value = "Địa chỉ: " + thongTinCongTy.DiaChi;
                        worksheet.Range(currentRow, 2, currentRow, 6).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        currentRow++;

                        // Liên hệ
                        worksheet.Cell(currentRow, 2).Value = $"SĐT: {thongTinCongTy.SoDienThoai} - Email: {thongTinCongTy.Email}";
                        worksheet.Range(currentRow, 2, currentRow, 6).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        currentRow++;

                        // Mã số thuế
                        worksheet.Cell(currentRow, 2).Value = "Mã số thuế: " + thongTinCongTy.MaSoThue;
                        worksheet.Range(currentRow, 2, currentRow, 6).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        currentRow += 2;

                        // Tiêu đề
                        worksheet.Cell(currentRow, 1).Value = "DANH SÁCH TỒN KHO";
                        worksheet.Range(currentRow, 1, currentRow, 6).Merge();
                        worksheet.Row(currentRow).Style.Font.Bold = true;
                        worksheet.Row(currentRow).Style.Font.FontSize = 16;
                        worksheet.Row(currentRow).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        currentRow += 2;

                        // Header
                        string[] headers = { "Mã Tồn Kho", "Tên Kho", "Tên Nhóm TS", "Số Lượng Nhập", "Số Lượng Xuất", "Số Lượng Tồn" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cell(currentRow, i + 1).Value = headers[i];
                            worksheet.Cell(currentRow, i + 1).Style.Font.Bold = true;
                            worksheet.Cell(currentRow, i + 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
                            worksheet.Cell(currentRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(currentRow, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        int dataStartRow = currentRow + 1;
                        int row = dataStartRow;

                        foreach (dynamic item in dgTonKho.ItemsSource)
                        {
                            worksheet.Cell(row, 1).Value = item.MaTonKho;
                            worksheet.Cell(row, 2).Value = item.TenKho;
                            worksheet.Cell(row, 3).Value = item.TenNhomTS;
                            worksheet.Cell(row, 4).Value = item.SoLuongNhap;
                            worksheet.Cell(row, 5).Value = item.SoLuongXuat;
                            worksheet.Cell(row, 6).Value = item.SoLuongTon;

                            for (int i = 1; i <= 6; i++)
                            {
                                worksheet.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(row, i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                worksheet.Cell(row, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            }

                            row++;
                        }

                        // Ngày xuất
                        int exportRow = row + 2;
                        worksheet.Cell(exportRow, 5).Value = "Ngày xuất:";
                        worksheet.Cell(exportRow, 6).Value = DateTime.Now;
                        worksheet.Cell(exportRow, 6).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
                        worksheet.Range(exportRow, 5, exportRow, 6).Style.Font.Italic = true;

                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(dialog.FileName);

                        MessageBox.Show("Xuất file Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = dialog.FileName,
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


        private void btnExportPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgTonKho.ItemsSource == null)
                {
                    MessageBox.Show("Không có dữ liệu để in.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    FlowDocument flowDocument = new FlowDocument();
                    flowDocument.PagePadding = new Thickness(50);
                    flowDocument.ColumnWidth = double.PositiveInfinity;

                    // Add company logo if exists
                    if (_thongTinCongTy != null && !string.IsNullOrEmpty(_thongTinCongTy.LogoPath) && File.Exists(_thongTinCongTy.LogoPath))
                    {
                        Image logo = new Image
                        {
                            Source = new BitmapImage(new Uri(_thongTinCongTy.LogoPath)),
                            Width = 100,
                            Height = 100,
                            Stretch = Stretch.Uniform
                        };
                        flowDocument.Blocks.Add(new BlockUIContainer(logo));
                    }

                    // Add company information
                    if (_thongTinCongTy != null)
                    {
                        Paragraph header = new Paragraph();
                        header.Inlines.Add(new Run(_thongTinCongTy.Ten) { FontSize = 20, FontWeight = FontWeights.Bold });
                        header.Inlines.Add(new LineBreak());
                        header.Inlines.Add(new Run($"Mã số thuế: {_thongTinCongTy.MaSoThue}"));
                        header.Inlines.Add(new LineBreak());
                        header.Inlines.Add(new Run($"Địa chỉ: {_thongTinCongTy.DiaChi}"));
                        header.Inlines.Add(new LineBreak());
                        header.Inlines.Add(new Run($"Số điện thoại: {_thongTinCongTy.SoDienThoai}"));
                        header.Inlines.Add(new LineBreak());
                        header.Inlines.Add(new Run($"Email: {_thongTinCongTy.Email}"));
                        header.Inlines.Add(new LineBreak());
                        header.Inlines.Add(new Run($"Người đại diện: {_thongTinCongTy.NguoiDaiDien}"));
                        header.Inlines.Add(new LineBreak());
                        if (!string.IsNullOrEmpty(_thongTinCongTy.GhiChu))
                        {
                            header.Inlines.Add(new Run($"Ghi chú: {_thongTinCongTy.GhiChu}"));
                        }
                        header.Margin = new Thickness(0, 0, 0, 20);
                        flowDocument.Blocks.Add(header);
                    }

                    // Add a line separator
                    flowDocument.Blocks.Add(new Paragraph(new Run(new string('-', 100))));

                    // Add title
                    Paragraph title = new Paragraph(new Run("DANH SÁCH TỒN KHO"))
                    {
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 20)
                    };
                    flowDocument.Blocks.Add(title);

                    // Create table
                    Table table = new Table();
                    table.CellSpacing = 0;
                    table.BorderThickness = new Thickness(1);
                    table.BorderBrush = Brushes.Black;

                    // Add columns
                    string[] headers = { "Mã Tồn Kho", "Tên Kho", "Tên Nhóm TS", "Số Nhập", "Số Xuất", "Số Tồn" };
                    double[] columnWidths = { 70, 110, 170, 90, 90, 90 };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        table.Columns.Add(new TableColumn { Width = new GridLength(columnWidths[i]) });
                    }

                    // Add header row
                    TableRow headerRow = new TableRow();
                    headerRow.Background = Brushes.LightGreen;
                    foreach (string header in headers)
                    {
                        TableCell cell = new TableCell(new Paragraph(new Run(header))
                        {
                            FontWeight = FontWeights.Bold,
                            TextAlignment = TextAlignment.Center
                        });
                        cell.BorderThickness = new Thickness(1);
                        cell.BorderBrush = Brushes.Black;
                        headerRow.Cells.Add(cell);
                    }
                    table.RowGroups.Add(new TableRowGroup());
                    table.RowGroups[0].Rows.Add(headerRow);

                    // Add data rows
                    foreach (dynamic item in dgTonKho.ItemsSource)
                    {
                        TableRow row = new TableRow();
                        string[] data = {
                            item.MaTonKho?.ToString() ?? "",
                            item.TenKho ?? "",
                            item.TenNhomTS ?? "",
                            item.SoLuongNhap?.ToString() ?? "0",
                            item.SoLuongXuat?.ToString() ?? "0",
                            item.SoLuongTon?.ToString() ?? "0"
                        };

                        for (int i = 0; i < data.Length; i++)
                        {
                            TableCell cell = new TableCell(new Paragraph(new Run(data[i]))
                            {
                                TextAlignment = TextAlignment.Center
                            });
                            cell.BorderThickness = new Thickness(1);
                            cell.BorderBrush = Brushes.Black;
                            row.Cells.Add(cell);
                        }
                        table.RowGroups[0].Rows.Add(row);
                    }

                    flowDocument.Blocks.Add(table);

                    // Print the document
                    DocumentPaginator paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                    printDialog.PrintDocument(paginator, "Danh sách tồn kho");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi in: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
