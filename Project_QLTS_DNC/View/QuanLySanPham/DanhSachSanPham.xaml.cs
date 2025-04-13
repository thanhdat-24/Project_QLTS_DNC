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

namespace Project_QLTS_DNC.View.QuanLySanPham
{
    public partial class DanhSachSanPham : UserControl
    {
        private ObservableCollection<TaiSanDTO> _listTaiSan;
        private CollectionViewSource _viewSource;
        private List<PhongFilter> _phongList;

        public DanhSachSanPham()
        {
            InitializeComponent();
            LoadDataAsync();
            InitializeFilters();

            // Đăng ký các event
            btnSearch.Click += BtnSearch_Click;
            txtSearch.KeyDown += TxtSearch_KeyDown;
            cboPhong.SelectionChanged += Filter_SelectionChanged;

            // Đảm bảo chúng ta thêm sự kiện cho nút xoá không phải trên cấp cao mà trong template
            // btnDelete.Click += BtnDelete_Click; 
            // Thay bằng việc để cell template xử lý trong XAML

            this.Loaded += DanhSachSanPham_Loaded;
        }

        private void DanhSachSanPham_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateStatusBar();
        }

        // Thêm phương thức này vào lớp DanhSachSanPham
        public void RefreshData()
        {
            // Gọi lại phương thức LoadDataAsync để làm mới dữ liệu
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                // Lấy danh sách phòng để điền vào ComboBox lọc
                _phongList = await GetPhongListAsync();
                cboPhong.ItemsSource = _phongList;

                // Lấy danh sách tài sản từ service
                var taiSanModels = await TaiSanService.LayDanhSachTaiSanAsync();

                // Chuyển đổi từ TaiSanModel sang TaiSanDTO
                _listTaiSan = new ObservableCollection<TaiSanDTO>(
                    taiSanModels.Select(model => TaiSanDTO.FromModel(model)));

                // Lấy thông tin phòng cho mỗi tài sản
                var phongCollection = await PhongService.LayDanhSachPhongAsync();

                // Cập nhật tên phòng cho mỗi tài sản
                foreach (var taiSan in _listTaiSan)
                {
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
                }

                // Thiết lập nguồn dữ liệu cho DataGrid
                _viewSource = new CollectionViewSource();
                _viewSource.Source = _listTaiSan;
                dgSanPham.ItemsSource = _viewSource.View;

                // Cập nhật trạng thái
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                ApplyFilters();
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_viewSource != null)
            {
                _viewSource.View.Refresh();
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
                                taiSan.MaPhong == ((PhongFilter)cboPhong.SelectedItem).MaPhong;

            e.Accepted = matchesSearch && matchesPhong;
        }

        private void UpdateStatusBar()
        {
            // Cập nhật trạng thái tổng số tài sản
            txtStatus.Text = $"Tổng số tài sản: {_listTaiSan?.Count ?? 0}";
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
                    // Gọi phương thức xóa tài sản từ TaiSanService
                    bool success = await TaiSanService.XoaTaiSanAsync(selectedTaiSan.MaTaiSan);

                    if (success)
                    {
                        // Xóa khỏi danh sách local
                        _listTaiSan.Remove(selectedTaiSan);

                        // Làm mới view
                        _viewSource.View.Refresh();
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