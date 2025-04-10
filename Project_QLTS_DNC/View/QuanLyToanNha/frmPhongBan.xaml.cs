using Project_QLTS_DNC.Models.ToaNha;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Project_QLTS_DNC.View.QuanLyToanNha
{
    /// <summary>
    /// Interaction logic for frmPhongBan.xaml
    /// </summary>
    public partial class frmPhongBan : UserControl
    {
        // Danh sách phòng ban hiển thị trong DataGrid
        public ObservableCollection<PhongBan> DanhSachPhongBan { get; set; } = new ObservableCollection<PhongBan>();

        // Danh sách gốc để lưu trữ dữ liệu ban đầu
        private List<PhongBan> DanhSachGoc { get; set; } = new List<PhongBan>();

        public frmPhongBan()
        {
            InitializeComponent();
            LoadData();
            dgPhongBan.ItemsSource = DanhSachPhongBan;
            UpdateStatusBar();
        }

        private static int _nextMaPhongBan = 3;

        private void LoadData()
        {
            // Dữ liệu mẫu
            DanhSachGoc = new List<PhongBan>
            {
                new PhongBan { MaPhongBan = 1, TenPhongBan = "Phòng Hành chính", MoTaPhongBan = "Phòng phụ trách giấy tờ, hành chính" },
                new PhongBan { MaPhongBan = 2, TenPhongBan = "Phòng Kế toán", MoTaPhongBan = "Phòng xử lý tài chính và kế toán" }
            };

            // Gán vào danh sách hiển thị
            DanhSachPhongBan.Clear();
            foreach (var pb in DanhSachGoc)
            {
                DanhSachPhongBan.Add(pb);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var themForm = new frmThemPB(); // Đã đổi tên form
            if (themForm.ShowDialog() == true && themForm.PhongBanMoi != null)
            {
                themForm.PhongBanMoi.MaPhongBan = _nextMaPhongBan++;
                DanhSachPhongBan.Add(themForm.PhongBanMoi);
                DanhSachGoc.Add(themForm.PhongBanMoi);
                UpdateStatusBar();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string tuKhoa = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(tuKhoa))
            {
                DanhSachPhongBan.Clear();
                foreach (var pb in DanhSachGoc)
                {
                    DanhSachPhongBan.Add(pb);
                }
                UpdateStatusBar();
                return;
            }

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

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.DataContext is PhongBan pbCanSua)
            {
                var form = new frmSuaPB(pbCanSua);
                if (form.ShowDialog() == true && form.PhongBanDaSua != null)
                {
                    pbCanSua.TenPhongBan = form.PhongBanDaSua.TenPhongBan;
                    pbCanSua.MoTaPhongBan = form.PhongBanDaSua.MoTaPhongBan;
                    dgPhongBan.Items.Refresh();
                    UpdateStatusBar();
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.DataContext is PhongBan pbCanXoa)
            {
                var result = MessageBox.Show($"Bạn có chắc muốn xóa phòng ban: {pbCanXoa.TenPhongBan}?",
                                             "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    DanhSachPhongBan.Remove(pbCanXoa);
                    DanhSachGoc.Remove(pbCanXoa);
                    UpdateStatusBar();
                }
            }
        }

        private void btnLoadDuLieu_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;

            DanhSachPhongBan.Clear();
            foreach (var pb in DanhSachGoc)
            {
                DanhSachPhongBan.Add(pb);
            }

            UpdateStatusBar();
        }
        private void UpdateStatusBar()
        {
            txtStatus.Text = $"Tổng số phòng ban: {DanhSachPhongBan.Count}";
        }
    }
}
