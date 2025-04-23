using Project_QLTS_DNC.Helpers;
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
    public partial class frmTang : UserControl
    {
        public ObservableCollection<Tang> DanhSachTang { get; set; } = new ObservableCollection<Tang>();
        private List<Tang> DanhSachGoc { get; set; } = new List<Tang>();

        public frmTang()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                if (!QuyenNguoiDungHelper.HasPermission("btnTang", "xem"))
                {
                    MessageBox.Show("Bạn không có quyền xem tầng tòa nhà!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var tangs = await TangService.LayDanhSachTangAsync();

                DanhSachTang.Clear();
                DanhSachGoc.Clear();

                foreach (var tang in tangs)
                {
                    DanhSachTang.Add(tang);
                    DanhSachGoc.Add(tang);
                }

                dgTang.ItemsSource = DanhSachTang;
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu tầng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnTang", "them"))
            {
                MessageBox.Show("Bạn không có quyền thêm tầng tòa nhà!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var form = new frmThemTang();
            if (form.ShowDialog() == true)
            {
                await LoadDataAsync(); // 🔁 Gọi lại API để load lại dữ liệu chính xác từ Supabase
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string tuKhoa = txtSearch.Text.Trim().ToLower();

            var ketQua = DanhSachGoc.Where(t =>
                t.MaTang.ToString().Contains(tuKhoa) ||
                (t.TenTang != null && t.TenTang.ToLower().Contains(tuKhoa))
            ).ToList();

            DanhSachTang.Clear();
            foreach (var item in ketQua)
            {
                DanhSachTang.Add(item);
            }

            if (!ketQua.Any())
                MessageBox.Show("Không tìm thấy tầng phù hợp.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

            UpdateStatusBar();
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnTang", "sua"))
            {
                MessageBox.Show("Bạn không có quyền sửa tầng tòa nhà!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var btn = sender as Button;
            if (btn?.DataContext is Tang tangChon)
            {
                var form = new frmSuaTang(tangChon);
                if (form.ShowDialog() == true && form.TangDaSua != null)
                {
                    try
                    {
                        var tangCapNhat = await TangService.CapNhatTangAsync(form.TangDaSua);

                        // Cập nhật lại danh sách
                        var index = DanhSachTang.IndexOf(tangChon);
                        if (index >= 0)
                        {
                            DanhSachTang[index] = tangCapNhat;
                            DanhSachGoc[index] = tangCapNhat;
                        }

                        dgTang.Items.Refresh();
                        UpdateStatusBar();

                        MessageBox.Show("Cập nhật tầng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi cập nhật tầng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnTang", "xoa"))
            {
                MessageBox.Show("Bạn không có quyền xóa tầng tòa nhà!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var btn = sender as Button;
            if (btn?.DataContext is Tang tangCanXoa)
            {
                var xacNhan = MessageBox.Show(
                    $"Bạn có chắc muốn xóa tầng '{tangCanXoa.TenTang}'?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (xacNhan != MessageBoxResult.Yes)
                    return;

                try
                {
                    bool ketQua = await TangService.XoaTangAsync(tangCanXoa.MaTang ?? 0);
                    if (ketQua)
                    {
                        DanhSachTang.Remove(tangCanXoa);
                        DanhSachGoc.Remove(tangCanXoa);
                        UpdateStatusBar();

                        MessageBox.Show("Xóa tầng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa tầng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa tầng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private async void btnLoadDuLieu_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void UpdateStatusBar()
        {
            txtStatus.Text = $"Tổng số tầng: {DanhSachTang.Count}";
        }
    }
}
