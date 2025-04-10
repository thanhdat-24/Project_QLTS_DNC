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
    /// Interaction logic for frmSuaPhong.xaml
    /// </summary>
    public partial class frmSuaPhong : Window
    {
        // Thuộc tính để lưu thông tin phòng đã chỉnh sửa
        public Phong PhongDaSua { get; private set; }

        // Constructor nhận thông tin phòng cần sửa
        public frmSuaPhong(Phong phongHienTai)
        {
            InitializeComponent();

            // Điền dữ liệu hiện tại vào form
            txtTenPhong.Text = phongHienTai.TenPhong;
            txtSucChuaPhong.Text = phongHienTai.SucChua.ToString();
            txtMoTaPhong.Text = phongHienTai.MoTaPhong;
        }

        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra dữ liệu hợp lệ
            if (string.IsNullOrWhiteSpace(txtTenPhong.Text))
            {
                MessageBox.Show("Vui lòng nhập tên phòng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(txtSucChuaPhong.Text, out int sucChua) || sucChua <= 0)
            {
                MessageBox.Show("Sức chứa phải là số dương!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Lưu thông tin phòng đã chỉnh sửa
            PhongDaSua = new Phong
            {
                TenPhong = txtTenPhong.Text,
                SucChua = sucChua,
                MoTaPhong = txtMoTaPhong.Text
            };

            DialogResult = true; // Đánh dấu form đã hoàn thành
            Close();
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Đánh dấu form bị hủy
            Close();
        }
    }
}
