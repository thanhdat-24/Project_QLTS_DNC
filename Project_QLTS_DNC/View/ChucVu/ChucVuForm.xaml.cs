using Project_QLTS_DNC.Helpers;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Services.ChucVu;
using Project_QLTS_DNC.ViewModels.NhanVien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Project_QLTS_DNC.View.ChucVu
{
    public partial class ChucVuForm : UserControl
    {
        public ChucVuForm()
        {
            InitializeComponent();
            this.DataContext = new DanhSachChucVuViewModel();
            Loaded += async (s, e) => await LoadDataGirdChucVu(); // Lúc này có thể gọi LoadDataGirdChucVu trực tiếp trong constructor
        }

        private async void btnThemChucVu_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnChucVu", "them"))
            {
                MessageBox.Show("Bạn không có quyền thêm chức vụ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var client = await SupabaseService.GetClientAsync();
            var themChucVuWindow = new ThemChucVuForm(this);
            themChucVuWindow.ShowDialog();
        }

        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnChucVu", "sua"))
            {
                MessageBox.Show("Bạn không có quyền sửa chức vụ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (dvDanhsachchucvu.SelectedItem != null)
            {
                ChucVuModel chucVuUpdate = (ChucVuModel)dvDanhsachchucvu.SelectedItem;
                ThemChucVuForm formSua = new ThemChucVuForm(chucVuUpdate, this);
                formSua.ShowDialog();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn chức vụ để sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {


            if (DataContext is DanhSachChucVuViewModel viewModel)
            {
                await viewModel.Refresh();
                txtTimKiem.Text = string.Empty; 

                LoadDataGirdChucVu();

                
            }
        }


        public async Task LoadDataGirdChucVu()
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnChucVu", "xem"))
            {
                MessageBox.Show("Bạn không có quyền xem chức vụ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var chucVuService = new ChucVuService();
            var danhSachChucVu = await chucVuService.GetAllChucVuAsync();
            dvDanhsachchucvu.ItemsSource = danhSachChucVu;
        }

        

        private async void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!QuyenNguoiDungHelper.HasPermission("btnChucVu", "xoa"))
                {
                    MessageBox.Show("Bạn không có quyền xóa chức vụ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                MessageBoxResult tb = MessageBox.Show("Bạn có chắc chắn muốn xóa không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (tb == MessageBoxResult.Yes)
                {
                    var button = sender as Button;
                    var chucVu = button?.DataContext as ChucVuModel;

                    if (chucVu == null)
                    {
                        MessageBox.Show("Không tìm thấy chức vụ.");
                        return;
                    }

                    var results = await ((DanhSachChucVuViewModel)DataContext).XoaChucVuAsync(chucVu.MaChucVu);
                    if (results)
                    {
                        MessageBox.Show("Chức vụ đã được xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    await LoadDataGirdChucVu();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private async void txtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtTimKiem.Text.Trim();
            if (DataContext is DanhSachChucVuViewModel viewModel)
            {
                var ketQua = await viewModel.TimKiemChucVuAsync(keyword);
                dvDanhsachchucvu.ItemsSource = ketQua;
            }
        }

    }
}
