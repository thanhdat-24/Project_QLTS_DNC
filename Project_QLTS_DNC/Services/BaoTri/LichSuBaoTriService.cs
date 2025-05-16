using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Models.NhanVien;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Supabase.Postgrest.Extensions;
using static Supabase.Postgrest.Constants;
using Project_QLTS_DNC.Services;

namespace Project_QLTS_DNC.ViewModel.Baotri
{
    public class LichSuBaoTriViewModel : INotifyPropertyChanged
    {
        private readonly LichSuBaoTriService _lichSuService = new LichSuBaoTriService();
        private readonly PhieuBaoTriService _phieuService = new PhieuBaoTriService();
        private List<int> _filteredMaTaiSanList;

        // Danh sách hiển thị trên giao diện
        private ObservableCollection<LichSuBaoTriExtended> _danhSachLichSu = new ObservableCollection<LichSuBaoTriExtended>();
        public ObservableCollection<LichSuBaoTriExtended> DanhSachLichSu
        {
            get => _danhSachLichSu;
            set
            {
                if (_danhSachLichSu != value)
                {
                    _danhSachLichSu = value;
                    OnPropertyChanged(nameof(DanhSachLichSu));
                }
            }
        }

        // Danh sách gốc để phục vụ tìm kiếm, lọc
        private List<LichSuBaoTriExtended> _allLichSu = new List<LichSuBaoTriExtended>();

        // Constructor mặc định
        public LichSuBaoTriViewModel()
        {
            _filteredMaTaiSanList = null;
        }

        // Constructor với danh sách mã tài sản cần lọc
        public LichSuBaoTriViewModel(List<int> maTaiSanList)
        {
            _filteredMaTaiSanList = maTaiSanList;
        }

        // Phương thức tải dữ liệu
        public async Task LoadDataAsync()
        {
            try
            {
                DanhSachLichSu.Clear();
                _allLichSu.Clear();

                // Lấy danh sách lịch sử bảo trì
                var lichSuList = await _lichSuService.GetLichSuBaoTriAsync(_filteredMaTaiSanList);
                Console.WriteLine($"Tải được {lichSuList.Count} bản ghi lịch sử bảo trì");

                // Lấy danh sách tất cả các phiếu bảo trì
                var allPhieuBaoTri = await _phieuService.GetPhieuBaoTriAsync();

                // Tạo từ điển với key là mã tài sản, value là số lần tài sản đó đã được bảo trì
                var baoTriCountDict = allPhieuBaoTri
                    .Where(p => p.MaTaiSan.HasValue)
                    .GroupBy(p => p.MaTaiSan.Value)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Lấy thông tin phòng ban của nhân viên (nếu cần)
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    MessageBox.Show("Không thể kết nối đến cơ sở dữ liệu!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var maNVList = lichSuList
                    .Where(l => l.MaNguoiThucHien.HasValue)
                    .Select(l => l.MaNguoiThucHien.Value)
                    .Distinct()
                    .ToList();

                // Dictionary lưu thông tin nhân viên
                var nhanVienDict = new Dictionary<int, NhanVienModel>();

                if (maNVList.Any())
                {
                    // Lấy thông tin nhân viên
                    var nhanVienQuery = client.Postgrest.Table<NhanVienModel>();
                    var nhanVienFilter = nhanVienQuery.Filter("ma_nv", Operator.In, maNVList);
                    var nhanVienResponse = await nhanVienFilter.Get();

                    nhanVienDict = nhanVienResponse.Models.ToDictionary(nv => nv.MaNV);
                }

                // Tạo danh sách mở rộng
                var extendedList = new List<LichSuBaoTriExtended>();

                // Tạo danh sách hiển thị và thêm STT
                int stt = 1;
                foreach (var lichSu in lichSuList)
                {
                    var extendedItem = new LichSuBaoTriExtended(lichSu)
                    {
                        STT = stt++
                    };

                    // Nếu có mã tài sản, tính số lần bảo trì
                    if (lichSu.MaTaiSan.HasValue && baoTriCountDict.ContainsKey(lichSu.MaTaiSan.Value))
                    {
                        extendedItem.SoLanBaoTri = baoTriCountDict[lichSu.MaTaiSan.Value];
                    }

                    // Lấy thông tin nhân viên
                    if (lichSu.MaNguoiThucHien.HasValue && nhanVienDict.ContainsKey(lichSu.MaNguoiThucHien.Value))
                    {
                        var nv = nhanVienDict[lichSu.MaNguoiThucHien.Value];
                        extendedItem.PhongBan = $"Phòng ban: {nv.MaPB}"; // Thay bằng tên phòng ban thực tế nếu có

                        // Cập nhật chi tiết hoạt động với thông tin nhân viên
                        extendedItem.ChiTietHoatDong += $"\nEmail: {nv.Email}\nSĐT: {nv.SDT}";
                    }

                    extendedList.Add(extendedItem);
                }

                // Cập nhật danh sách
                _allLichSu = extendedList;

                // Cập nhật ObservableCollection từ danh sách đã xử lý
                DanhSachLichSu = new ObservableCollection<LichSuBaoTriExtended>(extendedList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tải dữ liệu lịch sử bảo trì: {ex.Message}");
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Phương thức tìm kiếm theo từ khóa
        public void FilterByKeyword(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                // Hiển thị tất cả
                DanhSachLichSu = new ObservableCollection<LichSuBaoTriExtended>(_allLichSu);
                return;
            }

            keyword = keyword.ToLower();
            var filteredList = _allLichSu.Where(item =>
                (item.TenTaiSan?.ToLower().Contains(keyword) ?? false) ||
                (item.SoSeri?.ToLower().Contains(keyword) ?? false) ||
                (item.TenNguoiThucHien?.ToLower().Contains(keyword) ?? false) ||
                (item.TinhTrangTaiSan?.ToLower().Contains(keyword) ?? false) ||
                (item.GhiChu?.ToLower().Contains(keyword) ?? false)
            ).ToList();

            // Cập nhật STT
            UpdateSTT(filteredList);

            DanhSachLichSu = new ObservableCollection<LichSuBaoTriExtended>(filteredList);
        }

        // Phương thức lọc theo khoảng thời gian
        public void FilterByDateRange(DateTime? tuNgay, DateTime? denNgay)
        {
            if (!tuNgay.HasValue && !denNgay.HasValue)
            {
                // Hiển thị tất cả
                DanhSachLichSu = new ObservableCollection<LichSuBaoTriExtended>(_allLichSu);
                return;
            }

            List<LichSuBaoTriExtended> filteredList;

            if (tuNgay.HasValue && denNgay.HasValue)
            {
                // Có cả ngày bắt đầu và kết thúc
                filteredList = _allLichSu.Where(item =>
                    item.NgayThucHien >= tuNgay.Value &&
                    item.NgayThucHien <= denNgay.Value
                ).ToList();
            }
            else if (tuNgay.HasValue)
            {
                // Chỉ có ngày bắt đầu
                filteredList = _allLichSu.Where(item =>
                    item.NgayThucHien >= tuNgay.Value
                ).ToList();
            }
            else
            {
                // Chỉ có ngày kết thúc
                filteredList = _allLichSu.Where(item =>
                    item.NgayThucHien <= denNgay.Value
                ).ToList();
            }

            // Cập nhật STT
            UpdateSTT(filteredList);

            // Cập nhật danh sách hiển thị
            DanhSachLichSu = new ObservableCollection<LichSuBaoTriExtended>(filteredList);
        }

        // Phương thức tải lại tất cả dữ liệu
        public void ReloadData()
        {
            // Cập nhật STT trước khi hiển thị
            UpdateSTT(_allLichSu);

            DanhSachLichSu = new ObservableCollection<LichSuBaoTriExtended>(_allLichSu);
        }

        // Cập nhật STT cho danh sách
        private void UpdateSTT(List<LichSuBaoTriExtended> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].STT = i + 1;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Class mở rộng từ LichSuBaoTri để thêm các thông tin hiển thị
    public class LichSuBaoTriExtended : LichSuBaoTri, INotifyPropertyChanged
    {
        private int _stt;
        public int STT
        {
            get => _stt;
            set
            {
                if (_stt != value)
                {
                    _stt = value;
                    OnPropertyChanged(nameof(STT));
                }
            }
        }

        private int _soLanBaoTri;
        public int SoLanBaoTri
        {
            get => _soLanBaoTri;
            set
            {
                if (_soLanBaoTri != value)
                {
                    _soLanBaoTri = value;
                    OnPropertyChanged(nameof(SoLanBaoTri));
                }
            }
        }

        private string _chiTietHoatDong;
        public string ChiTietHoatDong
        {
            get => _chiTietHoatDong;
            set
            {
                if (_chiTietHoatDong != value)
                {
                    _chiTietHoatDong = value;
                    OnPropertyChanged(nameof(ChiTietHoatDong));
                }
            }
        }

        private string _phongBan;
        public string PhongBan
        {
            get => _phongBan;
            set
            {
                if (_phongBan != value)
                {
                    _phongBan = value;
                    OnPropertyChanged(nameof(PhongBan));
                }
            }
        }

        // Constructor
        public LichSuBaoTriExtended()
        {
            // Khởi tạo các giá trị mặc định
            _chiTietHoatDong = string.Empty;
            _phongBan = string.Empty;
        }

        // Constructor từ LichSuBaoTri
        public LichSuBaoTriExtended(LichSuBaoTri lichSu)
        {
            // Copy các thuộc tính từ LichSuBaoTri
            this.MaLichSu = lichSu.MaLichSu;
            this.MaTaiSan = lichSu.MaTaiSan;
            this.TenTaiSan = lichSu.TenTaiSan ?? "Không có tên";
            this.SoSeri = lichSu.SoSeri ?? "";
            this.NgayThucHien = lichSu.NgayThucHien;
            this.MaNguoiThucHien = lichSu.MaNguoiThucHien;
            this.TenNguoiThucHien = lichSu.TenNguoiThucHien ?? "Không xác định";
            this.TinhTrangTaiSan = lichSu.TinhTrangTaiSan ?? "Không xác định";
            this.GhiChu = lichSu.GhiChu ?? "";

            // Tạo chi tiết hoạt động dựa trên thông tin có sẵn
            this.ChiTietHoatDong = $"Người thực hiện: {this.TenNguoiThucHien}\n" +
                                  $"Ngày: {this.NgayThucHien:dd/MM/yyyy HH:mm:ss}\n" +
                                  $"Tài sản: {this.TenTaiSan}\n" +
                                  $"Số seri: {this.SoSeri}\n" +
                                  $"Tình trạng: {this.TinhTrangTaiSan}\n" +
                                  $"Chi tiết: {this.GhiChu}";
        }

        public new event PropertyChangedEventHandler PropertyChanged;
        protected new void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}