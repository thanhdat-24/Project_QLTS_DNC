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
        /// Interaction logic for frmThemToaNha.xaml
        /// </summary>
        public partial class frmThemToaNha : Window
        {
        // Đây là đối tượng Toà Nhà mới, sẽ được gán khi nhấn Lưu
        public ToaNha ToaNhaMoi { get; set; }

        public frmThemToaNha()
        {
            InitializeComponent();
        }


        private void btnLuu_Click(object sender, RoutedEventArgs e)
            {
            // Kiểm tra dữ liệu hợp lệ
            if (string.IsNullOrWhiteSpace(txtTenToaNha.Text))
            {
                MessageBox.Show("Vui lòng nhập tên toà nhà!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDiaChiTN.Text))
            {
                MessageBox.Show("Vui lòng nhập địa chỉ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSoDienThoaiTN.Text))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Gán dữ liệu vào đối tượng ToaNha
            ToaNhaMoi = new ToaNha
            {
                TenToaNha = txtTenToaNha.Text.Trim(),
                DiaChiTN = txtDiaChiTN.Text.Trim(),
                SoDienThoaiTN = txtSoDienThoaiTN.Text.Trim(),
                MoTaTN = txtMoTaTN.Text.Trim()
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
