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
using Project_QLTS_DNC.Models.BaoHong;
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
using Project_QLTS_DNC.Models.Phieu;

namespace Project_QLTS_DNC.View.DuyetPhieu.InPhieu
{
    public partial class frmInPhieuBaoHong : Window
    {
        private long maPhieuBaoHong;
        private List<BaoHongHienThi> danhSachChiTiet = new();
        private BaoHongHienThi thongTinPhieu;

        public frmInPhieuBaoHong(long maPhieu)
        {
            InitializeComponent();
            maPhieuBaoHong = maPhieu;
            Loaded += frmInPhieuBaoHong_Loaded;
        }

        public class BaoHongHienThi
        {
            public long MaPhieuBaoHong { get; set; }
            public string TenNV { get; set; }
            public string TenPhong { get; set; }
            public string TenTaiSan { get; set; }
            public string HinhThucGhiNhan { get; set; }
            public string MoTa { get; set; }
            public DateTime NgayBaoHong { get; set; }
            public bool? TrangThaiBool { get; set; }
            public string TrangThai => TrangThaiBool == true ? "Đã duyệt" : TrangThaiBool == false ? "Từ chối duyệt" : "Chưa duyệt";
        }

        private async void frmInPhieuBaoHong_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                var client = await SupabaseService.GetClientAsync();

                var dsPhieu = await client.From<BaoHong>().Filter("ma_phieu_bao_hong", Operator.Equals, maPhieuBaoHong).Get();
                var dsNV = await client.From<NhanVienModel>().Get();
                var dsPhong = await client.From<Phong>().Get();
                var dsTS = await client.From<TaiSanModel>().Get();

                var result = dsPhieu.Models.Select(p =>
                {
                    var nv = dsNV.Models.FirstOrDefault(x => x.MaNV == p.MaNV);
                    var phong = dsPhong.Models.FirstOrDefault(x => x.MaPhong == p.MaPhong);
                    var ts = dsTS.Models.FirstOrDefault(x => x.MaTaiSan == p.MaTaiSan);

                    return new BaoHongHienThi
                    {
                        MaPhieuBaoHong = p.MaPhieuBaoHong,
                        TenNV = nv?.TenNV ?? "(Không rõ)",
                        TenPhong = phong?.TenPhong ?? "(Không rõ)",
                        TenTaiSan = ts?.TenTaiSan ?? "(Không rõ)",
                        HinhThucGhiNhan = p.HinhThucGhiNhan,
                        MoTa = p.MoTa,
                        NgayBaoHong = p.NgayBaoHong,
                        TrangThaiBool = p.TrangThai
                    };
                }).ToList();

                danhSachChiTiet = result;
                var phieu = result.FirstOrDefault();

                if (phieu != null)
                {
                    thongTinPhieu = phieu;
                    txtMaPhieu.Text = "PBH" + phieu.MaPhieuBaoHong;
                    txtTenNV.Text = phieu.TenNV;
                    txtTenPhong.Text = phieu.TenPhong;
                    txtNgayBaoHong.Text = phieu.NgayBaoHong.ToString("dd/MM/yyyy");
                    txtMoTa.Text = phieu.MoTa;
                    txtTrangThai.Text = phieu.TrangThai;
                    txtStatus.Text = "Tổng số dòng chi tiết: 1";
                }

                dgChiTietPhieuBaoHong.ItemsSource = danhSachChiTiet;
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi load dữ liệu phiếu báo hỏng: " + ex.Message);
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
                    FileName = $"PhieuBaoHong_{maPhieuBaoHong}_{DateTime.Now:yyyyMMdd_HHmmss}"
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

                doc.Add(new Paragraph("PHIẾU BÁO HỎNG TÀI SẢN")
                    .SetFont(font).SetFontSize(16).SetBold()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                doc.Add(new Paragraph($"Số phiếu: PBH{maPhieuBaoHong}     Ngày báo hỏng: {thongTinPhieu.NgayBaoHong:dd/MM/yyyy}")
                    .SetFont(font).SetMarginBottom(10));

                Table info = new Table(UnitValue.CreatePercentArray(new float[] { 25, 75 })).UseAllAvailableWidth();
                info.AddCell(Cell("Phòng:", font, true)); info.AddCell(Cell(thongTinPhieu.TenPhong, font));
                info.AddCell(Cell("Người báo hỏng:", font, true)); info.AddCell(Cell(thongTinPhieu.TenNV, font));
                info.AddCell(Cell("Tên tài sản:", font, true)); info.AddCell(Cell(thongTinPhieu.TenTaiSan, font));
                info.AddCell(Cell("Hình thức ghi nhận:", font, true)); info.AddCell(Cell(thongTinPhieu.HinhThucGhiNhan, font));
                info.AddCell(Cell("Mô tả:", font, true)); info.AddCell(Cell(thongTinPhieu.MoTa, font));
                info.AddCell(Cell("Trạng thái:", font, true)); info.AddCell(Cell(thongTinPhieu.TrangThai, font));
                doc.Add(info);

                // Table chi tiết báo hỏng
                doc.Add(new Paragraph("\nDANH SÁCH TÀI SẢN BÁO HỎNG").SetFont(font).SetBold());
                Table table = new Table(new float[] { 100, 150, 150, 150 }).UseAllAvailableWidth();
                table.AddHeaderCell(new Cell().Add(new Paragraph("Mã phiếu").SetFont(font).SetBold()));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Tên tài sản").SetFont(font).SetBold()));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Hình thức ghi nhận").SetFont(font).SetBold()));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Mô tả chi tiết").SetFont(font).SetBold()));

                foreach (var ct in danhSachChiTiet)
                {
                    table.AddCell(new Paragraph("PBH" + ct.MaPhieuBaoHong).SetFont(font));
                    table.AddCell(new Paragraph(ct.TenTaiSan).SetFont(font));
                    table.AddCell(new Paragraph(ct.HinhThucGhiNhan).SetFont(font));
                    table.AddCell(new Paragraph(ct.MoTa).SetFont(font));
                }

                doc.Add(table);

                doc.Add(new Paragraph("\n\n"));
                Table sign = new Table(2).UseAllAvailableWidth();
                sign.AddCell(SignatureCell("NGƯỜI BÁO HỎNG", thongTinPhieu.TenNV, font));
                sign.AddCell(SignatureCell("NGƯỜI PHÊ DUYỆT", "", font));
                doc.Add(sign);

                doc.Add(new Paragraph($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                    .SetFont(font).SetFontSize(8).SetItalic()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
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
                .Add(new Paragraph(title).SetFont(font).SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER))
                .Add(new Paragraph("(Ký và ghi rõ họ tên)").SetFont(font).SetItalic().SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER))
                .Add(new Paragraph("\n\n\n"))
                .Add(new Paragraph(name).SetFont(font).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER))
                .SetBorder(Border.NO_BORDER);
        }
    }
}
