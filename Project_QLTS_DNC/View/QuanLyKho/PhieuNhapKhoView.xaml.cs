using MaterialDesignThemes.Wpf;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Project_QLTS_DNC.Models;
using ClosedXML.Excel;
using Microsoft.Win32;
using System.ComponentModel;
using System.Data;
using Supabase;


namespace Project_QLTS_DNC.View.QuanLyKho
{
    public partial class PhieuNhapKhoView : UserControl
    {
        private Supabase.Client _client;
        private Dictionary<int, string> _khoLookup = new();
        private Dictionary<int, string> _nccLookup = new();
        private Dictionary<int, string> _nvLookup = new();

        private string _keyword = "";
        private int? _filterMaKho = null;
        private int? _filterMaNhom = null;

        public PhieuNhapKhoView()
        {
            InitializeComponent();
            _ = Init();
        }

        private async Task Init()
        {
            await InitializeSupabaseAsync();
            await LoadKhoLookupAsync();
            await LoadNhaCungCapLookupAsync();
            await LoadNhanVienLookupAsync();
            await LoadPhieuNhapAsync();
        }

        private async Task InitializeSupabaseAsync()
        {
            string supabaseUrl = "https://your-project.supabase.co";
            string supabaseKey = "your-anon-key";

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false,
                AutoRefreshToken = false
            };

            _client = new Supabase.Client(supabaseUrl, supabaseKey, options);
            await _client.InitializeAsync();
        }

        private async Task LoadKhoLookupAsync()
        {
            var result = await _client.From<Kho>().Get();
            _khoLookup = result.Models.ToDictionary(k => k.MaKho, k => k.TenKho);

            cboPhong.ItemsSource = _khoLookup.Select(k => new ComboBoxItem
            {
                Content = k.Value,
                Tag = k.Key
            });
        }

        private async Task LoadNhaCungCapLookupAsync()
        {
            var result = await _client.From<NhaCungCapClass>().Get();
            _nccLookup = result.Models.ToDictionary(n => n.MaNCC, n => n.TenNCC);
        }

        private async Task LoadNhanVienLookupAsync()
        {
            var result = await _client.From<NhanVienClass>().Get();
            _nvLookup = result.Models.ToDictionary(nv => nv.MaNV, nv => nv.TenNV);
        }

        public class PhieuNhapModel
        {
            public int MaKho { get; set; }  // Đổi từ long thành int
            public string TenKho { get; set; }
            public DateTime NgayNhap { get; set; }
            public string MaNhaCungCap { get; set; }
            public string TenNhaCungCap { get; set; }
            public string NguoiLapPhieu { get; set; }
            public string TenNhanVien { get; set; }
            public string MaSP { get; set; }
            public string MaNhom { get; set; }
            public int SoLuong { get; set; }
            public decimal DonGia { get; set; }
            public decimal ThanhTien => SoLuong * DonGia;
        }

        private async Task LoadPhieuNhapAsync()
        {
            var list = new List<PhieuNhapModel>();

            var phieunhapResult = await _client.From<PhieuNhap>().Get();

            foreach (var pn in phieunhapResult.Models)
            {
                int maPhieu = pn.MaPhieuNhap;
                int maKho = pn.MaKho;
                int maNV = pn.MaNV;
                int maNCC = pn.MaNCC;

                if (_filterMaKho != null && maKho != _filterMaKho) continue;

                var ctResult = await _client
                    .From<ChiTietPhieuNhap>()
                    .Filter("ma_phieu_nhap", Supabase.Postgrest.Constants.Operator.Equals, maPhieu)
                    .Get();

                foreach (var ct in ctResult.Models)
                {
                    if (!string.IsNullOrEmpty(_keyword) && !ct.TenTaiSan.ToLower().Contains(_keyword.ToLower())) continue;
                    if (_filterMaNhom != null && ct.MaNhomTS != _filterMaNhom) continue;

                    list.Add(new PhieuNhapModel
                    {
                        MaKho = maKho,
                        TenKho = _khoLookup.GetValueOrDefault(maKho, $"Kho {maKho}"),
                        NgayNhap = pn.NgayNhap,
                        MaNhaCungCap = maNCC.ToString(),
                        TenNhaCungCap = _nccLookup.GetValueOrDefault(maNCC, $"NCC {maNCC}"),
                        NguoiLapPhieu = maNV.ToString(),
                        TenNhanVien = _nvLookup.GetValueOrDefault(maNV, $"NV {maNV}"),
                        MaSP = ct.MaChiTietPN.ToString(),
                        MaNhom = ct.MaNhomTS.ToString(),
                        SoLuong = ct.SoLuong,
                        DonGia = ct.DonGia
                    });
                }
            }

            dgSanPham.ItemsSource = list;
        }

        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _keyword = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(_keyword))
            {
                popupSuggest.IsOpen = false;
                return;
            }

            var result = await _client
                .From<ChiTietPhieuNhap>()
                .Select("ten_tai_san")
                .Filter("ten_tai_san", Supabase.Postgrest.Constants.Operator.ILike, $"%{_keyword}%")
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
                txtSearch.Text = lstSuggest.SelectedItem.ToString();
                _keyword = txtSearch.Text;
                popupSuggest.IsOpen = false;
                _ = LoadPhieuNhapAsync();
            }
        }

        private void cboPhong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboPhong.SelectedItem is ComboBoxItem item && int.TryParse(item.Tag?.ToString(), out int maKho))
                _filterMaKho = maKho;
            else
                _filterMaKho = null;

            _ = LoadPhieuNhapAsync();
        }

        private void cboNhomTS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboNhomTS.SelectedItem is ComboBoxItem item && int.TryParse(item.Tag?.ToString(), out int maNhom))
                _filterMaNhom = maNhom;
            else
                _filterMaNhom = null;

            _ = LoadPhieuNhapAsync();
        }

        private void btnThemKho_click(object sender, RoutedEventArgs e)
        {
            var form = new PhieuNhapKhoInput();
            form.ShowDialog();
            _ = LoadPhieuNhapAsync();
        }

        private void btnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgSanPham.ItemsSource is IEnumerable<PhieuNhapModel> data))
            {
                MessageBox.Show("Không có dữ liệu để xuất Excel.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog dlg = new SaveFileDialog
            {
                FileName = "PhieuNhapKho",
                DefaultExt = ".xlsx",
                Filter = "Excel Workbook (.xlsx)|*.xlsx"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("PhieuNhapKho");

                        ws.Cell(1, 1).Value = "PHIẾU NHẬP KHO";
                        ws.Range(1, 1, 1, 9).Merge();
                        ws.Row(1).Style.Font.SetBold();
                        ws.Row(1).Style.Font.FontSize = 16;
                        ws.Row(1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                        ws.Cell(2, 1).Value = "Tên Kho";
                        ws.Cell(2, 2).Value = "Ngày Nhập";
                        ws.Cell(2, 3).Value = "Nhà Cung Cấp";
                        ws.Cell(2, 4).Value = "Người Lập Phiếu";
                        ws.Cell(2, 5).Value = "Mã SP";
                        ws.Cell(2, 6).Value = "Mã Nhóm";
                        ws.Cell(2, 7).Value = "Số lượng";
                        ws.Cell(2, 8).Value = "Đơn giá";
                        ws.Cell(2, 9).Value = "Tổng tiền";

                        int row = 3;
                        foreach (var item in data)
                        {
                            ws.Cell(row, 1).Value = item.TenKho;
                            ws.Cell(row, 2).Value = item.NgayNhap;
                            ws.Cell(row, 3).Value = item.TenNhaCungCap;
                            ws.Cell(row, 4).Value = item.TenNhanVien;
                            ws.Cell(row, 5).Value = item.MaSP;
                            ws.Cell(row, 6).Value = item.MaNhom;
                            ws.Cell(row, 7).Value = item.SoLuong;
                            ws.Cell(row, 8).Value = item.DonGia;
                            ws.Cell(row, 9).Value = item.ThanhTien;
                            row++;
                        }

                        ws.Columns().AdjustToContents();
                        workbook.SaveAs(dlg.FileName);
                    }

                    MessageBox.Show("Xuất file Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất file Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


    }
}
