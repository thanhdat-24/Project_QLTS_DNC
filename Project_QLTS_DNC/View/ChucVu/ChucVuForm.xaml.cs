using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.ViewModels.NhanVien;
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


namespace Project_QLTS_DNC.View.ChucVu
{
    /// <summary>
    /// Interaction logic for ChucVuForm.xaml
    /// </summary>
    public partial class ChucVuForm : UserControl
    {
        public ChucVuForm()
        {
            InitializeComponent();
            DataContext = new DanhSachChucVuViewModel();
        }

        private async void btnThemChucVu_Click(object sender, RoutedEventArgs e)
        {
            var client = await SupabaseService.GetClientAsync();  
            var themChucVuWindow = new ThemChucVuForm(client);  
            themChucVuWindow.ShowDialog();
        }


        private void btnSua_Click(object sender, RoutedEventArgs e) 
        {
            if (dvDanhsachchucvu.SelectedItem != null)
            {
                ChucVuModel chucVuUpdate = (ChucVuModel)dvDanhsachchucvu.SelectedItem;

                
                ThemChucVuForm formSua = new ThemChucVuForm(chucVuUpdate);
                formSua.ShowDialog();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn chức vụ để sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            var DanhSachChucVuViewModel = (DanhSachChucVuViewModel)DataContext;
            if (DanhSachChucVuViewModel != null)
            {
                DanhSachChucVuViewModel.LoadChucVuAsync();
            }
            else
            {
                MessageBox.Show("Không thể tải lại danh sách chức vụ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private async void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtTimKiem.Text;

            
            if (string.IsNullOrWhiteSpace(keyword))
            {
                MessageBox.Show("Vui lòng nhập từ khóa tìm kiếm.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            
            if (DataContext is DanhSachChucVuViewModel viewModel)
            {
                await viewModel.TimKiemChucVuAsync(keyword);
            }
            else
            {
                MessageBox.Show("Không thể thực hiện tìm kiếm.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    var chucVu = button?.DataContext as ChucVuModel;

                    if (chucVu == null)
                    {
                        MessageBox.Show("Không tìm thấy chức vụ.");
                        return;
                    }

                    var results = await ((DanhSachChucVuViewModel)DataContext).XoaChucVuAsync(chucVu.MaChucVu);

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

        private void txtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnRefresh_Click(sender, e);
        }
    }
}
