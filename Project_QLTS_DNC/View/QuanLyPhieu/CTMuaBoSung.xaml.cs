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
using Project_QLTS_DNC.Models.Kho;
using Project_QLTS_DNC.Models.QLNhomTS;
using DocumentFormat.OpenXml.Drawing;
namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for CTMuaBoSung.xaml
    /// </summary>
    public partial class CTMuaBoSung : Window
    {
        private Supabase.Client _client;
        public CTMuaBoSung()
        {
            InitializeComponent();
            Loaded += PhieuCTMuaBS_Loaded;
            cboMaKho.SelectionChanged += CboMaKho_SelectionChanged;  // Thêm sự kiện chọn kho
        }

        private async void PhieuCTMuaBS_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            await LoadTonKhoWithTenKhoAsync();
            await LoadPhieuMuaMoiAsync();
            await LoadNhomTaiSanAsync();
        }
        private async Task LoadPhieuMuaMoiAsync()
        {
            var result = await _client.From<MuaMoiTS>().Get();

            if (result.Models != null)
            {
                cboMaPhieuDeNghi.ItemsSource = result.Models;
                cboMaPhieuDeNghi.DisplayMemberPath = "MaPhieuDeNghi";
                cboMaPhieuDeNghi.SelectedValuePath = "MaPhieuDeNghi";
            }
            else
            {
                cboMaPhieuDeNghi.ItemsSource = null;
            }
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
        public class TonKhoWithTenKho
        {
            public int MaTonKho { get; set; }
            public int MaKho { get; set; }
            public string TenKho { get; set; }
            public int SoLuongTon { get; set; }
            public DateTime NgayCapNhat { get; set; }

            // Các thuộc tính thêm vào nếu cần
            public int MaNhomTS { get; set; }
            public int SoLuongNhap { get; set; }
            public int SoLuongXuat { get; set; }
        }
        private async Task LoadTonKhoWithTenKhoAsync()
        {
            try
            {
                // Lấy dữ liệu từ bảng `TonKho`
                var resultTonKho = await _client.From<TonKho>().Get();

                // Lấy dữ liệu từ bảng `Kho`
                var resultKho = await _client.From<Kho>().Get();

                if (resultTonKho.Models != null && resultKho.Models != null)
                {
                    // Chuyển danh sách Kho thành dictionary để tìm kiếm nhanh
                    var khoDictionary = resultKho.Models.ToDictionary(k => k.MaKho, k => k.TenKho);

                    // Kết hợp dữ liệu từ TonKho và Kho bằng cách sử dụng dictionary
                    var tonKhoList = resultTonKho.Models
                        .GroupBy(item => item.MaKho) // Nhóm theo MaKho để loại bỏ trùng
                        .Select(group =>
                        {
                            var firstItem = group.First();
                            return new TonKhoWithTenKho
                            {
                                MaTonKho = firstItem.MaTonKho,
                                MaKho = firstItem.MaKho,
                                TenKho = khoDictionary.ContainsKey(firstItem.MaKho) ? khoDictionary[firstItem.MaKho] : "Không có tên kho",
                                SoLuongTon = firstItem.SoLuongTon,
                                NgayCapNhat = firstItem.NgayCapNhat,
                                MaNhomTS = firstItem.MaNhomTS,
                                SoLuongNhap = firstItem.SoLuongNhap,
                                SoLuongXuat = firstItem.SoLuongXuat
                            };
                        })
                        .ToList();

                    // Gán dữ liệu vào ComboBox
                    cboMaKho.ItemsSource = tonKhoList;
                    cboMaKho.DisplayMemberPath = "TenKho";  // Hiển thị tên kho
                    cboMaKho.SelectedValuePath = "MaKho";  // Lưu MaKho
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu kho: {ex.Message}");
            }
        }



        private async void CboMaKho_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Kiểm tra nếu có kho được chọn
            if (cboMaKho.SelectedValue != null)
            {
                // Gọi lại phương thức LoadNhomTaiSanAsync khi kho được chọn
                await LoadNhomTaiSanAsync();
            }
        }

        private async Task LoadNhomTaiSanAsync()
        {
            try
            {
                // Lấy dữ liệu từ cơ sở dữ liệu
                var tonKhoResult = await _client.From<TonKho>().Get();
                var nhomTaiSanResult = await _client.From<NhomTaiSan>().Get();

                if (tonKhoResult.Models != null && nhomTaiSanResult.Models != null)
                {
                    // Kiểm tra kho đã được chọn
                    if (cboMaKho.SelectedValue != null)
                    {
                        int selectedMaKho = Convert.ToInt32(cboMaKho.SelectedValue);

                        // Lọc dữ liệu tồn kho theo kho đã chọn
                        var filteredTonKho = tonKhoResult.Models
                            .Where(tk => tk.MaKho == selectedMaKho && tk.SoLuongTon < 10)
                            .ToList();

                        // Nếu có tài sản tồn kho dưới 10, tiếp tục xử lý
                        if (filteredTonKho.Any())
                        {
                            // Lấy danh sách mã nhóm tài sản từ dữ liệu tồn kho đã lọc
                            var filteredMaNhomTS = filteredTonKho.Select(tk => tk.MaNhomTS).Distinct().ToList();

                            // Lọc nhóm tài sản theo danh sách mã nhóm tài sản đã lọc
                            var nhomTaiSanLookup = nhomTaiSanResult.Models
                                .Where(nt => filteredMaNhomTS.Contains(nt.MaNhomTS))
                                .ToDictionary(nt => nt.MaNhomTS, nt => nt.TenNhom);

                            // Lọc và hiển thị tên nhóm tài sản từ mã nhóm tài sản
                            var nhomTaiSan = filteredTonKho
                                .Select(tk => new
                                {
                                    MaNhomTS = tk.MaNhomTS,
                                    TenNhomTS = nhomTaiSanLookup.ContainsKey(tk.MaNhomTS) ? nhomTaiSanLookup[tk.MaNhomTS] : "Không có tên"
                                })
                                .Distinct()
                                .ToList();

                            // Kiểm tra xem có nhóm tài sản không
                            if (nhomTaiSan.Any())
                            {
                                // Hiển thị tên nhóm tài sản trong ComboBox
                                cboTenSp.ItemsSource = nhomTaiSan;
                                cboTenSp.DisplayMemberPath = "TenNhomTS"; // Hiển thị tên
                                cboTenSp.SelectedValuePath = "MaNhomTS"; // Lưu mã nhóm tài sản khi chọn
                            }
                            else
                            {
                                MessageBox.Show("Không có nhóm tài sản nào.");
                                cboTenSp.ItemsSource = null; // Xóa các mục trong ComboBox
                            }
                        }
                        else
                        {
                            // Nếu không có tài sản nào tồn kho dưới 10, hiển thị thông báo
                            MessageBox.Show("Không có tài sản nào tồn kho dưới 10.");
                            cboTenSp.ItemsSource = null; // Xóa các mục trong ComboBox
                        }
                    }
                    else
                    {
 
                    }
                }
                else
                {
                    MessageBox.Show("Không có dữ liệu tồn kho hoặc nhóm tài sản.");
                    cboTenSp.ItemsSource = null; // Nếu không có dữ liệu, xóa ComboBox
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách nhóm tài sản: {ex.Message}");
            }
        }



        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void btnCapNhat_Click(object sender, RoutedEventArgs e)
        {
            var model = new ChiTietDeNghiMua
            {
                MaPhieuDeNghi = int.Parse(cboMaPhieuDeNghi.SelectedValue?.ToString() ?? "0"),
                TenTaiSan = cboTenSp.Text,
                SoLuong = int.TryParse(txtSoLuong.Text, out var sl) ? sl : 0,
                DonViTinh = txtDonViTinh.Text,
                MoTa = txtMoTa.Text,
                DuKienGia = int.TryParse(txtGia.Text, out var dg) ? dg : 0
            };

            var result = await _client
                .From<ChiTietDeNghiMua>()
                .Insert(model);

            if (result != null && result.Models != null)
            {
                MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close(); // đóng form nếu muốn
            }
            else
            {
                MessageBox.Show("Lưu thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
