using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Services;

namespace Project_QLTS_DNC.ViewModel.Baotri
{
    public class PhieuBaoTriViewModel : INotifyPropertyChanged
    {
        #region Properties
        private ObservableCollection<PhieuBaoTri> _dsBaoTri;
        public ObservableCollection<PhieuBaoTri> DsBaoTri
        {
            get => _dsBaoTri;
            set
            {
                _dsBaoTri = value;
                OnPropertyChanged(nameof(DsBaoTri));
            }
        }
        private PhieuBaoTri _selectedPhieuBaoTri;
        public PhieuBaoTri SelectedPhieuBaoTri
        {
            get => _selectedPhieuBaoTri;
            set
            {
                _selectedPhieuBaoTri = value;
                OnPropertyChanged(nameof(SelectedPhieuBaoTri));
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
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }
        private string _selectedTrangThai = "Tất cả trạng thái";
        public string SelectedTrangThai
        {
            get => _selectedTrangThai;
            set
            {
                _selectedTrangThai = value;
                OnPropertyChanged(nameof(SelectedTrangThai));
            }
        }
        private string _selectedLoaiBaoTri = "Tất cả loại";
        public string SelectedLoaiBaoTri
        {
            get => _selectedLoaiBaoTri;
            set
            {
                _selectedLoaiBaoTri = value;
                OnPropertyChanged(nameof(SelectedLoaiBaoTri));
            }
        }
        // Danh sách các tài sản cần bảo trì
        private ObservableCollection<PhieuBaoTri> _dsTaiSanCanBaoTri;
        public ObservableCollection<PhieuBaoTri> DsTaiSanCanBaoTri
        {
            get => _dsTaiSanCanBaoTri;
            set
            {
                _dsTaiSanCanBaoTri = value;
                OnPropertyChanged(nameof(DsTaiSanCanBaoTri));
            }
        }
        
        // Thêm Dictionary để lưu thông tin chi tiết tài sản
        private Dictionary<int, TaiSanInfo> _taiSanDict;
        #endregion

        private readonly PhieuBaoTriService _phieuBaoTriService;

        public PhieuBaoTriViewModel()
        {
            _phieuBaoTriService = new PhieuBaoTriService();
            DsBaoTri = new ObservableCollection<PhieuBaoTri>();
            DsTaiSanCanBaoTri = new ObservableCollection<PhieuBaoTri>();
            _taiSanDict = new Dictionary<int, TaiSanInfo>();
            // Không tự động gọi LoadDSBaoTriAsync() trong constructor
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Tạo lớp để lưu thông tin tài sản
        public class TaiSanInfo
        {
            public int MaTaiSan { get; set; }
            public string TenTaiSan { get; set; }
            public string SoSeri { get; set; }
            public string TinhTrangSP { get; set; }
        }

        #region Data Loading Methods
        // Tải danh sách phiếu bảo trì
        public async Task LoadDSBaoTriAsync()
        {
            try
            {
                IsLoading = true;
                Console.WriteLine("Bắt đầu tải danh sách bảo trì...");
                var danhSachBaoTri = await _phieuBaoTriService.GetPhieuBaoTriAsync();
                Console.WriteLine($"Đã nhận được {danhSachBaoTri?.Count ?? 0} phiếu bảo trì từ service");

                if (danhSachBaoTri == null || danhSachBaoTri.Count == 0)
                {
                    Console.WriteLine("Không tìm thấy dữ liệu bảo trì");
                    DsBaoTri = new ObservableCollection<PhieuBaoTri>();
                }
                else
                {
                    // Tải thông tin tài sản trước
                    await LoadTaiSanInfoAsync();
                    
                    // Bổ sung thông tin tên cho TenLoaiBaoTri, tạo TenNguoiPhuTrach và TenTaiSan nếu cần
                    await EnrichPhieuBaoTriDataAsync(danhSachBaoTri);

                    DsBaoTri = new ObservableCollection<PhieuBaoTri>(danhSachBaoTri);
                    Console.WriteLine("Đã gán dữ liệu cho DsBaoTri");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi chi tiết: {ex.ToString()}");
                MessageBox.Show($"Lỗi khi tải dữ liệu từ ViewModel: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Phương thức làm giàu dữ liệu
        private async Task EnrichPhieuBaoTriDataAsync(List<PhieuBaoTri> danhSachPhieu)
        {
            try
            {
                Console.WriteLine("Bổ sung thông tin hiển thị cho phiếu bảo trì...");

                // Đảm bảo đã tải thông tin tài sản
                if (_taiSanDict == null || _taiSanDict.Count == 0)
                {
                    await LoadTaiSanInfoAsync();
                }

                foreach (var phieu in danhSachPhieu)
                {
                    // Cập nhật thông tin hiển thị tài sản
                    UpdateTaiSanDisplayInfo(phieu);

                    // Gán tên nhân viên
                    if (phieu.MaNV.HasValue)
                    {
                        try
                        {
                            phieu.TenNguoiPhuTrach = await GetTenNhanVienAsync(phieu.MaNV.Value);
                        }
                        catch
                        {
                            phieu.TenNguoiPhuTrach = $"NV #{phieu.MaNV}";
                        }
                    }
                    else
                    {
                        phieu.TenNguoiPhuTrach = "Chưa phân công";
                    }
                }

                Console.WriteLine("Hoàn thành bổ sung thông tin hiển thị");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi làm giàu dữ liệu: {ex.Message}");
                // Không ném lại ngoại lệ, để quá trình tải tiếp tục mà không có thông tin bổ sung
            }
        }

        // Tìm kiếm phiếu bảo trì
        public async Task SearchPhieuBaoTriAsync(string keyword)
        {
            try
            {
                IsLoading = true;
                var danhSachBaoTri = await _phieuBaoTriService.SearchPhieuBaoTriAsync(keyword);

                // Bổ sung thông tin tên
                await EnrichPhieuBaoTriDataAsync(danhSachBaoTri);

                DsBaoTri = new ObservableCollection<PhieuBaoTri>(danhSachBaoTri);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm dữ liệu: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Lọc phiếu bảo trì theo loại và trạng thái
        public async Task FilterPhieuBaoTriAsync(string loaiBaoTri, string trangThai)
        {
            try
            {
                IsLoading = true;
                // Chuyển đổi tên loại bảo trì thành mã (nếu chọn loại cụ thể)
                int? maLoaiBaoTri = null;
                if (loaiBaoTri != "Tất cả loại")
                {
                    maLoaiBaoTri = ConvertTenLoaiToMa(loaiBaoTri);
                }
                // Gọi service để lấy dữ liệu đã lọc
                var danhSachBaoTri = await _phieuBaoTriService.GetPhieuBaoTriTheoLoaiTrangThaiAsync(
                    maLoaiBaoTri,
                    trangThai == "Tất cả trạng thái" ? null : trangThai);

                // Bổ sung thông tin tên
                await EnrichPhieuBaoTriDataAsync(danhSachBaoTri);

                DsBaoTri = new ObservableCollection<PhieuBaoTri>(danhSachBaoTri);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lọc dữ liệu: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Tải danh sách tài sản cần bảo trì
        public async Task LoadDanhSachCanBaoTriAsync()
        {
            try
            {
                IsLoading = true;
                var danhSachCanBaoTri = await _phieuBaoTriService.GetDanhSachCanBaoTriAsync();

                // Bổ sung thông tin tên
                await EnrichPhieuBaoTriDataAsync(danhSachCanBaoTri);

                DsTaiSanCanBaoTri = new ObservableCollection<PhieuBaoTri>(danhSachCanBaoTri);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách tài sản cần bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        // Thêm phương thức LoadTaiSanInfoAsync để tải thông tin chi tiết tài sản
        public async Task LoadTaiSanInfoAsync()
        {
            try
            {
                Console.WriteLine("Đang tải thông tin chi tiết tài sản...");
                // Lấy danh sách tất cả các tài sản từ service
                _taiSanDict = await _phieuBaoTriService.GetAllTaiSanInfoAsync();
                Console.WriteLine($"Đã tải được thông tin của {_taiSanDict?.Count ?? 0} tài sản");
                
                // Cập nhật thông tin hiển thị cho các phiếu đã có
                if (DsBaoTri != null && DsBaoTri.Count > 0)
                {
                    foreach (var phieu in DsBaoTri)
                    {
                        UpdateTaiSanDisplayInfo(phieu);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tải thông tin tài sản: {ex.Message}");
            }
        }
        
        // Helper để cập nhật thông tin hiển thị của tài sản
        private void UpdateTaiSanDisplayInfo(PhieuBaoTri phieu)
        {
            if (phieu.MaTaiSan.HasValue && _taiSanDict != null && _taiSanDict.TryGetValue(phieu.MaTaiSan.Value, out var taiSan))
            {
                // Định dạng theo yêu cầu: Tên tài sản, Số seri, Tình trạng
                phieu.TenTaiSan = $"{taiSan.TenTaiSan}\nSố sê-ri: {taiSan.SoSeri}\nTình trạng: {taiSan.TinhTrangSP}";
            }
            else if (phieu.MaTaiSan.HasValue)
            {
                phieu.TenTaiSan = $"Tài sản #{phieu.MaTaiSan}";
            }
            else
            {
                phieu.TenTaiSan = "Không xác định";
            }
        }
        #endregion

        #region CRUD Operations
        // Thêm phiếu bảo trì mới
        public async Task<bool> AddPhieuBaoTriAsync(PhieuBaoTri phieuBaoTri)
        {
            try
            {
                IsLoading = true;
                // Tạo mã mới cho phiếu bảo trì nếu chưa có
                if (phieuBaoTri.MaBaoTri <= 0)
                {
                    int maxMaBaoTri = await _phieuBaoTriService.GetMaxMaBaoTriAsync();
                    phieuBaoTri.MaBaoTri = maxMaBaoTri + 1;
                }
                // Thiết lập ngày bảo trì nếu chưa có
                if (phieuBaoTri.NgayBaoTri == DateTime.MinValue)
                {
                    phieuBaoTri.NgayBaoTri = DateTime.Now;
                }
                // Thêm phiếu bảo trì vào cơ sở dữ liệu
                bool result = await _phieuBaoTriService.AddPhieuBaoTriAsync(phieuBaoTri);
                if (result)
                {
                    // Cập nhật thông tin hiển thị trước khi thêm vào danh sách
                    UpdateTaiSanDisplayInfo(phieuBaoTri);
                    
                    if (phieuBaoTri.MaNV.HasValue)
                    {
                        phieuBaoTri.TenNguoiPhuTrach = await GetTenNhanVienAsync(phieuBaoTri.MaNV.Value);
                    }

                    // Thêm phiếu mới vào danh sách hiển thị nếu thành công
                    DsBaoTri.Add(phieuBaoTri);
                    OnPropertyChanged(nameof(DsBaoTri));
                    Console.WriteLine($"Đã thêm phiếu bảo trì mới với mã {phieuBaoTri.MaBaoTri} vào danh sách hiển thị");
                }
                else
                {
                    Console.WriteLine("Thêm phiếu bảo trì thất bại");
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong ViewModel khi thêm phiếu bảo trì: {ex.Message}");
                MessageBox.Show($"Lỗi khi thêm phiếu bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Cập nhật phiếu bảo trì
        public async Task<bool> UpdatePhieuBaoTriAsync(PhieuBaoTri phieuBaoTri)
        {
            try
            {
                IsLoading = true;
                bool result = await _phieuBaoTriService.UpdatePhieuBaoTriAsync(phieuBaoTri);
                if (result)
                {
                    // Cập nhật thông tin hiển thị trước khi cập nhật danh sách
                    UpdateTaiSanDisplayInfo(phieuBaoTri);
                    
                    if (phieuBaoTri.MaNV.HasValue)
                    {
                        phieuBaoTri.TenNguoiPhuTrach = await GetTenNhanVienAsync(phieuBaoTri.MaNV.Value);
                    }

                    // Tìm và cập nhật phiếu trong danh sách hiển thị
                    var existingItem = DsBaoTri.FirstOrDefault(p => p.MaBaoTri == phieuBaoTri.MaBaoTri);
                    if (existingItem != null)
                    {
                        int index = DsBaoTri.IndexOf(existingItem);
                        DsBaoTri[index] = phieuBaoTri;
                    }
                    OnPropertyChanged(nameof(DsBaoTri));
                }
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật phiếu bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Xóa phiếu bảo trì
        public async Task<bool> DeletePhieuBaoTriAsync(int maBaoTri)
        {
            try
            {
                IsLoading = true;
                bool result = await _phieuBaoTriService.DeletePhieuBaoTriAsync(maBaoTri);
                if (result)
                {
                    // Xóa phiếu khỏi danh sách hiển thị nếu thành công
                    var itemToRemove = DsBaoTri.FirstOrDefault(p => p.MaBaoTri == maBaoTri);
                    if (itemToRemove != null)
                    {
                        DsBaoTri.Remove(itemToRemove);
                        OnPropertyChanged(nameof(DsBaoTri));
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa phiếu bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion

        #region Helper Methods
        // Chuyển đổi tên loại bảo trì thành mã
        private int? ConvertTenLoaiToMa(string tenLoai)
        {
            switch (tenLoai)
            {
                case "Định kỳ": return 1;
                case "Đột xuất": return 2;
                case "Bảo hành": return 3;
                default: return null;
            }
        }

        // Lấy tên loại bảo trì từ mã
        public string GetTenLoaiBaoTri(int? maLoai)
        {
            if (!maLoai.HasValue) return "Không xác định";
            switch (maLoai.Value)
            {
                case 1: return "Định kỳ";
                case 2: return "Đột xuất";
                case 3: return "Bảo hành";
                default: return "Không xác định";
            }
        }

        // Tạo phiếu bảo trì mới với các giá trị mặc định
        public PhieuBaoTri CreateNewPhieuBaoTri()
        {
            // Lấy ngày hiện tại
            DateTime ngayHienTai = DateTime.Now;
            // Tạo phiếu mới với các giá trị mặc định
            var phieu = new PhieuBaoTri
            {
                // Không gán MaBaoTri, để cơ sở dữ liệu tự động tạo
                MaTaiSan = null, // Sẽ được chọn bởi người dùng
                MaLoaiBaoTri = 1, // Mặc định là bảo trì định kỳ (có thể thay đổi)
                NgayBaoTri = ngayHienTai,
                MaNV = null, // Sẽ được chọn bởi người dùng
                NoiDung = "", // Người dùng sẽ nhập
                TrangThai = "Chưa thực hiện", // Trạng thái mặc định
                ChiPhi = 0, // Chi phí mặc định
                GhiChu = "" // Người dùng sẽ nhập
            };

            // Thiết lập các giá trị hiển thị
            phieu.TenTaiSan = "Chọn tài sản";
            phieu.TenNguoiPhuTrach = "Chọn nhân viên";

            return phieu;
        }

        // Tạo bản sao của phiếu bảo trì để chỉnh sửa
        public PhieuBaoTri ClonePhieuBaoTri(PhieuBaoTri phieu)
        {
            var clonedPhieu = new PhieuBaoTri
            {
                MaBaoTri = phieu.MaBaoTri,
                MaTaiSan = phieu.MaTaiSan,
                MaLoaiBaoTri = phieu.MaLoaiBaoTri,
                NgayBaoTri = phieu.NgayBaoTri,
                MaNV = phieu.MaNV,
                NoiDung = phieu.NoiDung,
                TrangThai = phieu.TrangThai,
                ChiPhi = phieu.ChiPhi,
                GhiChu = phieu.GhiChu
            };

            // Sao chép thông tin hiển thị
            clonedPhieu.TenTaiSan = phieu.TenTaiSan;
            clonedPhieu.TenNguoiPhuTrach = phieu.TenNguoiPhuTrach;

            return clonedPhieu;
        }

        // Thêm phương thức để lấy thông tin tài sản
        public async Task<string> GetTenTaiSanAsync(int maTaiSan)
        {
            return await _phieuBaoTriService.GetTenTaiSanAsync(maTaiSan);
        }

        // Thêm phương thức để lấy thông tin nhân viên
        public async Task<string> GetTenNhanVienAsync(int maNV)
        {
            return await _phieuBaoTriService.GetTenNhanVienAsync(maNV);
        }
        #endregion
    }
}