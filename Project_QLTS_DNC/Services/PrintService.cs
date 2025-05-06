using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Printing;
using System.Linq;
using System.Windows.Media;
using Project_QLTS_DNC.DTOs;

namespace Project_QLTS_DNC.Services
{
    public class PrintService
    {
        /// <summary>
        /// In phiếu bàn giao trực tiếp không thông qua file PDF
        /// </summary>
        public static void InPhieuBanGiao(BanGiaoTaiSanDTO thongTinBanGiao, IEnumerable<ChiTietBanGiaoDTO> dsChiTiet)
        {
            try
            {
                // Tạo FlowDocument chứa nội dung in
                System.Windows.Documents.FlowDocument document = TaoMauPhieuBanGiao(thongTinBanGiao, dsChiTiet);

                // Hiển thị hộp thoại in
                System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Thiết lập trang in
                    document.PageHeight = printDialog.PrintableAreaHeight;
                    document.PageWidth = printDialog.PrintableAreaWidth;
                    document.PagePadding = new Thickness(50);
                    document.ColumnGap = 0;
                    document.ColumnWidth = printDialog.PrintableAreaWidth;

                    // Tạo paginator để xử lý in nhiều trang
                    System.Windows.Documents.IDocumentPaginatorSource paginatorSource = document;
                    printDialog.PrintDocument(paginatorSource.DocumentPaginator, $"Phiếu Bàn Giao - {thongTinBanGiao.MaBanGiaoTS}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in phiếu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Tạo mẫu phiếu bàn giao dạng FlowDocument để in
        /// </summary>
        private static System.Windows.Documents.FlowDocument TaoMauPhieuBanGiao(BanGiaoTaiSanDTO thongTinBanGiao, IEnumerable<ChiTietBanGiaoDTO> dsChiTiet)
        {
            // Khởi tạo document
            System.Windows.Documents.FlowDocument document = new System.Windows.Documents.FlowDocument();
            document.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            document.FontSize = 12;

            // Tiêu đề phiếu
            System.Windows.Documents.Paragraph title = new System.Windows.Documents.Paragraph(
                new System.Windows.Documents.Run("PHIẾU BÀN GIAO TÀI SẢN"))
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                TextAlignment = System.Windows.TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            document.Blocks.Add(title);

            // Thông tin phiếu
            System.Windows.Documents.Paragraph info = new System.Windows.Documents.Paragraph();
            info.Inlines.Add(new System.Windows.Documents.Run($"Số phiếu: {thongTinBanGiao.MaBanGiaoTS}")
            { FontWeight = FontWeights.Bold });
            info.Inlines.Add(new System.Windows.Documents.Run($"                            Ngày bàn giao: {thongTinBanGiao.NgayBanGiao:dd/MM/yyyy HH:mm}"));
            info.Margin = new Thickness(0, 0, 0, 10);
            document.Blocks.Add(info);

            // Thông tin chi tiết phiếu
            System.Windows.Documents.Table infoTable = new System.Windows.Documents.Table();
            infoTable.CellSpacing = 0;

            // Thêm cột cho bảng
            System.Windows.Documents.TableColumn column1 = new System.Windows.Documents.TableColumn();
            column1.Width = new GridLength(150);
            infoTable.Columns.Add(column1);

            System.Windows.Documents.TableColumn column2 = new System.Windows.Documents.TableColumn();
            column2.Width = new GridLength(350);
            infoTable.Columns.Add(column2);

            // Thêm các dòng thông tin
            ThemDongThongTin(infoTable, "Người lập phiếu:", thongTinBanGiao.TenNV);
            ThemDongThongTin(infoTable, "Phòng bàn giao:", thongTinBanGiao.TenPhong);
            ThemDongThongTin(infoTable, "Tòa nhà:", thongTinBanGiao.TenToaNha);
            ThemDongThongTin(infoTable, "Trạng thái:", thongTinBanGiao.TrangThaiText);
            ThemDongThongTin(infoTable, "Người tiếp nhận:", thongTinBanGiao.CbTiepNhan ?? "-");
            ThemDongThongTin(infoTable, "Nội dung:", thongTinBanGiao.NoiDung ?? "-");

            document.Blocks.Add(infoTable);

            // Tạo phân nhóm tài sản
            var danhSachNhom = dsChiTiet
                .GroupBy(x => new { x.MaNhomTS, x.TenNhomTS })
                .Select(g => new
                {
                    MaNhomTS = g.Key.MaNhomTS ?? 0,
                    TenNhomTS = g.Key.TenNhomTS ?? "Chưa phân loại",
                    SoLuong = g.Count()
                })
                .ToList();

            // Tiêu đề danh sách tài sản
            System.Windows.Documents.Paragraph tableTitle = new System.Windows.Documents.Paragraph(
                new System.Windows.Documents.Run("DANH SÁCH TÀI SẢN BÀN GIAO (THEO NHÓM)"))
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextAlignment = System.Windows.TextAlignment.Center,
                Margin = new Thickness(0, 20, 0, 10)
            };
            document.Blocks.Add(tableTitle);

            // Bảng danh sách tài sản theo nhóm
            System.Windows.Documents.Table assetTable = new System.Windows.Documents.Table();
            assetTable.CellSpacing = 0;

            // Thêm cột cho bảng tài sản
            System.Windows.Documents.TableColumn assetCol1 = new System.Windows.Documents.TableColumn();
            assetCol1.Width = new GridLength(80);
            assetTable.Columns.Add(assetCol1);

            System.Windows.Documents.TableColumn assetCol2 = new System.Windows.Documents.TableColumn();
            assetCol2.Width = new GridLength(300);
            assetTable.Columns.Add(assetCol2);

            System.Windows.Documents.TableColumn assetCol3 = new System.Windows.Documents.TableColumn();
            assetCol3.Width = new GridLength(80);
            assetTable.Columns.Add(assetCol3);

            // Thiết lập đường viền cho bảng
            assetTable.BorderBrush = Brushes.Black;
            assetTable.BorderThickness = new Thickness(1);

            // Tạo header
            System.Windows.Documents.TableRowGroup headerGroup = new System.Windows.Documents.TableRowGroup();
            System.Windows.Documents.TableRow headerRow = new System.Windows.Documents.TableRow();
            headerRow.Background = new SolidColorBrush(Colors.LightGray);

            // Header cells
            System.Windows.Documents.TableCell headerCell1 = ThemHeaderCell("Mã nhóm");
            headerRow.Cells.Add(headerCell1);

            System.Windows.Documents.TableCell headerCell2 = ThemHeaderCell("Tên nhóm tài sản");
            headerRow.Cells.Add(headerCell2);

            System.Windows.Documents.TableCell headerCell3 = ThemHeaderCell("Số lượng");
            headerRow.Cells.Add(headerCell3);

            headerGroup.Rows.Add(headerRow);
            assetTable.RowGroups.Add(headerGroup);

            // Thêm dữ liệu nhóm tài sản
            System.Windows.Documents.TableRowGroup dataGroup = new System.Windows.Documents.TableRowGroup();

            foreach (var nhom in danhSachNhom)
            {
                System.Windows.Documents.TableRow row = new System.Windows.Documents.TableRow();

                // Mã nhóm
                System.Windows.Documents.TableCell cell1 = new System.Windows.Documents.TableCell(
                    new System.Windows.Documents.Paragraph(
                        new System.Windows.Documents.Run(nhom.MaNhomTS.ToString())));
                cell1.TextAlignment = System.Windows.TextAlignment.Center;
                row.Cells.Add(cell1);

                // Tên nhóm
                System.Windows.Documents.TableCell cell2 = new System.Windows.Documents.TableCell(
                    new System.Windows.Documents.Paragraph(
                        new System.Windows.Documents.Run(nhom.TenNhomTS)));
                row.Cells.Add(cell2);

                // Số lượng
                System.Windows.Documents.TableCell cell3 = new System.Windows.Documents.TableCell(
                    new System.Windows.Documents.Paragraph(
                        new System.Windows.Documents.Run(nhom.SoLuong.ToString())));
                cell3.TextAlignment = System.Windows.TextAlignment.Center;
                row.Cells.Add(cell3);

                dataGroup.Rows.Add(row);
            }

            assetTable.RowGroups.Add(dataGroup);
            document.Blocks.Add(assetTable);

            // Phần chữ ký
            System.Windows.Documents.Paragraph signSpace = new System.Windows.Documents.Paragraph()
            { Margin = new Thickness(0, 30, 0, 0) };
            document.Blocks.Add(signSpace);

            System.Windows.Documents.Table signTable = new System.Windows.Documents.Table();
            signTable.CellSpacing = 10;

            // Ba cột chữ ký
            signTable.Columns.Add(new System.Windows.Documents.TableColumn());
            signTable.Columns.Add(new System.Windows.Documents.TableColumn());
            signTable.Columns.Add(new System.Windows.Documents.TableColumn());

            System.Windows.Documents.TableRowGroup signGroup = new System.Windows.Documents.TableRowGroup();
            System.Windows.Documents.TableRow signRow = new System.Windows.Documents.TableRow();

            // Người lập phiếu
            signRow.Cells.Add(new System.Windows.Documents.TableCell(
                TaoKhungChuKy("NGƯỜI LẬP PHIẾU", "(Ký và ghi rõ họ tên)", thongTinBanGiao.TenNV)));

            // Người phê duyệt
            signRow.Cells.Add(new System.Windows.Documents.TableCell(
                TaoKhungChuKy("NGƯỜI PHÊ DUYỆT", "(Ký và ghi rõ họ tên)", "")));

            // Người tiếp nhận
            signRow.Cells.Add(new System.Windows.Documents.TableCell(
                TaoKhungChuKy("NGƯỜI TIẾP NHẬN", "(Ký và ghi rõ họ tên)", thongTinBanGiao.CbTiepNhan ?? "")));

            signGroup.Rows.Add(signRow);
            signTable.RowGroups.Add(signGroup);
            document.Blocks.Add(signTable);

            // Footer
            System.Windows.Documents.Paragraph footer = new System.Windows.Documents.Paragraph(
                new System.Windows.Documents.Run($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm:ss}"))
            {
                FontSize = 9,
                FontStyle = FontStyles.Italic,
                TextAlignment = System.Windows.TextAlignment.Right,
                Margin = new Thickness(0, 30, 0, 0)
            };
            document.Blocks.Add(footer);

            return document;
        }

        /// <summary>
        /// Thêm một dòng thông tin vào bảng
        /// </summary>
        private static void ThemDongThongTin(System.Windows.Documents.Table table, string label, string value)
        {
            System.Windows.Documents.TableRowGroup rowGroup = new System.Windows.Documents.TableRowGroup();
            System.Windows.Documents.TableRow row = new System.Windows.Documents.TableRow();

            // Label
            System.Windows.Documents.TableCell labelCell = new System.Windows.Documents.TableCell(
                new System.Windows.Documents.Paragraph(
                    new System.Windows.Documents.Run(label) { FontWeight = FontWeights.Bold }));
            row.Cells.Add(labelCell);

            // Value
            System.Windows.Documents.TableCell valueCell = new System.Windows.Documents.TableCell(
                new System.Windows.Documents.Paragraph(
                    new System.Windows.Documents.Run(value)));
            row.Cells.Add(valueCell);

            rowGroup.Rows.Add(row);
            table.RowGroups.Add(rowGroup);
        }

        /// <summary>
        /// Thêm cell header vào bảng
        /// </summary>
        private static System.Windows.Documents.TableCell ThemHeaderCell(string text)
        {
            System.Windows.Documents.TableCell cell = new System.Windows.Documents.TableCell(
                new System.Windows.Documents.Paragraph(
                    new System.Windows.Documents.Run(text) { FontWeight = FontWeights.Bold }));
            cell.TextAlignment = System.Windows.TextAlignment.Center;
            return cell;
        }

        /// <summary>
        /// Tạo khung chữ ký
        /// </summary>
        private static System.Windows.Documents.Block TaoKhungChuKy(string title, string subtitle, string name)
        {
            System.Windows.Documents.BlockUIContainer container = new System.Windows.Documents.BlockUIContainer();
            StackPanel panel = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center };

            // Tiêu đề
            TextBlock titleBlock = new TextBlock(new System.Windows.Documents.Run(title))
            {
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            panel.Children.Add(titleBlock);

            // Phụ đề
            TextBlock subtitleBlock = new TextBlock(new System.Windows.Documents.Run(subtitle))
            {
                FontStyle = FontStyles.Italic,
                FontSize = 10,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            panel.Children.Add(subtitleBlock);

            // Khoảng trống cho chữ ký
            TextBlock spaceBlock = new TextBlock(new System.Windows.Documents.Run("\n\n\n"));
            panel.Children.Add(spaceBlock);

            // Tên người ký
            TextBlock nameBlock = new TextBlock(new System.Windows.Documents.Run(name))
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };
            panel.Children.Add(nameBlock);

            container.Child = panel;
            return container;
        }
    }
}