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
using Project_QLTS_DNC.Models.BanGiaoTaiSan;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.View.Common; // SuccessNotificationDialog
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Models.Kho;
using Project_QLTS_DNC.DTOs;
using iTextTextAlignment = iText.Layout.Properties.TextAlignment;
using iTextVerticalAlignment = iText.Layout.Properties.VerticalAlignment;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static Supabase.Postgrest.Constants;

namespace Project_QLTS_DNC.View.DuyetPhieu.InPhieu
{
    public partial class frmInPhieuBanGiao : Window
    {
        private int maPhieuBanGiao;
        private List<ChiTietBanGiaoDTO> danhSachChiTiet = new();
        private BanGiaoTaiSanDTO thongTinPhieu;

        public frmInPhieuBanGiao(int maPhieu)
        {
            InitializeComponent();
            maPhieuBanGiao = maPhieu;
            Loaded += frmInPhieuBanGiao_Loaded;
        }

        private async void frmInPhieuBanGiao_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                var client = await SupabaseService.GetClientAsync();

                var dsPhieu = await client.From<BanGiaoTaiSanModel>().Filter("ma_bang_giao_ts", Operator.Equals, maPhieuBanGiao).Get();
                var dsChiTiet = await client.From<ChiTietBanGiaoModel>().Filter("ma_bang_giao_ts", Operator.Equals, maPhieuBanGiao).Get();
                var dsNV = await client.From<NhanVienModel>().Get();
                var dsPhong = await client.From<Phong>().Get();
                var dsTaiSan = await client.From<TaiSanModel>().Get();
                var dsKho = await client.From<Kho>().Get();

                var phieu = dsPhieu.Models.FirstOrDefault();
                if (phieu == null) throw new Exception("Không tìm thấy phiếu bàn giao!");

                var nvBanGiao = dsNV.Models.FirstOrDefault(x => x.MaNV == phieu.MaNV);
                var phong = dsPhong.Models.FirstOrDefault(p => p.MaPhong == phieu.MaPhong);
                var kho = dsKho.Models.FirstOrDefault(k => k.MaKho == phieu.MaKho);

                thongTinPhieu = new BanGiaoTaiSanDTO
                {
                    MaBanGiaoTS = phieu.MaBanGiaoTS,
                    NgayBanGiao = phieu.NgayBanGiao,
                    MaNV = phieu.MaNV,
                    TenNV = nvBanGiao?.TenNV,
                    MaPhong = phieu.MaPhong,
                    TenPhong = phong?.TenPhong,
                    NoiDung = phieu.NoiDung,
                    TrangThai = phieu.TrangThai,
                    CbTiepNhan = phieu.CbTiepNhan,
                    MaKho = phieu.MaKho,
                    TenKho = kho?.TenKho
                };

                txtMaPhieu.Text = "PBG" + thongTinPhieu.MaBanGiaoTS;
                txtPhong.Text = thongTinPhieu.TenPhong;
                txtNguoiBanGiao.Text = thongTinPhieu.TenNV;
                txtCbTiepNhan.Text = thongTinPhieu.CbTiepNhan;
                txtNgayBanGiao.Text = thongTinPhieu.NgayBanGiao.ToString("dd/MM/yyyy");
                txtKho.Text = thongTinPhieu.TenKho;
                txtNoiDung.Text = thongTinPhieu.NoiDung;
                txtTrangThai.Text = thongTinPhieu.TrangThaiText;

                danhSachChiTiet = (from ct in dsChiTiet.Models
                                   join ts in dsTaiSan.Models on ct.MaTaiSan equals ts.MaTaiSan
                                   select new ChiTietBanGiaoDTO
                                   {
                                       MaChiTietBG = ct.MaChiTietBG,
                                       MaBanGiaoTS = ct.MaBanGiaoTS,
                                       MaTaiSan = ct.MaTaiSan,
                                       TenTaiSan = ts.TenTaiSan,
                                       ViTriTS = ct.ViTriTS,
                                       GhiChu = ct.GhiChu
                                   }).ToList();

                dgChiTietPhieuBanGiao.ItemsSource = danhSachChiTiet;
                txtStatus.Text = $"Tổng số dòng chi tiết: {danhSachChiTiet.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi load dữ liệu phiếu bàn giao: " + ex.Message);
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
                    FileName = $"PhieuBanGiao_{maPhieuBanGiao}_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (dialog.ShowDialog() == true)
                {
                    string filePath = dialog.FileName;
                    ExportToPDF(filePath);

                    // ✅ Gọi form SuccessNotificationDialog thay vì MessageBox
                    var dialogSuccess = new SuccessNotificationDialog(
                        "Xuất PDF thành công",
                        "Bạn có muốn mở file PDF vừa tạo không?",
                        filePath // đường dẫn để mở nếu người dùng nhấn "MỞ FILE"
                    );
                    dialogSuccess.ShowDialog(); // sẽ tự xử lý nút MỞ FILE hoặc OK
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

            doc.Add(new Paragraph("PHIẾU BÀN GIAO TÀI SẢN").SetFont(font).SetFontSize(16).SetBold().SetTextAlignment(iTextTextAlignment.CENTER));
            doc.Add(new Paragraph($"Số phiếu: PBG{thongTinPhieu.MaBanGiaoTS}     Ngày bàn giao: {thongTinPhieu.NgayBanGiao:dd/MM/yyyy}").SetFont(font).SetMarginBottom(10));

            Table info = new Table(UnitValue.CreatePercentArray(new float[] { 25, 75 })).UseAllAvailableWidth();
            info.AddCell(Cell("Phòng:", font, true)); info.AddCell(Cell(thongTinPhieu.TenPhong ?? "", font));
            info.AddCell(Cell("Người bàn giao:", font, true)); info.AddCell(Cell(thongTinPhieu.TenNV ?? "", font));
            info.AddCell(Cell("Người tiếp nhận:", font, true)); info.AddCell(Cell(thongTinPhieu.CbTiepNhan ?? "", font));
            info.AddCell(Cell("Kho:", font, true)); info.AddCell(Cell(thongTinPhieu.TenKho ?? "", font));
            info.AddCell(Cell("Nội dung:", font, true)); info.AddCell(Cell(thongTinPhieu.NoiDung ?? "", font));
            info.AddCell(Cell("Trạng thái:", font, true)); info.AddCell(Cell(thongTinPhieu.TrangThaiText ?? "", font));
            doc.Add(info);

            doc.Add(new Paragraph("\nDANH SÁCH TÀI SẢN BÀN GIAO").SetFont(font).SetBold());
            Table table = new Table(new float[] { 80, 200, 150, 200 }).UseAllAvailableWidth();
            string[] headers = { "Mã chi tiết", "Tên tài sản", "Vị trí", "Ghi chú" };
            foreach (var h in headers)
            {
                table.AddHeaderCell(new Cell()
                    .Add(new Paragraph(h).SetFont(font).SetBold())
                    .SetBackgroundColor(new DeviceRgb(230, 230, 230))
                 .SetTextAlignment(iTextTextAlignment.CENTER)
                   .SetVerticalAlignment(iTextVerticalAlignment.MIDDLE));
            }

            foreach (var ct in danhSachChiTiet)
            {
                table.AddCell(new Cell().Add(new Paragraph(ct.MaChiTietBG.ToString()).SetFont(font)).SetTextAlignment(iTextTextAlignment.CENTER).SetVerticalAlignment(iTextVerticalAlignment.MIDDLE));
                table.AddCell(new Cell().Add(new Paragraph(ct.TenTaiSan ?? "").SetFont(font)).SetTextAlignment(iTextTextAlignment.CENTER).SetVerticalAlignment(iTextVerticalAlignment.MIDDLE));
                table.AddCell(new Cell().Add(new Paragraph(ct.ViTriTS.ToString()).SetFont(font)).SetTextAlignment(iTextTextAlignment.CENTER).SetVerticalAlignment(iTextVerticalAlignment.MIDDLE));
                table.AddCell(new Cell().Add(new Paragraph(ct.GhiChu ?? "").SetFont(font)).SetTextAlignment(iTextTextAlignment.CENTER).SetVerticalAlignment(iTextVerticalAlignment.MIDDLE));
            }

            doc.Add(table);

            doc.Add(new Paragraph("\n\n"));
            Table sign = new Table(3).UseAllAvailableWidth();
            sign.AddCell(SignatureCell("NGƯỜI BÀN GIAO", thongTinPhieu.TenNV ?? "", font));
            sign.AddCell(SignatureCell("NGƯỜI TIẾP NHẬN", thongTinPhieu.CbTiepNhan ?? "", font));
            sign.AddCell(SignatureCell("NGƯỜI PHÊ DUYỆT", "", font));
            doc.Add(sign);

            doc.Add(new Paragraph($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm:ss}").SetFont(font).SetFontSize(8).SetItalic().SetTextAlignment(iTextTextAlignment.RIGHT));
        }

        private Cell Cell(string text, PdfFont font, bool bold = false)
        {
            var p = new Paragraph(text ?? "").SetFont(font);
            if (bold) p.SetBold();
            return new Cell().Add(p).SetBorder(Border.NO_BORDER).SetPadding(2);
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
    }
}