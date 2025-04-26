using iText.IO.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Properties;
using Project_QLTS_DNC.Models.NhaCungCap;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.PhieuNhapKho;
using Project_QLTS_DNC.Models.QLNhomTS;
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
using System.Windows.Shapes;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Borders;
using iText.Kernel.Geom;
using PdfParagraph = iText.Layout.Element.Paragraph;
using PdfBorder = iText.Layout.Borders.Border;
using PdfTable = iText.Layout.Element.Table;
using PdfTextAlignment = iText.Layout.Properties.TextAlignment;
using iText.Layout.Font;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for ChiTietMuaMoiView.xaml
    /// </summary>
    public partial class ChiTietMuaMoiView : Window
    {
        private readonly int _maPhieu;
        private Supabase.Client _client;
        public ChiTietMuaMoiView(int maPhieuMoi)
        {
            InitializeComponent();
            _maPhieu = maPhieuMoi;
            Loaded += ChiTietPhieuMuaMoiView_Loaded;
        }
        private async void ChiTietPhieuMuaMoiView_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            await LoadPhieuMuaMoiAsync();
            await LoadChiTietPhieuMuaMoiAsync();
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
        private async Task LoadPhieuMuaMoiAsync()
        {
            try
            {
                var result = await _client
                    .From<MuaMoiTS>()
                    .Where(x => x.MaPhieuDeNghi == _maPhieu)
                    .Get();

                var phieu = result.Models.FirstOrDefault();
                if (phieu == null)
                {
                    MessageBox.Show("Không tìm thấy phiếu nhập!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                txtMaPhieuMuaMoi.Text = phieu.MaPhieuDeNghi.ToString();
                txtNgayDeNghi.Text = phieu.NgayDeNghi.ToString("dd/MM/yyyy");
                txtDonViDeNghi.Text = phieu.DonViDeNghi.ToString();
                txtLyDo.Text = phieu.LyDo.ToString();
                txtGhiChu.Text = phieu.GhiChu.ToString();
                var nvResult = await _client.From<NhanVienModel>().Where(x => x.MaNV == phieu.MaNV).Get();
                txtNhanVienDeNghi.Text = nvResult.Models.FirstOrDefault()?.TenNV ?? "---";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load phiếu nhập: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private async Task LoadChiTietPhieuMuaMoiAsync()
        {
            try
            {
                // Đảm bảo _maPhieu có cùng kiểu dữ liệu với trường trong DB
                long maPhieuNumber;
                if (long.TryParse(_maPhieu.ToString(), out maPhieuNumber))
                {
                    var result = await _client
                        .From<ChiTietDeNghiMua>()
                        .Where(x => x.MaPhieuDeNghi == maPhieuNumber)
                        .Get();

                    if (result.Models != null && result.Models.Any())
                    {
                        var danhSachChiTiet = result.Models.ToList();
                        Console.WriteLine($"Đã tìm thấy {danhSachChiTiet.Count} chi tiết phiếu");
                        gridChiTiet.ItemsSource = danhSachChiTiet;
                    }
                    else
                    {
                        Console.WriteLine($"Không tìm thấy chi tiết phiếu với mã {_maPhieu}");
                        // Tạo một danh sách rỗng để tránh lỗi binding
                        gridChiTiet.ItemsSource = new List<ChiTietDeNghiMua>();
                    }
                }
                else
                {
                    Console.WriteLine($"Mã phiếu không phải là số: {_maPhieu}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chi tiết lỗi: {ex}");
                MessageBox.Show("Lỗi khi load chi tiết phiếu nhập: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a save file dialog
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"PhieuMuaMoiTaiSan_{txtMaPhieuMuaMoi.Text}_{DateTime.Now:yyyyMMdd}",
                    DefaultExt = ".pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    GeneratePDF(filePath);
                    MessageBox.Show("Xuất file PDF thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Ask if user wants to open the file
                    if (MessageBox.Show("Bạn có muốn mở file vừa tạo không?", "Xác nhận",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = filePath,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất PDF: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private Cell CreateCell(string text, PdfFont font, float fontSize, iText.Layout.Properties.TextAlignment alignment = iText.Layout.Properties.TextAlignment.LEFT)
        {
            Cell cell = new Cell().Add(new iText.Layout.Element.Paragraph(text).SetFont(font).SetFontSize(fontSize));
            cell.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
            cell.SetPadding(5);
            cell.SetTextAlignment(alignment);
            return cell;
        }
        private void GeneratePDF(string filePath)
        {
            // Create PDF writer and document
            PdfWriter writer = new PdfWriter(filePath);
            PdfDocument pdf = new PdfDocument(writer);
            iText.Layout.Document document = new iText.Layout.Document(pdf, PageSize.A4);
            document.SetMargins(36, 36, 36, 36);

            // Create fonts
            // Cách 1: Sử dụng font chuẩn với encoding hỗ trợ Unicode
            // Cách 2: Sử dụng font từ hệ thống
            string fontPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts) + "\\arial.ttf";
            PdfFont fontRegular = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);

            // Font đậm
            string fontBoldPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts) + "\\arialbd.ttf";
            PdfFont fontBold = PdfFontFactory.CreateFont(fontBoldPath, PdfEncodings.IDENTITY_H);


            // Title
            iText.Layout.Element.Paragraph title = new iText.Layout.Element.Paragraph("PHIẾU ĐỀ NGHỊ MUA MỚI TÀI SẢN")
                .SetFont(fontBold)
                .SetFontSize(16)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(title);
            DateTime currentDate = DateTime.Now;
            iText.Layout.Element.Paragraph dateInfo = new iText.Layout.Element.Paragraph($"Ngày xuất phiếu: {currentDate:dd/MM/yyyy}")
                .SetFont(fontRegular)
                .SetFontSize(10)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                .SetMarginBottom(20);
            document.Add(dateInfo);

            // Information table
            // Nếu muốn gần hơn nữa
            // Table chứa thông tin
            iText.Layout.Element.Table infoTable = new iText.Layout.Element.Table(1).UseAllAvailableWidth();

            // Row 1: Mã phiếu mua mới
            iText.Layout.Element.Paragraph p1 = new iText.Layout.Element.Paragraph();
            p1.Add(new iText.Layout.Element.Text("Mã phiếu mua mới:     ").SetFont(fontBold).SetFontSize(11));
            p1.Add(new iText.Layout.Element.Text(txtMaPhieuMuaMoi.Text).SetFont(fontRegular).SetFontSize(11));
            infoTable.AddCell(new Cell().Add(p1).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(5));

            // Row 2: Đơn vị đề nghị
            iText.Layout.Element.Paragraph p2 = new iText.Layout.Element.Paragraph();
            p2.Add(new iText.Layout.Element.Text("Đơn vị đề nghị:           ").SetFont(fontBold).SetFontSize(11));
            p2.Add(new iText.Layout.Element.Text(txtDonViDeNghi.Text).SetFont(fontRegular).SetFontSize(11));
            infoTable.AddCell(new Cell().Add(p2).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(5));

            // Row 3: Nhân viên đề nghị
            iText.Layout.Element.Paragraph p3 = new iText.Layout.Element.Paragraph();
            p3.Add(new iText.Layout.Element.Text("Nhân viên đề nghị:    ").SetFont(fontBold).SetFontSize(11));
            p3.Add(new iText.Layout.Element.Text(txtNhanVienDeNghi.Text).SetFont(fontRegular).SetFontSize(11));
            infoTable.AddCell(new Cell().Add(p3).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(5));

            // Row 4: Ngày đề nghị
            iText.Layout.Element.Paragraph p4 = new iText.Layout.Element.Paragraph();
            p4.Add(new iText.Layout.Element.Text("Ngày đề nghị:            ").SetFont(fontBold).SetFontSize(11));
            p4.Add(new iText.Layout.Element.Text(txtNgayDeNghi.Text).SetFont(fontRegular).SetFontSize(11));
            infoTable.AddCell(new Cell().Add(p4).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(5));

            // Row 5: Lý do
            iText.Layout.Element.Paragraph p5 = new iText.Layout.Element.Paragraph();
            p5.Add(new iText.Layout.Element.Text("Lý do:                          ").SetFont(fontBold).SetFontSize(11));
            p5.Add(new iText.Layout.Element.Text(txtLyDo.Text).SetFont(fontRegular).SetFontSize(11));
            infoTable.AddCell(new Cell().Add(p5).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(5));

            // Row 6: Ghi chú
            iText.Layout.Element.Paragraph p6 = new iText.Layout.Element.Paragraph();
            p6.Add(new iText.Layout.Element.Text("Ghi chú:                        ").SetFont(fontBold).SetFontSize(11));
            p6.Add(new iText.Layout.Element.Text(txtGhiChu.Text).SetFont(fontRegular).SetFontSize(11));
            infoTable.AddCell(new Cell().Add(p6).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(5));

            // Thêm table vào document
            document.Add(infoTable);
            document.Add(new iText.Layout.Element.Paragraph("\n"));


            // Asset details title
            iText.Layout.Element.Paragraph detailsTitle = new iText.Layout.Element.Paragraph("DANH SÁCH TÀI SẢN ĐỀ NGHỊ MUA MỚI")
                .SetFont(fontBold)
                .SetFontSize(13)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                .SetMarginBottom(10)
                .SetMarginTop(20);
            document.Add(detailsTitle);

            // Asset details table
            iText.Layout.Element.Table assetTable = new iText.Layout.Element.Table(5).UseAllAvailableWidth();

            // Table header
            assetTable.AddHeaderCell(CreateHeaderCell("Tên tài sản", fontBold));
            assetTable.AddHeaderCell(CreateHeaderCell("Số lượng", fontBold));
            assetTable.AddHeaderCell(CreateHeaderCell("Đơn vị tính", fontBold));
            assetTable.AddHeaderCell(CreateHeaderCell("Mô tả", fontBold));
            assetTable.AddHeaderCell(CreateHeaderCell("Giá dự kiến", fontBold));

            // Get assets from DataGrid
            var assets = gridChiTiet.ItemsSource as List<ChiTietDeNghiMua>;
            if (assets != null)
            {
                decimal totalAmount = 0;

                foreach (var asset in assets)
                {
                    assetTable.AddCell(CreateCellWithBorder(asset.TenTaiSan, fontRegular));
                    assetTable.AddCell(CreateCellWithBorder(asset.SoLuong.ToString(), fontRegular));
                    assetTable.AddCell(CreateCellWithBorder(asset.DonViTinh, fontRegular));
                    assetTable.AddCell(CreateCellWithBorder(asset.MoTa, fontRegular));
                    assetTable.AddCell(CreateCellWithBorder(FormatCurrency(asset.DuKienGia), fontRegular));

                    // Calculate total amount
                    totalAmount += asset.DuKienGia * asset.SoLuong;
                }

                // Total row
                assetTable.AddCell(CreateCellWithBorder("Tổng cộng:", fontBold, 4, iText.Layout.Properties.TextAlignment.RIGHT));
                assetTable.AddCell(CreateCellWithBorder(FormatCurrency(totalAmount), fontBold));
            }

            document.Add(assetTable);
            document.Add(new iText.Layout.Element.Paragraph("\n\n"));

            // Signatures
            iText.Layout.Element.Table signatureTable = new iText.Layout.Element.Table(3).UseAllAvailableWidth();

            signatureTable.AddCell(CreateSignatureCell("Người đề nghị", fontBold));
            signatureTable.AddCell(CreateSignatureCell("Người duyệt", fontBold));

            document.Add(signatureTable);


            // Close document
            document.Close();
        }

        // Helper methods for creating cells
        private Cell CreateCell(string text, PdfFont font, float fontSize)
        {
            Cell cell = new Cell().Add(new iText.Layout.Element.Paragraph(text).SetFont(font).SetFontSize(fontSize));
            cell.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
            cell.SetPadding(5);
            return cell;
        }

        private Cell CreateHeaderCell(string text, PdfFont font)
        {
            Cell cell = new Cell().Add(new iText.Layout.Element.Paragraph(text).SetFont(font).SetFontSize(11));
            cell.SetBackgroundColor(new DeviceRgb(220, 220, 220));
            cell.SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
            cell.SetPadding(5);
            return cell;
        }

        private Cell CreateCellWithBorder(string text, PdfFont font)
        {
            Cell cell = new Cell().Add(new iText.Layout.Element.Paragraph(text).SetFont(font).SetFontSize(10));
            cell.SetPadding(5);
            return cell;
        }

        private Cell CreateCellWithBorder(string text, PdfFont font, int colspan, iText.Layout.Properties.TextAlignment alignment)
        {
            Cell cell = new Cell(1, colspan).Add(new iText.Layout.Element.Paragraph(text).SetFont(font).SetFontSize(10));
            cell.SetTextAlignment(alignment);
            cell.SetPadding(5);
            return cell;
        }

        private Cell CreateSignatureCell(string text, PdfFont font)
        {
            Cell cell = new Cell();
            cell.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
            cell.SetPadding(5);
            cell.SetHeight(80);

            iText.Layout.Element.Paragraph title = new iText.Layout.Element.Paragraph(text)
                .SetFont(font)
                .SetFontSize(11)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            iText.Layout.Element.Paragraph signature = new iText.Layout.Element.Paragraph("(Ký, ghi rõ họ tên)")
                .SetFontSize(9)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                .SetItalic();

            cell.Add(title);
            cell.Add(signature);

            return cell;
        }

        private string FormatCurrency(decimal amount)
        {
            return string.Format("{0:#,##0} VNĐ", amount);
        }
    }
}