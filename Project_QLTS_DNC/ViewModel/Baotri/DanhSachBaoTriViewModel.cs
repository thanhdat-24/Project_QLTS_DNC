using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Services.BaoTri;

namespace Project_QLTS_DNC.ViewModel.Baotri
{
    public class DanhSachBaoTriViewModel : INotifyPropertyChanged
    {
        #region Properties
        private ObservableCollection<KiemKeTaiSan> _dsKiemKe;
        public ObservableCollection<KiemKeTaiSan> DsKiemKe
        {
            get => _dsKiemKe;
            set
            {
                _dsKiemKe = value;
                OnPropertyChanged(nameof(DsKiemKe));
            }
        }

        private CollectionViewSource _dsKiemKeView;
        public ICollectionView DsKiemKeView => _dsKiemKeView?.View;

        private ObservableCollection<LoaiBaoTri> _dsLoaiBaoTri;
        public ObservableCollection<LoaiBaoTri> DsLoaiBaoTri
        {
            get => _dsLoaiBaoTri;
            set
            {
                _dsLoaiBaoTri = value;
                OnPropertyChanged(nameof(DsLoaiBaoTri));
            }
        }

        private ObservableCollection<NhomTaiSan> _dsNhomTaiSan;
        public ObservableCollection<NhomTaiSan> DsNhomTaiSan
        {
            get => _dsNhomTaiSan;
            set
            {
                _dsNhomTaiSan = value;
                OnPropertyChanged(nameof(DsNhomTaiSan));
            }
        }

        private string _tuKhoaTimKiem;
        public string TuKhoaTimKiem
        {
            get => _tuKhoaTimKiem;
            set
            {
                _tuKhoaTimKiem = value;
                OnPropertyChanged(nameof(TuKhoaTimKiem));
            }
        }

        private string _loaiBaoTriDuocChon;
        public string LoaiBaoTriDuocChon
        {
            get => _loaiBaoTriDuocChon;
            set
            {
                _loaiBaoTriDuocChon = value;
                OnPropertyChanged(nameof(LoaiBaoTriDuocChon));
                ApplyFilter();
            }
        }

        private string _nhomTaiSanDuocChon;
        public string NhomTaiSanDuocChon
        {
            get => _nhomTaiSanDuocChon;
            set
            {
                _nhomTaiSanDuocChon = value;
                OnPropertyChanged(nameof(NhomTaiSanDuocChon));
                ApplyFilter();
            }
        }

        private string _tinhTrangDuocChon;
        public string TinhTrangDuocChon
        {
            get => _tinhTrangDuocChon;
            set
            {
                _tinhTrangDuocChon = value;
                OnPropertyChanged(nameof(TinhTrangDuocChon));
                ApplyFilter();
            }
        }

        private bool _tatCaDuocChon;
        public bool TatCaDuocChon
        {
            get => _tatCaDuocChon;
            set
            {
                _tatCaDuocChon = value;
                OnPropertyChanged(nameof(TatCaDuocChon));

                // Áp dụng trạng thái được chọn cho tất cả các mục
                if (DsKiemKe != null)
                {
                    foreach (var item in DsKiemKe)
                    {
                        item.IsSelected = value;
                    }
                }
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

        // Thông tin phân trang
        private int _trangHienTai = 1;
        public int TrangHienTai
        {
            get => _trangHienTai;
            set
            {
                _trangHienTai = value;
                OnPropertyChanged(nameof(TrangHienTai));
                LoadPageData();
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

        private int _soDongMoiTrang = 10;
        public int SoDongMoiTrang
        {
            get => _soDongMoiTrang;
            set
            {
                _soDongMoiTrang = value;
                OnPropertyChanged(nameof(SoDongMoiTrang));
                UpdatePagination();
            }
        }
        #endregion

        private readonly DSBaoTriService _dsBaotriService;
        private ObservableCollection<KiemKeTaiSan> _dsKiemKeGoc;

        public DanhSachBaoTriViewModel(bool autoLoad = true)
        {
            _dsBaotriService = new DSBaoTriService();
            _dsKiemKeView = new CollectionViewSource();
            DsKiemKe = new ObservableCollection<KiemKeTaiSan>();
            _dsKiemKeGoc = new ObservableCollection<KiemKeTaiSan>();

            // Khởi tạo các giá trị mặc định
            TuKhoaTimKiem = string.Empty;
            LoaiBaoTriDuocChon = "Tất cả loại";
            NhomTaiSanDuocChon = "Tất cả nhóm";
            TinhTrangDuocChon = "Dưới 50%";

            // Chỉ tự động tải dữ liệu nếu autoLoad = true
            if (autoLoad)
            {
                _ = LoadDSKiemKeAsync();
                _ = LoadDanhMucAsync();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task LoadDSKiemKeAsync()
        {
            try
            {
                IsLoading = true;

                // Lấy dữ liệu từ service
                var danhSach = await _dsBaotriService.GetKiemKeTaiSanAsync();

                // Lưu danh sách gốc
                _dsKiemKeGoc = new ObservableCollection<KiemKeTaiSan>(danhSach);

                // Cập nhật view
                _dsKiemKeView.Source = _dsKiemKeGoc;

                // Thiết lập filter ban đầu
                _dsKiemKeView.Filter += ApplyFilterToItem;

                OnPropertyChanged(nameof(DsKiemKeView));

                // Cập nhật phân trang
                UpdatePagination();

                // Tải dữ liệu trang đầu tiên
                LoadPageData();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdatePagination()
        {
            if (_dsKiemKeGoc == null || _dsKiemKeGoc.Count == 0)
            {
                TongSoTrang = 1;
                return;
            }

            // Tính số trang dựa trên số lượng item và số item trên mỗi trang
            TongSoTrang = (_dsKiemKeGoc.Count + SoDongMoiTrang - 1) / SoDongMoiTrang;

            // Đảm bảo trang hiện tại không vượt quá tổng số trang
            if (TrangHienTai > TongSoTrang)
                TrangHienTai = TongSoTrang;
        }

        private void LoadPageData()
        {
            if (_dsKiemKeGoc == null || _dsKiemKeGoc.Count == 0)
            {
                DsKiemKe = new ObservableCollection<KiemKeTaiSan>();
                return;
            }

            // Áp dụng bộ lọc
            var filteredItems = ApplyFilterToCollection(_dsKiemKeGoc);

            // Tính lại tổng số trang
            int totalItems = filteredItems.Count;
            TongSoTrang = (totalItems + SoDongMoiTrang - 1) / SoDongMoiTrang;

            // Đảm bảo trang hiện tại không vượt quá tổng số trang
            if (TrangHienTai > TongSoTrang && TongSoTrang > 0)
                TrangHienTai = TongSoTrang;

            // Tính vị trí bắt đầu của trang
            int startIndex = (TrangHienTai - 1) * SoDongMoiTrang;

            // Lấy dữ liệu cho trang hiện tại
            var pageItems = filteredItems
                .Skip(startIndex)
                .Take(SoDongMoiTrang)
                .ToList();

            // Cập nhật danh sách hiển thị
            DsKiemKe = new ObservableCollection<KiemKeTaiSan>(pageItems);
        }

        private async Task LoadDanhMucAsync()
        {
            try
            {
                // Lấy danh sách loại bảo trì từ database
                var loaiBaoTriService = new LoaiBaoTriService();
                var dsLoai = await loaiBaoTriService.GetLoaiBaoTriAsync();
                DsLoaiBaoTri = new ObservableCollection<LoaiBaoTri>(dsLoai);

                // Lấy danh sách nhóm tài sản từ database
                var nhomTaiSanService = new NhomTaiSanService();
                var dsNhom = await nhomTaiSanService.GetNhomTaiSanAsync();
                DsNhomTaiSan = new ObservableCollection<NhomTaiSan>(dsNhom);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi tải danh mục: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public void ApplyFilter()
        {
            // Refresh view để áp dụng bộ lọc mới
            DsKiemKeView?.Refresh();

            // Cập nhật lại dữ liệu phân trang
            LoadPageData();
        }

        private ObservableCollection<KiemKeTaiSan> ApplyFilterToCollection(ObservableCollection<KiemKeTaiSan> collection)
        {
            var result = new ObservableCollection<KiemKeTaiSan>();

            foreach (var item in collection)
            {
                if (FilterMatches(item))
                    result.Add(item);
            }

            return result;
        }

        private void ApplyFilterToItem(object sender, FilterEventArgs e)
        {
            if (e.Item is KiemKeTaiSan item)
            {
                e.Accepted = FilterMatches(item);
            }
        }

        private bool FilterMatches(KiemKeTaiSan item)
        {
            bool matchesSearchText = string.IsNullOrEmpty(TuKhoaTimKiem) ||
                                     item.MaTaiSan.ToString().Contains(TuKhoaTimKiem) ||
                                     (item.ViTriThucTe?.Contains(TuKhoaTimKiem) ?? false) ||
                                     (item.GhiChu?.Contains(TuKhoaTimKiem) ?? false);

            bool matchesLoaiBaoTri = LoaiBaoTriDuocChon == "Tất cả loại" ||
                                     item.LoaiBaoTri == LoaiBaoTriDuocChon;

            bool matchesNhomTaiSan = NhomTaiSanDuocChon == "Tất cả nhóm" ||
                                     item.NhomTaiSan == NhomTaiSanDuocChon;

            bool matchesTinhTrang = true;
            if (TinhTrangDuocChon == "Dưới 50%")
            {
                // Kiểm tra tình trạng < 50%
                if (int.TryParse(item.TinhTrang?.Replace("%", ""), out int tinhTrang))
                {
                    matchesTinhTrang = tinhTrang < 50;
                }
            }
            else if (TinhTrangDuocChon == "Trên 50%")
            {
                // Kiểm tra tình trạng >= 50%
                if (int.TryParse(item.TinhTrang?.Replace("%", ""), out int tinhTrang))
                {
                    matchesTinhTrang = tinhTrang >= 50;
                }
            }

            return matchesSearchText && matchesLoaiBaoTri && matchesNhomTaiSan && matchesTinhTrang;
        }

        public void ChuyenTrangTruoc()
        {
            if (TrangHienTai > 1)
                TrangHienTai--;
        }

        public void ChuyenTrangSau()
        {
            if (TrangHienTai < TongSoTrang)
                TrangHienTai++;
        }

        public void ChuyenDenTrangDau()
        {
            TrangHienTai = 1;
        }

        public void ChuyenDenTrangCuoi()
        {
            TrangHienTai = TongSoTrang;
        }

        public void ChuyenDenTrang(int trang)
        {
            if (trang >= 1 && trang <= TongSoTrang)
                TrangHienTai = trang;
        }

        public ObservableCollection<KiemKeTaiSan> GetSelectedItems()
        {
            return new ObservableCollection<KiemKeTaiSan>(
                DsKiemKe.Where(item => item.IsSelected)
            );
        }
    }

    public class LoaiBaoTriService
    {
        public async Task<List<LoaiBaoTri>> GetLoaiBaoTriAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    throw new Exception("Không thể kết nối Supabase Client");

                var response = await client.From<LoaiBaoTri>().Get();
                return response.Models;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi truy vấn dữ liệu loại bảo trì: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<LoaiBaoTri>();
            }
        }
    }

    public class NhomTaiSanService
    {
        public async Task<List<NhomTaiSan>> GetNhomTaiSanAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    throw new Exception("Không thể kết nối Supabase Client");

                var response = await client.From<NhomTaiSan>().Get();
                return response.Models;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi truy vấn dữ liệu nhóm tài sản: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<NhomTaiSan>();
            }
        }
    }
}