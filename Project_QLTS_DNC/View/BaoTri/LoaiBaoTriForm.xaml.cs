using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.ViewModel.Baotri;
using MaterialDesignThemes.Wpf;
using System.Windows.Media;

namespace Project_QLTS_DNC.View.BaoTri
{
    public partial class LoaiBaoTriForm : UserControl
    {
        private LoaiBaoTriViewModel viewModel;

        public LoaiBaoTriForm()
        {
            InitializeComponent();

            // Khởi tạo ViewModel và gán làm DataContext
            viewModel = new LoaiBaoTriViewModel();
            this.DataContext = viewModel;

            // Thiết lập sự kiện cho ComboBox số bản ghi trên trang
            cboPageSize.SelectionChanged += CboPageSize_SelectionChanged;

            // Đăng ký sự kiện LoadingRow để thêm các event handler cho các nút
            dgLoaiBaoTri.LoadingRow += DgLoaiBaoTri_LoadingRow;
        }

        private void DgLoaiBaoTri_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Loaded += Row_Loaded;
        }

        private void Row_Loaded(object sender, RoutedEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            if (row == null) return;

            LoaiBaoTri item = row.DataContext as LoaiBaoTri;
            if (item == null) return;

            // Tìm nút trong hàng
            Button editButton = FindButtonInRow(row, "Edit");
            Button deleteButton = FindButtonInRow(row, "Delete");

            // Gán sự kiện cho nút sửa
            if (editButton != null)
            {
                editButton.Click -= EditButton_Click; // Tránh đăng ký nhiều lần
                editButton.Click += EditButton_Click;
                editButton.Tag = item; // Lưu đối tượng vào Tag để dùng trong sự kiện
            }

            // Gán sự kiện cho nút xóa
            if (deleteButton != null)
            {
                deleteButton.Click -= DeleteButton_Click; // Tránh đăng ký nhiều lần
                deleteButton.Click += DeleteButton_Click;
                deleteButton.Tag = item; // Lưu đối tượng vào Tag để dùng trong sự kiện
            }
        }

        private Button FindButtonInRow(DataGridRow row, string iconKind)
        {
            // Lấy cột cuối cùng (cột chứa các nút)
            var cellContent = dgLoaiBaoTri.Columns[dgLoaiBaoTri.Columns.Count - 1].GetCellContent(row);
            if (cellContent == null) return null;

            // Tìm StackPanel chứa các nút
            StackPanel panel = FindVisualChild<StackPanel>(cellContent);
            if (panel == null) return null;

            // Tìm nút theo PackIcon.Kind
            foreach (var child in panel.Children)
            {
                if (child is Button button)
                {
                    PackIcon icon = FindVisualChild<PackIcon>(button);
                    if (icon != null && icon.Kind.ToString() == iconKind)
                    {
                        return button;
                    }
                }
            }

            return null;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

            Button button = sender as Button;
            LoaiBaoTri item = button?.Tag as LoaiBaoTri;

            if (item != null && viewModel.SuaCommand.CanExecute(item))
            {
                viewModel.SuaCommand.Execute(item);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            LoaiBaoTri item = button?.Tag as LoaiBaoTri;

            if (item != null && viewModel.XoaCommand.CanExecute(item))
            {
                viewModel.XoaCommand.Execute(item);
            }
        }

        // Helper method để tìm control con trong visual tree
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child != null && child is T result)
                    return result;

                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }

            return null;
        }

        private void CboPageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboPageSize.SelectedItem != null)
            {
                ComboBoxItem selectedItem = cboPageSize.SelectedItem as ComboBoxItem;
                if (selectedItem != null && int.TryParse(selectedItem.Content.ToString(), out int pageSize))
                {
                    viewModel.SoLuongTrenTrang = pageSize;
                }
            }
        }

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {

        }

        // Thêm code xử lý các event khác nếu cần
    }
}