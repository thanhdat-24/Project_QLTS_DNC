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
        #endregion

        private readonly PhieuBaoTriService _phieuBaoTriService;

        public PhieuBaoTriViewModel()
        {
            _phieuBaoTriService = new PhieuBaoTriService();
            DsBaoTri = new ObservableCollection<PhieuBaoTri>();
            DsTaiSanCanBaoTri = new ObservableCollection<PhieuBaoTri>();
            // Không tự động gọi LoadDSBaoTriAsync() trong constructor
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        // Tìm kiếm phiếu bảo trì
        public async Task SearchPhieuBaoTriAsync(string keyword)
        {
            try
            {
                IsLoading = true;
                var danhSachBaoTri = await _phieuBaoTriService.SearchPhieuBaoTriAsync(keyword);
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
            return new PhieuBaoTri
            {
                NgayBaoTri = DateTime.Now,
                TrangThai = "Tốt",
                ChiPhi = 0,
                GhiChu = ""
            };
        }

        // Tạo bản sao của phiếu bảo trì để chỉnh sửa
        public PhieuBaoTri ClonePhieuBaoTri(PhieuBaoTri phieu)
        {
            return new PhieuBaoTri
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