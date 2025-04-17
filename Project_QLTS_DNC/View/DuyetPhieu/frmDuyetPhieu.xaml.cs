using Project_QLTS_DNC.Models.DuyetPhieu;
using Project_QLTS_DNC.Models.BaoHong;
using Project_QLTS_DNC.Models.Phieu;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.View.DuyetPhieu.ChiTietPhieu;
using Project_QLTS_DNC.View.QuanLyKho;
using Supabase;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Supabase.Postgrest.Constants;

namespace Project_QLTS_DNC.View.DuyetPhieu
{
    public partial class frmDuyetPhieu : UserControl
    {
        private ObservableCollection<PhieuCanDuyet> DanhSachTatCaPhieu = new();

        public frmDuyetPhieu()
        {
            InitializeComponent();
            LoadThongKePhieu();
         }

        private async void dgPhieuCanDuyet_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDuLieuTongHopPhieuAsync();
        }

        private async Task LoadDuLieuTongHopPhieuAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                DanhSachTatCaPhieu.Clear();

                var tongHop = await client.From<TongHopPhieu>().Get();

                foreach (var item in tongHop.Models)
                {
                    if (item.MaPhieuNhap.HasValue)
                    {
                        var pn = await client.From<PhieuNhapKho>().Filter("ma_phieu_nhap", Operator.Equals, item.MaPhieuNhap.Value).Single();
                        DanhSachTatCaPhieu.Add(new PhieuCanDuyet { MaPhieu = $"PN{pn.MaPhieuNhap}", NgayTaoPhieu = pn.NgayNhap, TrangThaiBool = pn.TrangThai, LoaiPhieu = "Phiếu nhập" });
                    }

                    if (item.MaPhieuXuat.HasValue)
                    {
                        var px = await client.From<PhieuXuat>().Filter("ma_phieu_xuat", Operator.Equals, item.MaPhieuXuat.Value).Single();
                        DanhSachTatCaPhieu.Add(new PhieuCanDuyet { MaPhieu = $"PX{px.MaPhieuXuat}", NgayTaoPhieu = px.NgayXuat, TrangThaiBool = px.TrangThai, LoaiPhieu = "Phiếu xuất" });
                    }

                    if (item.MaPhieuDeNghiMua.HasValue)
                    {
                        var pdn = await client.From<phieudenghimua>().Filter("ma_phieu_de_nghi", Operator.Equals, item.MaPhieuDeNghiMua.Value).Single();
                        DanhSachTatCaPhieu.Add(new PhieuCanDuyet { MaPhieu = $"PDN{pdn.MaPhieuDeNghi}", NgayTaoPhieu = pdn.NgayDeNghi, TrangThaiBool = pdn.TrangThai, LoaiPhieu = "Phiếu đề nghị mua" });
                    }

                    if (item.MaPhieuBaoHong.HasValue)
                    {
                        var pbh = await client.From<BaoHong>().Filter("ma_phieu_bao_hong", Operator.Equals, item.MaPhieuBaoHong.Value).Single();
                        DanhSachTatCaPhieu.Add(new PhieuCanDuyet { MaPhieu = $"PBH{pbh.MaPhieuBaoHong}", NgayTaoPhieu = pbh.NgayBaoHong, TrangThaiBool = pbh.TrangThai, LoaiPhieu = "Phiếu báo hỏng" });
                    }
                }

                dgPhieuCanDuyet.ItemsSource = DanhSachTatCaPhieu;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi load dữ liệu tổng hợp: {ex.Message}");
            }
        }

        private async Task LocDuLieuTheoLoaiVaNgay()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                string loaiPhieu = (cboLoaiPhieu.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Tất cả";
                DateTime? tuNgay = dpTuNgay.SelectedDate;
                DateTime? denNgay = dpDenNgay.SelectedDate;

                var ketQua = new ObservableCollection<PhieuCanDuyet>();

                // Phiếu nhập
                if (loaiPhieu == "Tất cả" || loaiPhieu == "Phiếu nhập")
                {
                    var list = (await client.From<PhieuNhapKho>().Get()).Models;
                    foreach (var p in list)
                    {
                        if ((!tuNgay.HasValue || p.NgayNhap.Date >= tuNgay.Value.Date) &&
                            (!denNgay.HasValue || p.NgayNhap.Date <= denNgay.Value.Date))
                        {
                            ketQua.Add(new PhieuCanDuyet { MaPhieu = $"PN{p.MaPhieuNhap}", NgayTaoPhieu = p.NgayNhap, TrangThaiBool = p.TrangThai, LoaiPhieu = "Phiếu nhập" });
                        }
                    }
                }

                // Phiếu xuất
                if (loaiPhieu == "Tất cả" || loaiPhieu == "Phiếu xuất")
                {
                    var list = (await client.From<PhieuXuat>().Get()).Models;
                    foreach (var p in list)
                    {
                        if ((!tuNgay.HasValue || p.NgayXuat.Date >= tuNgay.Value.Date) &&
                            (!denNgay.HasValue || p.NgayXuat.Date <= denNgay.Value.Date))
                        {
                            ketQua.Add(new PhieuCanDuyet { MaPhieu = $"PX{p.MaPhieuXuat}", NgayTaoPhieu = p.NgayXuat, TrangThaiBool = p.TrangThai, LoaiPhieu = "Phiếu xuất" });
                        }
                    }
                }

                // Phiếu đề nghị mua
                if (loaiPhieu == "Tất cả" || loaiPhieu == "Phiếu đề nghị mua")
                {
                    var list = (await client.From<phieudenghimua>().Get()).Models;
                    foreach (var p in list)
                    {
                        if ((!tuNgay.HasValue || p.NgayDeNghi.Date >= tuNgay.Value.Date) &&
                            (!denNgay.HasValue || p.NgayDeNghi.Date <= denNgay.Value.Date))
                        {
                            ketQua.Add(new PhieuCanDuyet { MaPhieu = $"PDN{p.MaPhieuDeNghi}", NgayTaoPhieu = p.NgayDeNghi, TrangThaiBool = p.TrangThai, LoaiPhieu = "Phiếu đề nghị mua" });
                        }
                    }
                }

                // Phiếu báo hỏng
                if (loaiPhieu == "Tất cả" || loaiPhieu == "Phiếu báo hỏng")
                {
                    var list = (await client.From<BaoHong>().Get()).Models;
                    foreach (var p in list)
                    {
                        if ((!tuNgay.HasValue || p.NgayBaoHong.Date >= tuNgay.Value.Date) &&
                            (!denNgay.HasValue || p.NgayBaoHong.Date <= denNgay.Value.Date))
                        {
                            ketQua.Add(new PhieuCanDuyet { MaPhieu = $"PBH{p.MaPhieuBaoHong}", NgayTaoPhieu = p.NgayBaoHong, TrangThaiBool = p.TrangThai, LoaiPhieu = "Phiếu báo hỏng" });
                        }
                    }
                }

                DanhSachTatCaPhieu = ketQua;
                dgPhieuCanDuyet.ItemsSource = DanhSachTatCaPhieu;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lọc dữ liệu: {ex.Message}");
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) => TimKiemPhieu(txtSearch.Text);

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) => TimKiemPhieu(txtSearch.Text);

        private void TimKiemPhieu(string tuKhoa)
        {
            if (string.IsNullOrWhiteSpace(tuKhoa))
            {
                dgPhieuCanDuyet.ItemsSource = DanhSachTatCaPhieu;
                return;
            }

            tuKhoa = tuKhoa.Trim().ToLower();

            var ketQua = DanhSachTatCaPhieu.Where(p =>
                p.MaPhieu.ToLower().Contains(tuKhoa) ||
                p.NgayTaoPhieu.ToString("dd/MM/yyyy").Contains(tuKhoa) ||
                p.TrangThai.ToLower().Contains(tuKhoa)
            ).ToList();

            dgPhieuCanDuyet.ItemsSource = ketQua;
        }

        private void btnDetail_Click(object sender, RoutedEventArgs e)
        {
            var frm = new frmChiTietPhieuNhap();
            new Window
            {
                Title = "Chi tiết phiếu nhập",
                Content = frm,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowState = WindowState.Maximized
            }.ShowDialog();
        }

        private void btnXemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            if (dgPhieuCanDuyet.SelectedItem is PhieuCanDuyet selected && selected.MaPhieu.StartsWith("PN") &&
                long.TryParse(selected.MaPhieu.Substring(2), out long maPhieuNhap))
            {
                var frm = new frmXemChiTietNhap();
                frm.LoadTheoMaPhieu(maPhieuNhap);
                frm.OnPhieuDuyetThanhCong += async () =>
                {
                    await LoadDuLieuTongHopPhieuAsync();
                    LoadThongKePhieu();
                };

                new Window
                {
                    Title = $"Chi tiết phiếu nhập - PN{maPhieuNhap}",
                    Content = frm,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    WindowState = WindowState.Maximized
                }.ShowDialog();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một phiếu nhập để xem chi tiết.");
            }
        }

        private async void LoadThongKePhieu()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var list = (await client.From<PhieuNhapKho>().Get()).Models;
                txtTongPhieuCanDuyet.Text = list.Count(p => p.TrangThai == null).ToString();
                txtTongPhieuDaDuyet.Text = list.Count(p => p.TrangThai == true).ToString();
                txtTongPhieuTuChoi.Text = list.Count(p => p.TrangThai == false).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi load thống kê: {ex.Message}");
            }
        }

        private async void cboLoaiPhieu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LocDuLieuTheoLoaiVaNgay();
        }

        private async void dpTuNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            await LocDuLieuTheoLoaiVaNgay();
        }

        private async void dpDenNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            await LocDuLieuTheoLoaiVaNgay();
        }
    }
}
