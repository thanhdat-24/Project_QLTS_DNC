using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.ViewModel.Baotri;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    public partial class PhieuBaoTriView : UserControl
    {
        private readonly PhieuBaoTriService _phieuBaoTriService = new();
        private readonly PhieuBaoTriViewModel _viewModel;

        public PhieuBaoTriView()
        {
            InitializeComponent();
            _viewModel = new PhieuBaoTriViewModel();
            DataContext = _viewModel;
        }

        // Nếu cần gọi trực tiếp từ View
        public async Task LoadDSBaoTri()
        {
            try
            {
                List<PhieuBaoTri> dsBaoTri = await _phieuBaoTriService.GetPhieuBaoTriAsync();
                if (dgPhieuBaoTri.ItemsSource == null)
                {
                    dgPhieuBaoTri.ItemsSource = dsBaoTri;
                }
                else
                {
                    await _viewModel.LoadDSBaoTriAsync();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}