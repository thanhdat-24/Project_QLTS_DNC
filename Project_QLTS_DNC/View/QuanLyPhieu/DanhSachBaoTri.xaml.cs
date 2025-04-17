using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.ViewModel.Baotri;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    public partial class DanhSachBaoTri : UserControl
    {
        private DanhSachBaoTriViewModel _viewModel;

        public DanhSachBaoTri()
        {
            

            // Khởi tạo ViewModel và gán làm DataContext
            _viewModel = new DanhSachBaoTriViewModel();
            this.DataContext = _viewModel;

           
        }

        
    }
}