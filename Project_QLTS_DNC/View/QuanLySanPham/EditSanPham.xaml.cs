using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Model;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Services.QLTaiSanService;
using Project_QLTS_DNC.Models.QLTaiSan;
using static Project_QLTS_DNC.Model.SanPham;

namespace Project_QLTS_DNC.View.QuanLySanPham
{
    public partial class EditSanPham : Window
    {
        private DanhSachSanPham _parentWindow;
        private List<PhongFilter> _phongList;
        private TaiSanModel _taiSan;

        public EditSanPham(DanhSachSanPham parentWindow, TaiSanModel taiSan, List<PhongFilter> phongList)
        {
            InitializeComponent();
            _parentWindow = parentWindow;
            _taiSan = taiSan; // No conversion needed
            _phongList = phongList;

            LoadTaiSanData();
        }
        private void LoadTaiSanData()
        {

        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnCapNhat_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UpdateTaiSan()
        {

        }

        private async void UpdateTaiSanInDatabase(TaiSanModel taiSan)
        {

        }
    }
}
