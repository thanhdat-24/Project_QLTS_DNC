using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.Collections.Generic;

namespace Project_QLTS_DNC.ViewModel.Baotri
{
    public class LichSuBaoTriViewModel : INotifyPropertyChanged
    {
        private readonly LichSuBaoTriService _lichSuService;
        private readonly PhieuBaoTriService _phieuBaoTriService;
        private ObservableCollection<LichSuBaoTriItem> _danhSachLichSu;
        private List<int> _filteredMaTaiSanList;

        public ObservableCollection<LichSuBaoTriItem> DanhSachLichSu
        {
            get => _danhSachLichSu;
            set
            {
                _danhSachLichSu = value;
                OnPropertyChanged(nameof(DanhSachLichSu));
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

        // Constructor gốc
        public LichSuBaoTriViewModel()
        {
            _lichSuService = new LichSuBaoTriService();
            _phieuBaoTriService = new PhieuBaoTriService();
            DanhSachLichSu = new ObservableCollection<LichSuBaoTriItem>();
            _filteredMaTaiSanList = null; // Không có bộ lọc
        }

        // Constructor mới hỗ trợ lọc theo danh sách mã tài sản
        public LichSuBaoTriViewModel(List<int> filteredMaTaiSanList)
        {
            _lichSuService = new LichSuBaoTriService();
            _phieuBaoTriService = new PhieuBaoTriService();
            DanhSachLichSu = new ObservableCollection<LichSuBaoTriItem>();
            _filteredMaTaiSanList = filteredMaTaiSanList;

            // Log để kiểm tra
            if (_filteredMaTaiSanList != null && _filteredMaTaiSanList.Count > 0)
            {
                Console.WriteLine($"Đã khởi tạo ViewModel với {_filteredMaTaiSanList.Count} mã tài sản để lọc");
            }
        }

        public async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                // Lấy danh sách lịch sử
                var danhSachLichSu = await _lichSuService.GetLichSuBaoTriAsync();

                // In log để kiểm tra
                Console.WriteLine($"Đã tải được {danhSachLichSu.Count} bản ghi lịch sử từ cơ sở dữ liệu");

                // Lấy thống kê số lần bảo trì
                var soLanBaoTriDict = await _lichSuService.CountBaoTriByTaiSanAsync();

                DanhSachLichSu.Clear();

                // Lọc danh sách theo mã tài sản nếu có
                if (_filteredMaTaiSanList != null && _filteredMaTaiSanList.Count > 0)
                {
                    danhSachLichSu = danhSachLichSu.Where(ls =>
                        ls.MaTaiSan.HasValue && _filteredMaTaiSanList.Contains(ls.MaTaiSan.Value)).ToList();

                    Console.WriteLine($"Đã lọc còn {danhSachLichSu.Count} bản ghi theo {_filteredMaTaiSanList.Count} mã tài sản");
                }

                // Sắp xếp theo thời gian giảm dần (mới nhất lên đầu)
                var sortedLichSu = danhSachLichSu.OrderByDescending(ls => ls.NgayThucHien);

                // Log kiểm tra số lượng sau khi sắp xếp
                Console.WriteLine($"Số lượng bản ghi sau khi sắp xếp: {sortedLichSu.Count()}");

                // Hiển thị từng tài sản riêng biệt
                foreach (var lichSu in sortedLichSu)
                {
                    // Log cho từng bản ghi để debug
                    Console.WriteLine($"Xử lý bản ghi: MaTaiSan={lichSu.MaTaiSan}, TenTaiSan={lichSu.TenTaiSan}, NgayThucHien={lichSu.NgayThucHien}");

                    int soLanBaoTri = 0;

                    // Lấy số lần bảo trì cho tài sản này nếu có
                    if (lichSu.MaTaiSan.HasValue && soLanBaoTriDict.ContainsKey(lichSu.MaTaiSan.Value))
                    {
                        soLanBaoTri = soLanBaoTriDict[lichSu.MaTaiSan.Value];
                    }

                    // Tạo danh sách mã tài sản
                    List<int> danhSachMaTaiSan = new List<int>();
                    if (lichSu.MaTaiSan.HasValue)
                    {
                        danhSachMaTaiSan.Add(lichSu.MaTaiSan.Value);
                    }

                    // Tạo đối tượng hiển thị chi tiết cho mỗi tài sản
                    var lichSuItem = new LichSuBaoTriItem
                    {
                        LichSu = lichSu,
                        SoLanBaoTri = soLanBaoTri,
                        SoLuongTaiSan = 1, // Mỗi dòng hiển thị 1 tài sản
                        DanhSachMaTaiSan = danhSachMaTaiSan,
                        ChiTietTaiSan = $"{lichSu.TenTaiSan} (SN: {lichSu.SoSeri})"
                    };

                    // Thêm vào danh sách hiển thị
                    DanhSachLichSu.Add(lichSuItem);

                    // Log thông báo thêm thành công
                    Console.WriteLine($"Đã thêm lichSuItem vào DanhSachLichSu, count hiện tại: {DanhSachLichSu.Count}");
                }

                // Log kiểm tra kết quả cuối cùng
                Console.WriteLine($"Hoàn tất tải dữ liệu. Tổng số bản ghi: {DanhSachLichSu.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadDataAsync Exception: {ex.Message}");
                Console.WriteLine($"Chi tiết: {ex}");
                MessageBox.Show($"Lỗi khi tải dữ liệu lịch sử: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class LichSuBaoTriItem
    {
        public LichSuBaoTri LichSu { get; set; }
        public int SoLanBaoTri { get; set; }
        public int SoLuongTaiSan { get; set; }
        public List<int> DanhSachMaTaiSan { get; set; } = new List<int>();
        public string ChiTietTaiSan { get; set; }
    }
}