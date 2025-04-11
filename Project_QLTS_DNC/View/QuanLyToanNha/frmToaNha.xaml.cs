using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Project_QLTS_DNC.View.QuanLyToanNha
{
    public partial class frmToaNha : UserControl
    {
        public ObservableCollection<ToaNha> DanhSachToaNha { get; set; } = new ObservableCollection<ToaNha>();
        public ObservableCollection<ToaNha> DanhSachGoc { get; set; } = new ObservableCollection<ToaNha>();

        public frmToaNha()
        {
            InitializeComponent();
            Loaded += async (s, e) => await HienThiDuLieu();
        }

        public async Task HienThiDuLieu()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var result = await client.From<ToaNha>().Get();

                DanhSachToaNha.Clear();
                DanhSachGoc.Clear();

                foreach (var tn in result.Models)
                {
                    DanhSachToaNha.Add(tn);
                    DanhSachGoc.Add(tn);
                }

                dgToaNha.ItemsSource = DanhSachToaNha;
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu từ Supabase: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var form = new frmThemToaNha(); // tạo form thêm toà nhà
            if (form.ShowDialog() == true && form.ToaNhaMoi != null)
            {
                DanhSachToaNha.Add(form.ToaNhaMoi);
                DanhSachGoc.Add(form.ToaNhaMoi);
                UpdateStatusBar();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(keyword))
            {
                DanhSachToaNha.Clear();
                foreach (var tn in DanhSachGoc)
                    DanhSachToaNha.Add(tn);
            }
            else
            {
                var ketQua = DanhSachGoc.Where(tn =>
                    tn.MaToaNha.ToString().Contains(keyword) ||
                    (!string.IsNullOrEmpty(tn.TenToaNha) && tn.TenToaNha.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(tn.DiaChiTN) && tn.DiaChiTN.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(tn.SoDienThoaiTN) && tn.SoDienThoaiTN.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(tn.MoTaTN) && tn.MoTaTN.ToLower().Contains(keyword))
                ).ToList();

                DanhSachToaNha.Clear();
                foreach (var tn in ketQua)
                    DanhSachToaNha.Add(tn);
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var toaNhaChon = dgToaNha.SelectedItem as ToaNha;
            if (toaNhaChon == null)
            {
                MessageBox.Show("Vui lòng chọn một tòa nhà để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var xacNhan = MessageBox.Show($"Bạn có chắc chắn muốn xóa tòa nhà '{toaNhaChon.TenToaNha}'?",
                                           "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (xacNhan != MessageBoxResult.Yes)
                return;

            try
            {
                bool ketQua = await ToaNhaService.XoaTNAsync(toaNhaChon.MaToaNha.Value);
                if (ketQua)
                {
                    DanhSachToaNha.Remove(toaNhaChon);
                    DanhSachGoc.Remove(toaNhaChon);
                    UpdateStatusBar();

                    MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Không thể xóa tòa nhà!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLoadDuLieu_Click(object sender, RoutedEventArgs e)
        {
            _ = HienThiDuLieu(); // load lại
        }

        private void UpdateStatusBar()
        {
            txtStatus.Text = $"Tổng số tòa nhà: {DanhSachToaNha.Count}";
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var toaNhaChon = dgToaNha.SelectedItem as ToaNha;

            if (toaNhaChon == null)
            {
                MessageBox.Show("Vui lòng chọn một tòa nhà để sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Gọi form sửa với dữ liệu hiện tại
            var form = new frmSuaToaNha(toaNhaChon);
            if (form.ShowDialog() == true && form.ToaNhaDaSua != null)
            {
                try
                {
                    // Gửi dữ liệu mới lên Supabase
                    var toaNhaCapNhat = await ToaNhaService.CapNhatToaNhaAsync(form.ToaNhaDaSua);

                    // Cập nhật lại trong danh sách đang hiển thị
                    var index = DanhSachToaNha.IndexOf(toaNhaChon);
                    if (index >= 0)
                    {
                        DanhSachToaNha[index] = toaNhaCapNhat;
                        DanhSachGoc[index] = toaNhaCapNhat;
                    }

                    UpdateStatusBar();
                    MessageBox.Show("Cập nhật tòa nhà thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật tòa nhà: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


    }
}
