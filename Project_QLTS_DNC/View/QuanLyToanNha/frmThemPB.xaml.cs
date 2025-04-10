using Project_QLTS_DNC.Models.ToaNha;
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
    /// Interaction logic for frmThemPB.xaml
    /// </summary>
    public partial class frmThemPB : Window
    {
        // ✅ Thêm dòng này
        public PhongBan PhongBanMoi { get; set; }
        public frmThemPB()
        {
            InitializeComponent();
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra dữ liệu hợp lệ
            if (string.IsNullOrWhiteSpace(txtTenPB.Text))
            {
                MessageBox.Show("Vui lòng nhập tên phòng ban!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Tạo mới đối tượng PhongBan
            PhongBanMoi = new PhongBan
            {
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
