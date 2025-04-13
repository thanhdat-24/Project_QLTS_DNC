using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using ZXing.Rendering;
// Thêm các using cần thiết
using System.Drawing;
using System.Drawing.Imaging;
// Tránh xung đột namespace với iText
using iTextImage = iText.Layout.Element.Image;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iTextErrorCorrectionLevel = iText.Barcodes.Qrcode.ErrorCorrectionLevel;
using ZXingErrorCorrectionLevel = ZXing.QrCode.Internal.ErrorCorrectionLevel;
using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Services.QLTaiSanService;
using System.Windows.Input;
// Add explicit import for iText TextAlignment to avoid ambiguity
using iTextTextAlignment = iText.Layout.Properties.TextAlignment;
using System.ComponentModel;
using System.Diagnostics;

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
        private ObservableCollection<TaiSanQRDTO> _listTaiSan;
        private ObservableCollection<TaiSanQRDTO> _filteredTaiSan;
        private List<NhomTaiSanFilter> _nhomTSList;
        private int _totalItems = 0;
        private int _selectedItems = 0;

        public XuatQRCode(ObservableCollection<TaiSanDTO> dsTaiSan = null)
        {
            InitializeComponent();

            // Đăng ký events
            btnCancel.Click += BtnCancel_Click;
            btnExportQR.Click += BtnExportQR_Click;
            btnSelectAll.Click += BtnSelectAll_Click;
            btnUnselectAll.Click += BtnUnselectAll_Click;
            btnApplyFilter.Click += BtnApplyFilter_Click;
            txtSearchSeri.TextChanged += TxtSearchSeri_TextChanged;
            cboNhomTS.SelectionChanged += CboNhomTS_SelectionChanged;

            // Tải dữ liệu
            InitializeDataAsync(dsTaiSan);
        }

        private async void InitializeDataAsync(ObservableCollection<TaiSanDTO> dsTaiSan)
        {
            try
            {
                // Tải danh sách nhóm tài sản cho ComboBox filter
                await LoadNhomTaiSanAsync();

                if (dsTaiSan == null || dsTaiSan.Count == 0)
                {
                    // Nếu không có dữ liệu được truyền vào, tải từ service
                    var taiSanModels = await TaiSanService.LayDanhSachTaiSanAsync();
                    _listTaiSan = new ObservableCollection<TaiSanQRDTO>(
                        taiSanModels.Select(model => {
                            var dto = TaiSanDTO.FromModel(model);
                            return TaiSanQRDTO.FromTaiSanDTO(dto);
                        }));
                }
                else
                {
                    // Sử dụng dữ liệu đã truyền vào
                    _listTaiSan = new ObservableCollection<TaiSanQRDTO>(
                        dsTaiSan.Select(dto => TaiSanQRDTO.FromTaiSanDTO(dto)));
                }

                // Tải thông tin nhóm tài sản cho mỗi tài sản
                foreach (var taiSan in _listTaiSan)
                {
                    await taiSan.LoadNhomTaiSanInfoAsync();
                }

                // Gán nguồn dữ liệu cho DataGrid
                _filteredTaiSan = new ObservableCollection<TaiSanQRDTO>(_listTaiSan);
                dgSanPham.ItemsSource = _filteredTaiSan;

                _totalItems = _filteredTaiSan.Count;
                UpdateStatusText();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadNhomTaiSanAsync()
        {
            try
            {
                _nhomTSList = await NhomTaiSanService.GetNhomTaiSanFilterListAsync();
                cboNhomTS.ItemsSource = _nhomTSList;
                cboNhomTS.DisplayMemberPath = "TenNhomTS";
                cboNhomTS.SelectedIndex = 0; // Chọn "Tất cả" mặc định
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách nhóm tài sản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CboNhomTS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Áp dụng filter khi thay đổi combobox
            if (IsInitialized && _filteredTaiSan != null)
            {
                ApplyFilters();
            }
        }

        private void TxtSearchSeri_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Không cần apply filter ngay khi nhập, chỉ khi nhấn nút "Áp dụng" hoặc enter
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
            string searchText = txtSearchSeri.Text.ToLower().Trim();

            // Lọc theo số seri hoặc tên tài sản
            var filteredItems = _listTaiSan.Where(ts =>
                string.IsNullOrEmpty(searchText) ||
                (ts.SoSeri != null && ts.SoSeri.ToLower().Contains(searchText)) ||
                (ts.TenTaiSan != null && ts.TenTaiSan.ToLower().Contains(searchText))
            ).ToList();

            // Lọc theo nhóm tài sản (nếu đã chọn)
            if (cboNhomTS.SelectedItem != null && cboNhomTS.SelectedIndex > 0) // Bỏ qua item "Tất cả"
            {
                // Lấy mã nhóm tài sản đã chọn
                var selectedNhomTS = (NhomTaiSanFilter)cboNhomTS.SelectedItem;
                if (selectedNhomTS.MaNhomTS.HasValue)
                {
                    filteredItems = filteredItems.Where(ts => ts.MaNhomTS == selectedNhomTS.MaNhomTS).ToList();
                }
            }

            _filteredTaiSan.Clear();
            foreach (var item in filteredItems)
            {
                _filteredTaiSan.Add(item);
            }

            _totalItems = _filteredTaiSan.Count;
            _selectedItems = _filteredTaiSan.Count(item => item.IsSelected);
            UpdateStatusText();
        }

        private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _filteredTaiSan)
            {
                item.IsSelected = true;
            }
            _selectedItems = _filteredTaiSan.Count;
            UpdateStatusText();
        }

        private void BtnUnselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _filteredTaiSan)
            {
                item.IsSelected = false;
            }
            _selectedItems = 0;
            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            txtStatus.Text = $"Đã chọn: {_selectedItems} / {_totalItems} sản phẩm";
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            _selectedItems = _filteredTaiSan.Count(item => item.IsSelected);
            UpdateStatusText();
        }

        private void BtnExportQR_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = _filteredTaiSan.Where(item => item.IsSelected).ToList();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một sản phẩm để xuất mã QR.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = "QRCode_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    ExportQRCodeToPDF(saveFileDialog.FileName, selectedItems);
                    Mouse.OverrideCursor = null;

                    MessageBox.Show("Xuất mã QR thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Hỏi người dùng có muốn mở file PDF không
                    var result = MessageBox.Show("Bạn có muốn mở file PDF vừa tạo không?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        OpenPdfFile(saveFileDialog.FileName);
                    }
                }
                catch (Exception ex)
                {
                    Mouse.OverrideCursor = null;
                    // Ghi log lỗi
                    LogError(ex);
                    MessageBox.Show($"Lỗi khi xuất mã QR: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Phương thức mở file PDF với xử lý ngoại lệ chi tiết
        private void OpenPdfFile(string filePath)
        {
            try
            {
                // Kiểm tra file tồn tại trước
                if (File.Exists(filePath))
                {
                    // Sử dụng phương thức mở file mặc định của hệ điều hành
                    ProcessStartInfo startInfo = new ProcessStartInfo(filePath)
                    {
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(startInfo);
                }
                else
                {
                    MessageBox.Show("File không tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show($"Lỗi hệ thống khi mở file: {ex.Message}\nMã lỗi: {ex.ErrorCode}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                LogError(ex);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi không xác định khi mở file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                LogError(ex);
            }
        }

        // Phương thức ghi log lỗi
        private void LogError(Exception ex)
        {
            try
            {
                string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "QRCodeError.log");
                string errorMessage = $"Thời gian: {DateTime.Now}\n" +
                                      $"Lỗi: {ex.Message}\n" +
                                      $"Chi tiết: {ex.ToString()}\n" +
                                      $"Nguồn: {ex.Source}\n" +
                                      $"Ngăn xếp: {ex.StackTrace}\n" +
                                      "---------------------------------------------------\n";

                File.AppendAllText(logPath, errorMessage);
            }
            catch
            {
                // Nếu không thể ghi log, bỏ qua để tránh gây thêm lỗi
            }
        }

        private void ExportQRCodeToPDF(string filePath, List<TaiSanQRDTO> selectedItems)
        {
            // Cấu hình PDF writer
            using (PdfWriter writer = new PdfWriter(filePath))
            {
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    Document document = new Document(pdf);

                    // Tạo bảng 2x2 để hiển thị 4 mã QR trên mỗi trang
                    iText.Layout.Element.Table table = new iText.Layout.Element.Table(2);
                    table.SetWidth(UnitValue.CreatePercentValue(100));

                    int cellCount = 0;

                    foreach (var item in selectedItems)
                    {
                        // Tạo mã QR từ thông tin tài sản
                        string qrContent = $"https://yourwebsite.com/taisan?id={item.MaTaiSan}&seri={item.SoSeri}";

                        // Tạo QR code sử dụng ZXing
                        BarcodeWriter<Bitmap> barcodeWriter = new BarcodeWriter<Bitmap>();
                        barcodeWriter.Format = BarcodeFormat.QR_CODE;
                        barcodeWriter.Options = new QrCodeEncodingOptions
                        {
                            Height = 300,
                            Width = 300,
                            Margin = 0,
                            ErrorCorrection = ZXingErrorCorrectionLevel.H
                        };
                        barcodeWriter.Renderer = new BitmapRenderer();

                        using (Bitmap qrBitmap = barcodeWriter.Write(qrContent))
                        {
                            // Lưu QR code tạm thời vào một MemoryStream
                            using (MemoryStream ms = new MemoryStream())
                            {
                                qrBitmap.Save(ms, ImageFormat.Png);
                                byte[] imageBytes = ms.ToArray();

                                // Tạo cell cho mã QR
                                iText.Layout.Element.Cell cell = new iText.Layout.Element.Cell();
                                cell.SetPadding(10);
                                cell.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                                // Thêm hình ảnh mã QR vào cell
                                iTextImage qrImage = new iTextImage(ImageDataFactory.Create(imageBytes));
                                qrImage.SetWidth(UnitValue.CreatePercentValue(70));

                                // Use fully qualified iText TextAlignment to avoid ambiguity
                                qrImage.SetTextAlignment(iTextTextAlignment.CENTER);

                                // Tạo các dòng thông tin
                                Paragraph pTitle = new Paragraph("MÃ QR TÀI SẢN")
                                    .SetFontSize(12)
                                    .SetBold()
                                    .SetTextAlignment(iTextTextAlignment.CENTER);

                                Paragraph pTenTS = new Paragraph($"Tên TS: {item.TenTaiSan}")
                                    .SetFontSize(9);

                                Paragraph pMaTS = new Paragraph($"Mã TS: {item.MaTaiSan}")
                                    .SetFontSize(9);

                                Paragraph pSoSeri = new Paragraph($"Số Seri: {item.SoSeri}")
                                    .SetFontSize(9);

                                Paragraph pPhong = new Paragraph($"Phòng: {item.TenPhong ?? "Chưa phân phòng"}")
                                    .SetFontSize(9);

                                Paragraph pNhomTS = new Paragraph($"Nhóm TS: {item.TenNhomTS ?? "Không xác định"}")
                                    .SetFontSize(9);

                                // Thêm các thông tin vào cell
                                cell.Add(pTitle);
                                cell.Add(qrImage);
                                cell.Add(pTenTS);
                                cell.Add(pMaTS);
                                cell.Add(pSoSeri);
                                cell.Add(pPhong);
                                cell.Add(pNhomTS);

                                // Thêm cell vào bảng
                                table.AddCell(cell);

                                cellCount++;

                                // Mỗi khi đủ 4 cell (2x2), thêm bảng vào document và tạo bảng mới
                                if (cellCount % 4 == 0)
                                {
                                    document.Add(table);

                                    // Fix: Use simplified AreaBreak constructor (creates a page break)
                                    document.Add(new AreaBreak());

                                    table = new iText.Layout.Element.Table(2);
                                    table.SetWidth(UnitValue.CreatePercentValue(100));
                                }
                            }
                        }
                    }

                    // Nếu còn cell chưa được thêm vào document, thêm bảng cuối cùng
                    if (cellCount % 4 != 0)
                    {
                        // Thêm ô trống để đảm bảo đủ 4 ô
                        while (cellCount % 4 != 0)
                        {
                            iText.Layout.Element.Cell emptyCell = new iText.Layout.Element.Cell();
                            emptyCell.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                            table.AddCell(emptyCell);
                            cellCount++;
                        }
                        document.Add(table);
                    }
                }
            }
        }
    }
}