using Project_QLTS_DNC.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace Project_QLTS_DNC.View.QuanLyToanNha
{
    /// <summary>
    /// Interaction logic for frmToaNha.xaml
    /// </summary>
    public partial class frmToaNha : UserControl
    {
        public ObservableCollection<ToaNha> DanhSachToaNha { get; set; } = new ObservableCollection<ToaNha>();
        public ObservableCollection<ToaNha> DanhSachGoc { get; set; } = new ObservableCollection<ToaNha>();
        private int _nextMaToaNha = 1;

        public frmToaNha()
        {
            InitializeComponent();
            dgToaNha.ItemsSource = DanhSachToaNha;

            // Gọi hàm load dữ liệu luôn khi mở form
            btnLoadDuLieu_Click(null, null);
        }



        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var form = new frmThemToaNha(); // tạo form thêm toà nhà
            if (form.ShowDialog() == true && form.ToaNhaMoi != null)
            {
                form.ToaNhaMoi.MaToaNha = _nextMaToaNha++;
                DanhSachToaNha.Add(form.ToaNhaMoi);
                DanhSachGoc.Add(form.ToaNhaMoi);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(keyword))
            {
                // Nếu không nhập gì, hiển thị lại toàn bộ
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
                    (!string.IsNullOrEmpty(tn.SoDienThoaiTN) && tn.SoDienThoaiTN.ToLower().Contains(keyword))
                ).ToList();

                DanhSachToaNha.Clear();
                foreach (var tn in ketQua)
                    DanhSachToaNha.Add(tn);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgToaNha.SelectedItem is ToaNha selectedTN)
            {
                var form = new frmSuaToaNha(selectedTN); // truyền dữ liệu gốc vào form sửa
                if (form.ShowDialog() == true)
                {
                    // Sau khi sửa, cập nhật lại giao diện
                    var index = DanhSachToaNha.IndexOf(selectedTN);
                    if (index >= 0)
                    {
                        DanhSachToaNha[index] = form.ToaNhaDaSua;
                        dgToaNha.ItemsSource = null;
                        dgToaNha.ItemsSource = DanhSachToaNha;
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn toà nhà để sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgToaNha.SelectedItem is ToaNha selectedTN)
            {
                if (MessageBox.Show($"Bạn có chắc muốn xoá toà nhà \"{selectedTN.TenToaNha}\"?", "Xác nhận xoá", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    DanhSachToaNha.Remove(selectedTN);
                    DanhSachGoc.Remove(selectedTN);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn toà nhà để xoá!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnLoadDuLieu_Click(object sender, RoutedEventArgs e)
        {
            DanhSachToaNha.Clear();
            DanhSachGoc.Clear();

            DanhSachToaNha.Add(new ToaNha { MaToaNha = 1, TenToaNha = "Toà A", DiaChiTN = "123 ABC", SoDienThoaiTN = "0123456789", MoTaTN = "Chính" });
            DanhSachToaNha.Add(new ToaNha { MaToaNha = 2, TenToaNha = "Toà B", DiaChiTN = "456 XYZ", SoDienThoaiTN = "0987654321", MoTaTN = "Phụ" });
            DanhSachToaNha.Add(new ToaNha { MaToaNha = 3, TenToaNha = "Toà C", DiaChiTN = "789 DEF", SoDienThoaiTN = "0111222333", MoTaTN = "Văn phòng" });
            DanhSachToaNha.Add(new ToaNha { MaToaNha = 4, TenToaNha = "Toà D", DiaChiTN = "321 LMN", SoDienThoaiTN = "0222333444", MoTaTN = "Ký túc xá" });
            DanhSachToaNha.Add(new ToaNha { MaToaNha = 5, TenToaNha = "Toà E", DiaChiTN = "654 PQR", SoDienThoaiTN = "0333444555", MoTaTN = "Kho lưu trữ" });
            DanhSachToaNha.Add(new ToaNha { MaToaNha = 6, TenToaNha = "Toà F", DiaChiTN = "987 STU", SoDienThoaiTN = "0444555666", MoTaTN = "Trung tâm hội nghị" });


            foreach (var tn in DanhSachToaNha)
                DanhSachGoc.Add(tn);

            _nextMaToaNha = DanhSachToaNha.Max(t => t.MaToaNha) + 1;
            UpdateStatusBar();
        }
        private void UpdateStatusBar()
        {
            txtStatus.Text = $"Tổng số tầng: {DanhSachToaNha.Count}";
        }
    }
}
