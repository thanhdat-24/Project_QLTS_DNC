using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Layout.Borders;
using Microsoft.Win32;
using Project_QLTS_DNC.Models.DuyetPhieu;
using Project_QLTS_DNC.Models.NhanVien;
using iTextTextAlignment = iText.Layout.Properties.TextAlignment;
using Project_QLTS_DNC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Project_QLTS_DNC.View.DuyetPhieu.InPhieu
{
    public partial class frmInPhieuDeNghiMua : Window
    {
        private long maPhieu;
        private List<CTdenghimua> danhSachChiTiet = new();
        private denghimua thongTinPhieu;

        public frmInPhieuDeNghiMua(long maPhieuDN)
        {
            InitializeComponent();
            maPhieu = maPhieuDN;
            Loaded += frmInPhieuDeNghiMua_Loaded;
        }

        private async void frmInPhieuDeNghiMua_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                var client = await SupabaseService.GetClientAsync();

                var dsPhieu = await client.From<denghimua>().Where(x => x.MaPhieuDeNghi == maPhieu).Get();
                thongTinPhieu = dsPhieu.Models.FirstOrDefault();
                if (thongTinPhieu == null) throw new Exception("Không tìm thấy phiếu đề nghị mua!");

                var dsNhanVien = await client.From<NhanVienModel>().Get();
                var tenNV = dsNhanVien.Models.FirstOrDefault(x => x.MaNV == thongTinPhieu.MaNV)?.TenNV ?? "???";

                var dsChiTiet = await client.From<CTdenghimua>().Where(x => x.MaPhieuDeNghi == maPhieu).Get();

                txtMaPhieu.Text = "PDN" + thongTinPhieu.MaPhieuDeNghi;
                txtTenNV.Text = tenNV;
                txtDonVi.Text = ""; // Không lấy tên phòng
                txtNgayNhap.Text = thongTinPhieu.NgayDeNghiMua?.ToString("dd/MM/yyyy") ?? "";

                txtTenNCC.Text = thongTinPhieu.GhiChu;
                txtTrangThai.Text = thongTinPhieu.TrangThai == true ? "Đã duyệt" : thongTinPhieu.TrangThai == false ? "Từ chối duyệt" : "Chưa duyệt";

                danhSachChiTiet = dsChiTiet.Models;
                dgChiTietPhieuNhap.ItemsSource = danhSachChiTiet;
                txtStatus.Text = $"Tổng số dòng chi tiết: {danhSachChiTiet.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load dữ liệu phiếu đề nghị mua: " + ex.Message);
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
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"PhieuDeNghiMua_{maPhieu}_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (dialog.ShowDialog() == true)
                {
                    ExportToPDF(dialog.FileName);
                    MessageBox.Show("Xuất PDF thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất PDF: " + ex.Message);
            }
        }

        private void ExportToPDF(string filePath)
        {
            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            var doc = new Document(pdf, PageSize.A4);
            doc.SetMargins(36, 36, 36, 36);

            var font = PdfFontFactory.CreateFont("C:\\Windows\\Fonts\\arial.ttf", PdfEncodings.IDENTITY_H);

            doc.Add(new Paragraph("PHIẾU ĐỀ NGHỊ MUA TÀI SẢN").SetFont(font).SetFontSize(16).SetBold().SetTextAlignment(iTextTextAlignment.CENTER));
            doc.Add(new Paragraph($"Số phiếu: PDN{thongTinPhieu.MaPhieuDeNghi}     Ngày đề nghị: {thongTinPhieu.NgayDeNghiMua:dd/MM/yyyy}").SetFont(font).SetMarginBottom(10));

            Table info = new Table(UnitValue.CreatePercentArray(new float[] { 25, 75 })).UseAllAvailableWidth();
            info.AddCell(Cell("Nhân viên:", font, true)); info.AddCell(Cell(txtTenNV.Text, font));
            info.AddCell(Cell("Ghi chú:", font, true)); info.AddCell(Cell(txtTenNCC.Text, font));
            info.AddCell(Cell("Trạng thái:", font, true)); info.AddCell(Cell(txtTrangThai.Text, font));
            doc.Add(info);

            doc.Add(new Paragraph("\nDANH SÁCH TÀI SẢN ĐỀ NGHỊ MUA").SetFont(font).SetBold());
            Table table = new Table(new float[] { 60, 150, 100, 60, 100, 150 }).UseAllAvailableWidth();
            string[] headers = { "Mã chi tiết", "Tên tài sản", "Đơn vị tính", "Số lượng", "Giá dự kiến", "Mô tả" };
            foreach (var h in headers)
            {
                table.AddHeaderCell(new Cell().Add(new Paragraph(h).SetFont(font).SetBold())
                    .SetBackgroundColor(new DeviceRgb(230, 230, 230))
                     .SetTextAlignment(iTextTextAlignment.CENTER));
            }

            foreach (var ct in danhSachChiTiet)
            {
                table.AddCell(new Cell().Add(new Paragraph(ct.MaChiTietDNM.ToString()).SetFont(font))
                    .SetTextAlignment(iTextTextAlignment.CENTER));
                table.AddCell(new Cell().Add(new Paragraph(ct.TenTaiSan ?? "").SetFont(font))
                    .SetTextAlignment(iTextTextAlignment.CENTER));
                table.AddCell(new Cell().Add(new Paragraph(ct.DonViTinh ?? "").SetFont(font))
                    .SetTextAlignment(iTextTextAlignment.CENTER));
                table.AddCell(new Cell().Add(new Paragraph(ct.SoLuong.ToString()).SetFont(font))
                    .SetTextAlignment(iTextTextAlignment.CENTER));
                table.AddCell(new Cell().Add(new Paragraph((ct.DuKienGia.HasValue ? ct.DuKienGia.Value.ToString("N0") : "0") + " VNĐ").SetFont(font))
                    .SetTextAlignment(iTextTextAlignment.CENTER));
                table.AddCell(new Cell().Add(new Paragraph(ct.MoTa ?? "").SetFont(font))
                    .SetTextAlignment(iTextTextAlignment.CENTER));
            }

            doc.Add(table);

            //  THÊM SAU DÒNG NÀY:
            doc.Add(new Paragraph("\n\n"));

            //  BẢNG CHỮ KÝ
            Table sign = new Table(2).UseAllAvailableWidth();
            sign.AddCell(SignatureCell("NGƯỜI ĐỀ NGHỊ", txtTenNV.Text, font));
            sign.AddCell(SignatureCell("NGƯỜI PHÊ DUYỆT", "", font));
            doc.Add(sign);

            // Ngày in
            doc.Add(new Paragraph($"\n\nNgày in: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                .SetFont(font).SetFontSize(8).SetItalic().SetTextAlignment(iTextTextAlignment.RIGHT));



        }
        private Cell SignatureCell(string title, string name, PdfFont font)
        {
            return new Cell()
                .Add(new Paragraph(title).SetFont(font).SetBold().SetTextAlignment(iTextTextAlignment.CENTER))
                .Add(new Paragraph("(Ký và ghi rõ họ tên)").SetFont(font).SetItalic().SetFontSize(10).SetTextAlignment(iTextTextAlignment.CENTER))
                .Add(new Paragraph("\n\n\n"))
                .Add(new Paragraph(name ?? "").SetFont(font).SetTextAlignment(iTextTextAlignment.CENTER))
                .SetBorder(Border.NO_BORDER);
        }


        private Cell Cell(string text, PdfFont font, bool bold = false)
        {
            var p = new Paragraph(text).SetFont(font);
            if (bold) p.SetBold();
            return new Cell().Add(p).SetBorder(Border.NO_BORDER).SetPadding(2);
        }
    }
}
