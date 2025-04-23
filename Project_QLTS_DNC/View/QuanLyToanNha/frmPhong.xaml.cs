using Project_QLTS_DNC.Helpers;
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services.QLToanNha;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Project_QLTS_DNC.View.QuanLyToanNha
{
    public partial class frmPhong : UserControl
    {
        public ObservableCollection<Phong> DanhSachPhong { get; set; } = new ObservableCollection<Phong>();
        private List<Phong> DanhSachGoc { get; set; } = new List<Phong>();

        public frmPhong()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                if (!QuyenNguoiDungHelper.HasPermission("btnPhong", "xem"))
                {
                    MessageBox.Show("Bạn không có quyền xem phòng tòa nhà!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var danhSach = await PhongService.LayDanhSachPhongAsync();
                DanhSachPhong.Clear();
                DanhSachGoc.Clear();

                foreach (var phong in danhSach)
                {
                    DanhSachPhong.Add(phong);
                    DanhSachGoc.Add(phong);
                }

                dgPhong.ItemsSource = DanhSachPhong;
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string tuKhoa = txtSearch.Text.Trim().ToLower();

            var ketQua = string.IsNullOrWhiteSpace(tuKhoa)
                ? DanhSachGoc
                : DanhSachGoc.Where(p =>
                    p.MaPhong.ToString().Contains(tuKhoa) ||
                    (p.TenPhong != null && p.TenPhong.ToLower().Contains(tuKhoa)) ||
                    p.SucChua.ToString().Contains(tuKhoa) ||
                    (p.MoTaPhong != null && p.MoTaPhong.ToLower().Contains(tuKhoa))
                ).ToList();

            DanhSachPhong.Clear();
            foreach (var phong in ketQua)
                DanhSachPhong.Add(phong);

            if (ketQua.Count == 0)
                MessageBox.Show("Không tìm thấy kết quả phù hợp.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

            UpdateStatusBar();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnPhong", "them"))
            {
                MessageBox.Show("Bạn không có quyền thêm phòng tòa nhà!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var form = new frmThemPhong();
            form.ShowDialog(); // 👉 Chỉ mở form lên thôi, không xử lý gì sau đó hết
        }



        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnPhong", "xoa"))
            {
                MessageBox.Show("Bạn không có quyền xóa phòng tòa nhà!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if ((sender as Button)?.DataContext is Phong phongCanXoa)
            {
                var result = MessageBox.Show($"Bạn có chắc muốn xóa phòng '{phongCanXoa.TenPhong}'?",
                                             "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool xoaOk = await PhongService.XoaPhongAsync(phongCanXoa.MaPhong);

                        if (xoaOk)
                        {
                            DanhSachPhong.Remove(phongCanXoa);
                            DanhSachGoc.Remove(phongCanXoa);
                            UpdateStatusBar();

                            // ✅ Thêm dòng thông báo tại đây
                            MessageBox.Show("Xóa phòng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Không thể xóa phòng. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnPhong", "sua"))
            {
                MessageBox.Show("Bạn không có quyền sửa phòng tòa nhà!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if ((sender as Button)?.DataContext is Phong phongChon)
            {
                var form = new frmSuaPhong(phongChon);
                if (form.ShowDialog() == true && form.PhongDaSua != null)
                {
                    try
                    {
                        var phongMoi = await PhongService.CapNhatPhongAsync(form.PhongDaSua);

                        int index = DanhSachPhong.IndexOf(phongChon);
                        if (index >= 0)
                        {
                            DanhSachPhong[index] = phongMoi;
                            DanhSachGoc[index] = phongMoi;
                        }

                        dgPhong.Items.Refresh();
                        UpdateStatusBar();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi cập nhật: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnChiTiet_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int maPhong)
            {
                var form = new suc_chua_phong_nhom(maPhong);
                form.ShowDialog();
            }
        }


        private async void btnLoadDuLieu_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            await LoadData();
        }

        private void UpdateStatusBar()
        {
            txtStatus.Text = $"Tổng số phòng: {DanhSachPhong.Count}";
        }
    }
}
