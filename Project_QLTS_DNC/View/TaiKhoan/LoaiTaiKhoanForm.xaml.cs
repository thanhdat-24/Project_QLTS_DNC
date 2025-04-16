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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DocumentFormat.OpenXml.Bibliography;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Services.TaiKhoan;
using Project_QLTS_DNC.View.ChucVu;
using Project_QLTS_DNC.ViewModels.TaiKhoan;

namespace Project_QLTS_DNC.View.TaiKhoan
{
    /// <summary>
    /// Interaction logic for LoaiTaiKhoanForm.xaml
    /// </summary>
    public partial class LoaiTaiKhoanForm : UserControl
    {
        private readonly LoaiTaiKhoanService _loaiTaiKhoanService = new();
        public LoaiTaiKhoanForm()
        {
            InitializeComponent();
            DataContext = new LoaiTaiKhoanViewModel();
            _ = LoadDanhSachLoaiTaiKhoan();
        }



        private async Task LoadDanhSachLoaiTaiKhoan()
        {
            List<LoaiTaiKhoanModel> danhSach = await _loaiTaiKhoanService.LayDSLoaiTK();
            dgLoaiTaiKhoan.ItemsSource = danhSach;
        }

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            var themLoaiTaiKhoanWindow = new ThemLoaiTaiKhoanForm();
            themLoaiTaiKhoanWindow.ShowDialog();
        }

        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            if (dgLoaiTaiKhoan.SelectedItem != null)
            {
                LoaiTaiKhoanModel loaiTaiKhoanUpdate = (LoaiTaiKhoanModel)dgLoaiTaiKhoan.SelectedItem;


                ThemLoaiTaiKhoanForm formSua = new ThemLoaiTaiKhoanForm(loaiTaiKhoanUpdate);
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

                    var results = await((LoaiTaiKhoanViewModel)DataContext).XoaLoaiTaiKhoanAsync(loaiTK.MaLoaiTk);
                    await LoadDanhSachLoaiTaiKhoan();

                    if (results)
                        MessageBox.Show("Xóa thành công.");
                    else
                        MessageBox.Show("Xóa thất bại.");

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
                await LoadDanhSachLoaiTaiKhoan();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
    }
}
