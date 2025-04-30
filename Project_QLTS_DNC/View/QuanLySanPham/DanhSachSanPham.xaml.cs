using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Services.QLTaiSanService;
using Project_QLTS_DNC.Models.QLTaiSan;
using System.Windows.Media;
using Project_QLTS_DNC.Services.QLToanNha;
using ClosedXML.Excel;

namespace Project_QLTS_DNC.View.QuanLySanPham
{
    public partial class DanhSachSanPham : UserControl
    {
        public class ThongKeTaiSanTheoNhomDTO
        {
            public int? MaNhomTS { get; set; }
            public string TenNhomTS { get; set; }
            public int TongSoLuong { get; set; }
            public int SoLuongTonKho { get; set; }
            public int SoLuongDaSuDung { get; set; }
        }

        private enum ViewMode
        {
            DanhSach,
            ThongKe
        }

        private ObservableCollection<TaiSanDTO> _listTaiSan;
        private CollectionViewSource _viewSource;
        private List<PhongFilter> _phongList;
        private List<NhomTaiSanFilter> _nhomTSList;
        private ViewMode _currentViewMode = ViewMode.DanhSach;

        // Biến quản lý phân trang
        private int _pageSize = 10; // Số lượng tài sản hiển thị trên mỗi trang
        private int _currentPage = 1; // Trang hiện tại
        private int _totalPages = 1; // Tổng số trang
        private int _totalItems = 0; // Tổng số tài sản (sau khi áp dụng bộ lọc)

        public DanhSachSanPham()
        {
            InitializeComponent();
            LoadDataAsync();
            InitializeViewToggle();
            InitializeFilters();

            // Đăng ký các event (giữ nguyên các đăng ký hiện có)
            btnSearch.Click += BtnSearch_Click;
            txtSearch.KeyDown += TxtSearch_KeyDown;
            cboPhong.SelectionChanged += Filter_SelectionChanged;
            cboNhomTS.SelectionChanged += Filter_SelectionChanged;
            cboTinhTrang.SelectionChanged += Filter_SelectionChanged;
            btnRefresh.Click += BtnRefresh_Click;
            btnExportQRCode.Click += BtnExportQRCode_Click;
            btnExportExcel.Click += BtnExportExcel_Click;
            btnPreviousPage.Click += BtnPreviousPage_Click;
            btnNextPage.Click += BtnNextPage_Click;
            txtCurrentPage.KeyDown += TxtCurrentPage_KeyDown;
            this.Loaded += DanhSachSanPham_Loaded;
        }
        private void InitializeViewToggle()
        {
            // Đăng ký sự kiện cho nút chuyển đổi chế độ xem
            btnToggleView.Checked += BtnToggleView_Checked;
            btnToggleView.Unchecked += BtnToggleView_Unchecked;
        }

        private void BtnToggleView_Checked(object sender, RoutedEventArgs e)
        {
            SwitchToThongKeView();
        }

        private void BtnToggleView_Unchecked(object sender, RoutedEventArgs e)
        {
            SwitchToDanhSachView();
        }
        private void SwitchToThongKeView()
        {
            _currentViewMode = ViewMode.ThongKe;

            // Ẩn các điều khiển lọc
            cboPhong.Visibility = Visibility.Collapsed;
            cboNhomTS.Visibility = Visibility.Collapsed;
            cboTinhTrang.Visibility = Visibility.Collapsed;
            txtSearch.Visibility = Visibility.Collapsed;
            btnSearch.Visibility = Visibility.Collapsed;

            // Thay đổi tiêu đề
            txtTieuDe.Text = "THỐNG KÊ TÀI SẢN THEO NHÓM";

            // Ẩn/hiện DataGrid
            cardDanhSachTaiSan.Visibility = Visibility.Collapsed;
            cardThongKeTaiSan.Visibility = Visibility.Visible;

            // Tải dữ liệu thống kê
            LoadThongKeTaiSanTheoNhom();
        }
        private void SwitchToDanhSachView()
        {
            _currentViewMode = ViewMode.DanhSach;

            // Hiện các điều khiển lọc
            cboPhong.Visibility = Visibility.Visible;
            cboNhomTS.Visibility = Visibility.Visible;
            cboTinhTrang.Visibility = Visibility.Visible;
            txtSearch.Visibility = Visibility.Visible;
            btnSearch.Visibility = Visibility.Visible;

            // Thay đổi tiêu đề
            txtTieuDe.Text = "DANH SÁCH TÀI SẢN";

            // Ẩn/hiện DataGrid
            cardDanhSachTaiSan.Visibility = Visibility.Visible;
            cardThongKeTaiSan.Visibility = Visibility.Collapsed;

            // Tải lại danh sách tài sản
            LoadDataAsync();
        }
        private void LoadThongKeTaiSanTheoNhom()
        {
            try
            {
                // Tạo danh sách để thống kê
                var thongKeTaiSan = _listTaiSan
                    .GroupBy(ts =>
                    {
                        // Xác định nhóm tài sản
                        return new
                        {
                            MaNhomTS = ts.MaNhomTS ?? -1,
                            TenNhomTS = string.IsNullOrEmpty(ts.TenNhomTS) ? "Chưa xác định" : ts.TenNhomTS
                        };
                    })
                    .Select(g =>
                    {
                        // Nhóm tài sản theo nhóm
                        var nhomTaiSan = g.ToList();

                        return new ThongKeTaiSanTheoNhomDTO
                        {
                            MaNhomTS = g.Key.MaNhomTS == -1 ? null : g.Key.MaNhomTS,
                            TenNhomTS = g.Key.TenNhomTS,
                            TongSoLuong = nhomTaiSan.Count,
                            // Đếm số lượng tồn kho (MaPhong == null)
                            SoLuongTonKho = nhomTaiSan.Count(ts => !ts.MaPhong.HasValue),
                            // Đếm số lượng đã sử dụng (MaPhong != null)
                            SoLuongDaSuDung = nhomTaiSan.Count(ts => ts.MaPhong.HasValue)
                        };
                    })
                    .OrderByDescending(tk => tk.TongSoLuong)
                    .ToList();

                // Gán nguồn dữ liệu cho DataGrid
                dgThongKeTaiSan.ItemsSource = thongKeTaiSan;

                // Cập nhật trạng thái
                txtStatus.Text = $"Tổng số nhóm tài sản: {thongKeTaiSan.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu thống kê: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            // Hiển thị loading trước khi làm mới dữ liệu
            LoadingGrid.Visibility = Visibility.Visible;
            RefreshData();
        }

        private void BtnExportQRCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra nếu không có dữ liệu
                if (_listTaiSan == null || _listTaiSan.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu tài sản để xuất QR Code.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Tạo một bản sao của danh sách tài sản
                var dsTaiSan = new ObservableCollection<TaiSanDTO>(_listTaiSan.ToList());

                // Tạo form xuất QR Code
                XuatQRCode xuatQRForm = new XuatQRCode(dsTaiSan);
                xuatQRForm.Owner = Window.GetWindow(this);

                // Tạo overlay - đây là UIElement, không phải object
                Grid overlayGrid = new Grid();
                overlayGrid.Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0));

                // Hiển thị overlay trong DialogHost
                MainDialogHost.DialogContent = overlayGrid;
                MainDialogHost.IsOpen = true;

                // Đăng ký sự kiện đóng form để đóng overlay
                xuatQRForm.Closed += (s, args) =>
                {
                    // Đóng overlay khi form đóng
                    MainDialogHost.IsOpen = false;
                };

                // Hiển thị form XuatQRCode
                xuatQRForm.ShowDialog();
            }
            catch (Exception ex)
            {
                // Đảm bảo đóng overlay nếu có lỗi
                MainDialogHost.IsOpen = false;
                MessageBox.Show($"Lỗi khi mở form xuất QR Code: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Xem chi tiết
        /// </summary>
        private void BtnViewDetail_Click(object sender, RoutedEventArgs e)
        {
            // Lấy nguồn của sự kiện - là nút Xem chi tiết trong DataGridTemplateColumn
            Button viewButton = sender as Button;
            if (viewButton == null) return;

            // Lấy context (DataContext) của nút, có thể là TaiSanDTO hoặc DataGridRow
            var dataContext = viewButton.DataContext;
            TaiSanDTO selectedTaiSan = dataContext as TaiSanDTO;

            // Nếu không thể lấy trực tiếp, thử tìm từ DataGridRow
            if (selectedTaiSan == null)
            {
                var row = FindParent<DataGridRow>(viewButton);
                if (row != null)
                {
                    selectedTaiSan = row.DataContext as TaiSanDTO;
                }
            }

            if (selectedTaiSan == null)
            {
                MessageBox.Show("Vui lòng chọn tài sản để xem chi tiết.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Tạo overlay
                Grid overlayGrid = new Grid();
                overlayGrid.Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0));

                // Hiển thị overlay trong DialogHost
                MainDialogHost.DialogContent = overlayGrid;
                MainDialogHost.IsOpen = true;

                // Mở form chi tiết tài sản
                ChiTietTaiSan chiTietForm = new ChiTietTaiSan(selectedTaiSan.MaTaiSan);
                chiTietForm.Owner = Window.GetWindow(this);

                // Đăng ký sự kiện đóng form để đóng overlay
                chiTietForm.Closed += (s, args) =>
                {
                    // Đóng overlay khi form đóng
                    MainDialogHost.IsOpen = false;
                };

                chiTietForm.ShowDialog();
            }
            catch (Exception ex)
            {
                // Đảm bảo đóng overlay nếu có lỗi
                MainDialogHost.IsOpen = false;
                MessageBox.Show($"Lỗi khi mở form chi tiết tài sản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DanhSachSanPham_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateStatusBar();
        }

        public void RefreshData()
        {
            // Reset phân trang về trang đầu tiên khi làm mới dữ liệu
            _currentPage = 1;

            // Reset các bộ lọc về giá trị mặc định
            cboPhong.SelectedIndex = 0;
            cboNhomTS.SelectedIndex = 0;
            cboTinhTrang.SelectedIndex = 0;
            txtSearch.Text = string.Empty;

            // Gọi lại phương thức LoadDataAsync để làm mới dữ liệu
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                // Hiển thị indicator loading
                LoadingGrid.Visibility = Visibility.Visible;

                // Lấy danh sách phòng để điền vào ComboBox lọc
                _phongList = await GetPhongListAsync();
                cboPhong.ItemsSource = _phongList;

                // Lấy danh sách nhóm tài sản để điền vào ComboBox lọc
                _nhomTSList = await GetNhomTSListAsync();
                cboNhomTS.ItemsSource = _nhomTSList;

                // Thiết lập giá trị mặc định cho ComboBox tình trạng (nếu chưa được chọn)
                if (cboTinhTrang.SelectedIndex < 0)
                {
                    cboTinhTrang.SelectedIndex = 0; // "Tất cả"
                }

                // Lấy danh sách tài sản từ service
                var taiSanModels = await TaiSanService.LayDanhSachTaiSanAsync();

                // Chuyển đổi từ TaiSanModel sang TaiSanDTO
                _listTaiSan = new ObservableCollection<TaiSanDTO>(
                    taiSanModels.Select(model => TaiSanDTO.FromModel(model)));

                // Lấy thông tin phòng cho mỗi tài sản
                var phongCollection = await PhongService.LayDanhSachPhongAsync();

                // Lấy thông tin nhóm tài sản
                var nhomTSCollection = await NhomTaiSanService.LayDanhSachNhomTaiSanAsync();

                // Lấy thông tin chi tiết phiếu nhập để truy vấn MaNhomTS
                // Thêm biến tạm để lưu trữ chi tiết phiếu nhập đã truy vấn
                Dictionary<int, int> chiTietPNCache = new Dictionary<int, int>();

                // Cập nhật tên phòng và nhóm tài sản cho mỗi tài sản
                foreach (var taiSan in _listTaiSan)
                {
                    // Cập nhật tên phòng
                    if (taiSan.MaPhong.HasValue)
                    {
                        var phong = phongCollection.FirstOrDefault(p => p.MaPhong == taiSan.MaPhong.Value);
                        if (phong != null)
                        {
                            taiSan.TenPhong = phong.TenPhong;
                        }
                    }
                    else
                    {
                        taiSan.TenPhong = "Chưa phân phòng";
                    }

                    // Cập nhật mã và tên nhóm tài sản từ chi tiết phiếu nhập
                    if (taiSan.MaChiTietPN.HasValue)
                    {
                        try
                        {
                            int maNhomTS = -1;
                            // Kiểm tra xem đã truy vấn chi tiết phiếu nhập này chưa
                            if (chiTietPNCache.TryGetValue(taiSan.MaChiTietPN.Value, out maNhomTS))
                            {
                                taiSan.MaNhomTS = maNhomTS;
                            }
                            else
                            {
                                // Sử dụng service có sẵn để lấy thông tin chi tiết phiếu nhập
                                var chiTietPN = await ChiTietPhieuNhapService.LayChiTietPhieuNhapTheoMaAsync(taiSan.MaChiTietPN.Value);

                                if (chiTietPN != null)
                                {
                                    // Lưu vào cache để dùng lại
                                    chiTietPNCache[chiTietPN.MaChiTietPN] = chiTietPN.MaNhomTS;

                                    // Lưu mã nhóm tài sản vào DTO
                                    taiSan.MaNhomTS = chiTietPN.MaNhomTS;
                                }
                            }

                            // Tìm tên nhóm tài sản
                            if (taiSan.MaNhomTS.HasValue)
                            {
                                var nhomTS = nhomTSCollection.FirstOrDefault(n => n.MaNhomTS == taiSan.MaNhomTS.Value);
                                if (nhomTS != null)
                                {
                                    taiSan.TenNhomTS = nhomTS.TenNhom;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Xử lý nếu không thể lấy được thông tin chi tiết phiếu nhập
                            System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy thông tin chi tiết phiếu nhập: {ex.Message}");
                        }
                    }
                    else
                    {
                        // Nếu không có MaChiTietPN, có thể truy vấn trực tiếp từ service để lấy nhóm tài sản
                        try
                        {
                            int maNhomTS = await ChiTietPhieuNhapService.TimNhomTaiSanTheoTaiSanAsync(taiSan.MaTaiSan);
                            if (maNhomTS > 0)  // Nếu tìm thấy nhóm tài sản
                            {
                                taiSan.MaNhomTS = maNhomTS;

                                // Tìm tên nhóm tài sản
                                var nhomTS = nhomTSCollection.FirstOrDefault(n => n.MaNhomTS == maNhomTS);
                                if (nhomTS != null)
                                {
                                    taiSan.TenNhomTS = nhomTS.TenNhom;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Lỗi khi tìm nhóm tài sản: {ex.Message}");
                        }
                    }
                }

                // Thiết lập nguồn dữ liệu cho DataGrid
                _viewSource = new CollectionViewSource();
                _viewSource.Source = _listTaiSan;
                _viewSource.Filter += ApplyFilter;

                // Hiển thị DataGrid
                dgSanPham.Visibility = Visibility.Visible;

                // Cập nhật số trang và hiển thị dữ liệu trang đầu tiên
                UpdatePagingInfo();

                // Áp dụng phân trang
                ApplyPaging();

                // Cập nhật UI phân trang và trạng thái
                UpdatePagingUI();

                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Chi tiết lỗi: {ex.StackTrace}");
            }
            finally
            {
                // Ẩn indicator loading khi đã xong (hoặc có lỗi)
                LoadingGrid.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<List<NhomTaiSanFilter>> GetNhomTSListAsync()
        {
            try
            {
                // Sử dụng service để lấy danh sách nhóm tài sản
                return await NhomTaiSanService.GetNhomTaiSanFilterListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lấy danh sách nhóm tài sản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<NhomTaiSanFilter>
                {
                    new NhomTaiSanFilter { MaNhomTS = null, TenNhomTS = "Tất cả" }
                };
            }
        }

        private void InitializeFilters()
        {
            // Thiết lập bộ lọc cho ComboBox Phòng
            if (_viewSource != null)
            {
                _viewSource.Filter += ApplyFilter;
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            // Reset về trang đầu tiên khi áp dụng bộ lọc mới
            _currentPage = 1;
            ApplyFilters();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selectedTaiSan = dgSanPham.SelectedItem as TaiSanDTO;
            if (selectedTaiSan == null)
            {
                MessageBox.Show("Vui lòng chọn tài sản để chỉnh sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Convert TaiSanDTO to TaiSan model
            var taiSanModel = selectedTaiSan.ToModel();

            // Mở form chỉnh sửa tài sản
            EditSanPham editWindow = new EditSanPham(this, taiSanModel, _phongList);
            editWindow.ShowDialog();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Reset về trang đầu tiên khi áp dụng bộ lọc mới
                _currentPage = 1;
                ApplyFilters();
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Reset về trang đầu tiên khi áp dụng bộ lọc mới
            _currentPage = 1;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_viewSource != null)
            {
                // Xóa tài sản đã chọn trong DataGrid
                dgSanPham.SelectedItem = null;

                // Làm mới view để áp dụng bộ lọc
                _viewSource.View.Refresh();

                // Cập nhật thông tin phân trang sau khi lọc
                UpdatePagingInfo();

                // Cập nhật giao diện phân trang
                UpdatePagingUI();

                // Cập nhật thông tin trạng thái
                UpdateStatusBar();
            }
        }

        private void ApplyFilter(object sender, FilterEventArgs e)
        {
            var taiSan = e.Item as TaiSanDTO;
            if (taiSan == null)
            {
                e.Accepted = false;
                return;
            }

            // Lọc theo tên tài sản
            string searchText = txtSearch.Text.ToLower().Trim();
            bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                                 taiSan.TenTaiSan.ToLower().Contains(searchText);

            // Lọc theo phòng
            bool matchesPhong = cboPhong.SelectedItem == null ||
                                ((PhongFilter)cboPhong.SelectedItem).MaPhong == null ||
                                taiSan.MaPhong == ((PhongFilter)cboPhong.SelectedItem).MaPhong;

            // Lọc theo nhóm tài sản
            bool matchesNhomTS = cboNhomTS.SelectedItem == null ||
                                ((NhomTaiSanFilter)cboNhomTS.SelectedItem).MaNhomTS == null ||
                                (taiSan.MaNhomTS.HasValue && taiSan.MaNhomTS.Value == ((NhomTaiSanFilter)cboNhomTS.SelectedItem).MaNhomTS.Value);

            // Lọc theo tình trạng tài sản
            bool matchesTinhTrang = true;
            if (cboTinhTrang.SelectedIndex > 0) // Nếu không phải "Tất cả"
            {
                string selectedTinhTrang = ((ComboBoxItem)cboTinhTrang.SelectedItem).Content.ToString();
                matchesTinhTrang = taiSan.TinhTrangSP == selectedTinhTrang;
            }

            e.Accepted = matchesSearch && matchesPhong && matchesNhomTS && matchesTinhTrang;
        }

        private void UpdateStatusBar()
        {
            if (_viewSource != null && _viewSource.View != null)
            {
                // Đếm số lượng tài sản sau khi lọc
                int filteredCount = _viewSource.View.Cast<TaiSanDTO>().Count();
                // Cập nhật thông tin trạng thái
                txtStatus.Text = $"Tổng số tài sản: {filteredCount}";
            }
            else
            {
                txtStatus.Text = $"Tổng số tài sản: 0";
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Lấy nguồn của sự kiện - là nút Delete trong DataGridTemplateColumn
            Button deleteButton = sender as Button;
            if (deleteButton == null) return;

            // Lấy context (DataContext) của nút, có thể là TaiSanDTO hoặc DataGridRow
            var dataContext = deleteButton.DataContext;
            TaiSanDTO selectedTaiSan = dataContext as TaiSanDTO;

            // Nếu không thể lấy trực tiếp, thử tìm từ DataGridRow
            if (selectedTaiSan == null)
            {
                var row = FindParent<DataGridRow>(deleteButton);
                if (row != null)
                {
                    selectedTaiSan = row.DataContext as TaiSanDTO;
                }
            }

            if (selectedTaiSan == null)
            {
                MessageBox.Show("Vui lòng chọn tài sản để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa tài sản '{selectedTaiSan.TenTaiSan}'?",
                                         "Xác nhận xóa",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Hiển thị loading khi đang xóa
                    LoadingGrid.Visibility = Visibility.Visible;

                    // Gọi phương thức xóa tài sản từ TaiSanService
                    bool success = await TaiSanService.XoaTaiSanAsync(selectedTaiSan.MaTaiSan);

                    if (success)
                    {
                        // Xóa khỏi danh sách local
                        _listTaiSan.Remove(selectedTaiSan);

                        // Làm mới view
                        _viewSource.View.Refresh();

                        // Cập nhật thông tin phân trang
                        UpdatePagingInfo();

                        // Cập nhật giao diện phân trang
                        UpdatePagingUI();

                        // Cập nhật trạng thái
                        UpdateStatusBar();

                        MessageBox.Show("Xóa tài sản thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa tài sản.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa tài sản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    // Ẩn loading khi đã xong
                    LoadingGrid.Visibility = Visibility.Collapsed;
                }
            }
        }

        // Helper method to find parent of a specific type
        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        private async Task<List<PhongFilter>> GetPhongListAsync()
        {
            try
            {
                // Lấy danh sách phòng từ service
                var phongCollection = await PhongService.LayDanhSachPhongAsync();

                // Tạo danh sách PhongFilter từ dữ liệu phòng
                var phongFilterList = new List<PhongFilter>
                {
                    new PhongFilter { MaPhong = null, TenPhong = "Tất cả" }
                };

                // Thêm các phòng từ cơ sở dữ liệu vào danh sách
                foreach (var phong in phongCollection)
                {
                    phongFilterList.Add(new PhongFilter
                    {
                        MaPhong = phong.MaPhong,
                        TenPhong = phong.TenPhong
                    });
                }

                return phongFilterList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lấy danh sách phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<PhongFilter>
                {
                    new PhongFilter { MaPhong = null, TenPhong = "Tất cả" }
                };
            }
        }

        #region Phân trang

        /// <summary>
        /// Cập nhật thông tin phân trang dựa trên dữ liệu hiện tại sau khi lọc
        /// </summary>
        private void UpdatePagingInfo()
        {
            if (_viewSource != null && _viewSource.View != null)
            {
                // Lấy danh sách các tài sản sau khi lọc
                var filteredItems = _viewSource.View.Cast<TaiSanDTO>().ToList();

                // Cập nhật tổng số tài sản
                _totalItems = filteredItems.Count;

                // Tính tổng số trang
                _totalPages = (_totalItems + _pageSize - 1) / _pageSize; // Công thức làm tròn lên

                // Đảm bảo trang hiện tại không vượt quá tổng số trang
                if (_currentPage > _totalPages)
                {
                    _currentPage = _totalPages > 0 ? _totalPages : 1;
                }

                // Áp dụng phân trang vào DataGrid
                ApplyPaging();
            }
        }

        /// <summary>
        /// Áp dụng phân trang vào DataGrid
        /// </summary>
        private void ApplyPaging()
        {
            if (_viewSource != null && _viewSource.View != null)
            {
                // Lấy danh sách các tài sản sau khi lọc
                var filteredItems = _viewSource.View.Cast<TaiSanDTO>().ToList();

                // Tính chỉ số bắt đầu và kết thúc cho trang hiện tại
                int startIndex = (_currentPage - 1) * _pageSize;

                // Lấy các tài sản cho trang hiện tại
                var pagedItems = filteredItems
                    .Skip(startIndex)
                    .Take(_pageSize)
                    .ToList();

                // Cập nhật nguồn dữ liệu cho DataGrid
                dgSanPham.ItemsSource = pagedItems;
            }
        }

        /// <summary>
        /// Cập nhật giao diện phân trang
        /// </summary>
        private void UpdatePagingUI()
        {
            // Cập nhật số trang hiện tại và tổng số trang trong UI
            txtCurrentPage.Text = _currentPage.ToString();
            txtTotalPages.Text = _totalPages.ToString();

            // Cập nhật trạng thái nút phân trang
            btnPreviousPage.IsEnabled = _currentPage > 1;
            btnNextPage.IsEnabled = _currentPage < _totalPages;
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút trang trước
        /// </summary>
        private void BtnPreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                ApplyPaging();
                UpdatePagingUI();
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút trang sau
        /// </summary>
        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                ApplyPaging();
                UpdatePagingUI();
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi nhập số trang và nhấn Enter
        /// </summary>
        private void TxtCurrentPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    int newPage = int.Parse(txtCurrentPage.Text);
                    if (newPage > 0 && newPage <= _totalPages)
                    {
                        _currentPage = newPage;
                        ApplyPaging();
                        UpdatePagingUI();
                    }
                    else
                    {
                        // Nếu số trang không hợp lệ, đặt lại giá trị cũ
                        txtCurrentPage.Text = _currentPage.ToString();
                    }
                }
                catch
                {
                    // Nếu nhập không phải số, đặt lại giá trị cũ
                    txtCurrentPage.Text = _currentPage.ToString();
                }
            }
        }
        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Hiển thị loading khi đang xuất Excel
                LoadingGrid.Visibility = Visibility.Visible;

                // Kiểm tra nếu không có dữ liệu
                if (_listTaiSan == null || _listTaiSan.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu tài sản để xuất Excel.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadingGrid.Visibility = Visibility.Collapsed;
                    return;
                }

                // Hiển thị SaveFileDialog để chọn vị trí lưu file
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    DefaultExt = "xlsx",
                    FileName = $"DanhSachTaiSan_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;

                    // Gọi hàm xuất Excel và truyền đường dẫn file
                    ExportExcel(filePath);

                    // Hiển thị thông báo khi xuất file thành công
                    MessageBox.Show($"Xuất file Excel thành công. File được lưu tại:\n{filePath}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Ẩn loading khi đã xong
                LoadingGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void ExportExcel(string filePath)
        {
            // Tạo workbook mới
            using (var workbook = new XLWorkbook())
            {
                // Tạo worksheet
                var worksheet = workbook.Worksheets.Add("Danh sách tài sản");

                // Định dạng tiêu đề
                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Thêm tiêu đề cho các cột
                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Mã TS";
                worksheet.Cell(1, 3).Value = "Tên tài sản";
                worksheet.Cell(1, 4).Value = "Số Seri";
                worksheet.Cell(1, 5).Value = "Ngày sử dụng";
                worksheet.Cell(1, 6).Value = "Hạn BH";
                worksheet.Cell(1, 7).Value = "Tình trạng";
                worksheet.Cell(1, 8).Value = "Ghi chú";
                worksheet.Cell(1, 9).Value = "Phòng";
                worksheet.Cell(1, 10).Value = "Nhóm tài sản";

                // Đặt style cho tất cả các ô trong header
                var headerRange = worksheet.Range(1, 1, 1, 10);
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Lấy danh sách tài sản (áp dụng bộ lọc nếu có)
                var filteredItems = _viewSource != null && _viewSource.View != null
                    ? _viewSource.View.Cast<TaiSanDTO>().ToList()
                    : _listTaiSan.ToList();

                // Thêm dữ liệu vào worksheet
                for (int i = 0; i < filteredItems.Count; i++)
                {
                    var taiSan = filteredItems[i];
                    int row = i + 2; // Dòng 1 là header

                    worksheet.Cell(row, 1).Value = i + 1; // STT
                    worksheet.Cell(row, 2).Value = taiSan.MaTaiSan;
                    worksheet.Cell(row, 3).Value = taiSan.TenTaiSan;
                    worksheet.Cell(row, 4).Value = taiSan.SoSeri;

                    // Xử lý các giá trị DateTime
                    if (taiSan.NgaySuDung.HasValue)
                        worksheet.Cell(row, 5).Value = taiSan.NgaySuDung.Value.ToString("dd/MM/yyyy");
                    else
                        worksheet.Cell(row, 5).Value = "";

                    if (taiSan.HanBH.HasValue)
                        worksheet.Cell(row, 6).Value = taiSan.HanBH.Value.ToString("dd/MM/yyyy");
                    else
                        worksheet.Cell(row, 6).Value = "";

                    worksheet.Cell(row, 7).Value = taiSan.TinhTrangSP;
                    worksheet.Cell(row, 8).Value = taiSan.GhiChu;
                    worksheet.Cell(row, 9).Value = taiSan.TenPhong;
                    worksheet.Cell(row, 10).Value = taiSan.TenNhomTS;

                    // Đặt style cho dòng dữ liệu
                    var dataRange = worksheet.Range(row, 1, row, 10);
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                }

                // Căn giữa một số cột
                worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // STT
                worksheet.Column(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Mã TS
                worksheet.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Số Seri
                worksheet.Column(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Ngày sử dụng
                worksheet.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Hạn BH
                worksheet.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Tình trạng

                // Tự động điều chỉnh độ rộng các cột
                worksheet.Columns().AdjustToContents();

                // Lưu workbook
                workbook.SaveAs(filePath);
            }
        }

        #endregion
    }

    // Lớp hỗ trợ cho ComboBox phòng
    public class PhongFilter
    {
        public int? MaPhong { get; set; }
        public string TenPhong { get; set; }

        public override string ToString()
        {
            return TenPhong;
        }
    }
}