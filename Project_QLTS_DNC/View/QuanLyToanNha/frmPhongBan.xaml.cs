using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services.QLToanNha;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Project_QLTS_DNC.View.QuanLyToanNha
{
    public partial class frmPhongBan : UserControl
    {
        public ObservableCollection<PhongBan> DanhSachPhongBan { get; set; } = new ObservableCollection<PhongBan>();
        private List<PhongBan> DanhSachGoc { get; set; } = new List<PhongBan>();

        public frmPhongBan()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                DanhSachPhongBan.Clear();
                DanhSachGoc.Clear();

                var ds = await PhongBanService.LayDanhSachPhongBanAsync();
                foreach (var pb in ds)
                {
                    DanhSachPhongBan.Add(pb);
                    DanhSachGoc.Add(pb);
                }

                dgPhongBan.ItemsSource = DanhSachPhongBan;
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải phòng ban: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var themForm = new frmThemPB();
            if (themForm.ShowDialog() == true && themForm.PhongBanMoi != null)
            {
                try
                {
                    var pbMoi = await PhongBanService.ThemPhongBanAsync(themForm.PhongBanMoi);
                    DanhSachPhongBan.Add(pbMoi);
                    DanhSachGoc.Add(pbMoi);
                    UpdateStatusBar();
                    MessageBox.Show("Thêm phòng ban thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi thêm phòng ban: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string tuKhoa = txtSearch.Text.Trim().ToLower();

            var ketQua = DanhSachGoc.Where(pb =>
                pb.MaPhongBan.ToString().Contains(tuKhoa) ||
                (pb.TenPhongBan != null && pb.TenPhongBan.ToLower().Contains(tuKhoa)) ||
                (pb.MoTaPhongBan != null && pb.MoTaPhongBan.ToLower().Contains(tuKhoa))
            ).ToList();

            DanhSachPhongBan.Clear();
            foreach (var pb in ketQua)
            {
                DanhSachPhongBan.Add(pb);
            }

            if (ketQua.Count == 0)
            {
                MessageBox.Show("Không tìm thấy kết quả nào.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            UpdateStatusBar();
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PhongBan pbCanSua)
            {
                var form = new frmSuaPB(pbCanSua);
                if (form.ShowDialog() == true && form.PhongBanDaSua != null)
                {
                    try
                    {
                        var pbCapNhat = await PhongBanService.CapNhatPhongBanAsync(form.PhongBanDaSua);

                        var index = DanhSachPhongBan.IndexOf(pbCanSua);
                        if (index >= 0)
                        {
                            DanhSachPhongBan[index] = pbCapNhat;
                            DanhSachGoc[index] = pbCapNhat;
                            dgPhongBan.Items.Refresh();
                        }

                        MessageBox.Show("Cập nhật phòng ban thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        UpdateStatusBar();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi cập nhật: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PhongBan pbCanXoa)
            {
                var xacNhan = MessageBox.Show($"Bạn có chắc muốn xoá phòng ban: {pbCanXoa.TenPhongBan}?",
                                               "Xác nhận xoá", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (xacNhan == MessageBoxResult.Yes)
                {
                    try
                    {
                        int ma = pbCanXoa.MaPhongBan ?? 0;
                        bool ketQua = await PhongBanService.XoaPhongBanAsync(ma);
                        if (ketQua)
                        {
                            DanhSachPhongBan.Remove(pbCanXoa);
                            DanhSachGoc.Remove(pbCanXoa);
                            UpdateStatusBar();
                            MessageBox.Show("Xoá phòng ban thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi xoá: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void btnLoadDuLieu_Click(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private void UpdateStatusBar()
        {
            txtStatus.Text = $"Tổng số phòng ban: {DanhSachPhongBan.Count}";
        }
    }
}
