﻿using Project_QLTS_DNC.Models.ToaNha;
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
    /// Interaction logic for frmSuaToaNha.xaml
    /// </summary>
    public partial class frmSuaToaNha : Window
    {
        public ToaNha ToaNhaDaSua { get; private set; }

        public frmSuaToaNha(ToaNha toaNha)
        {
            InitializeComponent();

            // Gán dữ liệu ban đầu vào textbox
            ToaNhaDaSua = new ToaNha
            {
                MaToaNha = toaNha.MaToaNha,
                TenToaNha = toaNha.TenToaNha,
                DiaChiTN = toaNha.DiaChiTN,
                SoDienThoaiTN = toaNha.SoDienThoaiTN,
                MoTaTN = toaNha.MoTaTN
            };

            txtTenToaNha.Text = ToaNhaDaSua.TenToaNha;
            txtDiaChiTN.Text = ToaNhaDaSua.DiaChiTN;
            txtSoDienThoaiTN.Text = ToaNhaDaSua.SoDienThoaiTN;
            txtMoTaTN.Text = ToaNhaDaSua.MoTaTN;
        }


      


        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnCapNhat_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra hợp lệ
            if (string.IsNullOrWhiteSpace(txtTenToaNha.Text))
            {
                MessageBox.Show("Vui lòng nhập tên tòa nhà!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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

            // Gán dữ liệu đã chỉnh sửa vào đối tượng ToaNhaDaSua
            ToaNhaDaSua.TenToaNha = txtTenToaNha.Text.Trim();
            ToaNhaDaSua.DiaChiTN = txtDiaChiTN.Text.Trim();
            ToaNhaDaSua.SoDienThoaiTN = txtSoDienThoaiTN.Text.Trim();
            ToaNhaDaSua.MoTaTN = txtMoTaTN.Text.Trim();

            // Đóng form và cho biết đã sửa xong
            this.DialogResult = true;
            this.Close();
        }
    }
}
