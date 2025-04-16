using Project_QLTS_DNC.Models.DuyetPhieu;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.NhaCungCap;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Models.ToaNha;

using Project_QLTS_DNC.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Project_QLTS_DNC.View.DuyetPhieu.ChiTietPhieu
{
    public partial class frmChiTietPhieuNhap : UserControl
    {
        private DispatcherTimer autoRefreshTimer;
        private ObservableCollection<ChiTietPhieuHienThi> danhSachChiTiet = new();

        public frmChiTietPhieuNhap()
        {
            InitializeComponent();
            StartAutoRefresh();
        }

        private void StartAutoRefresh()
        {
            autoRefreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            autoRefreshTimer.Tick += (s, e) => LoadTatCaChiTietPhieuNhap();
            autoRefreshTimer.Start();
        }

        public class ChiTietPhieuHienThi
        {
            public long MaPhieu { get; set; }
            public long MaKho { get; set; }
            public string TenKho { get; set; }
            public long MaNV { get; set; }
            public string TenNV { get; set; }
            public long MaChiTietPN { get; set; }
            public long MaNhomTS { get; set; }
            public string TenNhomTS { get; set; }
            public long MaNCC { get; set; }
            public string TenNCC { get; set; }
            public string TenTS { get; set; }
            public DateTime? NgayNhap { get; set; }
            public int SoLuong { get; set; }
            public decimal DonGia { get; set; }
            public bool CanQLRieng { get; set; }
            public decimal TongTien { get; set; }
            public string TrangThai { get; set; }
        }

        private async void LoadTatCaChiTietPhieuNhap()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                var listPhieu = await client.From<PhieuNhapKhoInput>().Get();
                var listChiTiet = await client.From<ChiTietPN>().Get();
                var listKho = await client.From<Kho>().Get();
                var listNV = await client.From<NhanVienModel>().Get();
                var listNhomTS = await client.From<NhomTaiSan>().Get();
                var listNCC = await client.From<NhaCungCapClass>().Get();

                var result = (from pn in listPhieu.Models
                              join ct in listChiTiet.Models on pn.MaPhieuNhap equals ct.MaPhieuNhap
                              join kho in listKho.Models on pn.MaKho equals kho.MaKho
                              join nv in listNV.Models on pn.MaNV equals nv.MaNV
                              join nhom in listNhomTS.Models on ct.MaNhomTS equals nhom.MaNhomTS
                              join ncc in listNCC.Models on pn.MaNCC equals ncc.MaNCC
                              select new ChiTietPhieuHienThi
                              {
                                  MaPhieu = pn.MaPhieuNhap,
                                  MaKho = pn.MaKho,
                                  TenKho = kho.TenKho,
                                  MaNV = pn.MaNV,
                                  TenNV = nv.TenNV,
                                  MaChiTietPN = ct.MaChiTietPN ?? 0,
                                  MaNhomTS = ct.MaNhomTS,
                                  TenNhomTS = nhom.TenNhom,
                                  MaNCC = pn.MaNCC,
                                  TenNCC = ncc.TenNCC,
                                  TenTS = ct.TenTaiSan,
                                  NgayNhap = pn.NgayNhap,
                                  SoLuong = ct.SoLuong,
                                  DonGia = ct.DonGia ?? 0,
                                  CanQLRieng = ct.CanQuanLyRieng ?? false,
                                  TongTien = ct.TongTien ?? ((ct.DonGia ?? 0) * (decimal)ct.SoLuong),
                                  TrangThai = pn.TrangThai == true ? "Đã duyệt" :
            pn.TrangThai == false ? "Từ chối duyệt" : "Chưa duyệt"

                              }).ToList();

                danhSachChiTiet = new ObservableCollection<ChiTietPhieuHienThi>(result);
                dgChiTietPhieuNhap.ItemsSource = danhSachChiTiet;
                txtStatus.Text = $"Tổng số dòng chi tiết: {danhSachChiTiet.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi load chi tiết phiếu nhập: {ex.Message}");
            }
        }

        private void btnDuyet_Click(object sender, RoutedEventArgs e)
        {
            var frm = new frmDuyetChiTietNhap
            {
                Owner = Window.GetWindow(this)
            };
            frm.Show();
        }

        private void btnTuChoi_Click(object sender, RoutedEventArgs e)
        {
            var frm = new frmTuChoiNhap
            {
                Owner = Window.GetWindow(this)
            };
            frm.Show();
        }

        private void btnHuyBo_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this)?.Close();
    }
}
