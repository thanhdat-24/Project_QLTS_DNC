using Project_QLTS_DNC.Models.DuyetPhieu;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.View.DuyetPhieu.ChiTietPhieu;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
            await LoadDuLieuTatCaPhieuAsync();
        }

        private async Task LoadDuLieuTatCaPhieuAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                DanhSachTatCaPhieu.Clear();

                var phieuNhap = await client.From<PhieuNhapKhoInput>().Get();

                foreach (var pn in phieuNhap.Models)
                {
                    DanhSachTatCaPhieu.Add(new PhieuCanDuyet
                    {
                        MaPhieu = $"PN{pn.MaPhieuNhap}",
                        NgayTaoPhieu = pn.NgayNhap,
                        TrangThai = pn.TrangThai, // giữ kiểu bool?
                        LoaiPhieu = "Phiếu nhập"
                    });
                }

                dgPhieuCanDuyet.ItemsSource = DanhSachTatCaPhieu;

                if (DanhSachTatCaPhieu.Count > 0)
                    dgPhieuCanDuyet.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi load phiếu nhập: {ex.Message}");
            }
        }

        private async void LoadThongKePhieu()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<PhieuNhapKhoInput>().Get();
                var danhSach = response.Models;

                int daDuyet = danhSach.Count(p => p.TrangThai == true);
                int tuChoi = danhSach.Count(p => p.TrangThai == false);
                int canDuyet = danhSach.Count(p => p.TrangThai == null);

                txtTongPhieuCanDuyet.Text = canDuyet.ToString();
                txtTongPhieuDaDuyet.Text = daDuyet.ToString();
                txtTongPhieuTuChoi.Text = tuChoi.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load thống kê phiếu: " + ex.Message);
            }
        }

        private void btnXemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            if (dgPhieuCanDuyet.SelectedItem is PhieuCanDuyet selected &&
                selected.MaPhieu.StartsWith("PN") &&
                long.TryParse(selected.MaPhieu.Substring(2), out long maPhieuNhap))
            {
                var frm = new frmXemChiTietNhap();
                frm.LoadTheoMaPhieu(maPhieuNhap);

                frm.OnPhieuDuyetThanhCong += async () =>
                {
                    await LoadDuLieuTatCaPhieuAsync();
                    LoadThongKePhieu();
                };

                var window = new Window
                {
                    Title = $"Chi tiết phiếu nhập - PN{maPhieuNhap}",
                    Content = frm,
                    WindowState = WindowState.Maximized,
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    AllowsTransparency = true,
                    Background = System.Windows.Media.Brushes.White,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                window.ShowDialog();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một phiếu để xem chi tiết.");
            }
        }

        private void btnDetail_Click(object sender, RoutedEventArgs e)
        {
            var frm = new frmChiTietPhieuNhap();
            var window = new Window
            {
                Title = "Chi tiết phiếu nhập",
                Content = frm,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowState = WindowState.Maximized,
                WindowStyle = WindowStyle.SingleBorderWindow,
                ResizeMode = ResizeMode.NoResize
            };
            window.ShowDialog();
        }

        private async void cboLoaiPhieu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LocDuLieuTheoLoaiVaNgay();
        }

        private async void dpDenNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            await LocDuLieuTheoLoaiVaNgay();
        }

        private async void dpTuNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            await LocDuLieuTheoLoaiVaNgay();
        }

        private async Task LocDuLieuTheoLoaiVaNgay()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<PhieuNhapKhoInput>().Get();
                var danhSach = response.Models;

                string loaiPhieu = (cboLoaiPhieu.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Tất cả";
                DateTime? tuNgay = dpTuNgay.SelectedDate;
                DateTime? denNgay = dpDenNgay.SelectedDate;

                var loc = danhSach.AsEnumerable();

                if (loaiPhieu != "Tất cả" && loaiPhieu != "Phiếu nhập")
                {
                    loc = Enumerable.Empty<PhieuNhapKhoInput>();
                }

                if (tuNgay.HasValue)
                    loc = loc.Where(p => p.NgayNhap.Date >= tuNgay.Value.Date);

                if (denNgay.HasValue)
                    loc = loc.Where(p => p.NgayNhap.Date <= denNgay.Value.Date);

                DanhSachTatCaPhieu = new ObservableCollection<PhieuCanDuyet>(
                    loc.Select(p => new PhieuCanDuyet
                    {
                        MaPhieu = $"PN{p.MaPhieuNhap}",
                        NgayTaoPhieu = p.NgayNhap,
                        TrangThai = p.TrangThai, // bool?
                        LoaiPhieu = "Phiếu nhập"
                    }));

                dgPhieuCanDuyet.ItemsSource = DanhSachTatCaPhieu;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lọc dữ liệu: " + ex.Message);
            }
        }

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
                p.NgayTaoPhieu.ToString("d/M/yyyy").Contains(tuKhoa) ||
                (p.TrangThai == true ? "đã duyệt" : p.TrangThai == false ? "từ chối duyệt" : "chưa duyệt").ToLower().Contains(tuKhoa)
            ).ToList();

            dgPhieuCanDuyet.ItemsSource = ketQua;
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            TimKiemPhieu(txtSearch.Text);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            TimKiemPhieu(txtSearch.Text);
        }
    }
}
