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
                Document document = new Document(pdf, PageSize.A4);
                document.SetMargins(36, 36, 36, 36);

                PdfFont font = PdfFontFactory.CreateFont(@"C:\Windows\Fonts\arial.ttf", PdfEncodings.IDENTITY_H);

                document.Add(new Paragraph("PHIẾU NHẬP KHO")
                    .SetFont(font)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(16)
                    .SetBold()
                    .SetMarginBottom(10));

                // Thông tin phiếu
                Table info = new Table(new float[] { 120, 300 });
                info.SetWidth(UnitValue.CreatePercentValue(100));

                info.AddCell(new Cell().Add(new Paragraph("Mã phiếu nhập:").SetFont(font).SetBold()).SetBorder(iText.Layout.Borders.Border.NO_BORDER
));
                info.AddCell(new Cell().Add(new Paragraph(txtMaPhieuNhap.Text).SetFont(font)).SetBorder(iText.Layout.Borders.Border.NO_BORDER
));

                info.AddCell(new Cell().Add(new Paragraph("Ngày nhập:").SetFont(font).SetBold()).SetBorder(iText.Layout.Borders.Border.NO_BORDER
));
                info.AddCell(new Cell().Add(new Paragraph(txtNgayNhap.Text).SetFont(font)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                info.AddCell(new Cell().Add(new Paragraph("Nhà cung cấp:").SetFont(font).SetBold()).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                info.AddCell(new Cell().Add(new Paragraph(txtNhaCungCap.Text).SetFont(font)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                info.AddCell(new Cell().Add(new Paragraph("Kho nhập:").SetFont(font).SetBold()).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                info.AddCell(new Cell().Add(new Paragraph(txtKhoNhap.Text).SetFont(font)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                info.AddCell(new Cell().Add(new Paragraph("Nhân viên nhập:").SetFont(font).SetBold()).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                info.AddCell(new Cell().Add(new Paragraph(txtNhanVienNhap.Text).SetFont(font)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                info.AddCell(new Cell().Add(new Paragraph("Tổng tiền:").SetFont(font).SetBold()).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                info.AddCell(new Cell().Add(new Paragraph(txtTongTien.Text).SetFont(font)).SetBorder(iText.Layout.Borders.Border.NO_BORDER
));

                document.Add(info);
                document.Add(new Paragraph("\n").SetFont(font));

                // Bảng chi tiết
                Table table = new Table(new float[] { 30, 120, 100, 70, 70, 50 });
                table.SetWidth(UnitValue.CreatePercentValue(100));
                string[] headers = { "STT", "Tên tài sản", "Nhóm tài sản", "Số lượng", "Đơn giá", "QLR" };

                foreach (var header in headers)
                {
                    table.AddHeaderCell(new Cell().Add(new Paragraph(header).SetFont(font).SetBold())
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                      .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                }

                var danhSach = gridChiTiet.ItemsSource.Cast<ChiTietPhieuNhap>().ToList();
                for (int i = 0; i < danhSach.Count; i++)
                {
                    var item = danhSach[i];
                    table.AddCell(new Paragraph((i + 1).ToString()).SetFont(font).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                    table.AddCell(new Paragraph(item.TenTaiSan).SetFont(font));
                    table.AddCell(new Paragraph(item.TenNhomTS).SetFont(font));
                    table.AddCell(new Paragraph(item.SoLuong.ToString()).SetFont(font).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                    table.AddCell(new Paragraph(
    (item.DonGia ?? 0).ToString("N0"))
    .SetFont(font)
    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));

                    table.AddCell(new Paragraph(item.CanQuanLyRieng ? "✓" : "").SetFont(font).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                }

                document.Add(table);

                document.Add(new Paragraph("\n\n").SetFont(font));

                // Chữ ký
                Table sign = new Table(new float[] { 1, 1 });
                sign.SetWidth(UnitValue.CreatePercentValue(100));

                string tenNV = txtNhanVienNhap.Text;

                sign.AddCell(new Cell()
                    .Add(new Paragraph("NGƯỜI LẬP PHIẾU").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetFont(font).SetBold())
                    .Add(new Paragraph("(Ký, họ tên)").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetFont(font).SetItalic().SetFontSize(10))
                    .Add(new Paragraph("\n\n\n"))
                    .Add(new Paragraph(tenNV).SetFont(font).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER
));

                sign.AddCell(new Cell()
                    .Add(new Paragraph("NGƯỜI PHÊ DUYỆT").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetFont(font).SetBold())
                    .Add(new Paragraph("(Ký, họ tên)").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetFont(font).SetItalic().SetFontSize(10))
                    .Add(new Paragraph("\n\n\n"))
                    .Add(new Paragraph(""))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(sign);
                document.Add(new Paragraph($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm:ss}").SetFont(font).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetFontSize(8).SetItalic());

                document.Close();
            }
        }


        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"PhieuNhap_{txtMaPhieuNhap.Text}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    TaoFilePDF_PhieuNhap(saveDialog.FileName);
                    MessageBox.Show("Xuất phiếu nhập thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi in PDF: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}