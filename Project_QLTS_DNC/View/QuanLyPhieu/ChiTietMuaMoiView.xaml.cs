using iText.IO.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Properties;
using Project_QLTS_DNC.Models.NhaCungCap;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.PhieuNhapKho;
using Project_QLTS_DNC.Models.QLNhomTS;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.IO.Font;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using iText.Kernel.Geom;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for ChiTietMuaMoiView.xaml
    /// </summary>
    public partial class ChiTietMuaMoiView : Window
    {
        private readonly int _maPhieu;
        private Supabase.Client _client;
        public ChiTietMuaMoiView( int maPhieuMoi)
        {
            InitializeComponent();
            _maPhieu = maPhieuMoi;
            Loaded += ChiTietPhieuMuaMoiView_Loaded;
        }
        private async void ChiTietPhieuMuaMoiView_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            await LoadPhieuMuaMoiAsync();
            await LoadChiTietPhieuMuaMoiAsync();
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
        private async Task LoadPhieuMuaMoiAsync()
        {
            try
            {
                var result = await _client
                    .From<MuaMoiTS>()
                    .Where(x => x.MaPhieuDeNghi == _maPhieu)
                    .Get();

                var phieu = result.Models.FirstOrDefault();
                if (phieu == null)
                {
                    MessageBox.Show("Không tìm thấy phiếu nhập!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                txtMaPhieuMuaMoi.Text = phieu.MaPhieuDeNghi.ToString();
                txtNgayDeNghi.Text = phieu.NgayDeNghi.ToString("dd/MM/yyyy");
                txtDonViDeNghi.Text = phieu.DonViDeNghi.ToString();
                txtLyDo.Text = phieu.LyDo.ToString();
                txtGhiChu.Text = phieu.GhiChu.ToString();
                var nvResult = await _client.From<NhanVienModel>().Where(x => x.MaNV == phieu.MaNV).Get();
                txtNhanVienDeNghi.Text = nvResult.Models.FirstOrDefault()?.TenNV ?? "---";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load phiếu nhập: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private async Task LoadChiTietPhieuMuaMoiAsync()
        {
            try
            {
                // Đảm bảo _maPhieu có cùng kiểu dữ liệu với trường trong DB
                long maPhieuNumber;
                if (long.TryParse(_maPhieu.ToString(), out maPhieuNumber))
                {
                    var result = await _client
                        .From<ChiTietDeNghiMua>()
                        .Where(x => x.MaPhieuDeNghi == maPhieuNumber)
                        .Get();

                    if (result.Models != null && result.Models.Any())
                    {
                        var danhSachChiTiet = result.Models.ToList();
                        Console.WriteLine($"Đã tìm thấy {danhSachChiTiet.Count} chi tiết phiếu");
                        gridChiTiet.ItemsSource = danhSachChiTiet;
                    }
                    else
                    {
                        Console.WriteLine($"Không tìm thấy chi tiết phiếu với mã {_maPhieu}");
                        // Tạo một danh sách rỗng để tránh lỗi binding
                        gridChiTiet.ItemsSource = new List<ChiTietDeNghiMua>();
                    }
                }
                else
                {
                    Console.WriteLine($"Mã phiếu không phải là số: {_maPhieu}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chi tiết lỗi: {ex}");
                MessageBox.Show("Lỗi khi load chi tiết phiếu nhập: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
