using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Services.BaoTri;
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
                Console.WriteLine("ViewModel: Đang tải lại danh sách bảo trì...");
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                // Lấy danh sách phiếu bảo trì
                var response = await client.From<PhieuBaoTri>().Get();

                if (response?.Models != null)
                {
                    List<PhieuBaoTri> danhSachPhieu = new List<PhieuBaoTri>();

                    foreach (var phieu in response.Models)
                    {
                        // Lấy thêm thông tin tên tài sản, tên loại bảo trì, tên nhân viên
                        if (phieu.MaTaiSan.HasValue)
                        {
                            phieu.TenTaiSan = await GetTenTaiSanAsync(phieu.MaTaiSan.Value);
                        }

                        if (phieu.MaLoaiBaoTri.HasValue)
                        {
                            phieu.TenLoaiBaoTri = GetTenLoaiBaoTri(phieu.MaLoaiBaoTri.Value);
                        }

                        if (phieu.MaNV.HasValue)
                        {
                            phieu.TenNhanVien = await GetTenNhanVienAsync(phieu.MaNV.Value);
                        }

                        danhSachPhieu.Add(phieu);
                    }

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        DsBaoTri.Clear();
                        foreach (var item in danhSachPhieu)
                        {
                            DsBaoTri.Add(item);
                        }
                        OnPropertyChanged(nameof(DsBaoTri));
                    });

                    Console.WriteLine($"ViewModel: Đã tải {DsBaoTri.Count} phiếu bảo trì");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ViewModel: Lỗi khi tải danh sách bảo trì: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Tìm kiếm phiếu bảo trì
        // Tìm kiếm phiếu bảo trì
        public async Task SearchPhieuBaoTriAsync(string keyword)
        {
            try
            {
                IsLoading = true;
                var danhSachBaoTri = await _phieuBaoTriService.SearchPhieuBaoTriAsync(keyword);

                // Bổ sung thông tin tên tài sản, tên loại bảo trì, tên nhân viên
                foreach (var phieu in danhSachBaoTri)
                {
                    if (phieu.MaTaiSan.HasValue)
                    {
                        phieu.TenTaiSan = await GetTenTaiSanAsync(phieu.MaTaiSan.Value);
                    }

                    if (phieu.MaLoaiBaoTri.HasValue)
                    {
                        phieu.TenLoaiBaoTri = GetTenLoaiBaoTri(phieu.MaLoaiBaoTri.Value);
                    }

                    if (phieu.MaNV.HasValue)
                    {
                        phieu.TenNhanVien = await GetTenNhanVienAsync(phieu.MaNV.Value);
                    }
                }

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

                // Bổ sung thông tin tên tài sản, tên loại bảo trì, tên nhân viên
                foreach (var phieu in danhSachBaoTri)
                {
                    if (phieu.MaTaiSan.HasValue)
                    {
                        phieu.TenTaiSan = await GetTenTaiSanAsync(phieu.MaTaiSan.Value);
                    }

                    if (phieu.MaLoaiBaoTri.HasValue)
                    {
                        phieu.TenLoaiBaoTri = GetTenLoaiBaoTri(phieu.MaLoaiBaoTri.Value);
                    }

                    if (phieu.MaNV.HasValue)
                    {
                        phieu.TenNhanVien = await GetTenNhanVienAsync(phieu.MaNV.Value);
                    }
                }

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
        // Thêm phiếu bảo trì mới
        public async Task<bool> ThemPhieuBaoTriAsync(PhieuBaoTri phieuBaoTri)
        {
            try
            {
                IsLoading = true;

                // Lấy mã bảo trì lớn nhất hiện tại để tạo mã mới
                int maBaoTriMoi = await _phieuBaoTriService.GetMaxMaBaoTriAsync() + 1;
                phieuBaoTri.MaBaoTri = maBaoTriMoi;

                // Gọi service để thêm vào database
                bool result = await _phieuBaoTriService.AddPhieuBaoTriAsync(phieuBaoTri);

                // Nếu thêm thành công, thêm vào danh sách hiển thị
                if (result)
                {
                    await LoadDSBaoTriAsync(); // Tải lại toàn bộ danh sách
                    OnPropertyChanged(nameof(DsBaoTri));
                }

                return result;
            }
            catch (Exception ex)
            {
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


        public PhieuBaoTri CreateNewPhieuBaoTri()
        {
            // Tạo một phiếu bảo trì mới với các giá trị mặc định
            // KHÔNG đặt MaBaoTri vì sẽ được tự động tạo
            return new PhieuBaoTri
            {
                // Không đặt MaBaoTri ở đây
                NgayBaoTri = DateTime.Now,
                TrangThai = "Chờ xử lý",
                MaLoaiBaoTri = 2, // Mặc định là đột xuất
                ChiPhi = 0, // Chi phí mặc định là 0
                MaNV = null, // Người phụ trách có thể chưa được gán
                GhiChu = string.Empty
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