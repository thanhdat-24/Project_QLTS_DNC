using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using Microsoft.Win32;
using Project_QLTS_DNC.Models.Phieu;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using iTextTextAlignment = iText.Layout.Properties.TextAlignment;
using Project_QLTS_DNC.Models.PhieuXuatKho;
using Project_QLTS_DNC.Models.QLTaiSan;
using iText.Layout.Borders;

namespace Project_QLTS_DNC.View.DuyetPhieu.InPhieu
{
    public partial class frmInPhieuXuat : Window
    {
        private long maPhieuXuat;
        private List<ChiTietXuatDTO> danhSachChiTiet = new();
        private ChiTietPhieuXuatDTO thongTinPhieu;

        public frmInPhieuXuat(long maPhieu)
        {
            InitializeComponent();
            maPhieuXuat = maPhieu;
            Loaded += frmInPhieuXuat_Loaded;
        }

        private async void frmInPhieuXuat_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                var client = await SupabaseService.GetClientAsync();

                // Lấy các dữ liệu từ bảng liên quan
                var listPhieu = await client.From<PhieuXuat>().Get();
                var listChiTiet = await client.From<ChiTietPhieuXuatModel>().Get();
                var listKho = await client.From<Kho>().Get();
                var listNV = await client.From<NhanVienModel>().Get();
                var listTaiSan = await client.From<TaiSanModel>().Get();

                // Lấy phiếu xuất tương ứng
                var phieu = listPhieu.Models.FirstOrDefault(p => p.MaPhieuXuat == maPhieuXuat);
                if (phieu == null) throw new Exception("Không tìm thấy phiếu xuất!");

                var khoXuat = listKho.Models.FirstOrDefault(k => k.MaKho == phieu.MaKhoXuat)?.TenKho ?? "???";
                var khoNhan = listKho.Models.FirstOrDefault(k => k.MaKho == phieu.MaKhoNhan)?.TenKho ?? "???";
                var nhanVien = listNV.Models.FirstOrDefault(nv => nv.MaNV == phieu.MaNV)?.TenNV ?? "???";

                // Khởi tạo thông tin phiếu xuất
                thongTinPhieu = new ChiTietPhieuXuatDTO
                {
                    MaPhieu = phieu.MaPhieuXuat,
                    TenKhoXuat = khoXuat,
                    TenKhoNhan = khoNhan,
                    TenNV = nhanVien,
                    NgayXuat = phieu.NgayXuat,
                    TrangThai = phieu.TrangThai == true ? "Đã duyệt" : phieu.TrangThai == false ? "Từ chối duyệt" : "Chưa duyệt",
                    SoLuong = int.TryParse(phieu.SoLuong, out int soLuong) ? soLuong : 0 // Lấy số lượng từ PhieuXuat
                };

                // Cập nhật thông tin vào các TextBlock
                txtMaPhieu.Text = "PX" + thongTinPhieu.MaPhieu;
                txtTenKhoXuat.Text = thongTinPhieu.TenKhoXuat;
                txtTenKhoNhan.Text = thongTinPhieu.TenKhoNhan;
                txtTenNV.Text = thongTinPhieu.TenNV;
                txtNgayXuat.Text = thongTinPhieu.NgayXuat?.ToString("dd/MM/yyyy") ?? "";
                txtTrangThai.Text = thongTinPhieu.TrangThai;

                // Lấy chi tiết phiếu xuất
                danhSachChiTiet = (from ct in listChiTiet.Models
                                   where ct.MaPhieuXuat == phieu.MaPhieuXuat
                                   join ts in listTaiSan.Models on ct.MaTaiSan equals ts.MaTaiSan
                                   select new ChiTietXuatDTO
                                   {
                                       MaChiTiet = ct.MaChiTietXK,
                                       TenTaiSan = ts.TenTaiSan,
                                       SoLuong = thongTinPhieu.SoLuong, // Gán số lượng từ phiếu xuất
                                       TenKhoXuat = khoXuat,
                                       TenKhoNhan = khoNhan,
                                       GhiChu = ts.GhiChu ?? ""
                                   }).ToList();

                // Hiển thị dữ liệu lên UI
                dgChiTietPhieuXuat.ItemsSource = danhSachChiTiet;
                txtStatus.Text = $"Tổng số dòng chi tiết: {danhSachChiTiet.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi load dữ liệu phiếu xuất: " + ex.Message);
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
                    FileName = $"PhieuXuat_{maPhieuXuat}_{DateTime.Now:yyyyMMdd_HHmmss}"
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
            using (PdfWriter writer = new PdfWriter(filePath))
            using (PdfDocument pdf = new PdfDocument(writer))
            {
                Document doc = new Document(pdf, PageSize.A4);
                doc.SetMargins(36, 36, 36, 36);

                var font = PdfFontFactory.CreateFont("C:\\Windows\\Fonts\\arial.ttf", PdfEncodings.IDENTITY_H);

                doc.Add(new Paragraph("PHIẾU XUẤT KHO")
                    .SetFont(font).SetFontSize(16).SetBold()
                    .SetTextAlignment(iTextTextAlignment.CENTER));

                doc.Add(new Paragraph($"Số phiếu: PX{maPhieuXuat}     Ngày xuất: {thongTinPhieu.NgayXuat:dd/MM/yyyy}")
                    .SetFont(font).SetMarginBottom(10));

                Table info = new Table(UnitValue.CreatePercentArray(new float[] { 25, 75 })).UseAllAvailableWidth();
                info.AddCell(Cell("Kho xuất:", font, true)); info.AddCell(Cell(thongTinPhieu.TenKhoXuat, font));
                info.AddCell(Cell("Kho nhận:", font, true)); info.AddCell(Cell(thongTinPhieu.TenKhoNhan, font));
                info.AddCell(Cell("Nhân viên:", font, true)); info.AddCell(Cell(thongTinPhieu.TenNV, font));
                info.AddCell(Cell("Trạng thái:", font, true)); info.AddCell(Cell(thongTinPhieu.TrangThai, font));
                doc.Add(info);

                doc.Add(new Paragraph("\n"));

                Table table = new Table(new float[] { 30, 80, 150, 60, 100, 100, 100 }).UseAllAvailableWidth();
                string[] headers = { "STT", "Mã CT", "Tên tài sản", "SL", "Kho xuất", "Kho nhận", "Ghi chú" };
                foreach (var h in headers)
                    table.AddHeaderCell(new Cell().Add(new Paragraph(h).SetFont(font).SetBold()).SetBackgroundColor(new DeviceRgb(230, 230, 230)));

                for (int i = 0; i < danhSachChiTiet.Count; i++)
                {
                    var ct = danhSachChiTiet[i];
                    table.AddCell(new Paragraph((i + 1).ToString()).SetFont(font));
                    table.AddCell(new Paragraph(ct.MaChiTiet.ToString()).SetFont(font));
                    table.AddCell(new Paragraph(ct.TenTaiSan).SetFont(font));
                    table.AddCell(new Paragraph(ct.SoLuong.ToString()).SetFont(font)); // Hiển thị số lượng
                    table.AddCell(new Paragraph(ct.TenKhoXuat).SetFont(font));
                    table.AddCell(new Paragraph(ct.TenKhoNhan).SetFont(font));
                    table.AddCell(new Paragraph(ct.GhiChu).SetFont(font));
                }

                doc.Add(table);
                doc.Add(new Paragraph("\n\n"));
                Table sign = new Table(2).UseAllAvailableWidth();
                sign.AddCell(SignatureCell("NGƯỜI LẬP PHIẾU", thongTinPhieu.TenNV, font));
                sign.AddCell(SignatureCell("NGƯỜI PHÊ DUYỆT", "", font));
                doc.Add(sign);

                doc.Add(new Paragraph($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                    .SetFont(font).SetFontSize(8).SetItalic()
                    .SetTextAlignment(iTextTextAlignment.RIGHT));
            }
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

        private class ChiTietXuatDTO
        {
            public int MaChiTiet { get; set; }
            public string TenTaiSan { get; set; }
            public int SoLuong { get; set; }

            public string TenKhoXuat { get; set; }
            public string TenKhoNhan { get; set; }
            public string GhiChu { get; set; }
        }

        private class ChiTietPhieuXuatDTO
        {
            public long MaPhieu { get; set; }
            public int SoLuong { get; set; }
            public string TenKhoXuat { get; set; }
            public string TenKhoNhan { get; set; }
            public string TenNV { get; set; }
            public DateTime? NgayXuat { get; set; }
            public string TrangThai { get; set; }
        }
    }
}