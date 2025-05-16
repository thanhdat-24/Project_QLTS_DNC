using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.PhieuNhapKho;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.NhaCungCap;
using Project_QLTS_DNC.Models.Kho;
using Supabase;
using Project_QLTS_DNC.Models.QLNhomTS;

using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.IO.Font;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using iText.Kernel.Geom;
using System.Windows.Documents;
using System.Windows.Media;
using System.IO;
using System.Text.Json;
using Project_QLTS_DNC.Models.ThongTinCongTy;

using System.Windows.Media.Imaging;


namespace Project_QLTS_DNC.View.QuanLyKho
{
    public partial class ChiTietPhieuNhapView : Window
    {
        private readonly int _maPhieuNhap;
        private Supabase.Client _client;

        public ChiTietPhieuNhapView(int maPhieuNhap)
        {
            InitializeComponent();
            _maPhieuNhap = maPhieuNhap;
            Loaded += ChiTietPhieuNhapView_Loaded;
        }

        private async void ChiTietPhieuNhapView_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            await LoadPhieuNhapAsync();
            await LoadChiTietPhieuNhapAsync();
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

        private async Task LoadPhieuNhapAsync()
        {
            try
            {
                var result = await _client
                    .From<PhieuNhap>()
                    .Where(x => x.MaPhieuNhap == _maPhieuNhap)
                    .Get();

                var phieu = result.Models.FirstOrDefault();
                if (phieu == null)
                {
                    MessageBox.Show("Không tìm thấy phiếu nhập!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                txtMaPhieuNhap.Text = phieu.MaPhieuNhap.ToString();
                txtNgayNhap.Text = phieu.NgayNhap.ToString("dd/MM/yyyy");
                txtTongTien.Text = phieu.TongTien.ToString("N0") + " VNĐ";


                var khoResult = await _client.From<Kho>().Where(x => x.MaKho == phieu.MaKho).Get();
                txtKhoNhap.Text = khoResult.Models.FirstOrDefault()?.TenKho ?? "---";

                var nccResult = await _client.From<NhaCungCapClass>().Where(x => x.MaNCC == phieu.MaNCC).Get();
                txtNhaCungCap.Text = nccResult.Models.FirstOrDefault()?.TenNCC ?? "---";

                var nvResult = await _client.From<NhanVienModel>().Where(x => x.MaNV == phieu.MaNV).Get();
                txtNhanVienNhap.Text = nvResult.Models.FirstOrDefault()?.TenNV ?? "---";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load phiếu nhập: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadChiTietPhieuNhapAsync()
        {
            try
            {
                var result = await _client
                    .From<ChiTietPhieuNhap>()
                    .Where(x => x.MaPhieuNhap == _maPhieuNhap)
                    .Get();

                var danhSachChiTiet = result.Models.ToList();

                if (danhSachChiTiet.Count > 0)
                {
                    // Lấy toàn bộ danh sách nhóm tài sản
                    var nhomResult = await _client.From<NhomTaiSan>().Get();
                    var nhomLookup = nhomResult.Models.ToDictionary(n => n.MaNhomTS, n => n.TenNhom);

                    foreach (var item in danhSachChiTiet)
                    {
                        item.TenTaiSan = string.IsNullOrEmpty(item.TenTaiSan) ? "N/A" : item.TenTaiSan;

                        // 👉 Gán tên nhóm tài sản cho từng item
                        nhomLookup.TryGetValue(item.MaNhomTS, out string tenNhom);
                        item.TenNhomTS = tenNhom ?? "---";
                    }
                }

                gridChiTiet.ItemsSource = danhSachChiTiet;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load chi tiết phiếu nhập: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TaoFilePDF_PhieuNhap(string filePath)
        {
            using (PdfWriter writer = new PdfWriter(filePath))
            using (PdfDocument pdf = new PdfDocument(writer))
            {
                var document = new iText.Layout.Document(pdf, PageSize.A4);
                document.SetMargins(36, 36, 36, 36);

                var font = PdfFontFactory.CreateFont(@"C:\Windows\Fonts\arial.ttf", PdfEncodings.IDENTITY_H);

                document.Add(new iText.Layout.Element.Paragraph("PHIẾU NHẬP KHO")
                    .SetFont(font)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(16)
                    .SetBold()
                    .SetMarginBottom(10));

                // Thông tin phiếu
                var info = new iText.Layout.Element.Table(new float[] { 120, 300 });
                info.SetWidth(UnitValue.CreatePercentValue(100));

                info.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Mã phiếu nhập:").SetFont(font).SetBold()).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                info.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(txtMaPhieuNhap.Text).SetFont(font)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                info.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Ngày nhập:").SetFont(font).SetBold()).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                info.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(txtNgayNhap.Text).SetFont(font)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                info.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Nhà cung cấp:").SetFont(font).SetBold()).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                info.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(txtNhaCungCap.Text).SetFont(font)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                info.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Kho nhập:").SetFont(font).SetBold()).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                info.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(txtKhoNhap.Text).SetFont(font)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                info.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Nhân viên nhập:").SetFont(font).SetBold()).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                info.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(txtNhanVienNhap.Text).SetFont(font)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                info.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Tổng tiền:").SetFont(font).SetBold()).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                info.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(txtTongTien.Text).SetFont(font)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(info);
                document.Add(new iText.Layout.Element.Paragraph("\n").SetFont(font));

                // Bảng chi tiết
                var table = new iText.Layout.Element.Table(new float[] { 30, 120, 100, 70, 70, 50 });
                table.SetWidth(UnitValue.CreatePercentValue(100));
                string[] headers = { "STT", "Tên tài sản", "Nhóm tài sản", "Số lượng", "Đơn giá", "QLR" };

                foreach (var header in headers)
                {
                    table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(header).SetFont(font).SetBold())
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                }

                var danhSach = gridChiTiet.ItemsSource.Cast<ChiTietPhieuNhap>().ToList();
                for (int i = 0; i < danhSach.Count; i++)
                {
                    var item = danhSach[i];
                    table.AddCell(new iText.Layout.Element.Paragraph((i + 1).ToString()).SetFont(font).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                    table.AddCell(new iText.Layout.Element.Paragraph(item.TenTaiSan).SetFont(font));
                    table.AddCell(new iText.Layout.Element.Paragraph(item.TenNhomTS).SetFont(font));
                    table.AddCell(new iText.Layout.Element.Paragraph(item.SoLuong.ToString()).SetFont(font).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                    table.AddCell(new iText.Layout.Element.Paragraph((item.DonGia ?? 0).ToString("N0")).SetFont(font).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
                    table.AddCell(new iText.Layout.Element.Paragraph(item.CanQuanLyRieng ? "✓" : "").SetFont(font).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                }

                document.Add(table);
                document.Add(new iText.Layout.Element.Paragraph("\n\n").SetFont(font));

                // Chữ ký
                var sign = new iText.Layout.Element.Table(new float[] { 1, 1 });
                sign.SetWidth(UnitValue.CreatePercentValue(100));

                string tenNV = txtNhanVienNhap.Text;

                sign.AddCell(new iText.Layout.Element.Cell()
                    .Add(new iText.Layout.Element.Paragraph("NGƯỜI LẬP PHIẾU").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetFont(font).SetBold())
                    .Add(new iText.Layout.Element.Paragraph("(Ký, họ tên)").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetFont(font).SetItalic().SetFontSize(10))
                    .Add(new iText.Layout.Element.Paragraph("\n\n\n"))
                    .Add(new iText.Layout.Element.Paragraph(tenNV).SetFont(font).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                );

                sign.AddCell(new iText.Layout.Element.Cell()
                    .Add(new iText.Layout.Element.Paragraph("NGƯỜI PHÊ DUYỆT").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetFont(font).SetBold())
                    .Add(new iText.Layout.Element.Paragraph("(Ký, họ tên)").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetFont(font).SetItalic().SetFontSize(10))
                    .Add(new iText.Layout.Element.Paragraph("\n\n\n"))
                    .Add(new iText.Layout.Element.Paragraph(""))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(sign);
                document.Add(new iText.Layout.Element.Paragraph($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm:ss}").SetFont(font).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetFontSize(8).SetItalic());

                document.Close();
            }
        }

        private FlowDocument TaoFlowDocument_PhieuNhap()
        {
            var doc = new System.Windows.Documents.FlowDocument();
            doc.FontFamily = new System.Windows.Media.FontFamily("Arial");
            doc.FontSize = 13;
            doc.PagePadding = new Thickness(40);

            // Đọc thông tin công ty
            ThongTinCongTy thongTin = null;
            try
            {
                if (File.Exists("thongtincongty.json"))
                {
                    string json = File.ReadAllText("thongtincongty.json");
                    thongTin = JsonSerializer.Deserialize<Project_QLTS_DNC.Models.ThongTinCongTy.ThongTinCongTy>(json);
                }
            }
            catch { /* Có thể log lỗi nếu cần */ }

            // Thêm header công ty lên đầu
            if (thongTin != null)
            {
                // Grid 2 cột: logo - thông tin
                var headerGrid = new Grid
                {
                    Margin = new Thickness(0, 0, 0, 20),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                // Logo
                if (!string.IsNullOrEmpty(thongTin.LogoPath))
                {
                    string logoPath = thongTin.LogoPath;
                    if (!System.IO.Path.IsPathRooted(logoPath))
                        logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logoPath);

                    if (File.Exists(logoPath))
                    {
                        try
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.UriSource = new Uri(logoPath, UriKind.Absolute);
                            bitmap.EndInit();

                            var logoImage = new System.Windows.Controls.Image
                            {
                                Source = bitmap,
                                Height = 80,
                                Width = 80,
                                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                                Margin = new Thickness(0, 0, 10, 0)
                            };
                            Grid.SetColumn(logoImage, 0);
                            headerGrid.Children.Add(logoImage);
                        }
                        catch { }
                    }
                }

                // Thông tin công ty
                var infoPanel = new StackPanel
                {
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                infoPanel.Children.Add(new TextBlock
                {
                    Text = thongTin.Ten ?? "",
                    FontWeight = System.Windows.FontWeights.Bold,
                    FontSize = 20
                });
                infoPanel.Children.Add(new TextBlock { Text = $"Địa chỉ: {thongTin.DiaChi ?? ""}", Margin = new Thickness(0, 2, 0, 0) });
                infoPanel.Children.Add(new TextBlock { Text = $"Mã số thuế: {thongTin.MaSoThue ?? ""}", Margin = new Thickness(0, 2, 0, 0) });
                infoPanel.Children.Add(new TextBlock { Text = $"SĐT: {thongTin.SoDienThoai ?? ""}", Margin = new Thickness(0, 2, 0, 0) });
                infoPanel.Children.Add(new TextBlock { Text = $"Email: {thongTin.Email ?? ""}", Margin = new Thickness(0, 2, 0, 0) });
                Grid.SetColumn(infoPanel, 1);
                headerGrid.Children.Add(infoPanel);

                doc.Blocks.Add(new BlockUIContainer(headerGrid));
            }

            // Tiêu đề căn giữa
            var title = new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run("PHIẾU NHẬP KHO"));
            title.FontSize = 22;
            title.FontWeight = System.Windows.FontWeights.Bold;
            title.TextAlignment = System.Windows.TextAlignment.Center;
            title.Margin = new Thickness(0, 10, 0, 20);
            doc.Blocks.Add(title);



            // Bảng thông tin phiếu
            var infoTable = new System.Windows.Documents.Table();
            infoTable.CellSpacing = 0;
            infoTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new GridLength(140) });
            infoTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new GridLength(350) });
            var infoGroup = new System.Windows.Documents.TableRowGroup();
            infoTable.RowGroups.Add(infoGroup);

            void AddInfoRow(string label, string value)
            {
                var row = new System.Windows.Documents.TableRow();
                var cell1 = new System.Windows.Documents.TableCell(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(label)) { FontWeight = System.Windows.FontWeights.Bold });
                cell1.BorderBrush = System.Windows.Media.Brushes.Transparent;
                cell1.BorderThickness = new Thickness(0);
                cell1.Padding = new Thickness(0, 2, 5, 2);

                var cell2 = new System.Windows.Documents.TableCell(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(value)));
                cell2.BorderBrush = System.Windows.Media.Brushes.Transparent;
                cell2.BorderThickness = new Thickness(0);
                cell2.Padding = new Thickness(0, 2, 0, 2);

                row.Cells.Add(cell1);
                row.Cells.Add(cell2);
                infoGroup.Rows.Add(row);
            }
            AddInfoRow("Mã phiếu nhập:", txtMaPhieuNhap.Text);
            AddInfoRow("Ngày nhập:", txtNgayNhap.Text);
            AddInfoRow("Nhà cung cấp:", txtNhaCungCap.Text);
            AddInfoRow("Kho nhập:", txtKhoNhap.Text);
            AddInfoRow("Nhân viên nhập:", txtNhanVienNhap.Text);
            AddInfoRow("Tổng tiền:", txtTongTien.Text);

            infoTable.Margin = new Thickness(0, 0, 0, 20);
            doc.Blocks.Add(infoTable);

            // Bảng chi tiết căn giữa
            var detailSection = new Section();
            detailSection.TextAlignment = System.Windows.TextAlignment.Center;

            var detailTable = new System.Windows.Documents.Table();
            detailTable.CellSpacing = 0;
            detailTable.Margin = new Thickness(0, 0, 0, 30);
            var colWidths = new[] { 40.0, 140.0, 120.0, 70.0, 90.0, 50.0 };
            foreach (var w in colWidths)
                detailTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new GridLength(w) });

            var headerGroup = new System.Windows.Documents.TableRowGroup();
            var headerRow = new System.Windows.Documents.TableRow();
            string[] headers = { "STT", "Tên tài sản", "Nhóm tài sản", "Số lượng", "Đơn giá", "QLR" };
            foreach (var h in headers)
            {
                var cell = new System.Windows.Documents.TableCell(
                    new System.Windows.Documents.Paragraph(
                        new System.Windows.Documents.Run(h))
                    {
                        FontWeight = System.Windows.FontWeights.Bold,
                        TextAlignment = System.Windows.TextAlignment.Center
                    });
                cell.Background = System.Windows.Media.Brushes.LightGray;
                cell.BorderBrush = System.Windows.Media.Brushes.Black;
                cell.BorderThickness = new Thickness(1);
                cell.Padding = new Thickness(3, 4, 3, 4);
                headerRow.Cells.Add(cell);
            }
            headerGroup.Rows.Add(headerRow);
            detailTable.RowGroups.Add(headerGroup);

            var dataGroup = new System.Windows.Documents.TableRowGroup();
            var danhSach = gridChiTiet.ItemsSource.Cast<ChiTietPhieuNhap>().ToList();
            for (int i = 0; i < danhSach.Count; i++)
            {
                var item = danhSach[i];
                var row = new System.Windows.Documents.TableRow();

                System.Windows.Documents.TableCell MakeCell(string text, System.Windows.TextAlignment align = System.Windows.TextAlignment.Left, bool bold = false)
                {
                    var para = new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(text)) { TextAlignment = align };
                    if (bold) para.FontWeight = System.Windows.FontWeights.Bold;
                    var cell = new System.Windows.Documents.TableCell(para);
                    cell.BorderBrush = System.Windows.Media.Brushes.Black;
                    cell.BorderThickness = new Thickness(1);
                    cell.Padding = new Thickness(3, 2, 3, 2);
                    return cell;
                }

                row.Cells.Add(MakeCell((i + 1).ToString(), System.Windows.TextAlignment.Center));
                row.Cells.Add(MakeCell(item.TenTaiSan));
                row.Cells.Add(MakeCell(item.TenNhomTS));
                row.Cells.Add(MakeCell(item.SoLuong.ToString(), System.Windows.TextAlignment.Center));
                row.Cells.Add(MakeCell((item.DonGia ?? 0).ToString("N0"), System.Windows.TextAlignment.Right));
                row.Cells.Add(MakeCell(item.CanQuanLyRieng ? "✓" : "", System.Windows.TextAlignment.Center));
                dataGroup.Rows.Add(row);
            }
            detailTable.RowGroups.Add(dataGroup);

            // Thêm bảng vào section căn giữa, rồi thêm section vào doc
            detailSection.Blocks.Add(detailTable);
            doc.Blocks.Add(detailSection);


            // Chữ ký
            var signTable = new System.Windows.Documents.Table();
            signTable.Columns.Add(new System.Windows.Documents.TableColumn());
            signTable.Columns.Add(new System.Windows.Documents.TableColumn());
            var signGroup = new System.Windows.Documents.TableRowGroup();
            var signRow = new System.Windows.Documents.TableRow();

            var cell1 = new System.Windows.Documents.TableCell();
            var para1 = new System.Windows.Documents.Paragraph();
            para1.Inlines.Add(new System.Windows.Documents.Run("NGƯỜI LẬP PHIẾU") { FontWeight = System.Windows.FontWeights.Bold });
            para1.Inlines.Add(new System.Windows.Documents.LineBreak());
            para1.Inlines.Add(new System.Windows.Documents.Run("(Ký, họ tên)") { FontStyle = System.Windows.FontStyles.Italic, FontSize = 11 });
            para1.Inlines.Add(new System.Windows.Documents.LineBreak());
            para1.Inlines.Add(new System.Windows.Documents.LineBreak());
            para1.Inlines.Add(new System.Windows.Documents.LineBreak());
            para1.Inlines.Add(new System.Windows.Documents.Run(txtNhanVienNhap.Text) { FontWeight = System.Windows.FontWeights.Normal });
            para1.TextAlignment = System.Windows.TextAlignment.Center;
            cell1.Blocks.Add(para1);
            cell1.BorderThickness = new Thickness(0);

            var cell2 = new System.Windows.Documents.TableCell();
            var para2 = new System.Windows.Documents.Paragraph();
            para2.Inlines.Add(new System.Windows.Documents.Run("NGƯỜI PHÊ DUYỆT") { FontWeight = System.Windows.FontWeights.Bold });
            para2.Inlines.Add(new System.Windows.Documents.LineBreak());
            para2.Inlines.Add(new System.Windows.Documents.Run("(Ký, họ tên)") { FontStyle = System.Windows.FontStyles.Italic, FontSize = 11 });
            para2.Inlines.Add(new System.Windows.Documents.LineBreak());
            para2.Inlines.Add(new System.Windows.Documents.LineBreak());
            para2.Inlines.Add(new System.Windows.Documents.LineBreak());
            para2.TextAlignment = System.Windows.TextAlignment.Center;
            cell2.Blocks.Add(para2);
            cell2.BorderThickness = new Thickness(0);

            signRow.Cells.Add(cell1);
            signRow.Cells.Add(cell2);
            signGroup.Rows.Add(signRow);
            signTable.RowGroups.Add(signGroup);
            signTable.Margin = new Thickness(0, 30, 0, 10);
            doc.Blocks.Add(signTable);

            // Ngày in
            var datePara = new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm:ss}"))
            {
                FontStyle = System.Windows.FontStyles.Italic,
                FontSize = 10,
                TextAlignment = System.Windows.TextAlignment.Right
            };
            doc.Blocks.Add(datePara);

            return doc;
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    FlowDocument doc = TaoFlowDocument_PhieuNhap();
                    doc.Name = "PhieuNhapPrint";
                    doc.PageHeight = printDialog.PrintableAreaHeight;
                    doc.PageWidth = printDialog.PrintableAreaWidth;
                    doc.PagePadding = new Thickness(40);
                    doc.ColumnGap = 0;
                    doc.ColumnWidth = printDialog.PrintableAreaWidth;

                    IDocumentPaginatorSource idpSource = doc;
                    printDialog.PrintDocument(idpSource.DocumentPaginator, "In phiếu nhập");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi in: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}