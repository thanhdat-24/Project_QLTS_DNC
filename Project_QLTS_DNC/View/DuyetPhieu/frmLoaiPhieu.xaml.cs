using Project_QLTS_DNC.Models.DuyetPhieu;
using Project_QLTS_DNC.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Project_QLTS_DNC.View.DuyetPhieu
{
    public partial class frmLoaiPhieu : UserControl
    {
        public ObservableCollection<LoaiPhieu> DanhSachLoaiPhieu { get; set; } = new ObservableCollection<LoaiPhieu>();
        public ObservableCollection<LoaiPhieu> DanhSachGoc { get; set; } = new ObservableCollection<LoaiPhieu>();

        public frmLoaiPhieu()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadDuLieu();
        }

        private async Task LoadDuLieu()
        {
            var danhSach = await LoaiPhieuService.LayDanhSachLoaiPhieuAsync();
            DanhSachLoaiPhieu.Clear();
            DanhSachGoc.Clear();

            foreach (var item in danhSach)
            {
                DanhSachLoaiPhieu.Add(item);
                DanhSachGoc.Add(item);
            }

            dgLoaiPhieu.ItemsSource = DanhSachLoaiPhieu;
            CapNhatStatus();
        }

        private void CapNhatStatus()
        {
            txtStatus.Text = $"Tổng số loại phiếu: {DanhSachLoaiPhieu.Count}";
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(keyword))
            {
                DanhSachLoaiPhieu.Clear();
                foreach (var lp in DanhSachGoc)
                    DanhSachLoaiPhieu.Add(lp);
            }
            else
            {
                var ketQua = DanhSachGoc.Where(lp =>
                    lp.MaLoaiPhieu.ToString().Contains(keyword) ||
                    (!string.IsNullOrEmpty(lp.TenLoaiPhieu) && lp.TenLoaiPhieu.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(lp.MoTaLP) && lp.MoTaLP.ToLower().Contains(keyword))
                ).ToList();

                DanhSachLoaiPhieu.Clear();
                foreach (var lp in ketQua)
                    DanhSachLoaiPhieu.Add(lp);
            }

            CapNhatStatus();
        }

        private async void btnLoadDuLieu_Click(object sender, RoutedEventArgs e)
        {
            await LoadDuLieu();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var form = new frmThemLoaiPhieu();
            if (form.ShowDialog() == true && form.LoaiPhieuMoi != null)
            {
                DanhSachLoaiPhieu.Add(form.LoaiPhieuMoi);
                DanhSachGoc.Add(form.LoaiPhieuMoi);
                CapNhatStatus();
            }
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var loaiPhieuChon = dgLoaiPhieu.SelectedItem as LoaiPhieu;
            if (loaiPhieuChon == null)
            {
                MessageBox.Show("Vui lòng chọn loại phiếu cần sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var form = new frmSuaLoaiPhieu(loaiPhieuChon);
            if (form.ShowDialog() == true && form.LoaiPhieuDaSua != null)
            {
                var kq = await LoaiPhieuService.CapNhatLoaiPhieuAsync(form.LoaiPhieuDaSua);
                var index = DanhSachLoaiPhieu.IndexOf(loaiPhieuChon);
                if (index >= 0)
                {
                    DanhSachLoaiPhieu[index] = kq;
                    DanhSachGoc[index] = kq;
                }

                CapNhatStatus();
                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var loaiPhieuChon = dgLoaiPhieu.SelectedItem as LoaiPhieu;
            if (loaiPhieuChon == null)
            {
                MessageBox.Show("Vui lòng chọn loại phiếu cần xoá.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var xacNhan = MessageBox.Show($"Bạn có chắc chắn muốn xoá loại phiếu '{loaiPhieuChon.TenLoaiPhieu}'?",
                                           "Xác nhận xoá", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (xacNhan != MessageBoxResult.Yes)
                return;

            var kq = await LoaiPhieuService.XoaLoaiPhieuAsync(loaiPhieuChon.MaLoaiPhieu);
            if (kq)
            {
                DanhSachLoaiPhieu.Remove(loaiPhieuChon);
                DanhSachGoc.Remove(loaiPhieuChon);
                CapNhatStatus();
                MessageBox.Show("Xoá thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Không thể xoá loại phiếu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
