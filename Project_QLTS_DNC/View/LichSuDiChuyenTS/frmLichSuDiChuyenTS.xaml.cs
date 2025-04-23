using Project_QLTS_DNC.Models.LichSu;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.View.DuyetPhieu.ChiTietPhieu;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Project_QLTS_DNC.View.LichSuDiChuyenTS
{
    public partial class frmLichSuDiChuyenTS : UserControl
    {
        private ObservableCollection<LichSuDTO> _dsLichSu = new();
        private string _keyword = "";
        private int? _filterMaPhong = null;
        private string _filterTrangThai = null;

        public frmLichSuDiChuyenTS()
        {
            InitializeComponent();
            _ = Init();
        }

        private async Task Init()
        {
            await LoadPhongLookupAsync();
            await LoadLichSuAsync();
        }

        private async Task LoadPhongLookupAsync()
        {
            var dsPhong = await LichSuDiChuyenService.LayDanhSachPhongAsync();
            var dsPhongFilter = new List<object>
            {
                new { TenPhong = "Tất cả", MaPhong = (int?)null }
            };
            dsPhongFilter.AddRange(dsPhong.Select(p => new { p.TenPhong, MaPhong = (int?)p.MaPhong }));
            cboPhong.ItemsSource = dsPhongFilter;
            cboPhong.SelectedIndex = 0;
        }

        private async Task LoadLichSuAsync()
        {
            _dsLichSu = await LichSuDiChuyenService.LayDanhSachLichSuAsync();
            var filtered = _dsLichSu.Where(p =>
                (string.IsNullOrEmpty(_keyword) ||
                 p.TenTaiSan.Contains(_keyword, StringComparison.OrdinalIgnoreCase) ||
                 p.SoSeri.Contains(_keyword, StringComparison.OrdinalIgnoreCase) ||
                 p.TenPhongCu.Contains(_keyword, StringComparison.OrdinalIgnoreCase) ||
                 p.TenPhongMoi.Contains(_keyword, StringComparison.OrdinalIgnoreCase) ||
                 p.TenNhanVien.Contains(_keyword, StringComparison.OrdinalIgnoreCase))
                && (_filterMaPhong == null || p.MaPhongCu == _filterMaPhong)
                && (string.IsNullOrEmpty(_filterTrangThai) ||
                    (_filterTrangThai == "Chờ duyệt" && p.TrangThai == null) ||
                    (_filterTrangThai == "Đã duyệt" && p.TrangThai == true) ||
                    (_filterTrangThai == "Từ chối duyệt" && p.TrangThai == false))
            ).ToList();

            dgDiChuyen.ItemsSource = filtered;
            txtStatus.Text = $"Tổng số phiếu di chuyển: {filtered.Count}";
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _keyword = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(_keyword))
            {
                popupSuggest.IsOpen = false;
                return;
            }

            var suggestions = _dsLichSu.Where(p =>
                p.TenTaiSan.Contains(_keyword, StringComparison.OrdinalIgnoreCase) ||
                p.TenNhanVien.Contains(_keyword, StringComparison.OrdinalIgnoreCase) ||
                p.SoSeri.Contains(_keyword, StringComparison.OrdinalIgnoreCase))
                .Select(p => p.TenTaiSan)
                .Distinct()
                .Take(10)
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
                _ = LoadLichSuAsync();
            }
        }

        private void cboTrangThai_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboTrangThai.SelectedItem is ComboBoxItem item && item.Content != null)
            {
                string selected = item.Content.ToString();
                _filterTrangThai = selected == "Tất cả" ? null : selected;
            }
            else
            {
                _filterTrangThai = null;
            }

            _ = LoadLichSuAsync();
        }

        private void cboPhong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboPhong.SelectedItem != null)
            {
                var selected = cboPhong.SelectedItem as dynamic;
                _filterMaPhong = selected.MaPhong;
            }
            else
            {
                _filterMaPhong = null;
            }

            _ = LoadLichSuAsync();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _keyword = txtSearch.Text.Trim();
            _ = LoadLichSuAsync();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _keyword = "";
            txtSearch.Text = "";
            cboPhong.SelectedIndex = 0;
            cboTrangThai.SelectedIndex = 0;
            _ = LoadLichSuAsync();
        }

        private void btnChiTiet_Click(object sender, RoutedEventArgs e)
        {
            if (dgDiChuyen.SelectedItem is LichSuDTO selected)
            {
                var frm = new frmChiTietDiChuyenTS(selected.MaLichSu)
                {
                    Title = $"Chi tiết phiếu lịch sử - LS{selected.MaLichSu}",
                    Width = 1000,
                    Height = 720,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                frm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một phiếu để xem chi tiết!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnThemDiChuyen_click(object sender, RoutedEventArgs e)
        {
            var frm = new frmThemPhieuDiChuyen();
            frm.ShowDialog();
            _ = LoadLichSuAsync();
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgDiChuyen.SelectedItem is not LichSuDTO selected)
            {
                MessageBox.Show("Vui lòng chọn một phiếu để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show($"Bạn có chắc muốn xoá phiếu di chuyển \"LS{selected.MaLichSu}\"?",
                                          "Xác nhận xoá",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Warning);

            if (confirm == MessageBoxResult.Yes)
            {
                var success = await LichSuDiChuyenService.XoaPhieuLichSuAsync(selected.MaLichSu);
                if (success)
                {
                    MessageBox.Show("🗑Đã xoá phiếu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadLichSuAsync();
                }
                else
                {
                    MessageBox.Show("Xoá phiếu thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}