﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DocumentFormat.OpenXml.Bibliography;
using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.Services.TaiKhoan;
using Project_QLTS_DNC.ViewModels.TaiKhoan;

namespace Project_QLTS_DNC.View.TaiKhoan
{
    public partial class LoaiTaiKhoanForm : UserControl
    {
        private readonly LoaiTaiKhoanService _loaiTaiKhoanService = new();
        private LoaiTaiKhoanViewModel _viewModel;

        public LoaiTaiKhoanForm()
        {
            InitializeComponent();
            _viewModel = new LoaiTaiKhoanViewModel();
            DataContext = _viewModel;
            Loaded += LoaiTaiKhoanForm_Loaded;
        }

        private void LoaiTaiKhoanForm_Loaded(object sender, RoutedEventArgs e)
        {
            _ = LoadDanhSachLoaiTaiKhoan();
        }
        public async Task LoadDanhSachLoaiTaiKhoan()
        {
            List<LoaiTaiKhoanModel> danhSach = await _loaiTaiKhoanService.LayDSLoaiTK();
            dgLoaiTaiKhoan.ItemsSource = danhSach;
        }
        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            var themLoaiTaiKhoanWindow = new ThemLoaiTaiKhoanForm(this);
            themLoaiTaiKhoanWindow.ShowDialog();

           
        }

        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            if (dgLoaiTaiKhoan.SelectedItem != null)
            {
                LoaiTaiKhoanModel loaiTaiKhoanUpdate = (LoaiTaiKhoanModel)dgLoaiTaiKhoan.SelectedItem;
                
                ThemLoaiTaiKhoanForm formSua = new ThemLoaiTaiKhoanForm(loaiTaiKhoanUpdate, this);
                formSua.ShowDialog();
                
            }
            else
            {
                MessageBox.Show("Vui lòng chọn loại tài khoản để sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult tb = MessageBox.Show("Bạn có chắc chắn muốn xóa không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (tb == MessageBoxResult.Yes)
                {
                    var button = sender as Button;
                    var loaiTK = button?.DataContext as LoaiTaiKhoanModel;
                    if (loaiTK == null)
                    {
                        MessageBox.Show("Không tìm thấy loại tài khoản.");
                        return;
                    }

                    var results = await _viewModel.XoaLoaiTaiKhoanAsync(loaiTK.MaLoaiTk);
                    if (results)
                        await LoadDanhSachLoaiTaiKhoan();


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _viewModel.LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
    }
}