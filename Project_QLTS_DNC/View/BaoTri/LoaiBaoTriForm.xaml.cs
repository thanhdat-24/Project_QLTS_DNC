using System;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.ViewModel.Baotri;

namespace Project_QLTS_DNC.View.BaoTri
{
    /// <summary>
    /// Interaction logic for LoaiBaoTriForm.xaml
    /// </summary>
    public partial class LoaiBaoTriForm : UserControl
    {
        private readonly LoaiBaoTriViewModel _viewModel;

        public LoaiBaoTriForm()
        {
            InitializeComponent();

            // Khởi tạo ViewModel và gán làm DataContext
            _viewModel = new LoaiBaoTriViewModel();
            this.DataContext = _viewModel;

            // Đăng ký sự kiện cho combobox phân trang
            cboPageSize.SelectionChanged += CboPageSize_SelectionChanged;
        }

        private void CboPageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboPageSize.SelectedItem != null && _viewModel != null)
            {
                ComboBoxItem selected = cboPageSize.SelectedItem as ComboBoxItem;
                if (selected != null && int.TryParse(selected.Content.ToString(), out int pageSize))
                {
                    _viewModel.SoLuongTrenTrang = pageSize;
                }
            }
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.DataContext is LoaiBaoTri loaiBaoTri)
            {
                _viewModel.SuaCommand.Execute(loaiBaoTri);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.DataContext is LoaiBaoTri loaiBaoTri)
            {
                _viewModel.XoaCommand.Execute(loaiBaoTri);
            }
        }
    }
}