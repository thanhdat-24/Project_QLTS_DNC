using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Models.PhieuXuatKho;
using Project_QLTS_DNC.Services;

namespace Project_QLTS_DNC.ViewModel.Baotri
{
    public class LichSuSuaChuaViewModel : INotifyPropertyChanged
    {
        private readonly LichSuSuaChuaService _lichSuSuaChuaService;
        private readonly SupabaseService _supabaseService;
        private ObservableCollection<LichSuSuaChua> _danhSachLichSu;
        private ObservableCollection<LichSuSuaChua> _dsLichSuHienThi;
        private LichSuSuaChua _lichSuDangChon;
        private bool _dangTaiDuLieu;
        private string _tuKhoaTimKiem;
        private DateTime? _tuNgay;
        private DateTime? _denNgay;
        private ObservableCollection<NhomTaiSan> _danhSachNhomTaiSan;
        private NhomTaiSan _nhomTaiSanDuocChon;
        private List<TaiSanModel> _danhSachTaiSan;
        private List<ChiTietPhieuXuatModel> _danhSachChiTietXuat;

        // Biến cho phân trang
        private int _trangHienTai;
        private int _tongSoTrang;
        private int _soItemMoiTrang;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<LichSuSuaChua> DanhSachLichSu
        {
            get => _danhSachLichSu;
            set
            {
                _danhSachLichSu = value;
                OnPropertyChanged(nameof(DanhSachLichSu));
                CapNhatPhanTrang();
            }
        }

        public ObservableCollection<LichSuSuaChua> DsLichSuHienThi
        {
            get => _dsLichSuHienThi;
            set
            {
                _dsLichSuHienThi = value;
                OnPropertyChanged(nameof(DsLichSuHienThi));
            }
        }

        public LichSuSuaChua LichSuDangChon
        {
            get => _lichSuDangChon;
            set
            {
                _lichSuDangChon = value;
                OnPropertyChanged(nameof(LichSuDangChon));
            }
        }

        public ObservableCollection<NhomTaiSan> DanhSachNhomTaiSan
        {
            get => _danhSachNhomTaiSan;
            set
            {
                _danhSachNhomTaiSan = value;
                OnPropertyChanged(nameof(DanhSachNhomTaiSan));
            }
        }

        public NhomTaiSan NhomTaiSanDuocChon
        {
            get => _nhomTaiSanDuocChon;
            set
            {
                _nhomTaiSanDuocChon = value;
                OnPropertyChanged(nameof(NhomTaiSanDuocChon));
            }
        }

        public bool DangTaiDuLieu
        {
            get => _dangTaiDuLieu;
            set
            {
                _dangTaiDuLieu = value;
                OnPropertyChanged(nameof(DangTaiDuLieu));
            }
        }

        public string TuKhoaTimKiem
        {
            get => _tuKhoaTimKiem;
            set
            {
                _tuKhoaTimKiem = value;
                OnPropertyChanged(nameof(TuKhoaTimKiem));
            }
        }

        public DateTime? TuNgay
        {
            get => _tuNgay;
            set
            {
                _tuNgay = value;
                OnPropertyChanged(nameof(TuNgay));
            }
        }

        public DateTime? DenNgay
        {
            get => _denNgay;
            set
            {
                _denNgay = value;
                OnPropertyChanged(nameof(DenNgay));
            }
        }

        public int TrangHienTai
        {
            get => _trangHienTai;
            set
            {
                if (_trangHienTai != value)
                {
                    _trangHienTai = value;
                    OnPropertyChanged(nameof(TrangHienTai));
                    CapNhatDanhSachHienThi();
                }
            }
        }

        public int TongSoTrang
        {
            get => _tongSoTrang;
            set
            {
                _tongSoTrang = value;
                OnPropertyChanged(nameof(TongSoTrang));
            }
        }

        public int SoItemMoiTrang
        {
            get => _soItemMoiTrang;
            set
            {
                if (_soItemMoiTrang != value)
                {
                    _soItemMoiTrang = value;
                    OnPropertyChanged(nameof(SoItemMoiTrang));
                    CapNhatPhanTrang();
                }
            }
        }

        public ICommand TimKiemCommand { get; private set; }
        public ICommand LamMoiCommand { get; private set; }
        public ICommand LocDanhSachCommand { get; private set; }

        public LichSuSuaChuaViewModel()
        {
            _lichSuSuaChuaService = new LichSuSuaChuaService();
            _supabaseService = new SupabaseService();
            DanhSachLichSu = new ObservableCollection<LichSuSuaChua>();
            DsLichSuHienThi = new ObservableCollection<LichSuSuaChua>();
            DanhSachNhomTaiSan = new ObservableCollection<NhomTaiSan>();
            _danhSachTaiSan = new List<TaiSanModel>();
            _danhSachChiTietXuat = new List<ChiTietPhieuXuatModel>();

            // Khởi tạo giá trị mặc định cho phân trang
            _trangHienTai = 1;
            _soItemMoiTrang = 10;

            TimKiemCommand = new RelayCommand<object>(async param => await TimKiem());
            LamMoiCommand = new RelayCommand<object>(async param => await TaiDuLieu());
            LocDanhSachCommand = new RelayCommand<object>(async param => await LocDanhSach());

            // Tải dữ liệu khi khởi tạo ViewModel
            TaiDuLieu().ConfigureAwait(false);
            TaiDanhSachNhomTaiSan().ConfigureAwait(false);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task TaiDuLieu()
        {
            try
            {
                DangTaiDuLieu = true;
                var danhSach = await _lichSuSuaChuaService.LayTatCaLichSuSuaChua();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DanhSachLichSu = new ObservableCollection<LichSuSuaChua>(danhSach);
                    // Tải danh sách tài sản
                    TrangHienTai = 1;
                    CapNhatPhanTrang();
                });

                // Tải danh sách tài sản riêng biệt, không cần đợi
                await TaiDanhSachTaiSan();
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    string errorMessage = $"Lỗi khi tải dữ liệu: {ex.Message}";
                    if (ex.InnerException != null)
                        errorMessage += $"\nChi tiết: {ex.InnerException.Message}";

                    MessageBox.Show(errorMessage, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        // Phương thức tải danh sách nhóm tài sản
        public async Task TaiDanhSachNhomTaiSan()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<NhomTaiSan>().Get();

                // Thêm mục "Tất cả" vào đầu danh sách
                DanhSachNhomTaiSan.Clear();
                DanhSachNhomTaiSan.Add(new NhomTaiSan { MaNhomTS = 0, TenNhom = "Tất cả" });

                foreach (var item in response.Models)
                {
                    DanhSachNhomTaiSan.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách nhóm tài sản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Phương thức tải danh sách tài sản
        public async Task TaiDanhSachTaiSan()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<TaiSanModel>().Get();
                _danhSachTaiSan = response.Models;

                // Tải chi tiết xuất kho để lấy thông tin về nhóm tài sản
                var responseChiTiet = await client.From<ChiTietPhieuXuatModel>().Get();
                _danhSachChiTietXuat = responseChiTiet.Models;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách tài sản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 1. Trong LichSuSuaChuaViewModel.cs - thêm biến tránh thông báo trùng lặp
        private bool _isSearchInProgress = false;

        // 2. Sửa lại phương thức TimKiem
        public async Task TimKiem()
        {
            // Nếu đang trong quá trình tìm kiếm, không thực hiện thêm
            if (_isSearchInProgress)
                return;

            try
            {
                _isSearchInProgress = true;
                DangTaiDuLieu = true;

                // Kiểm tra ngày hợp lệ
                if (TuNgay.HasValue && DenNgay.HasValue && TuNgay > DenNgay)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() => {
                        MessageBox.Show("Ngày bắt đầu không được lớn hơn ngày kết thúc!", "Cảnh báo",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    });
                    return;
                }

                // Gọi phương thức tìm kiếm từ service
                var ketQua = await _lichSuSuaChuaService.TimKiemLichSuSuaChua(TuKhoaTimKiem, TuNgay, DenNgay);

                await Application.Current.Dispatcher.InvokeAsync(() => {
                    DanhSachLichSu.Clear();
                    foreach (var item in ketQua)
                    {
                        DanhSachLichSu.Add(item);
                    }

                    // Cập nhật phân trang và hiển thị
                    TrangHienTai = 1;
                    CapNhatPhanTrang();

                    // Chỉ hiển thị thông báo khi không có kết quả
                    if (DanhSachLichSu.Count == 0)
                    {
                        MessageBox.Show("Không tìm thấy kết quả phù hợp!", "Thông báo",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() => {
                    MessageBox.Show($"Lỗi khi tìm kiếm: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                DangTaiDuLieu = false;
                _isSearchInProgress = false;  // Đánh dấu đã hoàn thành tìm kiếm
            }
        }
        private string _loaiThaoTacDuocChon;
        public string LoaiThaoTacDuocChon
        {
            get => _loaiThaoTacDuocChon;
            set
            {
                _loaiThaoTacDuocChon = value;
                OnPropertyChanged(nameof(LoaiThaoTacDuocChon));
            }
        }

        public ObservableCollection<string> DanhSachLoaiThaoTac { get; } = new ObservableCollection<string>
{
    "Tất cả",
    "Xuất Excel",
    "In phiếu",
    "Xuất Excel danh sách",
    "In phiếu danh sách"
};
        public async Task LocDanhSach()
        {
            try
            {
                DangTaiDuLieu = true;

                // Lấy toàn bộ dữ liệu lịch sử sửa chữa
                var tatCaLichSu = await _lichSuSuaChuaService.TimKiemLichSuSuaChua(TuKhoaTimKiem, TuNgay, DenNgay);
                List<LichSuSuaChua> danhSachLichSuLoc = new List<LichSuSuaChua>();

                // Nếu không chọn nhóm tài sản hoặc chọn "Tất cả", xem xét tất cả
                if (NhomTaiSanDuocChon == null || NhomTaiSanDuocChon.MaNhomTS == 0)
                {
                    danhSachLichSuLoc = tatCaLichSu;
                }
                else
                {
                    // Lọc theo nhóm tài sản được chọn
                    // Tìm các tài sản thuộc nhóm được chọn
                    var maTaiSanThuocNhom = _danhSachChiTietXuat
                        .Where(ct => ct.MaNhomTS == NhomTaiSanDuocChon.MaNhomTS)
                        .Select(ct => ct.MaTaiSan)
                        .Distinct()
                        .ToList();

                    // Lọc lịch sử sửa chữa theo mã tài sản
                    foreach (var lichSu in tatCaLichSu)
                    {
                        if (lichSu.MaTaiSan.HasValue && maTaiSanThuocNhom.Contains(lichSu.MaTaiSan.Value))
                        {
                            danhSachLichSuLoc.Add(lichSu);
                        }
                    }
                }

                // THÊM MỚI: Lọc theo loại thao tác (nếu có)
                if (!string.IsNullOrEmpty(LoaiThaoTacDuocChon) && LoaiThaoTacDuocChon != "Tất cả")
                {
                    danhSachLichSuLoc = danhSachLichSuLoc.Where(ls => ls.LoaiThaoTac == LoaiThaoTacDuocChon).ToList();
                }

                // Cập nhật danh sách hiển thị
                DanhSachLichSu.Clear();
                foreach (var item in danhSachLichSuLoc)
                {
                    DanhSachLichSu.Add(item);
                }

                // Cập nhật phân trang và hiển thị
                TrangHienTai = 1;
                CapNhatPhanTrang();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lọc dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        // Các phương thức phân trang
        public void CapNhatPhanTrang()
        {
            if (DanhSachLichSu.Count == 0)
            {
                TongSoTrang = 1;
            }
            else
            {
                TongSoTrang = (int)Math.Ceiling(DanhSachLichSu.Count / (double)SoItemMoiTrang);
            }

            if (TrangHienTai > TongSoTrang)
            {
                TrangHienTai = TongSoTrang;
            }

            CapNhatDanhSachHienThi();
        }

        private void CapNhatDanhSachHienThi()
        {
            DsLichSuHienThi = new ObservableCollection<LichSuSuaChua>(
                DanhSachLichSu
                    .Skip((TrangHienTai - 1) * SoItemMoiTrang)
                    .Take(SoItemMoiTrang)
                    .ToList());
        }

        public void ChuyenTrangDau()
        {
            TrangHienTai = 1;
        }

        public void ChuyenTrangTruoc()
        {
            if (TrangHienTai > 1)
            {
                TrangHienTai--;
            }
        }

        public void ChuyenTrangSau()
        {
            if (TrangHienTai < TongSoTrang)
            {
                TrangHienTai++;
            }
        }

        public void ChuyenTrangCuoi()
        {
            TrangHienTai = TongSoTrang;
        }
    }
}