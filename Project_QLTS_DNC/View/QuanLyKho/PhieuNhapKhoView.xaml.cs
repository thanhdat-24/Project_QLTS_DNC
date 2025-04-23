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


namespace Project_QLTS_DNC.View.QuanLyKho
{
    public partial class PhieuNhapKhoView : UserControl
    {
        private Supabase.Client _client;
        private Dictionary<int, string> _khoLookup = new();
        private Dictionary<int, string> _nccLookup = new();
        private Dictionary<int, string> _nvLookup = new();

        private string _keyword = "";
        private int? _filterMaKho = null;
        private int? _filterMaNhom = null;
        private string _filterTrangThai = null;


        public PhieuNhapKhoView()
        {
            InitializeComponent();
            _ = Init();
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
                        .OrderByDescending(p => p.NgayNhap)
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
                    FileName = "DanhSachPhieuNhap.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Danh sách phiếu nhập");

                        // Header
                        string[] headers = { "Mã phiếu nhập", "Tên kho", "Người lập phiếu", "Nhà cung cấp", "Ngày nhập", "Tổng tiền", "Trạng thái" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cell(1, i + 1).Value = headers[i];
                            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                            worksheet.Cell(1, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(1, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        int row = 2;
                        foreach (dynamic item in dgSanPham.ItemsSource)
                        {
                            worksheet.Cell(row, 1).Value = item.MaPhieuNhap;
                            worksheet.Cell(row, 2).Value = item.TenKho;
                            worksheet.Cell(row, 3).Value = item.TenNhanVien;
                            worksheet.Cell(row, 4).Value = item.TenNCC;
                            worksheet.Cell(row, 5).Value = item.NgayNhap.ToString("dd/MM/yyyy");
                            worksheet.Cell(row, 6).Value = item.TongTien;
                            worksheet.Cell(row, 7).Value = item.TrangThai;

                            for (int i = 1; i <= 7; i++)
                            {
                                worksheet.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(row, i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                worksheet.Cell(row, i).Style.Alignment.Horizontal = (i == 6) ? XLAlignmentHorizontalValues.Right : XLAlignmentHorizontalValues.Center;
                            }

                            worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0 VNĐ";
                            row++;
                        }

                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show("Xuất file Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF Files|*.pdf",
                    Title = "Lưu file PDF",
                    FileName = "DanhSachPhieuNhap.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var document = new PdfSharpCore.Pdf.PdfDocument();
                    var page = document.AddPage();
                    page.Orientation = PdfSharpCore.PageOrientation.Landscape; 

                    var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
                    var fontHeader = new PdfSharpCore.Drawing.XFont("Arial", 14, PdfSharpCore.Drawing.XFontStyle.Bold);
                    var fontContent = new PdfSharpCore.Drawing.XFont("Arial", 10, PdfSharpCore.Drawing.XFontStyle.Regular);

                    double margin = 40;
                    double y = margin;
                    double rowHeight = 25;
                    double[] columnWidths = { 60, 100, 100, 150, 80, 90, 80 }; 

                    // Vẽ tiêu đề
                    gfx.DrawString("DANH SÁCH PHIẾU NHẬP", fontHeader, PdfSharpCore.Drawing.XBrushes.Black,
                        new PdfSharpCore.Drawing.XRect(0, y, page.Width, page.Height),
                        PdfSharpCore.Drawing.XStringFormats.TopCenter);

                    y += 40;

                    // Header bảng
                    string[] headers = { "Mã PN", "Tên kho", "Người lập", "Nhà cung cấp", "Ngày nhập", "Tổng tiền", "Trạng thái" };
                    double x = margin;
                    for (int i = 0; i < headers.Length; i++)
                    {
                        gfx.DrawRectangle(PdfSharpCore.Drawing.XPens.Black, PdfSharpCore.Drawing.XBrushes.LightBlue, x, y, columnWidths[i], rowHeight);
                        gfx.DrawString(headers[i], fontContent, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XRect(x, y, columnWidths[i], rowHeight), PdfSharpCore.Drawing.XStringFormats.Center);
                        x += columnWidths[i];
                    }

                    y += rowHeight;

                    // Nội dung bảng
                    foreach (dynamic item in dgSanPham.ItemsSource)
                    {
                        x = margin;
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
                            gfx.DrawRectangle(PdfSharpCore.Drawing.XPens.Black, x, y, columnWidths[i], rowHeight);
                            gfx.DrawString(data[i], fontContent, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XRect(x + 5, y + 5, columnWidths[i] - 10, rowHeight - 10), PdfSharpCore.Drawing.XStringFormats.TopLeft);
                            x += columnWidths[i];
                        }

                        y += rowHeight;

                        // Thêm trang mới nếu hết trang
                        if (y > page.Height - margin - rowHeight)
                        {
                            page = document.AddPage();
                            page.Orientation = PdfSharpCore.PageOrientation.Landscape; 
                            gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
                            y = margin;
                        }
                    }

                    document.Save(saveFileDialog.FileName);

                    MessageBox.Show("Xuất file PDF thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất PDF: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
