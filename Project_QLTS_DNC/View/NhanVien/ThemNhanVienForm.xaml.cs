﻿using Project_QLTS_DNC.ViewModels.NhanVien;
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

namespace Project_QLTS_DNC.View.NhanVien
{
    /// <summary>
    /// Interaction logic for ThemNhanVienForm.xaml
    /// </summary>
    public partial class ThemNhanVienForm : Window
    {
        public ThemNhanVienForm()
        {
            InitializeComponent();
            DataContext = new ThemNhanVienViewModel();
        }
    }
}
