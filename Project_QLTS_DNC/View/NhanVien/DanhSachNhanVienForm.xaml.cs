using Project_QLTS_DNC.ViewModel;
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
using Project_QLTS_DNC.ViewModels.NhanVien;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Services.NhanVien;

namespace Project_QLTS_DNC.View.NhanVien
{
    /// <summary>
    /// Interaction logic for DanhSachNhanVienForm.xaml
    /// </summary>
    public partial class DanhSachNhanVienForm : UserControl
    {
        private readonly NhanVienService _nhanVienService = new();

        public DanhSachNhanVienForm()
        {
            InitializeComponent();
            _ = LoadDanhSachNhanVienAsync(); // Gọi hàm async trong constructor
        }

        private async Task LoadDanhSachNhanVienAsync()
        {
            List<NhanVienModel> danhSach = await _nhanVienService.GetNhanVienList();
            employeeDataGrid.ItemsSource = danhSach;
        }

        

        private void btnThemNhanVien_Click(object sender, RoutedEventArgs e)
        {
            var themNhanVienWindow = new ThemNhanVienForm();
            themNhanVienWindow.Show();
        }
    }

}
