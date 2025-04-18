using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.ViewModel.Baotri;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    public partial class DanhSachBaoTriUserControl : UserControl
    {
        private DanhSachBaoTriViewModel _viewModel;
        private List<Button> _pageButtons;

        public DanhSachBaoTriUserControl()
        {
            InitializeComponent();

            // Khởi tạo ViewModel và gán làm DataContext
            _viewModel = new DanhSachBaoTriViewModel(false); // Không tự động tải dữ liệu
            this.DataContext = _viewModel;

          

            // Tải dữ liệu khi control được khởi tạo
            this.Loaded += DanhSachBaoTriUserControl_Loaded;
        }

        private void DanhSachBaoTriUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Tải dữ liệu khi control được tải
            _ = _viewModel.LoadDSKiemKeAsync();
        }

        

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.TrangHienTai) || e.PropertyName == nameof(_viewModel.TongSoTrang))
            {
                UpdatePaginationButtons();
            }
        }

        private void UpdatePaginationButtons()
        {
            // Cập nhật hiển thị của các nút phân trang
            int currentPage = _viewModel.TrangHienTai;
            int totalPages = _viewModel.TongSoTrang;

            // Xác định các trang cần hiển thị
            int startPage = Math.Max(1, currentPage - 1);
            int endPage = Math.Min(startPage + 2, totalPages);

            // Đảm bảo luôn hiển thị 3 nút nếu có đủ trang
            if (endPage - startPage < 2 && totalPages >= 3)
            {
                if (startPage == 1)
                    endPage = Math.Min(3, totalPages);
                else
                    startPage = Math.Max(1, endPage - 2);
            }

            // Cập nhật nội dung và trạng thái của các nút
            for (int i = 0; i < _pageButtons.Count; i++)
            {
                int pageNumber = startPage + i;
                Button button = _pageButtons[i];

                if (pageNumber <= totalPages)
                {
                    button.Content = pageNumber.ToString();
                    button.Tag = pageNumber;
                    button.Visibility = Visibility.Visible;

                    // Đánh dấu nút của trang hiện tại
                    if (pageNumber == currentPage)
                    {
                        button.Background = FindResource("PrimaryHueMidBrush") as SolidColorBrush;
                        button.Foreground = Brushes.White;
                    }
                    else
                    {
                        button.Background = Brushes.Transparent;
                        button.Foreground = Brushes.Black;
                        button.BorderBrush = new SolidColorBrush(Color.FromRgb(204, 204, 204));
                    }
                }
                else
                {
                    // Ẩn các nút thừa
                    button.Visibility = Visibility.Collapsed;
                }
            }

       
        }

        private void BtnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            // Cập nhật từ khóa tìm kiếm
            _viewModel.TuKhoaTimKiem = txtTimKiem.Text.Trim();

            // Áp dụng bộ lọc và tải lại dữ liệu
            _viewModel.ApplyFilter();
        }

        private void TxtTimKiem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Tìm kiếm khi nhấn Enter
                BtnTimKiem_Click(sender, e);
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                // Xử lý thay đổi theo loại ComboBox - đã được binding trong XAML
                if (comboBox == cboLoaiBaoTri)
                {
                    // ViewModel đã xử lý binding trực tiếp, không cần xử lý thêm
                    _viewModel.ApplyFilter();
                }
                else if (comboBox == cboNhomTaiSan)
                {
                    // ViewModel đã xử lý binding trực tiếp, không cần xử lý thêm
                    _viewModel.ApplyFilter();
                }
                else if (comboBox == cboTinhTrang)
                {
                    // ViewModel đã xử lý binding trực tiếp, không cần xử lý thêm
                    _viewModel.ApplyFilter();
                }
            }
        }

        private void ChkSelectAll_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // Cập nhật trạng thái chọn tất cả
            _viewModel.TatCaDuocChon = chkSelectAll.IsChecked ?? false;
        }

        private void PageButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int pageNumber)
            {
                _viewModel.ChuyenDenTrang(pageNumber);
            }
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Tạo dialog lưu file
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    Title = "Lưu file Excel",
                    FileName = "DanhSachTaiSanCanBaoTri.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // Gọi phương thức xuất Excel
                    XuatDanhSachTaiSanRaExcel(saveDialog.FileName);

                    MessageBox.Show("Xuất Excel thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất Excel: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void XuatDanhSachTaiSanRaExcel(string filePath)
        {
            // Thêm logic xuất Excel tại đây
            // Có thể sử dụng thư viện như EPPlus hoặc NPOI

            // Giả lập việc xuất file
            File.WriteAllText(filePath, "Nội dung xuất Excel");
        }

        private void BtnTaoPhieuBaoTri_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = _viewModel.GetSelectedItems();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một tài sản để tạo phiếu bảo trì!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // TODO: Mở form tạo phiếu bảo trì với các tài sản đã chọn
            MessageBox.Show($"Đã chọn {selectedItems.Count} tài sản để tạo phiếu bảo trì.",
                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnDong_Click(object sender, RoutedEventArgs e)
        {
            // Đóng form hoặc quay lại form trước đó
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.Close();
            }
        }
    }
}