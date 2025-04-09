using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Models;
namespace Project_QLTS_DNC.View.QuanLyToanNha
{

    /// <summary>
    /// Interaction logic for frmPhong.xaml
    /// </summary>
    public partial class frmPhong : UserControl
    {
      
        // Danh sách phòng hiển thị trong DataGrid
        public ObservableCollection<Phong> DanhSachPhong { get; set; } = new ObservableCollection<Phong>();

        // Danh sách gốc để lưu trữ dữ liệu ban đầu
        private List<Phong> DanhSachGoc { get; set; } = new List<Phong>();

        public frmPhong()
        {
            InitializeComponent();

            // Khởi tạo danh sách phòng và load dữ liệu mẫu
            LoadData();

            // Gán danh sách phòng vào DataGrid
            dgPhong.ItemsSource = DanhSachPhong;

            // Cập nhật trạng thái footer
            UpdateStatusBar();
        }
        private static int _nextMaPhong = 1;
        private void LoadData()
        {
            // Dữ liệu mẫu
            DanhSachGoc = new List<Phong>
            {
                new Phong { MaPhong = 1, TenPhong = "Phòng A", SucChua = 50, MoTaPhong = "Phòng họp lớn" },
                new Phong { MaPhong = 2, TenPhong = "Phòng B", SucChua = 30, MoTaPhong = "Phòng làm việc" }
            };

            // Gán vào danh sách hiển thị
            DanhSachPhong.Clear();
            foreach (var phong in DanhSachGoc)
            {
                DanhSachPhong.Add(phong);
            }
        }
        // Sự kiện tìm kiếm phòng
        private List<Phong> ketQua;
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string tuKhoa = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(tuKhoa))
            {
                DanhSachPhong.Clear();
                foreach (var phong in DanhSachGoc)
                {
                    DanhSachPhong.Add(phong);
                }
                UpdateStatusBar();
                return;
            }

            ketQua = DanhSachGoc.Where(phong =>
                phong.MaPhong.ToString().ToLower().Contains(tuKhoa) ||
                (phong.TenPhong != null && phong.TenPhong.ToLower().Contains(tuKhoa)) ||
                phong.SucChua.ToString().ToLower().Contains(tuKhoa) ||
                (phong.MoTaPhong != null && phong.MoTaPhong.ToLower().Contains(tuKhoa))
            ).ToList();

            DanhSachPhong.Clear();
            foreach (var phong in ketQua)
            {
                DanhSachPhong.Add(phong);
            }

            if (ketQua.Count == 0)
            {
                MessageBox.Show("Không tìm thấy kết quả nào.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                UpdateStatusBar();
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var themForm = new frmThemPhong(); // Constructor không tham số
            if (themForm.ShowDialog() == true && themForm.PhongMoi != null)
            {
                // Gán mã phòng tự动生成
                themForm.PhongMoi.MaPhong = _nextMaPhong++;
                DanhSachPhong.Add(themForm.PhongMoi);
                DanhSachGoc.Add(themForm.PhongMoi);

                // Cập nhật trạng thái footer
                UpdateStatusBar();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.DataContext is Phong phongCanXoa)
            {
                var result = MessageBox.Show($"Bạn có chắc muốn xóa phòng: {phongCanXoa.TenPhong}?",
                                             "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    DanhSachPhong.Remove(phongCanXoa);
                    DanhSachGoc.Remove(phongCanXoa);

                    // Cập nhật trạng thái footer
                    UpdateStatusBar();
                }
            }
        }

        private void btnLoadDuLieu_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty; // Xóa từ khóa tìm kiếm

            // Tải lại toàn bộ danh sách gốc
            DanhSachPhong.Clear();
            foreach (var phong in DanhSachGoc)
            {
                DanhSachPhong.Add(phong);
            }

            // Cập nhật trạng thái footer
            UpdateStatusBar();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.DataContext is Phong phongCanSua)
            {
                var form = new frmSuaPhong(phongCanSua); // Truyền thông tin phòng cần sửa
                if (form.ShowDialog() == true && form.PhongDaSua != null)
                {
                    // Cập nhật lại dữ liệu
                    phongCanSua.TenPhong = form.PhongDaSua.TenPhong;
                    phongCanSua.SucChua = form.PhongDaSua.SucChua;
                    phongCanSua.MoTaPhong = form.PhongDaSua.MoTaPhong;

                    // Làm mới DataGrid
                    dgPhong.Items.Refresh();

                    // Cập nhật trạng thái footer
                    UpdateStatusBar();
                }
            }
        }

        // Cập nhật thanh trạng thái (footer)
        private void UpdateStatusBar()
        {
            txtStatus.Text = $"Tổng số phòng: {DanhSachPhong.Count}";
        }
    }
}