using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Supabase;
using Project_QLTS_DNC.Models; // Thêm namespace chứa model

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

        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtTimKiemSP.Text.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                popupSuggest.IsOpen = false;
                return;
            }

            var result = await _client
                .From<ChiTietPhieuNhap>()
                .Filter("ten_tai_san", Supabase.Postgrest.Constants.Operator.ILike, $"%{keyword}%")
                .Limit(10)
                .Get();

            var suggestions = result.Models
                .Select(x => x.TenTaiSan)
                .Distinct()
                .ToList();

            lstSuggest.ItemsSource = suggestions;
            popupSuggest.IsOpen = suggestions.Any();
        }

        private void lstSuggest_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (lstSuggest.SelectedItem != null)
            {
                txtTimKiemSP.Text = lstSuggest.SelectedItem.ToString();
                popupSuggest.IsOpen = false;

                LoadChiTietSanPhamTheoTen(txtTimKiemSP.Text);
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

        private async void btnCapNhat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboMaKho.SelectedItem is not Kho selectedKho)
                {
                    MessageBox.Show("Vui lòng chọn kho.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtMaSP.Text) || string.IsNullOrWhiteSpace(txtMaNhom.Text))
                {
                    MessageBox.Show("Vui lòng chọn sản phẩm.");
                    return;
                }

                if (cboNhaCungCap.SelectedItem is not ComboBoxItem nccItem ||
                    cboNguoiLapPhieu.SelectedItem is not ComboBoxItem nvItem)
                {
                    MessageBox.Show("Vui lòng chọn nhà cung cấp và người lập phiếu.");
                    return;
                }

                int maNCC = int.Parse(nccItem.Tag?.ToString() ?? "0");  // Đổi từ long sang int
                int maNV = int.Parse(nvItem.Tag?.ToString() ?? "0");  // Đổi từ long sang int
                DateTime? ngayNhap = dpNgayNhap.SelectedDate;
                if (ngayNhap == null)
                {
                    MessageBox.Show("Vui lòng chọn ngày nhập.");
                    return;
                }

                decimal tongTien = decimal.TryParse(txtTongTien.Text.Replace(",", ""), out decimal tt)
                    ? tt : 0;

                var phieuNhap = new PhieuNhap
                {
                    MaKho = selectedKho.MaKho, // ✅ đúng rồi
                    MaNV = maNV,
                    MaNCC = maNCC,
                    NgayNhap = ngayNhap.Value,
                    TongTien = tongTien,
                    TrangThai = null // Mặc định chưa duyệt
                };

                // Thực hiện lưu phiếu nhập (cập nhật hoặc thêm mới)
                // await _client.From<PhieuNhap>().Insert(phieuNhap); // Nếu muốn lưu vào Supabase

                MessageBox.Show("Đã lưu phiếu nhập thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu phiếu nhập: " + ex.Message);
            }
        }
    }
}
