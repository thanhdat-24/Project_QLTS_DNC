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

            // Kiểm tra theo từng trường
            bool matchesSearchText = false;

            // Kiểm tra mã kiểm kê
            if (item.MaKiemKeTS.ToString().ToLower().Contains(keyword))
                matchesSearchText = true;
            // Kiểm tra mã tài sản
            else if (item.MaTaiSan.HasValue && item.MaTaiSan.ToString().ToLower().Contains(keyword))
                matchesSearchText = true;
            // Kiểm tra mã đợt kiểm kê
            else if (item.MaDotKiemKe.HasValue && item.MaDotKiemKe.ToString().ToLower().Contains(keyword))
                matchesSearchText = true;
            // Kiểm tra tên tài sản (bao gồm cả số seri)
            else if (item.TenTaiSan != null && item.TenTaiSan.ToLower().Contains(keyword))
                matchesSearchText = true;
            // Kiểm tra tên phòng
            else if (item.TenPhong != null && item.TenPhong.ToLower().Contains(keyword))
                matchesSearchText = true;
            // Kiểm tra tên đợt kiểm kê
            else if (item.TenDotKiemKe != null && item.TenDotKiemKe.ToLower().Contains(keyword))
                matchesSearchText = true;
            // Kiểm tra vị trí thực tế
            else if (item.ViTriThucTe.ToString().Contains(keyword))
                matchesSearchText = true;
            // Kiểm tra tình trạng
            else if (item.TinhTrang != null && item.TinhTrang.ToLower().Contains(keyword))
                matchesSearchText = true;
            // Kiểm tra ghi chú
            else if (item.GhiChu != null && item.GhiChu.ToLower().Contains(keyword))
                matchesSearchText = true;
            // Kiểm tra tên nhóm tài sản
            else if (item.TenNhomTS != null && item.TenNhomTS.ToLower().Contains(keyword))
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

        // Xử lý khi nhấn nút In phiếu
        private async void btnInPhieu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra nếu có tài sản nào được chọn không
                var selectedItems = _viewModel.DsKiemKe.Where(x => x.IsSelected).ToList();
                if (selectedItems.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn ít nhất một tài sản để in phiếu!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Tạo PrintDialog
                System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();
                // Hiển thị PrintDialog
                if (printDialog.ShowDialog() == true)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {
                        // Tạo một FlowDocument cho phiếu
                        System.Windows.Documents.FlowDocument document = new System.Windows.Documents.FlowDocument();
                        // Thiết lập thuộc tính trang
                        document.PagePadding = new System.Windows.Thickness(40);
                        document.ColumnWidth = 650; // Điều chỉnh độ rộng phù hợp với giấy A4
                        document.PageHeight = 29.7 * 96 / 2.54; // A4 height in pixels
                        document.PageWidth = 21.0 * 96 / 2.54;  // A4 width in pixels
                        document.FontFamily = new System.Windows.Media.FontFamily("Times New Roman");
                        document.FontSize = 12;
                        document.TextAlignment = System.Windows.TextAlignment.Left;

                        // Thêm nội dung phiếu với danh sách tài sản được chọn
                        await ThemNoiDungPhieuHopNhatAsync(document, selectedItems);

                        // In phiếu
                        printDialog.PrintDocument(((System.Windows.Documents.IDocumentPaginatorSource)document).DocumentPaginator,
                            "In phiếu bảo trì tài sản");
                        MessageBox.Show($"Đã in phiếu bảo trì cho {selectedItems.Count} tài sản thành công!", "Thông báo",
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
                MessageBox.Show($"Lỗi khi in phiếu bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Phương thức tạo nội dung phiếu hợp nhất với nhiều tài sản
        private async Task ThemNoiDungPhieuHopNhatAsync(System.Windows.Documents.FlowDocument document, List<KiemKeTaiSan> danhSachTaiSan)
        {
            // Tạo section cho phiếu
            System.Windows.Documents.Section section = new System.Windows.Documents.Section();

            // Tạo tiêu đề phiếu bảo trì với thiết kế đẹp hơn
            System.Windows.Documents.Paragraph titlePara = new System.Windows.Documents.Paragraph();
            titlePara.TextAlignment = System.Windows.TextAlignment.Center;
            titlePara.Margin = new System.Windows.Thickness(0, 20, 0, 5);
            System.Windows.Documents.Run titleRun = new System.Windows.Documents.Run("PHIẾU DANH SÁCH TÀI SẢN CẦN BẢO TRÌ");
            titleRun.FontSize = 18;
            titleRun.FontWeight = System.Windows.FontWeights.Bold;
            titlePara.Inlines.Add(titleRun);
            section.Blocks.Add(titlePara);

            // Thêm đường kẻ ngang dưới tiêu đề
            System.Windows.Documents.Paragraph linePara = new System.Windows.Documents.Paragraph();
            linePara.Margin = new System.Windows.Thickness(0, 0, 0, 15);
            System.Windows.Documents.Run lineRun = new System.Windows.Documents.Run("_________________________________________________");
            lineRun.FontWeight = System.Windows.FontWeights.Bold;
            linePara.TextAlignment = System.Windows.TextAlignment.Center;
            linePara.Inlines.Add(lineRun);
            section.Blocks.Add(linePara);

            // Thông tin chung về phiếu với định dạng đẹp hơn
            System.Windows.Documents.Paragraph infoPara = new System.Windows.Documents.Paragraph();
            infoPara.Margin = new System.Windows.Thickness(0, 0, 0, 15);

            // Thêm ngày lập phiếu với định dạng in đậm cho nhãn
            System.Windows.Documents.Run dateLabel = new System.Windows.Documents.Run("Ngày lập phiếu: ");
            dateLabel.FontWeight = System.Windows.FontWeights.Bold;
            infoPara.Inlines.Add(dateLabel);
            infoPara.Inlines.Add(new System.Windows.Documents.Run($"{DateTime.Now:dd/MM/yyyy}"));

            infoPara.Inlines.Add(new System.Windows.Documents.LineBreak());

            // Thêm tổng số tài sản với định dạng in đậm cho nhãn
            System.Windows.Documents.Run countLabel = new System.Windows.Documents.Run("Tổng số tài sản: ");
            countLabel.FontWeight = System.Windows.FontWeights.Bold;
            infoPara.Inlines.Add(countLabel);
            infoPara.Inlines.Add(new System.Windows.Documents.Run($"{danhSachTaiSan.Count}"));

            section.Blocks.Add(infoPara);

            // Tạo bảng thông tin tài sản với thiết kế đẹp hơn
            System.Windows.Documents.Table assetTable = new System.Windows.Documents.Table();
            assetTable.CellSpacing = 0;
            assetTable.BorderBrush = System.Windows.Media.Brushes.Black;
            assetTable.BorderThickness = new System.Windows.Thickness(1);

            // Định nghĩa các cột với độ rộng cụ thể - điều chỉnh để cân đối hơn
            assetTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(30) }); // STT
            assetTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(60) }); // Mã TS
            assetTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(140) }); // Tên TS - mở rộng cột này
            assetTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(90) }); // Phòng
            assetTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(80) }); // Nhóm TS
            assetTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(80) }); // Tình trạng
            assetTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(90) }); // Ghi chú

            // Khởi tạo RowGroup
            assetTable.RowGroups.Add(new System.Windows.Documents.TableRowGroup());

            // Thêm hàng tiêu đề với màu nền đậm hơn
            System.Windows.Documents.TableRow headerRow = new System.Windows.Documents.TableRow();
            headerRow.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200)); // Màu xám đậm hơn
            headerRow.FontWeight = System.Windows.FontWeights.Bold;

            // Các ô tiêu đề
            string[] headers = new string[] { "STT", "Mã TS", "Tên tài sản", "Phòng", "Nhóm TS", "Tình trạng", "Ghi chú" };
            foreach (string header in headers)
            {
                System.Windows.Documents.TableCell cell = new System.Windows.Documents.TableCell();
                cell.BorderBrush = System.Windows.Media.Brushes.Black;
                cell.BorderThickness = new System.Windows.Thickness(1);
                cell.Padding = new System.Windows.Thickness(5);
                cell.TextAlignment = System.Windows.TextAlignment.Center;

                System.Windows.Documents.Paragraph para = new System.Windows.Documents.Paragraph();
                para.Inlines.Add(new System.Windows.Documents.Run(header));
                cell.Blocks.Add(para);
                headerRow.Cells.Add(cell);
            }
            assetTable.RowGroups[0].Rows.Add(headerRow);

            // Lấy thông tin chi tiết cho mỗi tài sản và thêm vào bảng với định dạng xen kẽ
            var client = await SupabaseService.GetClientAsync();
            for (int i = 0; i < danhSachTaiSan.Count; i++)
            {
                var taiSan = danhSachTaiSan[i];

                // Tạo hàng mới với màu nền xen kẽ để dễ đọc
                System.Windows.Documents.TableRow row = new System.Windows.Documents.TableRow();
                if (i % 2 == 1)
                {
                    row.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 240, 240)); // Màu xám nhạt
                }

                // STT
                AddCellToRow(row, (i + 1).ToString(), System.Windows.TextAlignment.Center);

                // Mã tài sản
                AddCellToRow(row, taiSan.MaTaiSan?.ToString() ?? "N/A", System.Windows.TextAlignment.Center);

                // Tên tài sản
                AddCellToRow(row, taiSan.TenTaiSan ?? "Không xác định", System.Windows.TextAlignment.Left);

                // Phòng
                AddCellToRow(row, taiSan.TenPhong ?? "Không xác định", System.Windows.TextAlignment.Left);

                // Nhóm tài sản
                AddCellToRow(row, taiSan.TenNhomTS ?? "Không xác định", System.Windows.TextAlignment.Left);

                // Tình trạng
                AddCellToRow(row, taiSan.TinhTrang ?? "Không xác định", System.Windows.TextAlignment.Center);

                // Ghi chú
                AddCellToRow(row, taiSan.GhiChu ?? "", System.Windows.TextAlignment.Left);

                // Thêm hàng vào bảng
                assetTable.RowGroups[0].Rows.Add(row);
            }

            // Thêm bảng vào section
            section.Blocks.Add(assetTable);

            // Tạo khu vực nội dung công việc bảo trì với định dạng đẹp hơn
            System.Windows.Documents.Paragraph taskHeaderPara = new System.Windows.Documents.Paragraph();
            taskHeaderPara.Margin = new System.Windows.Thickness(0, 25, 0, 10);
            System.Windows.Documents.Run taskHeaderRun = new System.Windows.Documents.Run("NỘI DUNG CÔNG VIỆC BẢO TRÌ:");
            taskHeaderRun.FontWeight = System.Windows.FontWeights.Bold;
            taskHeaderRun.FontSize = 14;
            taskHeaderPara.Inlines.Add(taskHeaderRun);
            section.Blocks.Add(taskHeaderPara);

            // Tạo bảng cho nội dung công việc
            System.Windows.Documents.Table taskTable = new System.Windows.Documents.Table();
            taskTable.CellSpacing = 0;
            taskTable.BorderBrush = System.Windows.Media.Brushes.Black;
            taskTable.BorderThickness = new System.Windows.Thickness(1);
            taskTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(570) });
            taskTable.RowGroups.Add(new System.Windows.Documents.TableRowGroup());

            // Tạo 10 dòng trống để điền thông tin
            for (int i = 0; i < 10; i++)
            {
                System.Windows.Documents.TableRow taskRow = new System.Windows.Documents.TableRow();
                System.Windows.Documents.TableCell taskCell = new System.Windows.Documents.TableCell();
                taskCell.BorderBrush = System.Windows.Media.Brushes.Black;
                taskCell.BorderThickness = new System.Windows.Thickness(1);
                taskCell.Padding = new System.Windows.Thickness(5);

                // Tạo paragraph trống với chiều cao cố định
                System.Windows.Documents.Paragraph taskPara = new System.Windows.Documents.Paragraph();
                taskPara.Margin = new System.Windows.Thickness(0, 5, 0, 5);
                taskPara.Inlines.Add(new System.Windows.Documents.Run(" "));
                taskCell.Blocks.Add(taskPara);

                taskRow.Cells.Add(taskCell);
                taskTable.RowGroups[0].Rows.Add(taskRow);
            }
            section.Blocks.Add(taskTable);

            // Tạo khu vực ghi chú với định dạng đẹp hơn
            System.Windows.Documents.Paragraph noteHeaderPara = new System.Windows.Documents.Paragraph();
            noteHeaderPara.Margin = new System.Windows.Thickness(0, 25, 0, 10);
            System.Windows.Documents.Run noteHeaderRun = new System.Windows.Documents.Run("GHI CHÚ:");
            noteHeaderRun.FontWeight = System.Windows.FontWeights.Bold;
            noteHeaderRun.FontSize = 14;
            noteHeaderPara.Inlines.Add(noteHeaderRun);
            section.Blocks.Add(noteHeaderPara);

            // Tạo bảng cho ghi chú
            System.Windows.Documents.Table noteTable = new System.Windows.Documents.Table();
            noteTable.CellSpacing = 0;
            noteTable.BorderBrush = System.Windows.Media.Brushes.Black;
            noteTable.BorderThickness = new System.Windows.Thickness(1);
            noteTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(570) });
            noteTable.RowGroups.Add(new System.Windows.Documents.TableRowGroup());

            // Tạo 5 dòng trống cho ghi chú
            for (int i = 0; i < 5; i++)
            {
                System.Windows.Documents.TableRow noteRow = new System.Windows.Documents.TableRow();
                System.Windows.Documents.TableCell noteCell = new System.Windows.Documents.TableCell();
                noteCell.BorderBrush = System.Windows.Media.Brushes.Black;
                noteCell.BorderThickness = new System.Windows.Thickness(1);
                noteCell.Padding = new System.Windows.Thickness(5);

                // Tạo paragraph trống với chiều cao cố định
                System.Windows.Documents.Paragraph notePara = new System.Windows.Documents.Paragraph();
                notePara.Margin = new System.Windows.Thickness(0, 5, 0, 5);
                notePara.Inlines.Add(new System.Windows.Documents.Run(" "));
                noteCell.Blocks.Add(notePara);

                noteRow.Cells.Add(noteCell);
                noteTable.RowGroups[0].Rows.Add(noteRow);
            }
            section.Blocks.Add(noteTable);

            // Tạo bảng chữ ký - ĐÃ LOẠI BỎ NGƯỜI KIỂM TRA, chỉ còn 2 cột
            System.Windows.Documents.Table signatureTable = new System.Windows.Documents.Table();
            signatureTable.CellSpacing = 0;
            signatureTable.BorderThickness = new System.Windows.Thickness(0);
            signatureTable.Margin = new System.Windows.Thickness(0, 40, 0, 20);
            signatureTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            signatureTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            signatureTable.RowGroups.Add(new System.Windows.Documents.TableRowGroup());

            // Tạo dòng chữ ký
            System.Windows.Documents.TableRow signatureRow = new System.Windows.Documents.TableRow();

            // Người phụ trách
            AddSignatureCell(signatureRow, "Người phụ trách");

            // Người lập phiếu
            AddSignatureCell(signatureRow, "Người lập phiếu");

            signatureTable.RowGroups[0].Rows.Add(signatureRow);
            section.Blocks.Add(signatureTable);

            // Tạo footer đẹp hơn
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

        // Phương thức thêm ô vào hàng với căn chỉnh và định dạng đẹp hơn
        private void AddCellToRow(System.Windows.Documents.TableRow row, string text, System.Windows.TextAlignment alignment)
        {
            System.Windows.Documents.TableCell cell = new System.Windows.Documents.TableCell();
            cell.BorderBrush = System.Windows.Media.Brushes.Black;
            cell.BorderThickness = new System.Windows.Thickness(1);
            cell.Padding = new System.Windows.Thickness(5);

            System.Windows.Documents.Paragraph para = new System.Windows.Documents.Paragraph();
            para.TextAlignment = alignment;
            para.Inlines.Add(new System.Windows.Documents.Run(text));
            cell.Blocks.Add(para);

            row.Cells.Add(cell);
        }

        // Phương thức thêm ô chữ ký với định dạng đẹp hơn
        private void AddSignatureCell(System.Windows.Documents.TableRow row, string title)
        {
            System.Windows.Documents.TableCell cell = new System.Windows.Documents.TableCell();
            cell.BorderThickness = new System.Windows.Thickness(0);

            // Tiêu đề
            System.Windows.Documents.Paragraph titlePara = new System.Windows.Documents.Paragraph();
            titlePara.TextAlignment = System.Windows.TextAlignment.Center;
            titlePara.Margin = new System.Windows.Thickness(0, 0, 0, 60); // Tăng khoảng cách cho chữ ký
            System.Windows.Documents.Run titleRun = new System.Windows.Documents.Run(title);
            titleRun.FontWeight = System.Windows.FontWeights.Bold;
            titlePara.Inlines.Add(titleRun);
            cell.Blocks.Add(titlePara);

            // Chỗ ký
            System.Windows.Documents.Paragraph signPara = new System.Windows.Documents.Paragraph();
            signPara.TextAlignment = System.Windows.TextAlignment.Center;
            System.Windows.Documents.Run signRun = new System.Windows.Documents.Run("(Ký và ghi rõ họ tên)");
            signRun.FontStyle = System.Windows.FontStyles.Italic;
            signRun.FontSize = 10;
            signPara.Inlines.Add(signRun);
            cell.Blocks.Add(signPara);

            row.Cells.Add(cell);
        }


        // Phương thức lấy danh sách tài sản được chọn
        private List<KiemKeTaiSan> GetSelectedItems()
        {
            return _viewModel.DsKiemKe.Where(x => x.IsSelected).ToList();
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
            worksheet.Range("A1:I1").Merge();

            // Thêm ngày xuất báo cáo
            worksheet.Cell("A2").Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
            worksheet.Range("A2:I2").Merge();
            worksheet.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Cell("A2").Style.Font.Italic = true;

            // Thêm header dựa trên DataGrid
            var headerRow = 4;
            worksheet.Cell(headerRow, 1).Value = "STT";
            worksheet.Cell(headerRow, 2).Value = "Mã kiểm kê";
            worksheet.Cell(headerRow, 3).Value = "Đợt kiểm kê";
            worksheet.Cell(headerRow, 4).Value = "Tên tài sản";
            worksheet.Cell(headerRow, 5).Value = "Phòng";
            worksheet.Cell(headerRow, 6).Value = "Nhóm tài sản";
            worksheet.Cell(headerRow, 7).Value = "Tình trạng";
            worksheet.Cell(headerRow, 8).Value = "Vị trí";
            worksheet.Cell(headerRow, 9).Value = "Ghi chú";

            // Định dạng header
            var headerRange = worksheet.Range(headerRow, 1, headerRow, 9);
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
                worksheet.Cell(row, 3).Value = item.TenDotKiemKe ?? $"Đợt {item.MaDotKiemKe?.ToString() ?? "N/A"}";
                worksheet.Cell(row, 4).Value = item.TenTaiSan ?? $"Tài sản {item.MaTaiSan}"; // Tên tài sản kèm số seri
                worksheet.Cell(row, 5).Value = item.TenPhong ?? $"Phòng {item.MaPhong}"; // Tên phòng
                worksheet.Cell(row, 6).Value = item.TenNhomTS ?? "Chưa xác định"; // Tên nhóm tài sản
                worksheet.Cell(row, 7).Value = item.TinhTrang ?? "Chưa xác định";
                worksheet.Cell(row, 8).Value = item.ViTriThucTe.ToString();
                worksheet.Cell(row, 9).Value = item.GhiChu ?? "";
                row++;
            }

            // Canh lề và định dạng dữ liệu
            var dataRange = worksheet.Range(headerRow + 1, 1, row - 1, 9);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Căn giữa cho một số cột
            worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // STT
            worksheet.Column(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Mã kiểm kê
            worksheet.Column(3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Đợt kiểm kê
            worksheet.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Tình trạng

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