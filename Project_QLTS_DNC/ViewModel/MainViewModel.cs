using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Commands;
using System.Windows.Input;
using System.Windows;

namespace Project_QLTS_DNC.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Product> _products;
        private ObservableCollection<Product> _allProducts;
        private string _searchText;
        private string _selectedMaPhong;
        private string _selectedMaNhom;

        public ObservableCollection<Product> Products
        {
            get { return _products; }
            set { SetProperty(ref _products, value); }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (SetProperty(ref _searchText, value) && string.IsNullOrEmpty(value))
                {
                    // Auto-refresh list when search is cleared
                    SearchProducts();
                }
            }
        }

        public string SelectedMaPhong
        {
            get { return _selectedMaPhong; }
            set { SetProperty(ref _selectedMaPhong, value); }
        }

        public string SelectedMaNhom
        {
            get { return _selectedMaNhom; }
            set { SetProperty(ref _selectedMaNhom, value); }
        }

        public ICommand SearchCommand { get; private set; }
        public ICommand FilterCommand { get; private set; }
        public ICommand AddProductCommand { get; private set; }
        public ICommand ExportExcelCommand { get; private set; }

        public MainViewModel()
        {
            // Khởi tạo commands
            SearchCommand = new RelayCommand(param => SearchProducts());
            FilterCommand = new RelayCommand(param => FilterProducts());
            AddProductCommand = new RelayCommand(param => AddProduct());
            ExportExcelCommand = new RelayCommand(param => ExportToExcel());

            // Khởi tạo dữ liệu mẫu
            _allProducts = new ObservableCollection<Product>
            {
                new Product { MaSP = "SP001", TenSP = "Laptop Dell XPS 13", MaPhong = "P001", MaNhom = "N001", DonGia = 25000000, SoLuong = 10 },
                new Product { MaSP = "SP002", TenSP = "Máy in HP LaserJet", MaPhong = "P001", MaNhom = "N002", DonGia = 5000000, SoLuong = 5 },
                new Product { MaSP = "SP003", TenSP = "Màn hình Dell 27 inch", MaPhong = "P002", MaNhom = "N001", DonGia = 8000000, SoLuong = 15 },
                new Product { MaSP = "SP004", TenSP = "Bàn phím Logitech", MaPhong = "P002", MaNhom = "N003", DonGia = 1200000, SoLuong = 20 },
                new Product { MaSP = "SP005", TenSP = "Chuột không dây", MaPhong = "P003", MaNhom = "N003", DonGia = 450000, SoLuong = 30 }
            };

            // Hiển thị toàn bộ dữ liệu ban đầu
            Products = new ObservableCollection<Product>(_allProducts);

            // Mặc định chọn "Tất cả"
            SelectedMaPhong = "Tất cả";
            SelectedMaNhom = "Tất cả";
        }

        public void SearchProducts()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Nếu ô tìm kiếm trống, áp dụng bộ lọc hiện tại
                FilterProducts();
                return;
            }

            var searchText = SearchText.ToLower();
            var filteredList = new ObservableCollection<Product>(
                _allProducts.Where(p =>
                    p.MaSP.ToLower().Contains(searchText) ||
                    p.TenSP.ToLower().Contains(searchText))
            );

            // Áp dụng thêm bộ lọc hiện tại
            if (!string.IsNullOrEmpty(SelectedMaPhong) && SelectedMaPhong != "Tất cả")
            {
                filteredList = new ObservableCollection<Product>(
                    filteredList.Where(p => p.MaPhong == SelectedMaPhong)
                );
            }

            if (!string.IsNullOrEmpty(SelectedMaNhom) && SelectedMaNhom != "Tất cả")
            {
                filteredList = new ObservableCollection<Product>(
                    filteredList.Where(p => p.MaNhom == SelectedMaNhom)
                );
            }

            Products = filteredList;
        }

        public void FilterProducts()
        {
            var filteredList = _allProducts.AsEnumerable();

            if (!string.IsNullOrEmpty(SelectedMaPhong) && SelectedMaPhong != "Tất cả")
            {
                filteredList = filteredList.Where(p => p.MaPhong == SelectedMaPhong);
            }

            if (!string.IsNullOrEmpty(SelectedMaNhom) && SelectedMaNhom != "Tất cả")
            {
                filteredList = filteredList.Where(p => p.MaNhom == SelectedMaNhom);
            }

            // Nếu đang tìm kiếm, áp dụng thêm điều kiện tìm kiếm
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchText = SearchText.ToLower();
                filteredList = filteredList.Where(p =>
                    p.MaSP.ToLower().Contains(searchText) ||
                    p.TenSP.ToLower().Contains(searchText)
                );
            }

            Products = new ObservableCollection<Product>(filteredList);
        }

        private void AddProduct()
        {
            // Mã logic thêm sản phẩm ở đây
            MessageBox.Show("Chức năng đang được phát triển", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportToExcel()
        {
            // Mã logic xuất Excel ở đây
            MessageBox.Show("Chức năng đang được phát triển", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}