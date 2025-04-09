using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Project_QLTS_DNC.Model;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.Rendering;
// Thêm các using cần thiết
using System.Drawing;
using System.Drawing.Imaging;
// Tránh xung đột namespace với iText
using iTextImage = iText.Layout.Element.Image;
using iTextTextAlignment = iText.Layout.Properties.TextAlignment;
using iTextErrorCorrectionLevel = iText.Barcodes.Qrcode.ErrorCorrectionLevel;
using ZXingErrorCorrectionLevel = ZXing.QrCode.Internal.ErrorCorrectionLevel;
using static Project_QLTS_DNC.Model.SanPham;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;


namespace Project_QLTS_DNC.View.QuanLySanPham
{
    public class BitmapRenderer : ZXing.Rendering.IBarcodeRenderer<Bitmap>
    {
        public Bitmap Render(BitMatrix matrix, BarcodeFormat format, string content)
        {
            return Render(matrix, format, content, new EncodingOptions());
        }

        public Bitmap Render(BitMatrix matrix, BarcodeFormat format, string content, EncodingOptions options)
        {
            int width = matrix.Width;
            int height = matrix.Height;

            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bitmap.SetPixel(x, y, matrix[x, y] ? Color.Black : Color.White);
                }
            }

            return bitmap;
        }
    }

    public partial class XuatQRCode : Window
    {
        private readonly ObservableCollection<SanPham> _allSanPham;
        private List<SanPham> _selectedSanPhams = new List<SanPham>();
        private List<NhomTSFilter> _nhomTSList;

        public XuatQRCode(ObservableCollection<SanPham> sanPhams, List<NhomTSFilter> nhomTSList)
        {
            InitializeComponent();
            _allSanPham = sanPhams;
            _nhomTSList = nhomTSList;

            // Khởi tạo danh sách tài sản
            dgSanPham.ItemsSource = _allSanPham;

            // Cài đặt ComboBox nhóm tài sản
            cboNhomTS.ItemsSource = _nhomTSList;
            cboNhomTS.SelectedIndex = 0;

            // Đăng ký sự kiện
            btnApplyFilter.Click += BtnApplyFilter_Click;
            btnSelectAll.Click += BtnSelectAll_Click;
            btnUnselectAll.Click += BtnUnselectAll_Click;
            btnExportQR.Click += BtnExportQR_Click;
            btnCancel.Click += BtnCancel_Click;
            txtSearchSeri.TextChanged += TxtSearchSeri_TextChanged;
            cboNhomTS.SelectionChanged += CboNhomTS_SelectionChanged;

            // Cập nhật trạng thái ban đầu
            UpdateStatusText();
        }

        private void CboNhomTS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TxtSearchSeri_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            IEnumerable<SanPham> filteredList = _allSanPham;

            // Lọc theo Số Seri
            if (!string.IsNullOrEmpty(txtSearchSeri.Text))
            {
                filteredList = filteredList.Where(sp =>
                    sp.SoSeri.ToLower().Contains(txtSearchSeri.Text.ToLower()));
            }

            // Lọc theo MaNhomTS
            if (cboNhomTS.SelectedIndex > 0)
            {
                int selectedMaNhomTS = ((NhomTSFilter)cboNhomTS.SelectedItem).MaNhomTS;
                filteredList = filteredList.Where(sp => sp.MaNhomTS == selectedMaNhomTS);
            }

            // Cập nhật DataGrid
            dgSanPham.ItemsSource = filteredList;

            // Cập nhật trạng thái
            UpdateStatusText();
        }

        private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            // Chọn tất cả sản phẩm đang hiển thị
            foreach (SanPham sp in dgSanPham.Items)
            {
                sp.IsSelected = true;
            }
            UpdateStatusText();
        }

        private void BtnUnselectAll_Click(object sender, RoutedEventArgs e)
        {
            // Bỏ chọn tất cả sản phẩm
            foreach (SanPham sp in dgSanPham.Items)
            {
                sp.IsSelected = false;
            }
            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            int selectedCount = dgSanPham.Items.Cast<SanPham>().Count(sp => sp.IsSelected);
            txtStatus.Text = $"Đã chọn: {selectedCount} / {dgSanPham.Items.Count} sản phẩm";
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateStatusText();
        }

        private void BtnExportQR_Click(object sender, RoutedEventArgs e)
        {
            // Lấy danh sách sản phẩm đã chọn
            _selectedSanPhams = dgSanPham.Items.Cast<SanPham>()
                .Where(sp => sp.IsSelected)
                .ToList();

            if (_selectedSanPhams.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một sản phẩm để xuất mã QR!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Hiển thị SaveFileDialog để chọn nơi lưu file PDF
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                FileName = $"QRCode_SanPham_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    ExportQRCodeToPDF(saveFileDialog.FileName);
                    MessageBox.Show($"Đã xuất {_selectedSanPhams.Count} mã QR thành công!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Đóng cửa sổ sau khi xuất thành công
                    this.DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xuất mã QR: {ex.Message}",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportQRCodeToPDF(string filePath)
        {
            using (PdfWriter writer = new PdfWriter(filePath))
            {
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    Document document = new Document(pdf);
                    document.SetMargins(36, 36, 36, 36);

                    // Tạo tiêu đề
                    document.Add(new Paragraph("DANH SÁCH MÃ QR SẢN PHẨM")
                        .SetTextAlignment(iTextTextAlignment.CENTER)
                        .SetFontSize(16)
                        .SetBold());

                    document.Add(new Paragraph($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                        .SetTextAlignment(iTextTextAlignment.RIGHT)
                        .SetFontSize(10)
                        .SetItalic());

                    document.Add(new Paragraph("\n"));

                    // Số cột hiển thị trên mỗi trang
                    int cols = 2;

                    // Tạo bảng để hiển thị các mã QR
                    Table table = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                        .UseAllAvailableWidth();

                    int count = 0;
                    foreach (SanPham sp in _selectedSanPhams)
                    {
                        // Tạo mã QR cho sản phẩm
                        byte[] qrCodeImage = GenerateQRCode(sp);

                        // Tạo ô chứa mã QR và thông tin sản phẩm
                        Cell cell = new Cell();
                        cell.SetPadding(10);
                        cell.SetTextAlignment(iTextTextAlignment.CENTER);

                        // Thêm ảnh mã QR
                        ImageData imageData = ImageDataFactory.Create(qrCodeImage);
                        iTextImage image = new iTextImage(imageData).SetWidth(120).SetHeight(120);
                        cell.Add(image);

                        // Thêm thông tin sản phẩm
                        cell.Add(new Paragraph($"Mã SP: {sp.MaSP}").SetFontSize(8));
                        cell.Add(new Paragraph($"Tên: {sp.TenSanPham}").SetFontSize(8));
                        cell.Add(new Paragraph($"Số Seri: {sp.SoSeri}").SetFontSize(8).SetBold());
                        cell.Add(new Paragraph($"Ngày SD: {sp.NgaySuDung:dd/MM/yyyy}").SetFontSize(8));

                        table.AddCell(cell);

                        count++;

                        // Nếu đã đủ số cột và không phải là sản phẩm cuối cùng, thêm một dòng mới
                        if (count % cols == 0 && count < _selectedSanPhams.Count)
                        {
                            // Không cần thao tác thêm vì table tự động xuống dòng
                        }
                    }

                    // Nếu số sản phẩm là lẻ, thêm ô trống vào cuối
                    if (count % cols != 0)
                    {
                        for (int i = 0; i < cols - (count % cols); i++)
                        {
                            table.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                        }
                    }

                    document.Add(table);
                    document.Close();
                }
            }
        }

        private byte[] GenerateQRCode(SanPham sanPham)
        {
            // Tạo nội dung cho mã QR (có thể tùy chỉnh theo nhu cầu)
            string qrContent = $"MaSP:{sanPham.MaSP}|SoSeri:{sanPham.SoSeri}|Ten:{sanPham.TenSanPham}";

            // Thiết lập các tham số cho mã QR
            QrCodeEncodingOptions options = new QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = 300,
                Height = 300,
                Margin = 1,
                ErrorCorrection = ZXingErrorCorrectionLevel.H // Mức độ sửa lỗi cao nhất
            };

            // Tạo mã QR
            BarcodeWriter<Bitmap> writer = new BarcodeWriter<Bitmap>
            {
                Format = BarcodeFormat.QR_CODE,
                Options = options,
                Renderer = new BitmapRenderer()
            };

            var qrCodeBitmap = writer.Write(qrContent);

            // Chuyển đổi Bitmap thành byte array
            using (MemoryStream stream = new MemoryStream())
            {
                qrCodeBitmap.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}