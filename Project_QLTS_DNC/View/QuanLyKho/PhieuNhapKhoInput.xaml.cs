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
using Supabase;
using Project_QLTS_DNC.Models;

namespace Project_QLTS_DNC.View.QuanLyKho
{
    public partial class PhieuNhapKhoInput : Window
    {
        private Supabase.Client _client;

        public PhieuNhapKhoInput()
        {
            InitializeComponent();
            Loaded += PhieuNhapKhoInput_Loaded;
        }

        private async void PhieuNhapKhoInput_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            await LoadKhoAsync();
            await LoadNhaCungCapAsync();
            await LoadNhanVienAsync();
        }

        private async Task InitializeSupabaseAsync()
        {
            var supabaseUrl = "https://hoybfwnugefnpctgghha.supabase.co";
            var supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhveWJmd251Z2VmbnBjdGdnaGhhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDQxMDQ4OTEsImV4cCI6MjA1OTY4MDg5MX0.KxNfiOUFXHGgqZf3b3xOk6BR4sllMZG_-W-y_OPUwCI";

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false,
                AutoRefreshToken = false
            };

            _client = new Supabase.Client(supabaseUrl, supabaseKey, options);
            await _client.InitializeAsync();
        }

        private async Task LoadKhoAsync()
        {
            var result = await _client.From<Kho>().Get();
            cboMaKho.ItemsSource = result.Models;
            cboMaKho.DisplayMemberPath = "TenKho";
            cboMaKho.SelectedValuePath = "MaKho";
        }

        private async Task LoadNhaCungCapAsync()
        {
            try
            {
                // Làm trống các mục hiện có của ComboBox trước khi gán ItemsSource
                cboNhaCungCap.Items.Clear();

                // Lấy dữ liệu các nhà cung cấp từ Supabase
                var result = await _client.From<NhaCungCapClass>().Get();

                // Kiểm tra nếu có dữ liệu từ Supabase
                if (result.Models.Any())
                {
                    // Tạo danh sách ComboBoxItem từ kết quả
                    var list = result.Models.Select(x => new ComboBoxItem
                    {
                        Content = x.TenNCC,  // Tên nhà cung cấp
                        Tag = x.MaNCC       // Mã nhà cung cấp (lưu trong Tag để dễ dàng truy xuất)
                    }).ToList();

                    // Đưa dữ liệu vào ComboBox
                    cboNhaCungCap.ItemsSource = list;
                    cboNhaCungCap.DisplayMemberPath = "Content";  // Để ComboBox hiển thị tên nhà cung cấp
                    cboNhaCungCap.SelectedValuePath = "Tag";     // Để ComboBox lưu giá trị mã nhà cung cấp
                }
                else
                {
                    MessageBox.Show("Không có dữ liệu nhà cung cấp.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu nhà cung cấp: " + ex.Message);
            }
        }


        private async Task LoadNhanVienAsync()
        {
            try
            {
                // Làm trống các mục hiện có của ComboBox trước khi gán ItemsSource
                cboNguoiLapPhieu.Items.Clear();

                // Lấy dữ liệu nhân viên từ Supabase
                var result = await _client.From<NhanVienModel>().Get();

                // Kiểm tra nếu có dữ liệu từ Supabase
                if (result.Models.Any())
                {
                    // Tạo danh sách ComboBoxItem từ kết quả
                    var list = result.Models.Select(x => new ComboBoxItem
                    {
                        Content = x.TenNV,  // Tên nhân viên
                        Tag = x.MaNV        // Mã nhân viên (lưu trong Tag để dễ dàng truy xuất)
                    }).ToList();

                    // Đưa dữ liệu vào ComboBox
                    cboNguoiLapPhieu.ItemsSource = list;
                    cboNguoiLapPhieu.DisplayMemberPath = "Content";  // Để ComboBox hiển thị tên nhân viên
                    cboNguoiLapPhieu.SelectedValuePath = "Tag";     // Để ComboBox lưu giá trị mã nhân viên
                }
                else
                {
                    MessageBox.Show("Không có dữ liệu nhân viên.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu nhân viên: " + ex.Message);
            }
        }

        // Cập nhật lại việc tìm kiếm nhóm tài sản
        private async Task LoadNhomTaiSanByNameAsync(string groupName)
        {
            try
            {
                var result = await _client
                    .From<NhomTaiSan2>()  // Sử dụng bảng đúng
                    .Filter("ten_nhom_ts", Supabase.Postgrest.Constants.Operator.Equals, groupName)
                    .Limit(1)
                    .Get();

                var selectedGroup = result.Models.FirstOrDefault();
                if (selectedGroup != null)
                {
                    // Điền thông tin vào các TextBox
                    txtMaSP.Text = selectedGroup.MaNhomTS.ToString();
                    txtMaNhom.Text = selectedGroup.MaNhomTS.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải thông tin nhóm tài sản: " + ex.Message);
            }
        }

        private async void lstSuggest_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (lstSuggest.SelectedItem != null)
            {
                string selectedGroupName = lstSuggest.SelectedItem.ToString();
                txtTimKiemSP.Text = selectedGroupName;
                popupSuggest.IsOpen = false;

                // Tải thông tin mã nhóm và mã tài sản từ cơ sở dữ liệu
                await LoadNhomTaiSanByNameAsync(selectedGroupName);
            }
        }

        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtTimKiemSP.Text.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                popupSuggest.IsOpen = false;
                return;
            }

            try
            {
                // Lấy dữ liệu nhóm tài sản từ Supabase
                var result = await _client
                    .From<NhomTaiSan2>()
                    .Filter("ten_nhom_ts", Supabase.Postgrest.Constants.Operator.ILike, $"%{keyword}%")
                    .Limit(10)
                    .Get();

                var suggestions = result.Models
                    .Select(x => x.TenNhomTS)
                    .Distinct()
                    .ToList();

                lstSuggest.ItemsSource = suggestions;
                popupSuggest.IsOpen = suggestions.Any();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tìm kiếm nhóm sản phẩm: " + ex.Message);
            }
        }

        private async void LoadChiTietSanPhamTheoTen(string tenSP)
        {
            var result = await _client
                .From<ChiTietPhieuNhap>()
                .Filter("ten_tai_san", Supabase.Postgrest.Constants.Operator.Equals, tenSP)
                .Limit(1)
                .Get();

            var item = result.Models.FirstOrDefault();
            if (item != null)
            {
                txtMaSP.Text = item.MaChiTietPN.ToString();
                txtMaNhom.Text = item.MaNhomTS.ToString();
            }
        }

        private void UpdateTotal()
        {
            if (int.TryParse(txtSoLuong.Text, out int soLuong) &&
                decimal.TryParse(txtDonGia.Text, out decimal donGia))
            {
                decimal tongTien = soLuong * donGia;
                txtTongTien.Text = tongTien.ToString("N0");
            }
            else
            {
                txtTongTien.Text = "";
            }
        }

        private void txtSoLuong_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTotal();
        }

        private void txtDonGia_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTotal();
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private async Task<int> SinhMaPhieuNhapAsync()
        {
            try
            {
                var danhSachPhieuNhap = await _client.From<PhieuNhap>().Get();

                if (danhSachPhieuNhap?.Models == null || !danhSachPhieuNhap.Models.Any())
                    return 1;

                return danhSachPhieuNhap.Models.Max(p => p.MaPhieuNhap) + 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi sinh mã phiếu nhập: " + ex.Message);
                throw;
            }
        }
        private async Task<bool> ThemPhieuNhapAsync(Kho selectedKho, int maNCC, int maNV, DateTime ngayNhap, decimal tongTien)
        {
            try
            {
                int maPhieuMoi = await SinhMaPhieuNhapAsync();

                var phieuNhap = new PhieuNhap
                {
                    MaPhieuNhap = maPhieuMoi,
                    MaKho = selectedKho.MaKho,
                    MaNCC = maNCC,
                    MaNV = maNV,
                    NgayNhap = ngayNhap,
                    TongTien = tongTien,
                    TrangThai = null
                };

                var response = await _client.From<PhieuNhap>().Insert(phieuNhap);

                return response.Model != null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm phiếu nhập: " + ex.Message);
                return false;
            }
        }


        private async void btnCapNhat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra nếu kho được chọn
                if (cboMaKho.SelectedItem is not Kho selectedKho)
                {
                    MessageBox.Show("Vui lòng chọn kho.");
                    return;
                }

                // Kiểm tra nếu sản phẩm được chọn
                if (string.IsNullOrWhiteSpace(txtMaSP.Text) || string.IsNullOrWhiteSpace(txtMaNhom.Text))
                {
                    MessageBox.Show("Vui lòng chọn sản phẩm.");
                    return;
                }

                // Kiểm tra nhà cung cấp và người lập phiếu
                if (cboNhaCungCap.SelectedItem is ComboBoxItem nccItem &&
                    cboNguoiLapPhieu.SelectedItem is ComboBoxItem nvItem)
                {
                    int selectedMaNCC = (int)cboNhaCungCap.SelectedValue;
                    int selectedMaNV = (int)nvItem.Tag;

                    if (selectedMaNCC == 0 || selectedMaNV == 0)
                    {
                        MessageBox.Show("Vui lòng chọn nhà cung cấp và người lập phiếu.");
                        return;
                    }

                    DateTime? ngayNhap = dpNgayNhap.SelectedDate;
                    if (ngayNhap == null)
                    {
                        MessageBox.Show("Vui lòng chọn ngày nhập.");
                        return;
                    }

                    decimal tongTien = decimal.TryParse(txtTongTien.Text.Replace(",", ""), out decimal tt)
                        ? tt : 0;

                    // Gọi hàm đã tích hợp để thêm phiếu nhập
                    bool thanhCong = await ThemPhieuNhapAsync(selectedKho, selectedMaNCC, selectedMaNV, ngayNhap.Value, tongTien);

                    if (thanhCong)
                    {
                        MessageBox.Show("Đã lưu phiếu nhập thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Lưu phiếu nhập thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn nhà cung cấp và người lập phiếu.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu phiếu nhập: " + ex.Message);
            }
        }

    }
}
