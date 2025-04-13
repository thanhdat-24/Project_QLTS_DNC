using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services.QLToanNha;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Project_QLTS_DNC.View.QuanLyToanNha
{
    public partial class frmSuaPB : Window
    {
        public PhongBan PhongBanDaSua { get; private set; }
        private readonly PhongBan _phongBanGoc;
        private List<ToaNha> DanhSachToaNha = new();

        public frmSuaPB(PhongBan phongBan)
        {
            InitializeComponent();
            _phongBanGoc = phongBan;

            Loaded += async (s, e) =>
            {
                try
                {
                    DanhSachToaNha = (await ToaNhaService.LayDanhSachToaNhaAsync()).ToList();
                    cboTenToa.ItemsSource = DanhSachToaNha;
                    cboTenToa.DisplayMemberPath = "TenToaNha";
                    cboTenToa.SelectedValuePath = "MaToaNha";

                    // Hiển thị dữ liệu cũ
                    txtTenPB.Text = phongBan.TenPhongBan;
                    txtMoTaPB.Text = phongBan.MoTaPhongBan;
                    cboTenToa.SelectedValue = phongBan.MaToa;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi tải toà nhà: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnCapNhat_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenPB.Text))
            {
                MessageBox.Show("Vui lòng nhập tên phòng ban!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cboTenToa.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn tòa nhà!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PhongBanDaSua = new PhongBan
            {
                MaPhongBan = _phongBanGoc.MaPhongBan,
                TenPhongBan = txtTenPB.Text.Trim(),
                MoTaPhongBan = txtMoTaPB.Text.Trim(),
                MaToa = (int)cboTenToa.SelectedValue
            };

            DialogResult = true;
            Close();
        }
    }
}
