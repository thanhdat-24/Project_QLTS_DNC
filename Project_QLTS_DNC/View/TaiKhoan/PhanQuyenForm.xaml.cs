using Project_QLTS_DNC.Services.TaiKhoan;
using Project_QLTS_DNC.ViewModels.TaiKhoan;
using System.Windows;
using System.Windows.Controls;

namespace Project_QLTS_DNC.View.TaiKhoan
{
    public partial class PhanQuyenForm : UserControl
    {
        private readonly PhanQuyenFormViewModel viewModel;

        public PhanQuyenForm()
        {
            InitializeComponent();
            viewModel = new PhanQuyenFormViewModel(new LoaiTaiKhoanService());
            DataContext = viewModel;

            Loaded += async (_, __) => await viewModel.LoadDataAsync();
        }

        private async void btnLuuPQ_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.LuuThayDoiAsync();
        }
    }
}
