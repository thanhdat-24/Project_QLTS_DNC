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
    /// Interaction logic for frmTang.xaml
    /// </summary>
    public partial class frmTang : UserControl
    {
        public ObservableCollection<Tang> DanhSachTang { get; set; } = new ObservableCollection<Tang>();
        private List<Tang> DanhSachGoc { get; set; } = new List<Tang>();
        private static int _nextMaTang = 1;

        public frmTang()
        {
            InitializeComponent();
            LoadData();
            dgTang.ItemsSource = DanhSachTang;
            UpdateStatusBar();
        }

        private void LoadData()
        {
            DanhSachGoc = new List<Tang>
            {
                new Tang { MaTang = 1, TenTang = "Tầng 1" },
                new Tang { MaTang = 2, TenTang = "Tầng 2" }
            };

            DanhSachTang.Clear();
            foreach (var tang in DanhSachGoc)
            {
                DanhSachTang.Add(tang);
            }
        }



        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var form = new frmThemTang();
            if (form.ShowDialog() == true && form.TangMoi != null)
            {
                form.TangMoi.MaTang = _nextMaTang++;
                DanhSachTang.Add(form.TangMoi);
                DanhSachGoc.Add(form.TangMoi);
                UpdateStatusBar();
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

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.DataContext is Tang tangCanSua)
            {
                var form = new frmSuaTang(tangCanSua);
                if (form.ShowDialog() == true && form.TangDaSua != null)
                {
                    tangCanSua.TenTang = form.TangDaSua.TenTang;
                    dgTang.Items.Refresh();
                    UpdateStatusBar();
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.DataContext is Tang tangCanXoa)
            {
                var result = MessageBox.Show($"Xóa {tangCanXoa.TenTang}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    DanhSachTang.Remove(tangCanXoa);
                    DanhSachGoc.Remove(tangCanXoa);
                    UpdateStatusBar();
                }
            }
        }

        private void btnLoadDuLieu_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
            DanhSachTang.Clear();
            foreach (var t in DanhSachGoc)
                DanhSachTang.Add(t);

            UpdateStatusBar();
        }
        private void UpdateStatusBar()
        {
            txtStatus.Text = $"Tổng số tầng: {DanhSachTang.Count}";
        }
    }
}
