using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models.BanGiaoTaiSan;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Services.BanGiaoTaiSanService;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.IO.Font;
using iText.Kernel.Font;
using Microsoft.Win32;
using iText.IO.Image;
using System.IO;
using Border = iText.Layout.Borders.Border;
using TextAlignment = iText.Layout.Properties.TextAlignment;
using VerticalAlignment = iText.Layout.Properties.VerticalAlignment;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class ChiTietBanGiaoWindow : Window
    {
        private int _maBanGiao;
        private BanGiaoTaiSanDTO _thongTinBanGiao;
        private ObservableCollection<ChiTietBanGiaoDTO> _dsChiTiet;
        private bool _isThongTinChanged = false;

        public ChiTietBanGiaoWindow(int maBanGiao)
        {
            InitializeComponent();
            _maBanGiao = maBanGiao;

            // Load dữ liệu
            Loaded += ChiTietBanGiaoWindow_Loaded;
        }

        private async void ChiTietBanGiaoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                ShowLoading(true);

                // Lấy thông tin phiếu bàn giao
                var dsBanGiao = await BanGiaoTaiSanService.LayDanhSachPhieuBanGiaoAsync();
                _thongTinBanGiao = dsBanGiao.FirstOrDefault(p => p.MaBanGiaoTS == _maBanGiao);

                if (_thongTinBanGiao == null)
                {
                    MessageBox.Show($"Không tìm thấy thông tin phiếu bàn giao với mã {_maBanGiao}.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                // Hiển thị thông tin phiếu
                txtMaPhieu.Text = _thongTinBanGiao.MaBanGiaoTS.ToString();
                txtNgayBanGiao.Text = _thongTinBanGiao.NgayBanGiao.ToString("dd/MM/yyyy HH:mm");
                txtNguoiLap.Text = _thongTinBanGiao.TenNV;
                txtPhong.Text = _thongTinBanGiao.TenPhong;
                txtToaNha.Text = _thongTinBanGiao.TenToaNha;
                txtNoiDung.Text = _thongTinBanGiao.NoiDung;

                // Hiển thị thông tin người tiếp nhận
                txtNguoiTiepNhan.Text = _thongTinBanGiao.CbTiepNhan;

                // Định dạng trạng thái
                txtTrangThai.Text = _thongTinBanGiao.TrangThaiText;
                switch (_thongTinBanGiao.TrangThaiText)
                {
                    case "Chờ duyệt":
                        txtTrangThai.Foreground = new SolidColorBrush(Colors.Orange);
                        break;
                    case "Đã duyệt":
                        txtTrangThai.Foreground = new SolidColorBrush(Colors.Green);
                        break;
                    case "Từ chối duyệt":
                        txtTrangThai.Foreground = new SolidColorBrush(Colors.Red);
                        break;
                }

                // Lấy chi tiết bàn giao
                _dsChiTiet = await BanGiaoTaiSanService.LayDanhSachChiTietBanGiaoAsync(_maBanGiao);
                dgChiTietBanGiao.ItemsSource = _dsChiTiet;

                // Cập nhật tiêu đề cửa sổ
                this.Title = $"Chi Tiết Phiếu Bàn Giao - Mã phiếu: {_maBanGiao}";

                // Reset trạng thái thay đổi
                _isThongTinChanged = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void txtNguoiTiepNhan_TextChanged(object sender, TextChangedEventArgs e)
        {
            _isThongTinChanged = true;
        }

        private async void btnLuuThongTin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isThongTinChanged)
                {
                    MessageBox.Show("Không có thông tin nào thay đổi để lưu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                ShowLoading(true);

                // Cập nhật thông tin người tiếp nhận
                var client = await SupabaseService.GetClientAsync();

                // Lấy phiếu bàn giao hiện tại
                var response = await client.From<BanGiaoTaiSanModel>()
                    .Filter("ma_bang_giao_ts", Supabase.Postgrest.Constants.Operator.Equals, _maBanGiao)
                    .Get();

                if (response.Models.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy phiếu bàn giao để cập nhật.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var phieuBanGiao = response.Models.First();
                phieuBanGiao.CbTiepNhan = txtNguoiTiepNhan.Text.Trim();

                // Cập nhật vào database
                var updateResponse = await client.From<BanGiaoTaiSanModel>()
                    .Filter("ma_bang_giao_ts", Supabase.Postgrest.Constants.Operator.Equals, _maBanGiao)
                    .Update(phieuBanGiao);

                if (updateResponse.Models.Count > 0)
                {
                    MessageBox.Show("Cập nhật thông tin người tiếp nhận thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Cập nhật lại thông tin phiếu sau khi lưu
                    await LoadData();
                }
                else
                {
                    MessageBox.Show("Không thể cập nhật thông tin người tiếp nhận.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật thông tin: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void btnDong_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra nếu có thay đổi chưa lưu
            if (_isThongTinChanged)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Bạn có thông tin chưa lưu. Bạn có muốn lưu trước khi đóng không?",
                    "Xác nhận",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Gọi sự kiện lưu thông tin
                    btnLuuThongTin_Click(sender, e);
                    return;
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            this.Close();
        }

        private async void btnInPhieu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Hiển thị SaveFileDialog để chọn nơi lưu file PDF
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF Files|*.pdf",
                    Title = "Lưu phiếu bàn giao",
                    FileName = $"PhieuBanGiaoTaiSan_{_maBanGiao}_{DateTime.Now:yyyyMMdd_HHmmss}" // Add timestamp to filename
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    ShowLoading(true);

                    // Nếu người tiếp nhận chưa được cập nhật, hỏi người dùng có muốn cập nhật không
                    if (string.IsNullOrWhiteSpace(_thongTinBanGiao.CbTiepNhan) &&
                        !string.IsNullOrWhiteSpace(txtNguoiTiepNhan.Text) &&
                        _isThongTinChanged)
                    {
                        MessageBoxResult result = MessageBox.Show(
                            "Thông tin người tiếp nhận đã thay đổi nhưng chưa được lưu. Bạn có muốn lưu trước khi in phiếu không?",
                            "Xác nhận",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            await LuuNguoiTiepNhan();
                            await LoadData(); // Tải lại dữ liệu sau khi lưu
                        }
                    }

                    string filePath = saveFileDialog.FileName;
                    bool success = false;
                    int retryCount = 0;

                    // Try to create the PDF, with retry logic
                    while (!success && retryCount < 3)
                    {
                        try
                        {
                            // Tạo file PDF
                            await Task.Run(() => TaoFilePDF(filePath));
                            success = true;
                        }
                        catch (IOException ioEx) when (ioEx.HResult == unchecked((int)0x80070020))
                        {
                            // File is in use, modify filename and try again
                            retryCount++;
                            string directory = System.IO.Path.GetDirectoryName(filePath);
                            string filename = System.IO.Path.GetFileNameWithoutExtension(filePath);
                            string extension = System.IO.Path.GetExtension(filePath);

                            // Add a unique identifier to the filename
                            filePath = System.IO.Path.Combine(directory, $"{filename}_{Guid.NewGuid().ToString().Substring(0, 8)}{extension}");

                            if (retryCount >= 3)
                            {
                                throw; // Give up after 3 retries
                            }
                        }
                    }

                    MessageBox.Show($"Xuất phiếu bàn giao thành công!\nFile được lưu tại: {filePath}",
                        "Thông báo",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất phiếu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async Task LuuNguoiTiepNhan()
        {
            try
            {
                // Cập nhật thông tin người tiếp nhận
                var client = await SupabaseService.GetClientAsync();

                // Lấy phiếu bàn giao hiện tại
                var response = await client.From<BanGiaoTaiSanModel>()
                    .Filter("ma_bang_giao_ts", Supabase.Postgrest.Constants.Operator.Equals, _maBanGiao)
                    .Get();

                if (response.Models.Count == 0)
                {
                    throw new Exception("Không tìm thấy phiếu bàn giao để cập nhật.");
                }

                var phieuBanGiao = response.Models.First();
                phieuBanGiao.CbTiepNhan = txtNguoiTiepNhan.Text.Trim();

                // Cập nhật vào database
                var updateResponse = await client.From<BanGiaoTaiSanModel>()
                    .Filter("ma_bang_giao_ts", Supabase.Postgrest.Constants.Operator.Equals, _maBanGiao)
                    .Update(phieuBanGiao);

                if (updateResponse.Models.Count == 0)
                {
                    throw new Exception("Không thể cập nhật thông tin người tiếp nhận.");
                }

                // Cập nhật thông tin người tiếp nhận trong DTO
                _thongTinBanGiao.CbTiepNhan = txtNguoiTiepNhan.Text.Trim();
                _isThongTinChanged = false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lưu thông tin người tiếp nhận: {ex.Message}");
            }
        }
        private void TaoFilePDF(string filePath)
        {
            // Khởi tạo writer và document
            using (PdfWriter writer = new PdfWriter(filePath))
            using (PdfDocument pdf = new PdfDocument(writer))
            {
                // Thiết lập kích thước giấy A4
                PageSize pageSize = PageSize.A4;
                Document document = new Document(pdf, pageSize);
                document.SetMargins(36, 36, 36, 36); // 36 points = 0.5 inch margins

                // Tạo font tiếng Việt từ font có sẵn trong Windows
                PdfFont font = null;
                try
                {
                    // Thử dùng các font phổ biến có hỗ trợ tiếng Việt
                    string[] fontPaths = {
                // Font Times New Roman (phổ biến)
                @"C:\Windows\Fonts\times.ttf",
                // Font Arial (phổ biến)
                @"C:\Windows\Fonts\arial.ttf",
                // Font Segoe UI (mới hơn, có trong Windows 10)
                @"C:\Windows\Fonts\segoeui.ttf"
            };

                    // Thử từng font cho đến khi thành công
                    foreach (var fontPath in fontPaths)
                    {
                        try
                        {
                            if (System.IO.File.Exists(fontPath))
                            {
                                font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);
                                break;
                            }
                        }
                        catch { }
                    }

                    // Nếu không tìm thấy font nào, dùng font mặc định
                    if (font == null)
                    {
                        // Sử dụng font mặc định
                        font = PdfFontFactory.CreateFont();
                    }
                }
                catch (Exception)
                {
                    // Nếu có lỗi, sử dụng font mặc định
                    font = PdfFontFactory.CreateFont();
                }

                // Tiêu đề phiếu
                Paragraph title = new Paragraph("PHIẾU BÀN GIAO TÀI SẢN")
                    .SetFont(font)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(16)
                    .SetBold()
                    .SetMarginBottom(10);
                document.Add(title);

                // Số phiếu và ngày bàn giao
                Paragraph info = new Paragraph()
                    .SetFont(font)
                    .Add(new Text($"Số phiếu: {_thongTinBanGiao.MaBanGiaoTS}").SetFont(font).SetBold())
                    .Add(new Text($"                            Ngày bàn giao: {_thongTinBanGiao.NgayBanGiao:dd/MM/yyyy HH:mm}").SetFont(font))
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetMarginBottom(10);
                document.Add(info);

                // Thông tin người lập phiếu và phòng
                Table infoTable = new Table(new float[] { 150, 350 });
                infoTable.SetWidth(UnitValue.CreatePercentValue(100));

                // Người lập phiếu
                infoTable.AddCell(new Cell().Add(new Paragraph("Người lập phiếu:").SetFont(font).SetBold()).SetBorder(Border.NO_BORDER));
                infoTable.AddCell(new Cell().Add(new Paragraph(_thongTinBanGiao.TenNV).SetFont(font)).SetBorder(Border.NO_BORDER));

                // Phòng bàn giao
                infoTable.AddCell(new Cell().Add(new Paragraph("Phòng bàn giao:").SetFont(font).SetBold()).SetBorder(Border.NO_BORDER));
                infoTable.AddCell(new Cell().Add(new Paragraph(_thongTinBanGiao.TenPhong).SetFont(font)).SetBorder(Border.NO_BORDER));

                // Tòa nhà
                infoTable.AddCell(new Cell().Add(new Paragraph("Tòa nhà:").SetFont(font).SetBold()).SetBorder(Border.NO_BORDER));
                infoTable.AddCell(new Cell().Add(new Paragraph(_thongTinBanGiao.TenToaNha).SetFont(font)).SetBorder(Border.NO_BORDER));

                // Trạng thái
                infoTable.AddCell(new Cell().Add(new Paragraph("Trạng thái:").SetFont(font).SetBold()).SetBorder(Border.NO_BORDER));
                infoTable.AddCell(new Cell().Add(new Paragraph(_thongTinBanGiao.TrangThaiText).SetFont(font)).SetBorder(Border.NO_BORDER));

                // Người tiếp nhận (Thêm mới)
                infoTable.AddCell(new Cell().Add(new Paragraph("Người tiếp nhận:").SetFont(font).SetBold()).SetBorder(Border.NO_BORDER));
                infoTable.AddCell(new Cell().Add(new Paragraph(_thongTinBanGiao.CbTiepNhan ?? "-").SetFont(font)).SetBorder(Border.NO_BORDER));

                // Nội dung
                infoTable.AddCell(new Cell().Add(new Paragraph("Nội dung:").SetFont(font).SetBold()).SetBorder(Border.NO_BORDER));
                infoTable.AddCell(new Cell().Add(new Paragraph(_thongTinBanGiao.NoiDung ?? "-").SetFont(font)).SetBorder(Border.NO_BORDER));

                document.Add(infoTable);
                document.Add(new Paragraph("\n").SetFont(font));

                // Tạo bảng chi tiết tài sản
                Paragraph tableTitle = new Paragraph("DANH SÁCH TÀI SẢN BÀN GIAO")
                    .SetFont(font)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(14)
                    .SetBold()
                    .SetMarginBottom(10);
                document.Add(tableTitle);

                // Tạo bảng chi tiết
                Table table = new Table(new float[] { 30, 70, 180, 80, 70, 80 });
                table.SetWidth(UnitValue.CreatePercentValue(100));

                // Header của bảng
                Cell[] headerCells = new Cell[] {
            new Cell().Add(new Paragraph("STT").SetFont(font).SetBold()),
            new Cell().Add(new Paragraph("Mã tài sản").SetFont(font).SetBold()),
            new Cell().Add(new Paragraph("Tên tài sản").SetFont(font).SetBold()),
            new Cell().Add(new Paragraph("Số Seri").SetFont(font).SetBold()),
            new Cell().Add(new Paragraph("Vị trí").SetFont(font).SetBold()),
            new Cell().Add(new Paragraph("Ghi chú").SetFont(font).SetBold())
        };

                foreach (var cell in headerCells)
                {
                    cell.SetBackgroundColor(new DeviceRgb(220, 220, 220));
                    cell.SetTextAlignment(TextAlignment.CENTER);
                    // Bỏ qua phần thiết lập căn chỉnh dọc vì đang gây lỗi
                    cell.SetPadding(5);
                    table.AddHeaderCell(cell);
                }

                // Dữ liệu cho bảng
                for (int i = 0; i < _dsChiTiet.Count; i++)
                {
                    var item = _dsChiTiet[i];

                    // STT
                    Cell cellSTT = new Cell().Add(new Paragraph((i + 1).ToString()).SetFont(font))
                        .SetTextAlignment(TextAlignment.CENTER);
                    // Bỏ qua phần thiết lập căn chỉnh dọc vì đang gây lỗi
                    table.AddCell(cellSTT);

                    // Mã tài sản
                    Cell cellMaTS = new Cell().Add(new Paragraph(item.MaTaiSan.ToString()).SetFont(font))
                        .SetTextAlignment(TextAlignment.CENTER);
                    table.AddCell(cellMaTS);

                    // Tên tài sản
                    Cell cellTenTS = new Cell().Add(new Paragraph(item.TenTaiSan).SetFont(font));
                    table.AddCell(cellTenTS);

                    // Số Seri
                    Cell cellSeri = new Cell().Add(new Paragraph(item.SoSeri ?? "-").SetFont(font))
                        .SetTextAlignment(TextAlignment.CENTER);
                    table.AddCell(cellSeri);

                    // Vị trí
                    Cell cellViTri = new Cell().Add(new Paragraph(item.ViTriTS.ToString()).SetFont(font))
                        .SetTextAlignment(TextAlignment.CENTER);
                    table.AddCell(cellViTri);

                    // Ghi chú
                    Cell cellGhiChu = new Cell().Add(new Paragraph(item.GhiChu ?? "-").SetFont(font));
                    table.AddCell(cellGhiChu);
                }

                document.Add(table);

                // Thêm phần chữ ký
                document.Add(new Paragraph("\n\n").SetFont(font));
                Table signatureTable = new Table(new float[] { 1, 1, 1 });
                signatureTable.SetWidth(UnitValue.CreatePercentValue(100));

                // Người lập phiếu
                Cell nguoiLapCell = new Cell()
                    .Add(new Paragraph("NGƯỜI LẬP PHIẾU")
                        .SetFont(font)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBold())
                    .Add(new Paragraph("(Ký và ghi rõ họ tên)")
                        .SetFont(font)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetItalic()
                        .SetFontSize(10))
                    .Add(new Paragraph("\n\n\n").SetFont(font))
                    .Add(new Paragraph(_thongTinBanGiao.TenNV)
                        .SetFont(font)
                        .SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER);

                // Người kiểm tra/phê duyệt
                Cell nguoiDuyetCell = new Cell()
                    .Add(new Paragraph("NGƯỜI PHÊ DUYỆT")
                        .SetFont(font)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBold())
                    .Add(new Paragraph("(Ký và ghi rõ họ tên)")
                        .SetFont(font)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetItalic()
                        .SetFontSize(10))
                    .Add(new Paragraph("\n\n\n").SetFont(font))
                    .Add(new Paragraph(""))
                    .SetBorder(Border.NO_BORDER);

                // Người nhận
                Cell nguoiNhanCell = new Cell()
                    .Add(new Paragraph("NGƯỜI TIẾP NHẬN")
                        .SetFont(font)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBold())
                    .Add(new Paragraph("(Ký và ghi rõ họ tên)")
                        .SetFont(font)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetItalic()
                        .SetFontSize(10))
                    .Add(new Paragraph("\n\n\n").SetFont(font))
                    .Add(new Paragraph(string.IsNullOrWhiteSpace(_thongTinBanGiao.CbTiepNhan) ? "" : _thongTinBanGiao.CbTiepNhan)
                        .SetFont(font)
                        .SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER);

                signatureTable.AddCell(nguoiLapCell);
                signatureTable.AddCell(nguoiDuyetCell);
                signatureTable.AddCell(nguoiNhanCell);

                document.Add(signatureTable);

                // Thêm footer
                Paragraph footer = new Paragraph($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                    .SetFont(font)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetFontSize(8)
                    .SetItalic();
                document.Add(footer);

                // Đóng document
                document.Close();
            }
        }

        private void ShowLoading(bool isShow)
        {
            LoadingOverlay.Visibility = isShow ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}