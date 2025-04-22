using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DocumentFormat.OpenXml.Bibliography;
using Project_QLTS_DNC.Helpers;
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
            if (!QuyenNguoiDungHelper.HasPermission("btnLoaiTaiKhoan", "xem"))
            {
                MessageBox.Show("Bạn không có quyền xem loại tài khoản!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //List<LoaiTaiKhoanModel> danhSach = await _loaiTaiKhoanService.LayDSLoaiTK();
            //dgLoaiTaiKhoan.ItemsSource = danhSach;
            await _viewModel.LoadDataAsync(); // ✅ sử dụng viewmodel

        }
        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnLoaiTaiKhoan", "them"))
            {
                MessageBox.Show("Bạn không có quyền thêm loại tài khoản!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var themLoaiTaiKhoanWindow = new ThemLoaiTaiKhoanForm(this);
            themLoaiTaiKhoanWindow.ShowDialog();

           
        }

        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnLoaiTaiKhoan", "sua"))
            {
                MessageBox.Show("Bạn không có quyền sửa loại tài khoản!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
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
            if (!QuyenNguoiDungHelper.HasPermission("btnLoaiTaiKhoan", "xoa"))
            {
                MessageBox.Show("Bạn không có quyền xóa loại tài khoản!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
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

        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoaiTaiKhoanViewModel vm)
            {
                var keyword = txtTimKiem.Text;
                vm.TuKhoa = keyword; // TuKhoa đã gọi TimKiem() luôn rồi
            }
        }

    }
}