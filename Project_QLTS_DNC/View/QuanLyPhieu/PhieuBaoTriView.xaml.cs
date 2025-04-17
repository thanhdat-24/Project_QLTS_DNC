using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.ViewModel.Baotri;
using Project_QLTS_DNC.Views;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    public partial class PhieuBaoTriView : UserControl
    {
        private readonly PhieuBaoTriService _phieuBaoTriService = new();
        private readonly PhieuBaoTriViewModel _viewModel;
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _pageSize = 10;

        public PhieuBaoTriView()
        {
            InitializeComponent();
            _viewModel = new PhieuBaoTriViewModel();
            DataContext = _viewModel;
            Loaded += PhieuBaoTriView_Loaded;
            chkSelectAll.Checked += ChkSelectAll_Checked;
            chkSelectAll.Unchecked += ChkSelectAll_Unchecked;
            // Đăng ký sự kiện cho các nút
            btnIn.Click += btnIn_Click;
        }

        private async void PhieuBaoTriView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDSBaoTriAsync();
        }

        // Phương thức tải danh sách bảo trì
        public async Task LoadDSBaoTriAsync()
        {
            try
            {
                await _viewModel.LoadDSBaoTriAsync();
                Console.WriteLine($"Đã tải {_viewModel.DsBaoTri.Count} phiếu bảo trì từ cơ sở dữ liệu");

                // Áp dụng phân trang
                ApplyPagination();

                // Cập nhật thông tin phân trang
                UpdatePaginationInfo();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi chi tiết khi tải dữ liệu: {ex.Message}");
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Xử lý sự kiện khi ấn nút thêm phiếu bảo trì
        private async void btnThem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Tạo phiếu bảo trì mới với giá trị mặc định
                var phieuMoi = _viewModel.CreateNewPhieuBaoTri();

                // Mở cửa sổ thêm mới
                var addWindow = new Project_QLTS_DNC.Views.EditPhieuBaoTriWindow(phieuMoi);
                addWindow.Title = "Thêm phiếu bảo trì mới";
                addWindow.Owner = Window.GetWindow(this);

                // Hiển thị cửa sổ dưới dạng dialog
                bool? result = addWindow.ShowDialog();

                // Nếu người dùng đã lưu phiếu mới
                if (result == true)
                {
                    // Thêm phiếu bảo trì vào cơ sở dữ liệu
                    bool success = await _viewModel.AddPhieuBaoTriAsync(addWindow.PhieuBaoTri);

                    if (success)
                    {
                        MessageBox.Show("Thêm phiếu bảo trì thành công!", "Thông báo",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        // Tải lại danh sách bảo trì để cập nhật giao diện
                        await LoadDSBaoTriAsync();
                    }
                    else
                    {
                        MessageBox.Show("Không thể thêm phiếu bảo trì. Vui lòng kiểm tra lại dữ liệu!", "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm phiếu bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Xử lý sự kiện khi ấn nút tìm kiếm
        private async void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            string searchText = txtTimKiem.Text?.Trim() ?? "";
            _currentPage = 1;
            await _viewModel.SearchPhieuBaoTriAsync(searchText);
            ApplyPagination();
            UpdatePaginationInfo();
        }

        // Xử lý sự kiện khi nhấn phím trong ô tìm kiếm
        private void txtTimKiem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnTimKiem_Click(sender, e);
            }
        }

        // Xử lý sự kiện khi thay đổi giá trị trong ComboBox bộ lọc
        private async void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (sender is ComboBox comboBox)
            {
                string trangThai = "Tất cả trạng thái";
                string loaiBaoTri = "Tất cả loại";
                if (comboBox.Name == "cboTrangThai")
                {
                    trangThai = (comboBox.SelectedItem as ComboBoxItem)?.Content as string ?? "Tất cả trạng thái";
                    _viewModel.SelectedTrangThai = trangThai;
                    loaiBaoTri = _viewModel.SelectedLoaiBaoTri;
                }
                else if (comboBox.Name == "cboLoaiBaoTri")
                {
                    loaiBaoTri = (comboBox.SelectedItem as ComboBoxItem)?.Content as string ?? "Tất cả loại";
                    _viewModel.SelectedLoaiBaoTri = loaiBaoTri;
                    trangThai = _viewModel.SelectedTrangThai;
                }
                _currentPage = 1;
                // Áp dụng bộ lọc
                await _viewModel.FilterPhieuBaoTriAsync(loaiBaoTri, trangThai);
                // Cập nhật giao diện
                ApplyPagination();
                UpdatePaginationInfo();
            }
        }

        // Áp dụng phân trang cho danh sách
        private void ApplyPagination()
        {
            if (_viewModel.DsBaoTri == null) return;
            // Tính tổng số trang
            int totalItems = _viewModel.DsBaoTri.Count;
            _totalPages = Math.Max(1, (int)Math.Ceiling((double)totalItems / _pageSize));
            if (_currentPage > _totalPages)
            {
                _currentPage = _totalPages;
            }
            else if (_currentPage < 1)
            {
                _currentPage = 1;
            }
            // Lấy dữ liệu cho trang hiện tại
            var pagedData = _viewModel.DsBaoTri
                .Skip((_currentPage - 1) * _pageSize)
                .Take(_pageSize)
                .ToList();
            // Cập nhật DataGrid
            dgPhieuBaoTri.ItemsSource = pagedData;
        }

        // Cập nhật thông tin phân trang
        private void UpdatePaginationInfo()
        {
            txtCurrentPage.Text = _currentPage.ToString();
            txtTotalPages.Text = _totalPages.ToString();
        }

        // Xử lý các nút điều hướng phân trang
        private void btnFirstPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage = 1;
                ApplyPagination();
                UpdatePaginationInfo();
            }
        }

        private void btnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                ApplyPagination();
                UpdatePaginationInfo();
            }
        }

        private void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                ApplyPagination();
                UpdatePaginationInfo();
            }
        }

        private void btnLastPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage = _totalPages;
                ApplyPagination();
                UpdatePaginationInfo();
            }
        }

        // Xử lý sự kiện khi thay đổi kích thước trang
        private void cboPageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || !(sender is ComboBox comboBox)) return;
            string selectedValue = (comboBox.SelectedItem as ComboBoxItem)?.Content as string;
            if (!string.IsNullOrEmpty(selectedValue) && int.TryParse(selectedValue, out int newPageSize))
            {
                _pageSize = newPageSize;
                _currentPage = 1;
                ApplyPagination();
                UpdatePaginationInfo();
            }
        }

        // Xử lý nút chỉnh sửa
        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is PhieuBaoTri phieu)
            {
                try
                {
                    // Tạo bản sao của đối tượng phiếu để tránh thay đổi đối tượng gốc
                    var phieuCopy = _viewModel.ClonePhieuBaoTri(phieu);

                    // Sử dụng bản sao để mở cửa sổ chỉnh sửa
                    Project_QLTS_DNC.Views.EditPhieuBaoTriWindow editWindow = new Project_QLTS_DNC.Views.EditPhieuBaoTriWindow(phieuCopy);
                    editWindow.Owner = Window.GetWindow(this);

                    // Hiển thị cửa sổ dưới dạng dialog
                    bool? result = editWindow.ShowDialog();

                    // Nếu người dùng đã lưu thay đổi (nhấn nút Save)
                    if (result == true)
                    {
                        // Thêm logging để debug
                        Console.WriteLine($"Cập nhật phiếu: {editWindow.PhieuBaoTri.MaBaoTri}, {editWindow.PhieuBaoTri.TrangThai}");

                        // Cập nhật phiếu bảo trì vào cơ sở dữ liệu
                        bool success = await _viewModel.UpdatePhieuBaoTriAsync(editWindow.PhieuBaoTri);

                        if (success)
                        {
                            MessageBox.Show("Cập nhật phiếu bảo trì thành công!", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            // Tải lại danh sách bảo trì để cập nhật giao diện
                            await LoadDSBaoTriAsync();
                        }
                        else
                        {
                            MessageBox.Show("Không thể cập nhật phiếu bảo trì! Kiểm tra log để biết thêm chi tiết.", "Lỗi",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi mở cửa sổ chỉnh sửa: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Xử lý khi nhấn nút xóa
        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is PhieuBaoTri phieu)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa phiếu bảo trì có mã {phieu.MaBaoTri}?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = await _viewModel.DeletePhieuBaoTriAsync(phieu.MaBaoTri);

                        if (success)
                        {
                            MessageBox.Show("Xóa phiếu bảo trì thành công!", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            // Cập nhật giao diện
                            ApplyPagination();
                            UpdatePaginationInfo();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa phiếu bảo trì: {ex.Message}", "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // Xử lý sự kiện khi checkbox "Chọn tất cả" được check
        private void ChkSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgPhieuBaoTri.ItemsSource is IEnumerable<PhieuBaoTri> items)
            {
                foreach (var item in items)
                {
                    item.IsSelected = true;
                }
                dgPhieuBaoTri.Items.Refresh();
            }
        }

        // Xử lý sự kiện khi checkbox "Chọn tất cả" được uncheck
        private void ChkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgPhieuBaoTri.ItemsSource is IEnumerable<PhieuBaoTri> items)
            {
                foreach (var item in items)
                {
                    item.IsSelected = false;
                }
                dgPhieuBaoTri.Items.Refresh();
            }
        }

        // Xử lý khi nhấn nút xuất Excel
        private void btnIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Lấy danh sách dữ liệu hiện tại (có thể là dữ liệu đã lọc)
                var danhSachXuat = new List<PhieuBaoTri>();

                // Nếu có dữ liệu được chọn thì xuất các phiếu đã chọn
                List<PhieuBaoTri> selectedPhieu = GetSelectedPhieuBaoTri();
                if (selectedPhieu.Count > 0)
                {
                    danhSachXuat = selectedPhieu;
                }
                // Nếu không có phiếu nào được chọn, xuất toàn bộ danh sách hiện tại
                else if (dgPhieuBaoTri.ItemsSource is IEnumerable<PhieuBaoTri> items)
                {
                    danhSachXuat = items.ToList();
                }

                if (danhSachXuat.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất Excel!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Hiển thị dialog chọn vị trí lưu file
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    DefaultExt = "xlsx",
                    FileName = $"DanhSachPhieuBaoTri_{DateTime.Now:yyyyMMdd}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Gọi phương thức xuất Excel từ service
                    _phieuBaoTriService.ExportToExcel(danhSachXuat, saveFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất Excel: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Xử lý khi nhấn nút In phiếu
        private void btnInPhieu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra xem có phiếu nào được chọn không
                List<PhieuBaoTri> selectedPhieu = GetSelectedPhieuBaoTri();
                if (selectedPhieu.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn ít nhất một phiếu để in!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Tạo PrintDialog
                System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();

                // Hiển thị PrintDialog
                if (printDialog.ShowDialog() == true)
                {
                    // Tạo một FlowDocument duy nhất chứa tất cả các phiếu đã chọn
                    System.Windows.Documents.FlowDocument document = new System.Windows.Documents.FlowDocument();

                    // Thiết lập thuộc tính trang
                    document.PagePadding = new System.Windows.Thickness(40);
                    document.ColumnWidth = 650; // Điều chỉnh độ rộng phù hợp với giấy A4
                    document.PageHeight = 29.7 * 96 / 2.54; // A4 height in pixels
                    document.PageWidth = 21.0 * 96 / 2.54;  // A4 width in pixels
                    document.FontFamily = new System.Windows.Media.FontFamily("Times New Roman");
                    document.FontSize = 12;
                    document.TextAlignment = System.Windows.TextAlignment.Left;

                    // Thêm từng phiếu vào document
                    for (int i = 0; i < selectedPhieu.Count; i++)
                    {
                        // Thêm nội dung phiếu hiện tại
                        ThemNoiDungPhieu(document, selectedPhieu[i]);

                        // Thêm page break nếu không phải phiếu cuối cùng
                        if (i < selectedPhieu.Count - 1)
                        {
                            System.Windows.Documents.Section section = new System.Windows.Documents.Section();
                            section.BreakPageBefore = true;
                            document.Blocks.Add(section);
                        }
                    }

                    // In tất cả các phiếu trong một lần
                    printDialog.PrintDocument(((System.Windows.Documents.IDocumentPaginatorSource)document).DocumentPaginator,
                        "In phiếu bảo trì");

                    MessageBox.Show($"Đã in {selectedPhieu.Count} phiếu bảo trì thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in phiếu bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Phương thức thêm nội dung phiếu vào FlowDocument
        private void ThemNoiDungPhieu(System.Windows.Documents.FlowDocument document, PhieuBaoTri phieu)
        {
            // Tạo section cho phiếu
            System.Windows.Documents.Section section = new System.Windows.Documents.Section();

            // Tạo header - logo và tiêu đề công ty
            System.Windows.Documents.Paragraph headerPara = new System.Windows.Documents.Paragraph();
            headerPara.Margin = new System.Windows.Thickness(0, 0, 0, 10);

            // Thêm dòng Công ty
            System.Windows.Documents.Run companyRun = new System.Windows.Documents.Run("CÔNG TY CỔ PHẦN XYZ");
            companyRun.FontWeight = System.Windows.FontWeights.Bold;
            companyRun.FontSize = 14;
            headerPara.Inlines.Add(companyRun);
            headerPara.Inlines.Add(new System.Windows.Documents.LineBreak());

            // Thêm dòng địa chỉ
            System.Windows.Documents.Run addressRun = new System.Windows.Documents.Run("Địa chỉ: 123 Đường ABC, Quận XYZ, TP. Hồ Chí Minh");
            addressRun.FontSize = 11;
            headerPara.Inlines.Add(addressRun);
            section.Blocks.Add(headerPara);

            // Tạo tiêu đề phiếu bảo trì
            System.Windows.Documents.Paragraph titlePara = new System.Windows.Documents.Paragraph();
            titlePara.TextAlignment = System.Windows.TextAlignment.Center;
            titlePara.Margin = new System.Windows.Thickness(0, 20, 0, 20);

            System.Windows.Documents.Run titleRun = new System.Windows.Documents.Run("PHIẾU BẢO TRÌ TÀI SẢN");
            titleRun.FontSize = 18;
            titleRun.FontWeight = System.Windows.FontWeights.Bold;
            titlePara.Inlines.Add(titleRun);
            section.Blocks.Add(titlePara);

            // Tạo bảng thông tin phiếu
            System.Windows.Documents.Table infoTable = new System.Windows.Documents.Table();
            infoTable.CellSpacing = 0;
            infoTable.BorderBrush = System.Windows.Media.Brushes.Black;
            infoTable.BorderThickness = new System.Windows.Thickness(1);

            // Định nghĩa cột với độ rộng cụ thể
            infoTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(200) });
            infoTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(400) });

            // Khởi tạo RowGroup
            infoTable.RowGroups.Add(new System.Windows.Documents.TableRowGroup());

            // Thêm hàng thông tin với định dạng đồng nhất
            AddTableRow(infoTable, "Mã phiếu bảo trì:", phieu.MaBaoTri.ToString());
            AddTableRow(infoTable, "Mã tài sản:", phieu.MaTaiSan?.ToString() ?? "N/A");

            // Chuyển đổi mã loại bảo trì thành tên
            string loaiBaoTri = "Không xác định";
            switch (phieu.MaLoaiBaoTri)
            {
                case 1: loaiBaoTri = "Định kỳ"; break;
                case 2: loaiBaoTri = "Đột xuất"; break;
                case 3: loaiBaoTri = "Bảo hành"; break;
            }

            AddTableRow(infoTable, "Loại bảo trì:", loaiBaoTri);
            AddTableRow(infoTable, "Ngày bảo trì:", phieu.NgayBaoTri?.ToString("dd/MM/yyyy") ?? "N/A");
            AddTableRow(infoTable, "Người phụ trách:", phieu.MaNV?.ToString() ?? "N/A");
            AddTableRow(infoTable, "Trạng thái:", phieu.TrangThai);
            AddTableRow(infoTable, "Chi phí:", string.Format("{0:N0} VNĐ", phieu.ChiPhi));

            section.Blocks.Add(infoTable);

            // Tạo nội dung bảo trì
            System.Windows.Documents.Paragraph contentHeaderPara = new System.Windows.Documents.Paragraph();
            contentHeaderPara.Margin = new System.Windows.Thickness(0, 20, 0, 10);

            System.Windows.Documents.Run contentHeaderRun = new System.Windows.Documents.Run("NỘI DUNG BẢO TRÌ:");
            contentHeaderRun.FontWeight = System.Windows.FontWeights.Bold;
            contentHeaderPara.Inlines.Add(contentHeaderRun);
            section.Blocks.Add(contentHeaderPara);

            // Tạo border cho nội dung
            System.Windows.Documents.Table contentTable = new System.Windows.Documents.Table();
            contentTable.CellSpacing = 0;
            contentTable.BorderBrush = System.Windows.Media.Brushes.Black;
            contentTable.BorderThickness = new System.Windows.Thickness(1);
            contentTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(600) });
            contentTable.RowGroups.Add(new System.Windows.Documents.TableRowGroup());

            System.Windows.Documents.TableRow contentRow = new System.Windows.Documents.TableRow();
            System.Windows.Documents.TableCell contentCell = new System.Windows.Documents.TableCell();
            contentCell.Padding = new System.Windows.Thickness(8);

            System.Windows.Documents.Paragraph contentPara = new System.Windows.Documents.Paragraph();
            contentPara.TextAlignment = System.Windows.TextAlignment.Justify;

            System.Windows.Documents.Run contentRun = new System.Windows.Documents.Run(phieu.NoiDung ?? "");
            contentPara.Inlines.Add(contentRun);
            contentCell.Blocks.Add(contentPara);
            contentRow.Cells.Add(contentCell);
            contentTable.RowGroups[0].Rows.Add(contentRow);
            section.Blocks.Add(contentTable);

            // Thêm ghi chú nếu có
            if (!string.IsNullOrEmpty(phieu.GhiChu))
            {
                System.Windows.Documents.Paragraph noteHeaderPara = new System.Windows.Documents.Paragraph();
                noteHeaderPara.Margin = new System.Windows.Thickness(0, 20, 0, 10);

                System.Windows.Documents.Run noteHeaderRun = new System.Windows.Documents.Run("GHI CHÚ:");
                noteHeaderRun.FontWeight = System.Windows.FontWeights.Bold;
                noteHeaderPara.Inlines.Add(noteHeaderRun);
                section.Blocks.Add(noteHeaderPara);

                // Tạo border cho ghi chú
                System.Windows.Documents.Table noteTable = new System.Windows.Documents.Table();
                noteTable.CellSpacing = 0;
                noteTable.BorderBrush = System.Windows.Media.Brushes.Black;
                noteTable.BorderThickness = new System.Windows.Thickness(1);
                noteTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(600) });
                noteTable.RowGroups.Add(new System.Windows.Documents.TableRowGroup());

                System.Windows.Documents.TableRow noteRow = new System.Windows.Documents.TableRow();
                System.Windows.Documents.TableCell noteCell = new System.Windows.Documents.TableCell();
                noteCell.Padding = new System.Windows.Thickness(8);

                System.Windows.Documents.Paragraph notePara = new System.Windows.Documents.Paragraph();
                notePara.TextAlignment = System.Windows.TextAlignment.Justify;

                System.Windows.Documents.Run noteRun = new System.Windows.Documents.Run(phieu.GhiChu);
                notePara.Inlines.Add(noteRun);
                noteCell.Blocks.Add(notePara);
                noteRow.Cells.Add(noteCell);
                noteTable.RowGroups[0].Rows.Add(noteRow);
                section.Blocks.Add(noteTable);
            }

            // Tạo bảng chữ ký
            System.Windows.Documents.Table signatureTable = new System.Windows.Documents.Table();
            signatureTable.CellSpacing = 0;
            signatureTable.BorderThickness = new System.Windows.Thickness(0);
            signatureTable.Margin = new System.Windows.Thickness(0, 40, 0, 20);

            signatureTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            signatureTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            signatureTable.RowGroups.Add(new System.Windows.Documents.TableRowGroup());

            // Tạo dòng chữ ký
            System.Windows.Documents.TableRow signatureRow = new System.Windows.Documents.TableRow();

            // Cột người lập phiếu
            System.Windows.Documents.TableCell leftCell = new System.Windows.Documents.TableCell();
            leftCell.BorderThickness = new System.Windows.Thickness(0);

            System.Windows.Documents.Paragraph leftHeaderPara = new System.Windows.Documents.Paragraph();
            leftHeaderPara.TextAlignment = System.Windows.TextAlignment.Center;
            leftHeaderPara.Margin = new System.Windows.Thickness(0, 0, 0, 50); // Khoảng cách cho chữ ký

            System.Windows.Documents.Run leftHeaderRun = new System.Windows.Documents.Run("Người lập phiếu");
            leftHeaderRun.FontWeight = System.Windows.FontWeights.Bold;
            leftHeaderPara.Inlines.Add(leftHeaderRun);
            leftCell.Blocks.Add(leftHeaderPara);

            // Thêm chỗ để ký tên
            System.Windows.Documents.Paragraph leftSignPara = new System.Windows.Documents.Paragraph();
            leftSignPara.TextAlignment = System.Windows.TextAlignment.Center;

            System.Windows.Documents.Run leftSignRun = new System.Windows.Documents.Run("(Ký và ghi rõ họ tên)");
            leftSignRun.FontStyle = System.Windows.FontStyles.Italic;
            leftSignRun.FontSize = 10;
            leftSignPara.Inlines.Add(leftSignRun);
            leftCell.Blocks.Add(leftSignPara);
            signatureRow.Cells.Add(leftCell);

            // Cột người phụ trách
            System.Windows.Documents.TableCell rightCell = new System.Windows.Documents.TableCell();
            rightCell.BorderThickness = new System.Windows.Thickness(0);

            System.Windows.Documents.Paragraph rightHeaderPara = new System.Windows.Documents.Paragraph();
            rightHeaderPara.TextAlignment = System.Windows.TextAlignment.Center;
            rightHeaderPara.Margin = new System.Windows.Thickness(0, 0, 0, 50); // Khoảng cách cho chữ ký

            System.Windows.Documents.Run rightHeaderRun = new System.Windows.Documents.Run("Người phụ trách");
            rightHeaderRun.FontWeight = System.Windows.FontWeights.Bold;
            rightHeaderPara.Inlines.Add(rightHeaderRun);
            rightCell.Blocks.Add(rightHeaderPara);

            // Thêm chỗ để ký tên
            System.Windows.Documents.Paragraph rightSignPara = new System.Windows.Documents.Paragraph();
            rightSignPara.TextAlignment = System.Windows.TextAlignment.Center;

            System.Windows.Documents.Run rightSignRun = new System.Windows.Documents.Run("(Ký và ghi rõ họ tên)");
            rightSignRun.FontStyle = System.Windows.FontStyles.Italic;
            rightSignRun.FontSize = 10;
            rightSignPara.Inlines.Add(rightSignRun);
            rightCell.Blocks.Add(rightSignPara);
            signatureRow.Cells.Add(rightCell);
            signatureTable.RowGroups[0].Rows.Add(signatureRow);
            section.Blocks.Add(signatureTable);

            // Tạo footer
            System.Windows.Documents.Paragraph footerPara = new System.Windows.Documents.Paragraph();
            footerPara.TextAlignment = System.Windows.TextAlignment.Right;
            footerPara.Margin = new System.Windows.Thickness(0, 20, 0, 0);

            System.Windows.Documents.Run footerRun = new System.Windows.Documents.Run("Ngày in: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            footerRun.FontStyle = System.Windows.FontStyles.Italic;
            footerRun.FontSize = 10;
            footerPara.Inlines.Add(footerRun);
            section.Blocks.Add(footerPara);

            // Thêm section vào document
            document.Blocks.Add(section);
        }

        // Phương thức thêm hàng vào bảng thông tin
        private void AddTableRow(System.Windows.Documents.Table table, string label, string value)
        {
            System.Windows.Documents.TableRow row = new System.Windows.Documents.TableRow();
            row.Background = System.Windows.Media.Brushes.White;

            // Cột nhãn
            System.Windows.Documents.TableCell labelCell = new System.Windows.Documents.TableCell();
            labelCell.BorderBrush = System.Windows.Media.Brushes.Black;
            labelCell.BorderThickness = new System.Windows.Thickness(1);
            labelCell.Background = System.Windows.Media.Brushes.LightGray;
            labelCell.Padding = new System.Windows.Thickness(8);

            System.Windows.Documents.Paragraph labelPara = new System.Windows.Documents.Paragraph();
            System.Windows.Documents.Run labelRun = new System.Windows.Documents.Run(label);
            labelRun.FontWeight = System.Windows.FontWeights.Bold;
            labelPara.Inlines.Add(labelRun);
            labelCell.Blocks.Add(labelPara);
            row.Cells.Add(labelCell);

            // Cột giá trị
            System.Windows.Documents.TableCell valueCell = new System.Windows.Documents.TableCell();
            valueCell.BorderBrush = System.Windows.Media.Brushes.Black;
            valueCell.BorderThickness = new System.Windows.Thickness(1);
            valueCell.Padding = new System.Windows.Thickness(8);

            System.Windows.Documents.Paragraph valuePara = new System.Windows.Documents.Paragraph();
            System.Windows.Documents.Run valueRun = new System.Windows.Documents.Run(value);
            valuePara.Inlines.Add(valueRun);
            valueCell.Blocks.Add(valuePara);
            row.Cells.Add(valueCell);

            // Thêm hàng vào bảng
            table.RowGroups[0].Rows.Add(row);
        }

        // Lấy danh sách các phiếu bảo trì đã được chọn
        private List<PhieuBaoTri> GetSelectedPhieuBaoTri()
        {
            List<PhieuBaoTri> selectedPhieus = new List<PhieuBaoTri>();

            if (dgPhieuBaoTri.ItemsSource is IEnumerable<PhieuBaoTri> items)
            {
                foreach (var item in items)
                {
                    if (item.IsSelected)
                    {
                        selectedPhieus.Add(item);
                    }
                }
            }

            return selectedPhieus;
        }
    }
}