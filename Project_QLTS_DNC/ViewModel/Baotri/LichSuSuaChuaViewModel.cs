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
using Project_QLTS_DNC.Models.NhanVien;
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
        private List<NhanVienModel> _danhSachNhanVien;
        private Dictionary<int, string> _cacheTenPhong;

        // Biến cho phân trang
        private int _trangHienTai;
        private int _tongSoTrang;
        private int _soItemMoiTrang;
        private bool _isSearchInProgress = false;
        private string _loaiThaoTacDuocChon;

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
            _danhSachNhanVien = new List<NhanVienModel>();
            _cacheTenPhong = new Dictionary<int, string>();

            // Khởi tạo giá trị mặc định cho phân trang
            _trangHienTai = 1;
            _soItemMoiTrang = 10;

            TimKiemCommand = new RelayCommand<object>(async param => await TimKiem());
            LamMoiCommand = new RelayCommand<object>(async param => await TaiDuLieuDayDu());
            LocDanhSachCommand = new RelayCommand<object>(async param => await LocDanhSach());

            // Tải dữ liệu khi khởi tạo ViewModel
            TaiDuLieuDayDu().ConfigureAwait(false);
            TaiDanhSachNhomTaiSan().ConfigureAwait(false);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Phương thức mới: Tải dữ liệu đầy đủ từ nhiều bảng
        public async Task TaiDuLieuDayDu()
        {
            try
            {
                DangTaiDuLieu = true;

                // Lấy dữ liệu lịch sử sửa chữa
                var danhSach = await _lichSuSuaChuaService.LayTatCaLichSuSuaChua();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    DanhSachLichSu = new ObservableCollection<LichSuSuaChua>(danhSach);
                    TrangHienTai = 1;
                    CapNhatPhanTrang();
                });

                // Tải danh sách tài sản và thông tin liên quan
                await TaiDanhSachTaiSan();

                // Tải thông tin chi tiết cho mỗi mục lịch sử
                await TaiThongTinChiTiet();
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

                // Tải danh sách tài sản
                var response = await client.From<TaiSanModel>().Get();
                _danhSachTaiSan = response.Models;

                // Tải chi tiết xuất kho để lấy thông tin về nhóm tài sản
                var responseChiTiet = await client.From<ChiTietPhieuXuatModel>().Get();
                _danhSachChiTietXuat = responseChiTiet.Models;

                // Tải danh sách nhân viên
                var responseNhanVien = await client.From<NhanVienModel>().Get();
                _danhSachNhanVien = responseNhanVien.Models;

                // Tải thông tin phòng (tùy theo cấu trúc của bạn)
                // Ví dụ: Tạo một dictionary lưu tên phòng theo mã phòng
                // Bạn cần thay đổi tên lớp và các thuộc tính tùy theo DB của bạn
                /*
                var responsePhong = await client.From<PhongModel>().Get();
                foreach (var phong in responsePhong.Models)
                {
                    _cacheTenPhong[phong.MaPhong] = phong.TenPhong;
                }
                */
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách tài sản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Phương thức mới: Tải thông tin chi tiết cho mỗi mục lịch sử
        private async Task TaiThongTinChiTiet()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var lichSu in DanhSachLichSu)
                    {
                        // Cập nhật thông tin tài sản
                        if (lichSu.MaTaiSan.HasValue)
                        {
                            var taiSan = _danhSachTaiSan.FirstOrDefault(ts => ts.MaTaiSan == lichSu.MaTaiSan.Value);
                            if (taiSan != null)
                            {
                                lichSu.TenTaiSan = taiSan.TenTaiSan;
                                lichSu.SoSeri = taiSan.SoSeri;
                                lichSu.TinhTrangTaiSan = taiSan.TinhTrangSP;

                                // Cập nhật thông tin phòng nếu có
                                if (taiSan.MaPhong.HasValue && _cacheTenPhong.ContainsKey(taiSan.MaPhong.Value))
                                {
                                    lichSu.TenPhong = _cacheTenPhong[taiSan.MaPhong.Value];
                                }

                                // Cập nhật thông tin nhóm tài sản
                                var chiTietXuat = _danhSachChiTietXuat
                                    .FirstOrDefault(ct => ct.MaTaiSan == taiSan.MaTaiSan);

                                if (chiTietXuat != null && chiTietXuat.MaNhomTS.HasValue)
                                {
                                    var nhomTaiSan = DanhSachNhomTaiSan
                                        .FirstOrDefault(n => n.MaNhomTS == chiTietXuat.MaNhomTS.Value);

                                    if (nhomTaiSan != null)
                                    {
                                        lichSu.TenNhomTaiSan = nhomTaiSan.TenNhom;
                                    }
                                }
                            }
                        }

                        // Cập nhật thông tin người thực hiện
                        if (lichSu.MaNV.HasValue)
                        {
                            var nhanVien = _danhSachNhanVien
                                .FirstOrDefault(nv => nv.MaNV == lichSu.MaNV.Value);

                            if (nhanVien != null)
                            {
                                lichSu.TenNguoiThucHien = nhanVien.TenNV;
                            }
                        }
                    }

                    // Cập nhật lại danh sách hiển thị
                    CapNhatDanhSachHienThi();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin chi tiết: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Giữ các phương thức hiện có
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

        // Phương thức tìm kiếm
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

                // Tải thông tin chi tiết cho kết quả tìm kiếm
                await TaiThongTinChiTiet();
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

        // Phương thức lọc danh sách
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

                // Lọc theo loại thao tác (nếu có)
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

                // Tải thông tin chi tiết cho kết quả lọc
                await TaiThongTinChiTiet();
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