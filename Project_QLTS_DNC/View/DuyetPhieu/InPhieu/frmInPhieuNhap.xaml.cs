using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using Microsoft.Win32;
using Project_QLTS_DNC.Models.DuyetPhieu;
using Project_QLTS_DNC.Models.NhaCungCap;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Models.TonKho;
using iText.Layout.Borders;
using Project_QLTS_DNC.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using iTextTextAlignment = iText.Layout.Properties.TextAlignment;
using Project_QLTS_DNC.View.DuyetPhieu.ChiTietPhieu;
using Project_QLTS_DNC.View.Common; // thêm namespace chứa SuccessNotificationDialog

namespace Project_QLTS_DNC.View.DuyetPhieu.InPhieu
{
    public partial class frmInPhieuNhap : Window
    {
        private ObservableCollection<frmXemChiTietNhap.ChiTietPhieuHienThi> danhSachChiTiet = new();
        private frmXemChiTietNhap.ChiTietPhieuHienThi thongTinPhieu;
        private long maPhieuNhap;

        public frmInPhieuNhap(long maPhieu)
        {
            InitializeComponent();
            maPhieuNhap = maPhieu;
            Loaded += frmInPhieuNhap_Loaded;
        }

        private async void frmInPhieuNhap_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                var client = await SupabaseService.GetClientAsync();

                var listPhieu = await client.From<PhieuNhapKho>().Get();
                var listChiTiet = await client.From<ChiTietPN>().Get();
                var listKho = await client.From<Kho>().Get();
                var listNV = await client.From<NhanVienModel>().Get();
                var listNCC = await client.From<NhaCungCapClass>().Get();
                var listNhomTS = await client.From<NhomTaiSan>().Get();

                var result = (from pn in listPhieu.Models
                              join ct in listChiTiet.Models on pn.MaPhieuNhap equals ct.MaPhieuNhap
                              join kho in listKho.Models on pn.MaKho equals kho.MaKho
                              join nv in listNV.Models on pn.MaNV equals nv.MaNV
                              join ncc in listNCC.Models on pn.MaNCC equals ncc.MaNCC
                              join nhom in listNhomTS.Models on ct.MaNhomTS equals nhom.MaNhomTS
                              where pn.MaPhieuNhap == maPhieuNhap
                              select new frmXemChiTietNhap.ChiTietPhieuHienThi
                              {
                                  MaPhieu = pn.MaPhieuNhap,
                                  MaKho = pn.MaKho,
                                  TenKho = kho.TenKho,
                                  TenNV = nv.TenNV,
                                  MaChiTietPN = ct.MaChiTietPN ?? 0,
                                  MaNhomTS = ct.MaNhomTS,
                                  TenNhomTS = nhom.TenNhom,
                                  TenNCC = ncc.TenNCC,
                                  TenTS = ct.TenTaiSan,
                                  NgayNhap = pn.NgayNhap,
                                  SoLuong = ct.SoLuong,
                                  DonGia = ct.DonGia ?? 0,
                                  CanQLRieng = ct.CanQuanLyRieng ?? false,
                                  TongTien = ct.TongTien ?? ((ct.DonGia ?? 0) * ct.SoLuong),
                                  TrangThai = pn.TrangThai == true ? "Đã duyệt" :
                                              pn.TrangThai == false ? "Từ chối duyệt" : "Chưa duyệt"
                              }).ToList();

                danhSachChiTiet = new ObservableCollection<frmXemChiTietNhap.ChiTietPhieuHienThi>(result);
                dgChiTietPhieuNhap.ItemsSource = danhSachChiTiet;

                thongTinPhieu = result.FirstOrDefault();
                if (thongTinPhieu != null)
                {
                    txtMaPhieu.Text = "PN" + thongTinPhieu.MaPhieu;
                    txtTenKho.Text = thongTinPhieu.TenKho;
                    txtTenNV.Text = thongTinPhieu.TenNV;
                    txtNgayNhap.Text = thongTinPhieu.NgayNhap?.ToString("dd/MM/yyyy");
                    txtTenNCC.Text = thongTinPhieu.TenNCC;
                    txtTrangThai.Text = thongTinPhieu.TrangThai;

                    var tong = danhSachChiTiet.Sum(x => x.TongTien);
                    txtTongTien.Text = string.Format("{0:N0} VNĐ", tong);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi load phiếu nhập: " + ex.Message);
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void btnDong_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnLuuPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog dialog = new SaveFileDialog
                {
                    Filter = "PDF File|*.pdf",
                    FileName = $"PhieuNhap_{thongTinPhieu.MaPhieu}_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (dialog.ShowDialog() == true)
                {
                    string filePath = dialog.FileName;
                    ExportPhieuNhapToPDF(filePath);

                    // Mở thông báo có nút mở file
                    var dialogSuccess = new SuccessNotificationDialog(
                        "Xuất PDF thành công",
                        "Bạn có muốn mở file vừa tạo không?",
                        filePath);
                    dialogSuccess.ShowDialog(); // Có nút MỞ FILE bên trong dialog này
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất PDF: " + ex.Message);
            }
        }

        private void ExportPhieuNhapToPDF(string filePath)
        {
            using (PdfWriter writer = new PdfWriter(filePath))
            using (PdfDocument pdf = new PdfDocument(writer))
            {
                Document document = new Document(pdf, PageSize.A4);
                document.SetMargins(36, 36, 36, 36);
                PdfFont font = PdfFontFactory.CreateFont(@"C:\Windows\Fonts\arial.ttf", PdfEncodings.IDENTITY_H);

                document.Add(new Paragraph("PHIẾU NHẬP KHO")
                    .SetFont(font).SetFontSize(16).SetBold().SetTextAlignment(iTextTextAlignment.CENTER));

                document.Add(new Paragraph($"Số phiếu: PN{thongTinPhieu.MaPhieu}     Ngày nhập: {thongTinPhieu.NgayNhap:dd/MM/yyyy}")
                    .SetFont(font).SetMarginBottom(10));

                Table infoTable = new Table(UnitValue.CreatePercentArray(new float[] { 25, 75 }))
                    .SetWidth(UnitValue.CreatePercentValue(100));

                infoTable.AddCell(new Cell().Add(new Paragraph("Kho nhập:").SetFont(font).SetBold()).SetBorder(Border.NO_BORDER).SetPadding(2));
                infoTable.AddCell(new Cell().Add(new Paragraph(thongTinPhieu.TenKho).SetFont(font)).SetBorder(Border.NO_BORDER).SetPadding(2));

                infoTable.AddCell(new Cell().Add(new Paragraph("Nhân viên:").SetFont(font).SetBold()).SetBorder(Border.NO_BORDER).SetPadding(2));
                infoTable.AddCell(new Cell().Add(new Paragraph(thongTinPhieu.TenNV).SetFont(font)).SetBorder(Border.NO_BORDER).SetPadding(2));

                infoTable.AddCell(new Cell().Add(new Paragraph("Nhà cung cấp:").SetFont(font).SetBold()).SetBorder(Border.NO_BORDER).SetPadding(2));
                infoTable.AddCell(new Cell().Add(new Paragraph(thongTinPhieu.TenNCC).SetFont(font)).SetBorder(Border.NO_BORDER).SetPadding(2));

                infoTable.AddCell(new Cell().Add(new Paragraph("Trạng thái:").SetFont(font).SetBold()).SetBorder(Border.NO_BORDER).SetPadding(2));
                infoTable.AddCell(new Cell().Add(new Paragraph(thongTinPhieu.TrangThai).SetFont(font)).SetBorder(Border.NO_BORDER).SetPadding(2));

                document.Add(infoTable);
                document.Add(new Paragraph("\n"));

                Table table = new Table(new float[] { 40, 80, 150, 60, 80, 80, 60 }).UseAllAvailableWidth();
                string[] headers = { "STT", "Mã CT", "Tên tài sản", "SL", "Đơn giá", "Tổng tiền", "QL riêng" };
                foreach (var h in headers)
                {
                    table.AddHeaderCell(new Cell().Add(new Paragraph(h).SetFont(font).SetBold()).SetBackgroundColor(new DeviceRgb(220, 220, 220)));
                }

                for (int i = 0; i < danhSachChiTiet.Count; i++)
                {
                    var ct = danhSachChiTiet[i];
                    table.AddCell(new Paragraph((i + 1).ToString()).SetFont(font));
                    table.AddCell(new Paragraph(ct.MaChiTietPN.ToString()).SetFont(font));
                    table.AddCell(new Paragraph(ct.TenTS).SetFont(font));
                    table.AddCell(new Paragraph(ct.SoLuong.ToString()).SetFont(font));
                    table.AddCell(new Paragraph($"{ct.DonGia:N0}").SetFont(font));
                    table.AddCell(new Paragraph($"{ct.TongTien:N0}").SetFont(font));
                    table.AddCell(new Paragraph(ct.CanQLRieng ? "✓" : "").SetFont(font));
                }

                document.Add(table);
                document.Add(new Paragraph($"\nTổng cộng: {txtTongTien.Text}").SetFont(font).SetBold().SetTextAlignment(iTextTextAlignment.RIGHT));

                document.Add(new Paragraph("\n\n"));
                Table signTable = new Table(2).UseAllAvailableWidth();

                signTable.AddCell(new Cell()
                    .Add(new Paragraph("NGƯỜI LẬP PHIẾU").SetFont(font).SetBold().SetTextAlignment(iTextTextAlignment.CENTER))
                    .Add(new Paragraph("(Ký và ghi rõ họ tên)").SetFont(font).SetFontSize(10).SetItalic().SetTextAlignment(iTextTextAlignment.CENTER))
                    .Add(new Paragraph("\n\n\n"))
                    .Add(new Paragraph(thongTinPhieu.TenNV).SetFont(font).SetTextAlignment(iTextTextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER));

                signTable.AddCell(new Cell()
                    .Add(new Paragraph("NGƯỜI PHÊ DUYỆT").SetFont(font).SetBold().SetTextAlignment(iTextTextAlignment.CENTER))
                    .Add(new Paragraph("(Ký và ghi rõ họ tên)").SetFont(font).SetFontSize(10).SetItalic().SetTextAlignment(iTextTextAlignment.CENTER))
                    .Add(new Paragraph("\n\n\n"))
                    .Add(new Paragraph("").SetFont(font).SetTextAlignment(iTextTextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER));

                document.Add(signTable);
                document.Add(new Paragraph($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                    .SetFont(font).SetFontSize(8).SetItalic().SetTextAlignment(iTextTextAlignment.RIGHT));
            }
        }
    }
}
