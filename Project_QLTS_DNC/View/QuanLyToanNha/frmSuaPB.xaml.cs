using Project_QLTS_DNC.Models;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for frmSuaPB.xaml
    /// </summary>
    public partial class frmSuaPB : Window
    {
        public PhongBan PhongBanDaSua { get; private set; } // ← Dòng này bắt buộc

        private readonly PhongBan _phongBanGoc;

       

        public frmSuaPB(PhongBan phongBan)
        {
            InitializeComponent();

            _phongBanGoc = phongBan;

            // Gán dữ liệu cũ vào form
            txtTenPhongBan.Text = phongBan.TenPhongBan;
            txtMoTaPhongBan.Text = phongBan.MoTaPhongBan;
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra dữ liệu
            if (string.IsNullOrWhiteSpace(txtTenPhongBan.Text))
            {
                MessageBox.Show("Vui lòng nhập tên phòng ban!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Cập nhật dữ liệu
            PhongBanDaSua = new PhongBan
            {
                MaPhongBan = _phongBanGoc.MaPhongBan,
                TenPhongBan = txtTenPhongBan.Text.Trim(),
                MoTaPhongBan = txtMoTaPhongBan.Text.Trim()
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
