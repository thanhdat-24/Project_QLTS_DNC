using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services.QLToanNha;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Project_QLTS_DNC.View.QuanLyToanNha
{
    public partial class frmThemPB : Window
    {
        public PhongBan PhongBanMoi { get; private set; }
        private List<ToaNha> DanhSachToaNha = new();

        public frmThemPB()
        {
            InitializeComponent();
            Loaded += frmThemPB_Loaded;
        }

        private async void frmThemPB_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DanhSachToaNha = (await ToaNhaService.LayDanhSachToaNhaAsync()).ToList();
                cboTenToa.ItemsSource = DanhSachToaNha;
                cboTenToa.DisplayMemberPath = "TenToaNha";      // tên hiển thị
                cboTenToa.SelectedValuePath = "MaToaNha";       // giá trị lưu
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách tòa nhà: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (cboTenToa.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn tòa nhà!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTenPB.Text))
            {
                MessageBox.Show("Vui lòng nhập tên phòng ban!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PhongBanMoi = new PhongBan
            {
                MaToa = (int)cboTenToa.SelectedValue,
                TenPhongBan = txtTenPB.Text.Trim(),
                MoTaPhongBan = txtMoTaPB.Text.Trim()
            };

            this.DialogResult = true;
            this.Close();
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
