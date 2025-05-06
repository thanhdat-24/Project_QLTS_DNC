using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project_QLTS_DNC.ViewModel.Baotri;
namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for LichSuSuaChuaUserControl.xaml
    /// </summary>
    public partial class LichSuSuaChuaUserControl : UserControl
    {
        private LichSuSuaChuaViewModel _viewModel;
        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && sender is ComboBox)
            {
                _viewModel.LocDanhSach().ConfigureAwait(false);
            }
        }

        // Trong constructor, đăng ký sự kiện
        public LichSuSuaChuaUserControl()
        {
            InitializeComponent();
            _viewModel = new LichSuSuaChuaViewModel();
            DataContext = _viewModel;

            // Đăng ký sự kiện cho các nút
            btnTimKiem.Click += btnTimKiem_Click;
            cboLocDanhSach.SelectionChanged += LocDanhSach_SelectionChanged;
            cboLoaiThaoTac.SelectionChanged += FilterComboBox_SelectionChanged;
            btnFirstPage.Click += FirstPage_Click;
            btnPrevPage.Click += PrevPage_Click;
            btnNextPage.Click += NextPage_Click;
            btnLastPage.Click += LastPage_Click;
            cboPageSize.SelectionChanged += PageSize_Changed;

            // Đăng ký sự kiện cho DatePicker
            datePickerTuNgay.SelectedDateChanged += DatePicker_SelectedDateChanged;
            datePickerDenNgay.SelectedDateChanged += DatePicker_SelectedDateChanged;

            // Thiết lập giá trị mặc định cho bộ lọc ngày
            DateTime ngayHienTai = DateTime.Now;
            datePickerDenNgay.SelectedDate = ngayHienTai;
            datePickerTuNgay.SelectedDate = ngayHienTai.AddMonths(-1);

            // Cập nhật giá trị vào ViewModel
            _viewModel.TuNgay = datePickerTuNgay.SelectedDate;
            _viewModel.DenNgay = datePickerDenNgay.SelectedDate;
        }

        
       // 3. Trong LichSuSuaChuaUserControl.xaml.cs - Sửa lại sự kiện DatePicker_SelectedDateChanged
private bool _isDateChanging = false;

private async void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
{
    if (DataContext == null || _isDateChanging) return;

    try
    {
        _isDateChanging = true;
        
        _viewModel.TuNgay = datePickerTuNgay.SelectedDate;
        _viewModel.DenNgay = datePickerDenNgay.SelectedDate;

        // Chỉ tự động tìm kiếm khi cả hai ngày đều hợp lệ
        if (_viewModel.TuNgay.HasValue && _viewModel.DenNgay.HasValue)
        {
            if (_viewModel.TuNgay <= _viewModel.DenNgay)
            {
                await _viewModel.TimKiem();
            }
            else
            {
                MessageBox.Show("Ngày bắt đầu không được lớn hơn ngày kết thúc!", "Cảnh báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
    finally
    {
        _isDateChanging = false;
    }
}
        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            // Lấy ngày bắt đầu và kết thúc từ DatePicker
            _viewModel.TuNgay = datePickerTuNgay.SelectedDate;
            _viewModel.DenNgay = datePickerDenNgay.SelectedDate;

            // Gọi phương thức tìm kiếm
            _viewModel.TimKiem().ConfigureAwait(false);
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Thiết lập giá trị mặc định cho bộ lọc ngày
            DateTime ngayHienTai = DateTime.Now;
            datePickerDenNgay.SelectedDate = ngayHienTai;
            datePickerTuNgay.SelectedDate = ngayHienTai.AddMonths(-1);

            // Cập nhật giá trị vào ViewModel
            _viewModel.TuNgay = datePickerTuNgay.SelectedDate;
            _viewModel.DenNgay = datePickerDenNgay.SelectedDate;

            // Tải dữ liệu ban đầu
            _viewModel.TaiDuLieu().ConfigureAwait(false);
        }
        private void TimKiem_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.TimKiem().ConfigureAwait(false);
        }
        private void LocDanhSach_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboLocDanhSach.SelectedItem != null)
            {
                // Đảm bảo rằng item được chọn không phải là null
                _viewModel.LocDanhSach().ConfigureAwait(false);
            }
        }
        private void FirstPage_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ChuyenTrangDau();
        }
        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ChuyenTrangTruoc();
        }
        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ChuyenTrangSau();
        }
        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ChuyenTrangCuoi();
        }
        private void PageSize_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (cboPageSize.SelectedItem != null)
            {
                ComboBoxItem selectedItem = cboPageSize.SelectedItem as ComboBoxItem;
                if (selectedItem != null && int.TryParse(selectedItem.Content.ToString(), out int pageSize))
                {
                    _viewModel.SoItemMoiTrang = pageSize;
                    _viewModel.CapNhatPhanTrang();
                }
            }
        }
       
        private void txtTimKiem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Lấy ngày bắt đầu và kết thúc từ DatePicker
                _viewModel.TuNgay = datePickerTuNgay.SelectedDate;
                _viewModel.DenNgay = datePickerDenNgay.SelectedDate;

                // Gọi phương thức tìm kiếm
                _viewModel.TimKiem().ConfigureAwait(false);
                e.Handled = true;
            }
        }
    }
}