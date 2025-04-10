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
    /// Interaction logic for frmSuaTang.xaml
    /// </summary>
    public partial class frmSuaTang : Window
    {
        public Tang TangDaSua { get; set; }

        private readonly Tang _tangCanSua;
        public frmSuaTang(Tang tang)
        {
            InitializeComponent();
            _tangCanSua = tang;

            // Hiển thị dữ liệu cũ lên form
            txtTenTang.Text = _tangCanSua.TenTang;
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenTang.Text))
            {
                MessageBox.Show("Vui lòng nhập tên tầng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Cập nhật dữ liệu
            TangDaSua = new Tang
            {
                MaTang = _tangCanSua.MaTang, // giữ lại mã tầng cũ
                TenTang = txtTenTang.Text.Trim()
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
