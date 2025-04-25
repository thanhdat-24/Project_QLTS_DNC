using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Models.LichSu;
using Project_QLTS_DNC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Project_QLTS_DNC.View.LichSuDiChuyenTS
{
    public partial class frmThemPhieuDiChuyen : Window
    {
        private List<Phong> danhSachPhong = new();
        private List<Tang> danhSachTang = new();
        private List<NhanVienModel> danhSachNhanVien = new();

        public frmThemPhieuDiChuyen()
        {
            InitializeComponent();
            _ = LoadDanhSachCombo();
        }

        private async Task LoadDanhSachCombo()
        {
            var client = await SupabaseService.GetClientAsync();

            var tangResponse = await client.From<Tang>().Get();
            danhSachTang = tangResponse.Models;

            var phongResponse = await client.From<Phong>().Get();
            danhSachPhong = phongResponse.Models;

            var taiSanResponse = await client.From<TaiSanModel>().Get();
            var maPhongCoTaiSan = taiSanResponse.Models
                .Where(ts => ts.MaPhong.HasValue)
                .Select(ts => ts.MaPhong.Value)
                .Distinct()
                .ToList();

            var phongCoTaiSan = danhSachPhong
                .Where(p => maPhongCoTaiSan.Contains(p.MaPhong))
                .ToList();

            cboPhongCuCanDiChuyen.ItemsSource = phongCoTaiSan;
            cboPhongCuCanDiChuyen.DisplayMemberPath = "TenPhong";
            cboPhongCuCanDiChuyen.SelectedValuePath = "MaPhong";

            cboPhongDiChuyenTaiSanDen.ItemsSource = danhSachPhong;
            cboPhongDiChuyenTaiSanDen.DisplayMemberPath = "TenPhong";
            cboPhongDiChuyenTaiSanDen.SelectedValuePath = "MaPhong";

            var nvResponse = await client.From<NhanVienModel>().Get();
            danhSachNhanVien = nvResponse.Models;
            cboNhanVien.ItemsSource = danhSachNhanVien;
            cboNhanVien.DisplayMemberPath = "TenNV";
            cboNhanVien.SelectedValuePath = "MaNV";
        }

        private async Task LoadTaiSanTrongPhong(int maPhong)
        {
            var client = await SupabaseService.GetClientAsync();
            var response = await client.From<TaiSanModel>()
                .Filter("ma_phong", Supabase.Postgrest.Constants.Operator.Equals, maPhong)
                .Get();

            dgTaiSanKho.ItemsSource = response.Models.Select(t => new TaiSanHienThi
            {
                MaTaiSan = t.MaTaiSan,
                TenTaiSan = t.TenTaiSan,
                SoSeri = t.SoSeri,
                TenNhomTS = "",
                ViTriList = new List<ViTriItem> { new ViTriItem { HienThi = "Mặc định" } },
                GhiChu = t.GhiChu,
                IsSelected = false
            }).ToList();
        }

        private void cboPhongCuCanDiChuyen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboPhongCuCanDiChuyen.SelectedItem is not Phong phongCu) return;

            var phongMoiList = danhSachPhong
                .Where(p => p.MaPhong != phongCu.MaPhong)
                .ToList();

            cboPhongDiChuyenTaiSanDen.ItemsSource = phongMoiList;

            _ = LoadTaiSanTrongPhong(phongCu.MaPhong);
        }

        private void cboPhongDiChuyenTaiSanDen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Không cần kiểm tra tòa nhà nữa → để trống
        }

        private async void btnLapPhieu_Click(object sender, RoutedEventArgs e)
        {
            if (cboPhongCuCanDiChuyen.SelectedItem is not Phong phongCu ||
                cboPhongDiChuyenTaiSanDen.SelectedItem is not Phong phongMoi ||
                cboNhanVien.SelectedItem is not NhanVienModel nv ||
                !dateBanGiao.SelectedDate.HasValue)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin");
                return;
            }

            var taiSans = dgTaiSanKho.ItemsSource as IEnumerable<TaiSanHienThi>;
            var taiSanChon = taiSans?.Where(t => t.IsSelected).ToList();

            if (taiSanChon == null || taiSanChon.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một tài sản để di chuyển.");
                return;
            }

            var client = await SupabaseService.GetClientAsync();

            foreach (var ts in taiSanChon)
            {
                var phieu = new LichSuDiChuyenTaiSan
                {
                    MaTaiSan = ts.MaTaiSan,
                    MaPhongCu = phongCu.MaPhong,
                    MaPhongMoi = phongMoi.MaPhong,
                    MaNhanVien = nv.MaNV,
                    NgayBanGiao = dateBanGiao.SelectedDate.Value.Date + DateTime.Now.TimeOfDay,
                    GhiChu = txtGhiChu.Text?.Trim(),
                    TrangThai = null,
                    NV_tiep_nhan = txtNVTiepNhan.Text?.Trim()  // ✅ thêm dòng này
                };

                await client.From<LichSuDiChuyenTaiSan>().Insert(phieu);
            }

            MessageBox.Show("✅ Lập phiếu di chuyển thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cboViTri_Loaded(object sender, RoutedEventArgs e) { }
        private void cboViTri_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void dgTaiSanKho_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
    }

    public class TaiSanHienThi
    {
        public int MaTaiSan { get; set; }
        public string TenTaiSan { get; set; }
        public string SoSeri { get; set; }
        public string TenNhomTS { get; set; }
        public List<ViTriItem> ViTriList { get; set; }
        public string GhiChu { get; set; }
        public bool IsSelected { get; set; }
    }

    public class ViTriItem
    {
        public string HienThi { get; set; }
    }
}
