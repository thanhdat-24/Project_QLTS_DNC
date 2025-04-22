using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;
using System.Windows.Data;
using System.Windows.Data;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.ViewModel.Baotri;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ClosedXML.Excel;
using Project_QLTS_DNC.Services;
using KiemKeTaiSanModel = Project_QLTS_DNC.Models.KiemKe.KiemKeTaiSanModel;


namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    public partial class DanhSachBaoTriUserControl : UserControl
    {
        private DanhSachBaoTriViewModel _viewModel;
        private List<Button> _pageButtons;
        private ICollectionView collectionView;
        private void RegisterFilterEvents()
        {
            // Đăng ký sự kiện SelectionChanged cho các ComboBox
            cboNhomTaiSan.SelectionChanged += Filter_SelectionChanged;
            cboTinhTrang.SelectionChanged += Filter_SelectionChanged;
        }

        // Cập nhật phương thức InitializeComponent hoặc constructor để đăng ký sự kiện
        public DanhSachBaoTriUserControl()
        {
            InitializeComponent();

            // Khởi tạo ViewModel và gán làm DataContext
            _viewModel = new DanhSachBaoTriViewModel(false); // Không tự động tải dữ liệu
            this.DataContext = _viewModel;

            // Đăng ký sự kiện cho nút xuất Excel
            btnXuatExcel.Click += BtnXuatExcel_Click;

            // Đăng ký sự kiện cho checkbox chọn tất cả
            chkSelectAll.Checked += ChkSelectAll_CheckedChanged;
            chkSelectAll.Unchecked += ChkSelectAll_CheckedChanged;

            // Đăng ký sự kiện cho nút tạo phiếu bảo trì
            btnTaoPhieuBaoTri.Click += BtnTaoPhieuBaoTri_Click;

            // Đăng ký sự kiện cho ComboBox lọc
            RegisterFilterEvents();

            // Đăng ký sự kiện cho TextBox tìm kiếm
            txtTimKiem.TextChanged += TxtTimKiem_TextChanged;
            btnTimKiem.Click += BtnTimKiem_Click;

            // Tải dữ liệu khi control được khởi tạo
            this.Loaded += DanhSachBaoTriUserControl_Loaded;
        }
        // Cập nhật phương thức LoadNhomTaiSanAsync
        private async Task LoadNhomTaiSanAsync()
        {
            try
            {
                // Tạo instance của service
                var nhomTaiSanService = new NhomTaiSanService();

                // Lấy danh sách nhóm tài sản từ database
                var dsNhom = await nhomTaiSanService.GetNhomTaiSanAsync();

                // Xóa các item cũ trong ComboBox
                cboNhomTaiSan.Items.Clear();

                // Thêm item mặc định "Tất cả nhóm"
                ComboBoxItem defaultItem = new ComboBoxItem();
                defaultItem.Content = "Tất cả nhóm";
                defaultItem.IsSelected = true;
                defaultItem.Tag = null; // Không có mã nhóm
                cboNhomTaiSan.Items.Add(defaultItem);

                // Thêm các nhóm tài sản từ database
                foreach (var nhom in dsNhom)
                {
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = nhom.TenNhom;
                    item.Tag = nhom.MaNhomTS; // Lưu mã nhóm để sử dụng khi lọc
                    cboNhomTaiSan.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách nhóm tài sản: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void DanhSachBaoTriUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Tải danh sách nhóm tài sản
                await LoadNhomTaiSanAsync();

                // Tải dữ liệu khi control được tải
                await _viewModel.LoadDSKiemKeAsync();

                // Cập nhật UI phân trang
                UpdatePaginationUI();

                // Đăng ký sự kiện PropertyChanged
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;

                // Thiết lập CollectionView và bộ lọc
                InitializeCollectionView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeCollectionView()
        {
            // Khởi tạo CollectionView từ nguồn dữ liệu
            if (_viewModel.DsKiemKe != null)
            {
                collectionView = CollectionViewSource.GetDefaultView(_viewModel.DsKiemKe);
                // Thiết lập filter
                collectionView.Filter = item => FilterMatches(item as KiemKeTaiSan);
                // Gán CollectionView cho DataGrid
                dgDanhSachTaiSan.ItemsSource = collectionView;
            }
        }

        private bool FilterMatches(KiemKeTaiSan item)
        {
            if (item == null)
                return false;

            // Chuẩn bị từ khóa tìm kiếm, chuẩn hóa để so sánh dễ dàng hơn
            string keyword = _viewModel.TuKhoaTimKiem?.Trim().ToLower() ?? "";

            // Nếu không có từ khóa, luôn trả về true cho điều kiện tìm kiếm
            if (string.IsNullOrEmpty(keyword))
                return true;

            // Kiểm tra theo từng trường, sử dụng ToLower() thay vì StringComparison.OrdinalIgnoreCase
            bool matchesSearchText = false;

            // Kiểm tra mã kiểm kê
            if (item.MaKiemKeTS != null && item.MaKiemKeTS.ToString().ToLower().Contains(keyword))
                matchesSearchText = true;
            // Kiểm tra mã tài sản
            else if (item.MaTaiSan.HasValue && item.MaTaiSan.ToString().ToLower().Contains(keyword))
                matchesSearchText = true;
            // Kiểm tra mã đợt kiểm kê
            else if (item.MaDotKiemKe.HasValue && item.MaDotKiemKe.ToString().ToLower().Contains(keyword))
                matchesSearchText = true;
            // Kiểm tra tên tài sản
            else if (item.TenTaiSan != null && item.TenTaiSan.ToLower().Contains(keyword))
                matchesSearchText = true;
            // Kiểm tra tên phòng
            else if (item.TenPhong != null && item.TenPhong.ToLower().Contains(keyword))
                matchesSearchText = true;
            // Kiểm tra tên đợt kiểm kê
            else if (item.TenDotKiemKe != null && item.TenDotKiemKe.ToLower().Contains(keyword))
                matchesSearchText = true;
            // Kiểm tra vị trí thực tế
            else if (item.ViTriThucTe != null && item.ViTriThucTe.ToLower().Contains(keyword))
                matchesSearchText = true;
            // Kiểm tra tình trạng
            else if (item.TinhTrang != null && item.TinhTrang.ToLower().Contains(keyword))
                matchesSearchText = true;
            // Kiểm tra ghi chú
            else if (item.GhiChu != null && item.GhiChu.ToLower().Contains(keyword))
                matchesSearchText = true;

            return matchesSearchText;
        }

        private void UpdatePaginationUI()
        {
            // Cập nhật thông tin trang hiện tại và tổng số trang
            txtCurrentPage.Text = _viewModel.TrangHienTai.ToString();
            txtTotalPages.Text = _viewModel.TongSoTrang.ToString();
            // Cập nhật trạng thái của các nút điều hướng
            btnFirstPage.IsEnabled = _viewModel.TrangHienTai > 1;
            btnPrevPage.IsEnabled = _viewModel.TrangHienTai > 1;
            btnNextPage.IsEnabled = _viewModel.TrangHienTai < _viewModel.TongSoTrang;
            btnLastPage.IsEnabled = _viewModel.TrangHienTai < _viewModel.TongSoTrang;

            // Đảm bảo các nút luôn hiển thị
            btnFirstPage.Visibility = Visibility.Visible;
            btnPrevPage.Visibility = Visibility.Visible;
            btnNextPage.Visibility = Visibility.Visible;
            btnLastPage.Visibility = Visibility.Visible;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.TrangHienTai) || e.PropertyName == nameof(_viewModel.TongSoTrang))
            {
                UpdatePaginationUI();
            }
            else if (e.PropertyName == nameof(_viewModel.DsKiemKe))
            {
                // Nếu danh sách thay đổi, cập nhật lại CollectionView
                InitializeCollectionView();
            }
        }

        private void btnFirstPage_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ChuyenDenTrangDau();
        }

        private void btnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ChuyenTrangTruoc();
        }

        private void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ChuyenTrangSau();
        }

        private void btnLastPage_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ChuyenDenTrangCuoi();
        }

        private void cboPageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboPageSize.SelectedItem is ComboBoxItem item && _viewModel != null)
            {
                if (int.TryParse(item.Content.ToString(), out int pageSize))
                {
                    _viewModel.SoDongMoiTrang = pageSize;
                    _viewModel.ChuyenDenTrangDau(); // Reset về trang đầu tiên khi đổi số dòng mỗi trang
                }
            }
        }

        private void TxtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Cập nhật từ khóa tìm kiếm trong ViewModel
            _viewModel.TuKhoaTimKiem = txtTimKiem.Text.Trim();

            // Nếu có CollectionView, cập nhật lại filter
            if (collectionView != null)
            {
                collectionView.Refresh();
            }
        }

        private void BtnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            // Cập nhật từ khóa tìm kiếm
            _viewModel.TuKhoaTimKiem = txtTimKiem.Text.Trim();

            // Nếu có CollectionView, cập nhật lại filter
            if (collectionView != null)
            {
                collectionView.Refresh();
            }
            else
            {
                // Sử dụng phương thức cũ nếu CollectionView chưa được thiết lập
                _viewModel.ApplyFilter();
            }
        }

        private void TxtTimKiem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Tìm kiếm khi nhấn Enter
                BtnTimKiem_Click(sender, e);
            }
        }

        // Cập nhật phương thức Filter_SelectionChanged với cách lọc mới
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                if (comboBox == cboNhomTaiSan)
                {
                    // Lấy item được chọn từ ComboBox
                    var selectedItem = comboBox.SelectedItem as ComboBoxItem;
                    if (selectedItem != null)
                    {
                        // Lưu tên nhóm tài sản đã chọn vào ViewModel
                        _viewModel.NhomTaiSanDuocChon = selectedItem.Content.ToString();

                        // Debug: In ra thông tin để theo dõi
                        System.Diagnostics.Debug.WriteLine($"Đã chọn nhóm tài sản: {_viewModel.NhomTaiSanDuocChon}");
                    }
                }
                else if (comboBox == cboTinhTrang)
                {
                    // Lấy item được chọn từ ComboBox tình trạng
                    var selectedItem = comboBox.SelectedItem as ComboBoxItem;
                    if (selectedItem != null)
                    {
                        _viewModel.TinhTrangDuocChon = selectedItem.Content.ToString();

                        // Debug: In ra thông tin để theo dõi
                        System.Diagnostics.Debug.WriteLine($"Đã chọn tình trạng: {_viewModel.TinhTrangDuocChon}");
                    }
                }

                // Nếu có CollectionView, cập nhật lại filter
                if (collectionView != null)
                {
                    collectionView.Refresh();
                }
                else
                {
                    // Sử dụng phương thức cũ nếu CollectionView chưa được thiết lập
                    _viewModel.ApplyFilter();
                }
            }
        }
        private void ChkSelectAll_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // Kiểm tra nếu sender là CheckBox
            if (sender is CheckBox checkBox)
            {
                // Cập nhật trạng thái chọn tất cả
                _viewModel.TatCaDuocChon = checkBox.IsChecked ?? false;

                // Cập nhật các dòng trong datagrid
                if (_viewModel.DsKiemKe != null)
                {
                    foreach (var item in _viewModel.DsKiemKe)
                    {
                        item.IsSelected = _viewModel.TatCaDuocChon;
                    }
                }
            }
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
                // Kiểm tra nếu không có dữ liệu
                if (_viewModel.DsKiemKe == null || _viewModel.DsKiemKe.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất Excel!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                // Hiển thị hộp thoại lưu file
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    Title = "Lưu file Excel",
                    FileName = $"DanhSachTaiSanCanBaoTri_{DateTime.Now:yyyyMMdd}.xlsx"
                };
                if (saveDialog.ShowDialog() == true)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {
                        // Xuất Excel với tất cả dữ liệu đã lọc
                        XuatDanhSachTaiSanRaExcel(saveDialog.FileName);
                        MessageBox.Show("Xuất Excel thành công!", "Thông báo",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show($"Lỗi khi xuất Excel: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void XuatDanhSachTaiSanRaExcel(string filePath)
        {
            try
            {
                // Sử dụng thư viện ClosedXML để xuất Excel
                using (var workbook = new XLWorkbook())
                {
                    // Tạo worksheet
                    var worksheet = workbook.Worksheets.Add("Danh sách bảo trì");

                    // Định dạng tiêu đề
                    worksheet.Cell("A1").Value = "DANH SÁCH TÀI SẢN CẦN BẢO TRÌ";
                    worksheet.Cell("A1").Style.Font.Bold = true;
                    worksheet.Cell("A1").Style.Font.FontSize = 16;
                    worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Range("A1:H1").Merge();

                    // Thêm ngày xuất báo cáo
                    worksheet.Cell("A2").Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
                    worksheet.Range("A2:H2").Merge();
                    worksheet.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    worksheet.Cell("A2").Style.Font.Italic = true;

                    // Thêm header dựa trên DataGrid
                    var headerRow = 4;
                    worksheet.Cell(headerRow, 1).Value = "STT";
                    worksheet.Cell(headerRow, 2).Value = "Mã kiểm kê";
                    worksheet.Cell(headerRow, 3).Value = "Đợt kiểm kê";
                    worksheet.Cell(headerRow, 4).Value = "Tên tài sản";
                    worksheet.Cell(headerRow, 5).Value = "Phòng";
                    worksheet.Cell(headerRow, 6).Value = "Tình trạng";
                    worksheet.Cell(headerRow, 7).Value = "Vị trí";
                    worksheet.Cell(headerRow, 8).Value = "Ghi chú";

                    // Định dạng header
                    var headerRange = worksheet.Range(headerRow, 1, headerRow, 8);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Lấy dữ liệu đã lọc
                    var filteredData = _viewModel.DsKiemKe.ToList();

                    // Điền dữ liệu
                    int row = headerRow + 1;
                    for (int i = 0; i < filteredData.Count; i++)
                    {
                        var item = filteredData[i];

                        worksheet.Cell(row, 1).Value = i + 1; // STT
                        worksheet.Cell(row, 2).Value = item.MaKiemKeTS;
                        worksheet.Cell(row, 3).Value = item.MaDotKiemKe?.ToString() ?? "Chưa xác định";
                        worksheet.Cell(row, 4).Value = item.TenTaiSan ?? $"Tài sản {item.MaTaiSan}"; // Hiển thị tên thay vì mã
                        worksheet.Cell(row, 5).Value = item.TenPhong ?? $"Phòng {item.MaPhong}"; // Hiển thị tên thay vì mã
                        worksheet.Cell(row, 6).Value = item.TinhTrang ?? "Chưa xác định";
                        worksheet.Cell(row, 7).Value = item.ViTriThucTe ?? "";
                        worksheet.Cell(row, 8).Value = item.GhiChu ?? "";

                        row++;
                    }

                    // Canh lề và định dạng dữ liệu
                    var dataRange = worksheet.Range(headerRow + 1, 1, row - 1, 8);
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Căn giữa cho một số cột
                    worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // STT
                    worksheet.Column(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Mã kiểm kê
                    worksheet.Column(3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Đợt kiểm kê
                    worksheet.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Tình trạng

                    // Tự động điều chỉnh độ rộng cột
                    worksheet.Columns().AdjustToContents();

                    // Thêm chân trang
                    int footerRow = row + 2;
                    worksheet.Cell(footerRow, 1).Value = "Tổng số tài sản:";
                    worksheet.Cell(footerRow, 2).Value = filteredData.Count.ToString();
                    worksheet.Cell(footerRow, 1).Style.Font.Bold = true;

                    // Lưu file
                    workbook.SaveAs(filePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất Excel: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                throw; // Rethrow để caller biết lỗi
            }
        }
        private async void BtnTaoPhieuBaoTri_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Lấy Supabase client
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    MessageBox.Show("Không thể kết nối đến cơ sở dữ liệu!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Mở form tạo phiếu bảo trì mới (không có tài sản được chọn)
                var formTaoPhieu = new DSBaoTriInputForm(client);
                formTaoPhieu.Owner = Window.GetWindow(this);
                var result = formTaoPhieu.ShowDialog();

                // Nếu người dùng đã lưu thành công, tải lại dữ liệu
                if (result == true)
                {
                    await _viewModel.LoadDSKiemKeAsync();
                    MessageBox.Show("Đã tạo phiếu bảo trì thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở form tạo phiếu bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void BtnSua_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Lấy item được chọn từ CommandParameter
                if (sender is Button button && button.CommandParameter is int maKiemKeTS)
                {
                    // Tìm item trong danh sách
                    var item = _viewModel.DsKiemKe.FirstOrDefault(x => x.MaKiemKeTS == maKiemKeTS);
                    if (item == null)
                    {
                        MessageBox.Show("Không tìm thấy thông tin tài sản!", "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Lấy client Supabase
                    var client = await SupabaseService.GetClientAsync();
                    if (client == null)
                    {
                        MessageBox.Show("Không thể kết nối đến cơ sở dữ liệu!", "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Mở form sửa
                    var formSua = new DSBaoTriInputForm(client, item);
                    formSua.Owner = Window.GetWindow(this);
                    var result = formSua.ShowDialog();

                    // Nếu lưu thành công, tải lại dữ liệu
                    if (result == true)
                    {
                        await _viewModel.LoadDSKiemKeAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở form sửa: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                // Gọi phương thức xóa và chờ kết quả
                bool xoaThanhCong = await _viewModel.XoaTaiSanDaChonAsync();

                // Nếu xóa thành công, không cần tải lại dữ liệu (đã làm trong XoaTaiSanDaChonAsync)

                Mouse.OverrideCursor = null;
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show(
                    $"Lỗi khi xóa tài sản: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

    }
}