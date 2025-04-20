using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Microsoft.IdentityModel.Tokens;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Services.BaoTri;
using Project_QLTS_DNC.Services.QLTaiSanService;
using Project_QLTS_DNC.Services.QLToanNha;

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
                if (_tatCaDuocChon != value)
                {
                    _tatCaDuocChon = value;
                    OnPropertyChanged(nameof(TatCaDuocChon));

                    // Áp dụng trạng thái được chọn cho tất cả các mục trong danh sách hiển thị
                    if (DsKiemKe != null)
                    {
                        foreach (var item in DsKiemKe)
                        {
                            item.IsSelected = value;
                        }
                    }
                }
            }
        }
        // Thêm vào DanhSachBaoTriViewModel.cs
        public void UpdateSelectAllState()
        {
            if (DsKiemKe != null && DsKiemKe.Count > 0)
            {
                // Kiểm tra nếu tất cả các item đều được chọn
                bool allSelected = DsKiemKe.All(item => item.IsSelected);

                // Cập nhật trạng thái TatCaDuocChon mà không gây ra vòng lặp vô hạn
                if (_tatCaDuocChon != allSelected)
                {
                    _tatCaDuocChon = allSelected;
                    OnPropertyChanged(nameof(TatCaDuocChon));
                }
            }
        }
        // Thêm vào DanhSachBaoTriViewModel.cs
        private void RegisterItemPropertyChanged()
        {
            if (DsKiemKe != null)
            {
                foreach (var item in DsKiemKe)
                {
                    // Đăng ký sự kiện
                    item.PropertyChanged += Item_PropertyChanged;
                }
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(KiemKeTaiSan.IsSelected))
            {
                // Cập nhật trạng thái chọn tất cả khi có item thay đổi
                UpdateSelectAllState();
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
        private readonly TaiSanService _taiSanService;
        private readonly PhongService _phongService;
        private ObservableCollection<KiemKeTaiSan> _dsKiemKeGoc;


        public DanhSachBaoTriViewModel(bool autoLoad = true)
        {
            // Khởi tạo các service
            _dsBaotriService = new DSBaoTriService();
            _taiSanService = new TaiSanService(); // Đảm bảo dòng này được thực thi
            _phongService = new PhongService();   // Đảm bảo dòng này được thực thi

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
                // Tải danh mục trước
                await LoadDanhMucAsync();

                // Lấy dữ liệu từ service
                var danhSach = await _dsBaotriService.GetKiemKeTaiSanAsync();
                try
                {
                    if (_taiSanService != null && _phongService != null)
                    {
                        var dsTaiSan = await _taiSanService.GetDanhSachTaiSanAsync();
                        var dsPhong = await _phongService.GetDanhSachPhongAsync();

                        // Cập nhật tên tài sản và tên phòng
                        foreach (var item in danhSach)
                        {
                            // Chuyển đổi int? sang int để so sánh
                            var taiSanId = item.MaTaiSan.GetValueOrDefault();
                            var phongId = item.MaPhong.GetValueOrDefault();
                            var dotKiemKeId = item.MaDotKiemKe.GetValueOrDefault();

                            var taiSan = dsTaiSan.FirstOrDefault(ts => ts.MaTaiSan == taiSanId);
                            if (taiSan != null)
                            {
                                item.TenTaiSan = taiSan.TenTaiSan;
                            }
                            else
                            {
                                item.TenTaiSan = $"Tài sản {taiSanId}";
                            }

                            var phong = dsPhong.FirstOrDefault(p => p.MaPhong == phongId);
                            if (phong != null)
                            {
                                item.TenPhong = phong.TenPhong;
                            }
                            else
                            {
                                item.TenPhong = $"Phòng {phongId}";
                            }

                            // Xử lý tên đợt kiểm kê (nếu có)
                            if (string.IsNullOrEmpty(item.TenDotKiemKe) && item.MaDotKiemKe.HasValue)
                            {
                                // Thiết lập giá trị mặc định
                                item.TenDotKiemKe = $"Đợt kiểm kê {dotKiemKeId}";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi để theo dõi
                    System.Diagnostics.Debug.WriteLine($"Lỗi khi cập nhật tên: {ex.Message}");
                    // Bỏ qua lỗi này để tránh làm hỏng toàn bộ hàm
                    // Vẫn hiển thị dữ liệu nhưng không có tên tài sản và tên phòng
                }

                // Lưu danh sách gốc
                _dsKiemKeGoc = new ObservableCollection<KiemKeTaiSan>(danhSach);

                // Cập nhật view - Sửa lỗi cú pháp ở đây
                if (_dsKiemKeView == null)
                {
                    _dsKiemKeView = new CollectionViewSource();
                }

                _dsKiemKeView.Source = _dsKiemKeGoc;  // Sửa lỗi cú pháp

                // Thiết lập filter ban đầu
                ICollectionView view = _dsKiemKeView.View;
                if (view != null)
                {
                    // Xóa filter cũ nếu có
                    if (view.Filter != null)
                    {
                        // Không có cách trực tiếp để xóa filter, nên tạo view mới
                        _dsKiemKeView = new CollectionViewSource();
                        _dsKiemKeView.Source = _dsKiemKeGoc;  // Sửa lỗi cú pháp
                        view = _dsKiemKeView.View;
                    }

                    // Thêm filter mới
                    view.Filter = item => FilterMatches((KiemKeTaiSan)item);
                }

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
            if (_dsKiemKeGoc == null)
                return;

            // Áp dụng bộ lọc để lấy danh sách đã lọc
            var filteredItems = ApplyFilterToCollection(_dsKiemKeGoc);

            // Tính lại tổng số trang
            int totalItems = filteredItems.Count;
            TongSoTrang = (totalItems + SoDongMoiTrang - 1) / SoDongMoiTrang;

            // Đảm bảo trang hiện tại không vượt quá tổng số trang
            if (TrangHienTai > TongSoTrang && TongSoTrang > 0)
                TrangHienTai = TongSoTrang;
            else if (TongSoTrang == 0)
                TrangHienTai = 1;

            // Thông báo thay đổi
            OnPropertyChanged(nameof(TrangHienTai));
            OnPropertyChanged(nameof(TongSoTrang));
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

            // Đăng ký sự kiện cho các item mới
            RegisterItemPropertyChanged();
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
        // Thêm vào class DanhSachBaoTriViewModel
        public async Task RefreshCurrentPageAsync()
        {
            await LoadDSKiemKeAsync();
            // Di chuyển đến trang hiện tại
            if (TrangHienTai > TongSoTrang)
                TrangHienTai = TongSoTrang;
            LoadPageData();
        }
        // Chuyển từ bool thành Task<bool> để có thể await
        public async Task<bool> XoaTaiSanDaChonAsync()
        {
            try
            {
                var selectedItems = GetSelectedItems();
                if (selectedItems.Count == 0)
                {
                    System.Windows.MessageBox.Show(
                        "Vui lòng chọn ít nhất một tài sản để xóa!",
                        "Thông báo",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information);
                    return false;
                }

                // Hiển thị thông báo xác nhận
                var result = System.Windows.MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa {selectedItems.Count} tài sản đã chọn không?",
                    "Xác nhận xóa",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.No)
                    return false;

                // Tạo bản sao của danh sách đã chọn để tránh lỗi
                var itemsToDelete = selectedItems.ToList();

                // Hiển thị loading
                System.Windows.MessageBox.Show(
                    "Đang xóa dữ liệu... Vui lòng chờ trong giây lát.",
                    "Đang xử lý",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);

                // Gọi service để xóa các tài sản đã chọn
                bool xoaThanhCong = await _dsBaotriService.XoaNhieuTaiSanAsync(itemsToDelete);

                if (xoaThanhCong)
                {
                    // Tải lại toàn bộ dữ liệu để đảm bảo đồng bộ với database
                    await LoadDSKiemKeAsync();

                    System.Windows.MessageBox.Show(
                        $"Đã xóa tài sản thành công!",
                        "Thông báo",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information);

                    return true;
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        "Xóa tài sản không thành công. Có thể do lỗi kết nối hoặc quyền truy cập. " +
                        "Vui lòng kiểm tra lại kết nối mạng và quyền hạn của tài khoản.",
                        "Lỗi",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);

                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Lỗi khi xóa tài sản: {ex.Message}\n\nChi tiết lỗi: {ex.StackTrace}",
                    "Lỗi",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);

                return false;
            }
        }


        private bool FilterMatches(KiemKeTaiSan item)
        {
            bool matchesSearchText = string.IsNullOrEmpty(TuKhoaTimKiem) ||
                                   item.MaTaiSan.ToString().Contains(TuKhoaTimKiem) ||
                                   item.MaDotKiemKe.ToString().Contains(TuKhoaTimKiem) ||
                                   (item.TenTaiSan?.Contains(TuKhoaTimKiem, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                   (item.ViTriThucTe?.Contains(TuKhoaTimKiem, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                   (item.GhiChu?.Contains(TuKhoaTimKiem, StringComparison.OrdinalIgnoreCase) ?? false);



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

            return matchesSearchText && matchesTinhTrang;
        }
        public class TaiSanService
        {
            public async Task<List<TaiSanModel>> GetDanhSachTaiSanAsync()
            {
                try
                {
                    var client = await SupabaseService.GetClientAsync();
                    if (client == null)
                        throw new Exception("Không thể kết nối Supabase Client");

                    var response = await client
                        .From<TaiSanModel>()
                        .Select("*")
                        .Order("ma_tai_san", Supabase.Postgrest.Constants.Ordering.Ascending)
                        .Get();

                    return response.Models;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi truy vấn dữ liệu tài sản: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return new List<TaiSanModel>();
                }
            }
        }
        public class PhongService
        {
            public async Task<List<Phong>> GetDanhSachPhongAsync()
            {
                try
                {
                    var client = await SupabaseService.GetClientAsync();
                    if (client == null)
                        throw new Exception("Không thể kết nối Supabase Client");

                    var response = await client
                        .From<Phong>()
                        .Select("*")
                        .Order("ma_phong", Supabase.Postgrest.Constants.Ordering.Ascending)
                        .Get();

                    return response.Models;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi truy vấn dữ liệu phòng: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return new List<Phong>();
                }
            }
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