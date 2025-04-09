using Project_QLTS_DNC.Models;
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
    /// Interaction logic for frmThemPhong.xaml
    /// </summary>
    public partial class frmThemPhong : Window
    {
        public Phong PhongMoi { get; private set; }

        public frmThemPhong()
        {
            InitializeComponent();
            Title = "Thêm phòng mới";
        }

        public frmThemPhong(Phong phongHienTai)
        {
            InitializeComponent();
            Title = "Chỉnh sửa phòng";

            // Điền dữ liệu hiện tại vào form
            txtTenP.Text = phongHienTai.TenPhong;
            txtSucChuaP.Text = phongHienTai.SucChua.ToString();
            txtMoTaP.Text = phongHienTai.MoTaPhong;
        }


        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra dữ liệu hợp lệ
            if (string.IsNullOrWhiteSpace(txtTenP.Text))
            {
                MessageBox.Show("Vui lòng nhập tên phòng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(txtSucChuaP.Text, out int sucChua) || sucChua <= 0)
            {
                MessageBox.Show("Sức chứa phải là số dương!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Lưu thông tin phòng
            PhongMoi = new Phong
            {
                TenPhong = txtTenP.Text,
                SucChua = sucChua,
                MoTaPhong = txtMoTaP.Text
            };

            DialogResult = true;
            Close();
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
