using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.ViewModel.Baotri;
using Project_QLTS_DNC.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;

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

        // Trong PhieuBaoTriView.xaml.cs
        private async void PhieuBaoTriView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("Đang tải dữ liệu...");
                await LoadDSBaoTriAsync();
                Console.WriteLine("Tải dữ liệu hoàn tất");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tải: {ex.Message}");
            }
        }

        // Phương thức tải danh sách bảo trì
        public async Task LoadDSBaoTriAsync()
        {
            try
            {
                await _viewModel.LoadDSBaoTriAsync();
                Console.WriteLine($"Đã tải {_viewModel.DsBaoTri.Count} phiếu bảo trì từ cơ sở dữ liệu");

                // Cập nhật thông tin tài sản
                await LoadTaiSanInfoAsync();

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

        // Phương thức mới để tải thông tin tài sản
        private async Task LoadTaiSanInfoAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client != null && _viewModel.DsBaoTri != null && _viewModel.DsBaoTri.Any())
                {
                    // Lấy danh sách mã tài sản duy nhất
                    var uniqueAssetIds = _viewModel.DsBaoTri
                        .Where(p => p.MaTaiSan.HasValue)
                        .Select(p => p.MaTaiSan.Value)
                        .Distinct()
                        .ToList();

                    if (uniqueAssetIds.Any())
                    {
                        // Lấy thông tin tài sản từ Supabase
                        var assets = await client.From<TaiSanModel>()
                            .Where(t => uniqueAssetIds.Contains(t.MaTaiSan))
                            .Get();

                        var assetList = assets.Models;

                        // Cập nhật thông tin tài sản vào phiếu bảo trì
                        foreach (var phieu in _viewModel.DsBaoTri)
                        {
                            if (phieu.MaTaiSan.HasValue)
                            {
                                var asset = assetList.FirstOrDefault(a => a.MaTaiSan == phieu.MaTaiSan.Value);
                                if (asset != null)
                                {
                                    // Cập nhật định dạng thông tin tài sản: Tên + Số seri + Tình trạng
                                    phieu.TenTaiSan = $"{asset.TenTaiSan} - {asset.SoSeri} - {asset.TinhTrangSP}";
                                }
                                else
                                {
                                    phieu.TenTaiSan = $"TS{phieu.MaTaiSan} (Không tìm thấy)";
                                }
                            }
                            else
                            {
                                phieu.TenTaiSan = "Không có";
                            }
                        }
                    }

                    // Cập nhật thông tin người phụ trách
                    var uniqueStaffIds = _viewModel.DsBaoTri
                        .Where(p => p.MaNV.HasValue)
                        .Select(p => p.MaNV.Value)
                        .Distinct()
                        .ToList();

                    if (uniqueStaffIds.Any())
                    {
                        var staffs = await client.From<NhanVienModel>()
                            .Where(n => uniqueStaffIds.Contains(n.MaNV))
                            .Get();

                        var staffList = staffs.Models;

                        foreach (var phieu in _viewModel.DsBaoTri)
                        {
                            if (phieu.MaNV.HasValue)
                            {
                                var staff = staffList.FirstOrDefault(s => s.MaNV == phieu.MaNV.Value);
                                phieu.TenNguoiPhuTrach = staff?.TenNV ?? $"NV{phieu.MaNV} (Không tìm thấy)";
                            }
                            else
                            {
                                phieu.TenNguoiPhuTrach = "Không có";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tải thông tin tài sản: {ex.Message}");
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
                var addWindow = new EditPhieuBaoTriWindow(phieuMoi);
                addWindow.Title = "Thêm phiếu bảo trì mới";
                addWindow.Owner = Window.GetWindow(this);
                // Hiển thị cửa sổ dưới dạng dialog
                bool? result = addWindow.ShowDialog();
                // Nếu người dùng đã lưu phiếu mới
                if (result == true)
                {
                    // Log thông tin trước khi thêm để debug
                    Console.WriteLine($"Thông tin phiếu bảo trì trước khi thêm: " +
                        $"MaTaiSan={addWindow.PhieuBaoTri.MaTaiSan}, " +
                        $"NoiDung={addWindow.PhieuBaoTri.NoiDung}, " +
                        $"TrangThai={addWindow.PhieuBaoTri.TrangThai}");
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
            await LoadTaiSanInfoAsync(); // Cập nhật thông tin tài sản sau khi tìm kiếm
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
                await LoadTaiSanInfoAsync(); // Cập nhật thông tin tài sản sau khi lọc
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
                            await LoadDSBaoTriAsync();
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
        private async void btnIn_Click(object sender, RoutedEventArgs e)
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
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {
                        // Gọi phương thức xuất Excel từ service
                        _phieuBaoTriService.ExportToExcel(danhSachXuat, saveFileDialog.FileName);

                        // THÊM MỚI: Lưu hoạt động xuất Excel vào lịch sử
                        var theoDoiService = new TheoDoiHoatDongService();
                        string ghiChu = $"Xuất Excel phiếu bảo trì vào file {saveFileDialog.FileName}";
                        await theoDoiService.LuuHoatDongXuatExcel(danhSachXuat, ghiChu);

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
                MessageBox.Show($"Lỗi khi xuất Excel: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Xử lý khi nhấn nút In phiếu PDF
        private async void btnInPhieu_Click(object sender, RoutedEventArgs e)
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
                    // Thêm hiệu ứng waiting cursor
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {
                        // Đọc thông tin công ty
                        var thongTinCongTy = ThongTinCongTyService.DocThongTinCongTy();

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

                        if (!string.IsNullOrEmpty(thongTinCongTy.LogoPath) && File.Exists(thongTinCongTy.LogoPath))
                        {
                            // Tạo StackPanel để chứa logo và thông tin công ty
                            StackPanel headerPanel = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                Margin = new Thickness(0, 0, 0, 20)
                            };

                            // Logo bên trái
                            Image logoImage = new Image
                            {
                                Source = new BitmapImage(new Uri(thongTinCongTy.LogoPath, UriKind.Absolute)),
                                Height = 80,
                                Width = 80,
                                Margin = new Thickness(0, 0, 20, 0)
                            };
                            headerPanel.Children.Add(logoImage);

                            // Thông tin công ty bên phải
                            StackPanel infoPanel = new StackPanel
                            {
                                VerticalAlignment = VerticalAlignment.Center
                            };

                            TextBlock companyName = new TextBlock
                            {
                                Text = thongTinCongTy.Ten,
                                FontSize = 16,
                                FontWeight = FontWeights.Bold
                            };
                            infoPanel.Children.Add(companyName);

                            infoPanel.Children.Add(new TextBlock
                            {
                                Text = "Địa chỉ: " + thongTinCongTy.DiaChi,
                                FontSize = 10
                            });
                            infoPanel.Children.Add(new TextBlock
                            {
                                Text = $"SĐT: {thongTinCongTy.SoDienThoai} - Email: {thongTinCongTy.Email}",
                                FontSize = 10
                            });
                            infoPanel.Children.Add(new TextBlock
                            {
                                Text = "Mã số thuế: " + thongTinCongTy.MaSoThue,
                                FontSize = 10
                            });

                            headerPanel.Children.Add(infoPanel);

                            // Thêm vào document
                            BlockUIContainer container = new BlockUIContainer(headerPanel);
                            document.Blocks.Add(container);
                        }


                        // Thêm tiêu đề chung cho tất cả phiếu
                        System.Windows.Documents.Paragraph titlePara = new System.Windows.Documents.Paragraph();
                        titlePara.TextAlignment = System.Windows.TextAlignment.Center;
                        titlePara.Margin = new System.Windows.Thickness(0, 20, 0, 20);
                        System.Windows.Documents.Run titleRun = new System.Windows.Documents.Run("DANH SÁCH PHIẾU BẢO TRÌ TÀI SẢN");
                        titleRun.FontSize = 18;
                        titleRun.FontWeight = System.Windows.FontWeights.Bold;
                        titlePara.Inlines.Add(titleRun);
                        document.Blocks.Add(titlePara);

                        // Thêm ngày in ở trên cùng
                        System.Windows.Documents.Paragraph datePara = new System.Windows.Documents.Paragraph();
                        datePara.TextAlignment = System.Windows.TextAlignment.Right;
                        datePara.Margin = new System.Windows.Thickness(0, 0, 0, 20);
                        System.Windows.Documents.Run dateRun = new System.Windows.Documents.Run("Ngày in: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                        dateRun.FontStyle = System.Windows.FontStyles.Italic;
                        dateRun.FontSize = 10;
                        datePara.Inlines.Add(dateRun);
                        document.Blocks.Add(datePara);

                        // Tạo bảng chung cho tất cả các phiếu
                        System.Windows.Documents.Table mainTable = new System.Windows.Documents.Table();
                        mainTable.CellSpacing = 0;
                        mainTable.BorderBrush = System.Windows.Media.Brushes.Black;
                        mainTable.BorderThickness = new System.Windows.Thickness(1);
                        // Định nghĩa các cột cho bảng
                        mainTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(50) }); // STT
                        mainTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(80) }); // Mã phiếu
                        mainTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(80) }); // Mã tài sản
                        mainTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(120) }); // Tên tài sản
                        mainTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(80) }); // Loại bảo trì
                        mainTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(80) }); // Ngày bảo trì
                        mainTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(120) }); // Người phụ trách
                        mainTable.Columns.Add(new System.Windows.Documents.TableColumn() { Width = new System.Windows.GridLength(80) }); // Chi phí
                                                                                                                                         // Khởi tạo RowGroup
                        mainTable.RowGroups.Add(new System.Windows.Documents.TableRowGroup());
                        // Tạo hàng tiêu đề
                        System.Windows.Documents.TableRow headerRow = new System.Windows.Documents.TableRow();
                        headerRow.Background = System.Windows.Media.Brushes.LightGray;
                        // Thêm các tiêu đề cột
                        AddHeaderCell(headerRow, "STT");
                        AddHeaderCell(headerRow, "Mã phiếu");
                        AddHeaderCell(headerRow, "Mã TS");
                        AddHeaderCell(headerRow, "Tên tài sản");
                        AddHeaderCell(headerRow, "Loại BT");
                        AddHeaderCell(headerRow, "Ngày BT");
                        AddHeaderCell(headerRow, "Người phụ trách");
                        AddHeaderCell(headerRow, "Chi phí (VNĐ)");
                        // Thêm hàng tiêu đề vào bảng
                        mainTable.RowGroups[0].Rows.Add(headerRow);

                        // Thêm dữ liệu từng phiếu vào bảng
                        decimal tongChiPhi = 0;
                        for (int i = 0; i < selectedPhieu.Count; i++)
                        {
                            await AddPhieuRowAsync(mainTable, selectedPhieu[i], i + 1);
                            // Kiểm tra null và cộng dồn chi phí
                            if (selectedPhieu[i].ChiPhi.HasValue)
                            {
                                tongChiPhi += selectedPhieu[i].ChiPhi.Value;
                            }
                        }

                        // Thêm hàng tổng chi phí
                        System.Windows.Documents.TableRow totalRow = new System.Windows.Documents.TableRow();
                        totalRow.Background = System.Windows.Media.Brushes.LightGray;

                        // Tạo ô "Tổng chi phí" (gộp 7 cột)
                        System.Windows.Documents.TableCell totalLabelCell = new System.Windows.Documents.TableCell();
                        totalLabelCell.BorderBrush = System.Windows.Media.Brushes.Black;
                        totalLabelCell.BorderThickness = new System.Windows.Thickness(1);
                        totalLabelCell.Padding = new System.Windows.Thickness(5);
                        totalLabelCell.ColumnSpan = 7;

                        System.Windows.Documents.Paragraph totalLabelPara = new System.Windows.Documents.Paragraph();
                        totalLabelPara.TextAlignment = System.Windows.TextAlignment.Right;
                        System.Windows.Documents.Run totalLabelRun = new System.Windows.Documents.Run("Tổng chi phí:");
                        totalLabelRun.FontWeight = System.Windows.FontWeights.Bold;
                        totalLabelPara.Inlines.Add(totalLabelRun);
                        totalLabelCell.Blocks.Add(totalLabelPara);
                        totalRow.Cells.Add(totalLabelCell);

                        // Tạo ô giá trị tổng chi phí
                        System.Windows.Documents.TableCell totalValueCell = new System.Windows.Documents.TableCell();
                        totalValueCell.BorderBrush = System.Windows.Media.Brushes.Black;
                        totalValueCell.BorderThickness = new System.Windows.Thickness(1);
                        totalValueCell.Padding = new System.Windows.Thickness(5);

                        System.Windows.Documents.Paragraph totalValuePara = new System.Windows.Documents.Paragraph();
                        totalValuePara.TextAlignment = System.Windows.TextAlignment.Right;
                        System.Windows.Documents.Run totalValueRun = new System.Windows.Documents.Run(string.Format("{0:N0}", tongChiPhi));
                        totalValueRun.FontWeight = System.Windows.FontWeights.Bold;
                        totalValuePara.Inlines.Add(totalValueRun);
                        totalValueCell.Blocks.Add(totalValuePara);
                        totalRow.Cells.Add(totalValueCell);

                        // Thêm hàng tổng vào bảng
                        mainTable.RowGroups[0].Rows.Add(totalRow);

                        // Thêm bảng vào document
                        document.Blocks.Add(mainTable);
                        // Thêm phần chữ ký
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
                        System.Windows.Documents.Run rightHeaderRun = new System.Windows.Documents.Run("Xác nhận của quản lý");
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
                        document.Blocks.Add(signatureTable);

                        // In tất cả các phiếu trong một lần
                        printDialog.PrintDocument(((System.Windows.Documents.IDocumentPaginatorSource)document).DocumentPaginator,
                            "In phiếu bảo trì");

                        // THÊM MỚI: Lưu hoạt động in phiếu vào lịch sử
                        var theoDoiService = new TheoDoiHoatDongService();
                        string ghiChu = $"In phiếu bảo trì - {selectedPhieu.Count} phiếu";
                        await theoDoiService.LuuHoatDongInPhieu(selectedPhieu, ghiChu);

                        MessageBox.Show($"Đã in {selectedPhieu.Count} phiếu bảo trì thành công!", "Thông báo",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    finally
                    {
                        // Đảm bảo cursor luôn được reset về bình thường, kể cả khi có lỗi
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
        private void AddHeaderCell(System.Windows.Documents.TableRow row, string text)
        {
            System.Windows.Documents.TableCell cell = new System.Windows.Documents.TableCell();
            cell.BorderBrush = System.Windows.Media.Brushes.Black;
            cell.BorderThickness = new System.Windows.Thickness(1);
            cell.Padding = new System.Windows.Thickness(5);

            System.Windows.Documents.Paragraph paragraph = new System.Windows.Documents.Paragraph();
            paragraph.TextAlignment = System.Windows.TextAlignment.Center;
            System.Windows.Documents.Run run = new System.Windows.Documents.Run(text);
            run.FontWeight = System.Windows.FontWeights.Bold;
            paragraph.Inlines.Add(run);

            cell.Blocks.Add(paragraph);
            row.Cells.Add(cell);
        }
        private async Task AddPhieuRowAsync(System.Windows.Documents.Table table, PhieuBaoTri phieu, int stt)
        {
            System.Windows.Documents.TableRow row = new System.Windows.Documents.TableRow();

            // Thêm các ô dữ liệu
            AddDataCell(row, stt.ToString(), System.Windows.TextAlignment.Center);
            AddDataCell(row, phieu.MaBaoTri.ToString(), System.Windows.TextAlignment.Center);
            AddDataCell(row, phieu.MaTaiSan?.ToString() ?? "", System.Windows.TextAlignment.Center);
            AddDataCell(row, phieu.TenTaiSan ?? "", System.Windows.TextAlignment.Left);
            AddDataCell(row, phieu.TenLoaiBaoTri ?? "", System.Windows.TextAlignment.Center);
            AddDataCell(row, phieu.NgayBaoTri.HasValue ? phieu.NgayBaoTri.Value.ToString("dd/MM/yyyy") : "", System.Windows.TextAlignment.Center);
            AddDataCell(row, phieu.TenNguoiPhuTrach ?? "", System.Windows.TextAlignment.Left);
            AddDataCell(row, phieu.ChiPhi.HasValue ? string.Format("{0:N0}", phieu.ChiPhi.Value) : "0", System.Windows.TextAlignment.Right);

            // Thêm hàng vào bảng
            table.RowGroups[0].Rows.Add(row);

            // Để có hiệu ứng async, thêm một Task.Delay ngắn
            await Task.Delay(1);
        }

        private void AddDataCell(System.Windows.Documents.TableRow row, string text, System.Windows.TextAlignment alignment)
        {
            System.Windows.Documents.TableCell cell = new System.Windows.Documents.TableCell();
            cell.BorderBrush = System.Windows.Media.Brushes.Black;
            cell.BorderThickness = new System.Windows.Thickness(1);
            cell.Padding = new System.Windows.Thickness(5);

            System.Windows.Documents.Paragraph paragraph = new System.Windows.Documents.Paragraph();
            paragraph.TextAlignment = alignment;
            System.Windows.Documents.Run run = new System.Windows.Documents.Run(text);
            paragraph.Inlines.Add(run);

            cell.Blocks.Add(paragraph);
            row.Cells.Add(cell);
        }


        private async void btnInPhieuChiTiet_Click(object sender, RoutedEventArgs e)
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
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {
                        // Đọc thông tin công ty
                        var thongTinCongTy = ThongTinCongTyService.DocThongTinCongTy();

                        // (Mã code in phiếu chi tiết hiện tại của bạn)
                        // Thêm thông tin công ty vào đầu FlowDocument giống như btnInPhieu_Click

                        // THÊM MỚI: Lưu hoạt động in phiếu chi tiết vào lịch sử
                        var theoDoiService = new TheoDoiHoatDongService();
                        string ghiChu = $"In phiếu bảo trì chi tiết - {selectedPhieu.Count} phiếu";
                        await theoDoiService.LuuHoatDongInPhieu(selectedPhieu, ghiChu);

                        MessageBox.Show($"Đã in {selectedPhieu.Count} phiếu bảo trì chi tiết thành công!", "Thông báo",
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