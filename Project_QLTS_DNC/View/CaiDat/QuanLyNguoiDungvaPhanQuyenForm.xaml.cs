﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Project_QLTS_DNC.View.CaiDat
{
    /// <summary>
    /// Interaction logic for QuanLyNguoiDungvaPhanQuyenForm.xaml
    /// </summary>
    public partial class QuanLyNguoiDungvaPhanQuyenForm : UserControl
    {
        public QuanLyNguoiDungvaPhanQuyenForm()
        {
            InitializeComponent();
        }

        private void btnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            var themPhanQuyenWindow = new ThemPhanQuyenForm();
            themPhanQuyenWindow.ShowDialog();
        }
    }
}
