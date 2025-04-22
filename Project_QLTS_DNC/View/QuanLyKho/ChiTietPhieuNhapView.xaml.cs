// Project_QLTS_DNC/View/QuanLyKho/ChiTietPhieuNhapView.xaml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.PhieuNhapKho;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.NhaCungCap;
using Project_QLTS_DNC.Models.Kho;
using Supabase;
using Project_QLTS_DNC.Models.QLNhomTS;
using System.IO;
using System.Windows.Media.Imaging;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using SkiaSharp;

namespace Project_QLTS_DNC.View.QuanLyKho
{
    public partial class ChiTietPhieuNhapView : Window
    {
        private readonly int _maPhieuNhap;
        private Supabase.Client _client;

        public ChiTietPhieuNhapView(int maPhieuNhap)
        {
            InitializeComponent();
            _maPhieuNhap = maPhieuNhap;
            Loaded += ChiTietPhieuNhapView_Loaded;
        }

        private async void ChiTietPhieuNhapView_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            await LoadPhieuNhapAsync();
            await LoadChiTietPhieuNhapAsync();
        }

        private async Task InitializeSupabaseAsync()
        {
            string supabaseUrl = "https://hoybfwnugefnpctgghha.supabase.co";
            string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhveWJmd251Z2VmbnBjdGdnaGhhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDQxMDQ4OTEsImV4cCI6MjA1OTY4MDg5MX0.KxNfiOUFXHGgqZf3b3xOk6BR4sllMZG_-W-y_OPUwCI";

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false,
                AutoRefreshToken = false
            };

            _client = new Supabase.Client(supabaseUrl, supabaseKey, options);
            await _client.InitializeAsync();
        }

        private async Task LoadPhieuNhapAsync()
        {
            try
            {
                var result = await _client
                    .From<PhieuNhap>()
                    .Where(x => x.MaPhieuNhap == _maPhieuNhap)
                    .Get();

                var phieu = result.Models.FirstOrDefault();
                if (phieu == null)
                {
                    MessageBox.Show("Không tìm thấy phiếu nhập!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                txtMaPhieuNhap.Text = phieu.MaPhieuNhap.ToString();
                txtNgayNhap.Text = phieu.NgayNhap.ToString("dd/MM/yyyy");
                txtTongTien.Text = phieu.TongTien.ToString("N0") + " VNĐ";
                

                var khoResult = await _client.From<Kho>().Where(x => x.MaKho == phieu.MaKho).Get();
                txtKhoNhap.Text = khoResult.Models.FirstOrDefault()?.TenKho ?? "---";

                var nccResult = await _client.From<NhaCungCapClass>().Where(x => x.MaNCC == phieu.MaNCC).Get();
                txtNhaCungCap.Text = nccResult.Models.FirstOrDefault()?.TenNCC ?? "---";

                var nvResult = await _client.From<NhanVienModel>().Where(x => x.MaNV == phieu.MaNV).Get();
                txtNhanVienNhap.Text = nvResult.Models.FirstOrDefault()?.TenNV ?? "---";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load phiếu nhập: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadChiTietPhieuNhapAsync()
        {
            try
            {
                var result = await _client
                    .From<ChiTietPhieuNhap>()
                    .Where(x => x.MaPhieuNhap == _maPhieuNhap)
                    .Get();

                var danhSachChiTiet = result.Models.ToList();

                if (danhSachChiTiet.Count > 0)
                {
                    // Lấy toàn bộ danh sách nhóm tài sản
                    var nhomResult = await _client.From<NhomTaiSan>().Get();
                    var nhomLookup = nhomResult.Models.ToDictionary(n => n.MaNhomTS, n => n.TenNhom);

                    foreach (var item in danhSachChiTiet)
                    {
                        item.TenTaiSan = string.IsNullOrEmpty(item.TenTaiSan) ? "N/A" : item.TenTaiSan;

                        // 👉 Gán tên nhóm tài sản cho từng item
                        nhomLookup.TryGetValue(item.MaNhomTS, out string tenNhom);
                        item.TenNhomTS = tenNhom ?? "---";
                    }
                }

                gridChiTiet.ItemsSource = danhSachChiTiet;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load chi tiết phiếu nhập: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Chọn nơi lưu file PDF
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = "ChiTietPhieuNhap.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // 👉 Chụp hình Form hiện tại thành Bitmap
                    RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                        (int)this.ActualWidth, (int)this.ActualHeight, 96d, 96d, System.Windows.Media.PixelFormats.Pbgra32);
                    renderBitmap.Render(this);

                    // 👉 Encode hình thành stream
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                    byte[] imageData;
                    using (var stream = new MemoryStream())
                    {
                        encoder.Save(stream);
                        imageData = stream.ToArray();
                    }

                    // 👉 Tạo file PDF
                    using (var document = new PdfDocument())
                    {
                        PdfPage page = document.AddPage();
                        XGraphics gfx = XGraphics.FromPdfPage(page);

                        using (var ms = new MemoryStream(imageData))
                        {
                            var img = XImage.FromStream(() => new MemoryStream(imageData));
                            gfx.DrawImage(img, 0, 0, page.Width, page.Height);
                        }

                        document.Save(saveDialog.FileName);
                    }

                    MessageBox.Show($"Đã xuất file PDF thành công:\n{saveDialog.FileName}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in PDF: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}