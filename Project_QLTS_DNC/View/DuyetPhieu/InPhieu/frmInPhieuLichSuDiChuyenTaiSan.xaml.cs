using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using iText.Kernel.Geom;
using Microsoft.Win32;
using Project_QLTS_DNC.Models.LichSu;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static Supabase.Postgrest.Constants;

using iTextTextAlignment = iText.Layout.Properties.TextAlignment;
using iTextVerticalAlignment = iText.Layout.Properties.VerticalAlignment;

namespace Project_QLTS_DNC.View.DuyetPhieu.InPhieu
{
    public partial class frmInPhieuLichSuDiChuyenTaiSan : Window
    {
        private long maLichSu;
        private LichSuHienThi thongTinPhieu;
        private List<LichSuHienThi> danhSachChiTiet = new();

        public frmInPhieuLichSuDiChuyenTaiSan(long ma)
        {
            InitializeComponent();
            maLichSu = ma;
            Loaded += frmInPhieuLichSuDiChuyenTaiSan_Loaded;
        }

        public class LichSuHienThi
        {
            public long MaLichSu { get; set; }
            public string TenNhanVien { get; set; }
            public string TenTaiSan { get; set; }
            public string SoSeri { get; set; }
            public string TenPhongCu { get; set; }
            public string TenPhongMoi { get; set; }
            public string GhiChu { get; set; }
            public DateTime NgayBanGiao { get; set; }
        }

        private async void frmInPhieuLichSuDiChuyenTaiSan_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                var client = await SupabaseService.GetClientAsync();

                var dsLichSu = await client.From<LichSuDiChuyenTaiSan>().Filter("ma_lich_su", Operator.Equals, maLichSu).Get();
                var dsNV = await client.From<NhanVienModel>().Get();
                var dsTS = await client.From<TaiSanModel>().Get();
                var dsPhong = await client.From<Phong>().Get();

                var result = dsLichSu.Models.Select(p =>
                {
                    var nv = dsNV.Models.FirstOrDefault(x => x.MaNV == p.MaNhanVien);
                    var ts = dsTS.Models.FirstOrDefault(x => x.MaTaiSan == p.MaTaiSan);
                    var phongCu = dsPhong.Models.FirstOrDefault(x => x.MaPhong == p.MaPhongCu);
                    var phongMoi = dsPhong.Models.FirstOrDefault(x => x.MaPhong == p.MaPhongMoi);

                    return new LichSuHienThi
                    {
                        MaLichSu = p.MaLichSu,
                        TenNhanVien = nv?.TenNV ?? "(Không rõ)",
                        TenTaiSan = ts?.TenTaiSan ?? "(Không rõ)",
                        SoSeri = ts?.SoSeri ?? "(Không rõ)",
                        TenPhongCu = phongCu?.TenPhong ?? "(Không rõ)",
                        TenPhongMoi = phongMoi?.TenPhong ?? "(Không rõ)",
                        GhiChu = p.GhiChu,
                        NgayBanGiao = p.NgayBanGiao ?? DateTime.Now
                    };
                }).ToList();

                danhSachChiTiet = result;
                thongTinPhieu = result.FirstOrDefault();

                if (thongTinPhieu != null)
                {
                    txtMaLichSu.Text = "LS" + thongTinPhieu.MaLichSu;
                    txtTenNhanVien.Text = thongTinPhieu.TenNhanVien;
                    txtTenPhongCu.Text = thongTinPhieu.TenPhongCu;
                    txtTenPhongMoi.Text = thongTinPhieu.TenPhongMoi;
                    txtNgayBanGiao.Text = thongTinPhieu.NgayBanGiao.ToString("dd/MM/yyyy");
                    txtTrangThai.Text = "Đã duyệt";
                    txtStatus.Text = $"Tổng số dòng chi tiết: {danhSachChiTiet.Count}";
                    dgChiTietLichSu.ItemsSource = danhSachChiTiet;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi load dữ liệu: " + ex.Message);
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void btnDong_Click(object sender, RoutedEventArgs e) => Close();

        private void btnLuuPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog dialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"LichSuDiChuyen_{maLichSu}_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (dialog.ShowDialog() == true)
                {
                    ExportToPDF(dialog.FileName);
                    MessageBox.Show("✅ Xuất PDF thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi xuất PDF: " + ex.Message);
            }
        }

        private void ExportToPDF(string filePath)
        {
            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            var doc = new Document(pdf, PageSize.A4);
            doc.SetMargins(36, 36, 36, 36);
            var font = PdfFontFactory.CreateFont("C:\\Windows\\Fonts\\arial.ttf", PdfEncodings.IDENTITY_H);

            doc.Add(new Paragraph("PHIẾU LỊCH SỬ DI CHUYỂN TÀI SẢN")
                .SetFont(font).SetFontSize(16).SetBold().SetTextAlignment(iTextTextAlignment.CENTER));

            doc.Add(new Paragraph($"Số phiếu: LS{maLichSu}     Ngày bàn giao: {thongTinPhieu.NgayBanGiao:dd/MM/yyyy}")
                .SetFont(font).SetMarginBottom(10));

            Table info = new Table(UnitValue.CreatePercentArray(new float[] { 25, 75 })).UseAllAvailableWidth();
            info.SetMarginBottom(10);
            info.AddCell(Cell("Phòng cũ:", font, true)); info.AddCell(Cell(thongTinPhieu.TenPhongCu, font));
            info.AddCell(Cell("Phòng mới:", font, true)); info.AddCell(Cell(thongTinPhieu.TenPhongMoi, font));
            info.AddCell(Cell("Nhân viên:", font, true)); info.AddCell(Cell(thongTinPhieu.TenNhanVien, font));
            doc.Add(info);

            doc.Add(new Paragraph("DANH SÁCH TÀI SẢN").SetFont(font).SetBold().SetMarginBottom(5));

            Table table = new Table(new float[] { 100, 150, 150, 150, 150 }).UseAllAvailableWidth();
            string[] headers = { "Mã phiếu", "Tên tài sản", "Số Seri", "Ghi chú", "Ngày bàn giao" };
            foreach (var header in headers)
            {
                table.AddHeaderCell(new Cell()
                    .Add(new Paragraph(header).SetFont(font).SetBold().SetTextAlignment(iTextTextAlignment.CENTER))
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetTextAlignment(iTextTextAlignment.CENTER)
                    .SetVerticalAlignment(iTextVerticalAlignment.MIDDLE)
                    .SetPadding(5));
            }

            foreach (var ct in danhSachChiTiet)
            {
                table.AddCell(new Cell().Add(new Paragraph("LS" + ct.MaLichSu).SetFont(font)).SetTextAlignment(iTextTextAlignment.CENTER));
                table.AddCell(new Cell().Add(new Paragraph(ct.TenTaiSan).SetFont(font)));
                table.AddCell(new Cell().Add(new Paragraph(ct.SoSeri).SetFont(font)));
                table.AddCell(new Cell().Add(new Paragraph(ct.GhiChu).SetFont(font)));
                table.AddCell(new Cell().Add(new Paragraph(ct.NgayBanGiao.ToString("dd/MM/yyyy")).SetFont(font)).SetTextAlignment(iTextTextAlignment.CENTER));
            }

            doc.Add(table);

            doc.Add(new Paragraph("\n\n"));
            Table sign = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth();
            sign.AddCell(SignatureCell("NGƯỜI BÀN GIAO", thongTinPhieu.TenNhanVien, font));
            sign.AddCell(SignatureCell("NGƯỜI DUYỆT", "", font));
            doc.Add(sign);

            doc.Add(new Paragraph($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                .SetFont(font).SetFontSize(8).SetItalic()
                .SetTextAlignment(iTextTextAlignment.RIGHT));
        }

        private Cell Cell(string text, PdfFont font, bool bold = false)
        {
            var p = new Paragraph(text).SetFont(font);
            if (bold) p.SetBold();
            return new Cell().Add(p).SetBorder(Border.NO_BORDER).SetPadding(2);
        }

        private Cell SignatureCell(string title, string name, PdfFont font)
        {
            return new Cell()
                .Add(new Paragraph(title).SetFont(font).SetBold().SetTextAlignment(iTextTextAlignment.CENTER))
                .Add(new Paragraph("(Ký và ghi rõ họ tên)").SetFont(font).SetItalic().SetFontSize(10).SetTextAlignment(iTextTextAlignment.CENTER))
                .Add(new Paragraph("\n\n\n"))
                .Add(new Paragraph(name).SetFont(font).SetTextAlignment(iTextTextAlignment.CENTER))
                .SetBorder(Border.NO_BORDER);
        }
    }
}