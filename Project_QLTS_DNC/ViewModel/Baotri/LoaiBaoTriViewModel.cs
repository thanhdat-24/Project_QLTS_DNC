using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Services.BaoTri;
using Project_QLTS_DNC.View.BaoTri;

namespace Project_QLTS_DNC.ViewModel.Baotri
{
    public class LoaiBaoTriViewModel : INotifyPropertyChanged
    {
        private readonly Project_QLTS_DNC.Services.BaoTri.LoaiBaoTriService _loaiBaoTriService;

        #region Properties
        private ObservableCollection<LoaiBaoTri> _listLoaiBaoTri;
        public ObservableCollection<LoaiBaoTri> listLoaiBaoTri
        {
            get => _listLoaiBaoTri;
            set
            {
                _listLoaiBaoTri = value;
                OnPropertyChanged(nameof(listLoaiBaoTri));
            }
        }
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        private ObservableCollection<LoaiBaoTri> _allLoaiBaoTri;
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                // Thực hiện tìm kiếm tự động khi người dùng nhập
                if (TimKiemCommand != null)
                {
                    TimKiemCommand.Execute(_searchText);
                }
            }
        }

        private bool _isAdmin = true; // Giả sử người dùng hiện tại là admin
        public bool IsAdmin
        {
            get => _isAdmin;
            set
            {
                _isAdmin = value;
                OnPropertyChanged(nameof(IsAdmin));
            }
        }

        // Phân trang
        private int _trangHienTai = 1;
        public int TrangHienTai
        {
            get => _trangHienTai;
            set
            {
                _trangHienTai = value;
                OnPropertyChanged(nameof(TrangHienTai));
                ApplyPaging();
            }
        }

        private int _tongSoTrang = 1;
        public int TongSoTrang
        {
            get => _tongSoTrang;
            set
            {
                _tongSoTrang = value;
                OnPropertyChanged(nameof(TongSoTrang));
            }
        }

        private int _soLuongTrenTrang = 10;
        public int SoLuongTrenTrang
        {
            get => _soLuongTrenTrang;
            set
            {
                _soLuongTrenTrang = value;
                OnPropertyChanged(nameof(SoLuongTrenTrang));
                TrangHienTai = 1; // Reset về trang đầu khi thay đổi số lượng hiển thị
                CalculateTotalPages();
                ApplyPaging();
            }
        }

        private int _tongSoBanGhi = 0;
        public int TongSoBanGhi
        {
            get => _tongSoBanGhi;
            set
            {
                _tongSoBanGhi = value;
                OnPropertyChanged(nameof(TongSoBanGhi));
                CalculateTotalPages();
            }
        }

        private bool _showPagination = true;
        public bool ShowPagination
        {
            get => _showPagination;
            set
            {
                _showPagination = value;
                OnPropertyChanged(nameof(ShowPagination));
            }
        }

        // Properties cho form thêm/sửa
        private LoaiBaoTri _selectedLoaiBaoTri;
        public LoaiBaoTri SelectedLoaiBaoTri
        {
            get => _selectedLoaiBaoTri;
            set
            {
                _selectedLoaiBaoTri = value;
                OnPropertyChanged(nameof(SelectedLoaiBaoTri));
            }
        }
        #endregion

        #region Commands
        public ICommand TimKiemCommand { get; private set; }
        public ICommand RefreshDataCommand { get; private set; }
        public ICommand ThemMoiCommand { get; private set; }
        public ICommand SuaCommand { get; private set; }
        public ICommand XoaCommand { get; private set; }
        public ICommand VeTrangDauCommand { get; private set; }
        public ICommand VeTrangTruocCommand { get; private set; }
        public ICommand DenTrangSauCommand { get; private set; }
        public ICommand DenTrangCuoiCommand { get; private set; }
        public ICommand PageSizeChangedCommand { get; private set; }
        #endregion

        public LoaiBaoTriViewModel()
        {

            _loaiBaoTriService = new Project_QLTS_DNC.Services.BaoTri.LoaiBaoTriService();

            // Khởi tạo commands
            TimKiemCommand = new RelayCommand<string>(ExecuteTimKiem);
            RefreshDataCommand = new RelayCommand<object>(param => ExecuteRefresh());
            ThemMoiCommand = new RelayCommand<object>(param => ExecuteThemMoi());
            SuaCommand = new RelayCommand<LoaiBaoTri>(ExecuteSua);
            XoaCommand = new RelayCommand<LoaiBaoTri>(ExecuteXoa);

            // Các lệnh phân trang
            VeTrangDauCommand = new RelayCommand<object>(param => TrangHienTai = 1);
            VeTrangTruocCommand = new RelayCommand<object>(param => TrangHienTai = Math.Max(1, TrangHienTai - 1));
            DenTrangSauCommand = new RelayCommand<object>(param => TrangHienTai = Math.Min(TongSoTrang, TrangHienTai + 1));
            DenTrangCuoiCommand = new RelayCommand<object>(param => TrangHienTai = TongSoTrang);
            PageSizeChangedCommand = new RelayCommand<int>(size => SoLuongTrenTrang = size);

            // Load dữ liệu ban đầu
            LoadData();
        }

        #region Methods
        public async void LoadData()
        {
            try
            {
                // Sửa lỗi: Phương thức đúng là LayDanhSachLoaiBT()
                var dsLoaiBaoTri = await _loaiBaoTriService.LayDanhSachLoaiBT();
                _allLoaiBaoTri = new ObservableCollection<LoaiBaoTri>(dsLoaiBaoTri);
                TongSoBanGhi = _allLoaiBaoTri.Count;

                // Hiển thị phân trang chỉ khi có nhiều bản ghi
                ShowPagination = TongSoBanGhi > SoLuongTrenTrang;
                CalculateTotalPages();
                ApplyPaging();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExecuteTimKiem(string searchText)
        {
            try
            {
                IsLoading = true;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    // Nếu không có từ khóa tìm kiếm, hiển thị tất cả
                    var allData = await _loaiBaoTriService.LayDanhSachLoaiBT();
                    _allLoaiBaoTri = new ObservableCollection<LoaiBaoTri>(allData);
                }
                else
                {
                    // Sử dụng phương thức TimKiemLoaiBaoTri từ service
                    var searchResult = await _loaiBaoTriService.TimKiemLoaiBaoTri(searchText);
                    _allLoaiBaoTri = new ObservableCollection<LoaiBaoTri>(searchResult);
                }

                TongSoBanGhi = _allLoaiBaoTri.Count;
                // Reset về trang đầu sau khi tìm kiếm
                TrangHienTai = 1;
                CalculateTotalPages();
                ApplyPaging();
                ShowPagination = TongSoBanGhi > SoLuongTrenTrang;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteRefresh()
        {
            SearchText = string.Empty;
            LoadData();
        }

        private void ExecuteThemMoi()
        {
            try
            {
                // Hiển thị form thêm mới
                var dialogForm = new LoaiBaoTriDialog();
                if (dialogForm.ShowDialog() == true && dialogForm.LoaiBaoTriResult != null)
                {
                    // Lưu dữ liệu và refresh
                    _ = ThemLoaiBaoTri(dialogForm.LoaiBaoTriResult);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở form thêm mới: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteSua(LoaiBaoTri loaiBaoTri)
        {
            try
            {
                if (loaiBaoTri == null) return;

                // Hiển thị form chỉnh sửa
                var dialogForm = new LoaiBaoTriDialog(loaiBaoTri);
                if (dialogForm.ShowDialog() == true && dialogForm.LoaiBaoTriResult != null)
                {
                    // Lưu dữ liệu và refresh
                    _ = CapNhatLoaiBaoTri(dialogForm.LoaiBaoTriResult);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở form chỉnh sửa: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteXoa(LoaiBaoTri loaiBaoTri)
        {
            try
            {
                if (loaiBaoTri == null) return;

                var result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa loại bảo trì '{loaiBaoTri.TenLoai}'?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _ = XoaLoaiBaoTri(loaiBaoTri.MaLoaiBaoTri);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ThemLoaiBaoTri(LoaiBaoTri loaiBaoTri)
        {
            try
            {
                // Gọi phương thức ThemLoaiBaoTri từ service
                await _loaiBaoTriService.ThemLoaiBaoTri(loaiBaoTri);
                MessageBox.Show("Thêm loại bảo trì thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData(); // Tải lại dữ liệu
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm loại bảo trì: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CapNhatLoaiBaoTri(LoaiBaoTri loaiBaoTri)
        {
            try
            {
                // Gọi phương thức CapNhatLoaiBaoTri từ service
                await _loaiBaoTriService.CapNhatLoaiBaoTri(loaiBaoTri);
                MessageBox.Show("Cập nhật loại bảo trì thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData(); // Tải lại dữ liệu
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật loại bảo trì: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task XoaLoaiBaoTri(int maLoaiBaoTri)
        {
            try
            {
                // Gọi phương thức XoaLoaiBaoTri từ service
                await _loaiBaoTriService.XoaLoaiBaoTri(maLoaiBaoTri);
                MessageBox.Show("Xóa loại bảo trì thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData(); // Tải lại dữ liệu
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa loại bảo trì: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateTotalPages()
        {
            TongSoTrang = (TongSoBanGhi + SoLuongTrenTrang - 1) / SoLuongTrenTrang;
            if (TongSoTrang == 0) TongSoTrang = 1; // Tối thiểu 1 trang
        }

        private void ApplyPaging()
        {
            if (_allLoaiBaoTri == null || _allLoaiBaoTri.Count == 0)
            {
                listLoaiBaoTri = new ObservableCollection<LoaiBaoTri>();
                return;
            }

            var pagedItems = _allLoaiBaoTri
                .Skip((TrangHienTai - 1) * SoLuongTrenTrang)
                .Take(SoLuongTrenTrang)
                .ToList();

            listLoaiBaoTri = new ObservableCollection<LoaiBaoTri>(pagedItems);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    // RelayCommand để gọi các lệnh trong ViewModel
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}