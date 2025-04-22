using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Helpers;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Services.QLToanNha;
using Project_QLTS_DNC.View.ChucVu;

namespace Project_QLTS_DNC.View.NhanVien
{
    /// <summary>
    /// Interaction logic for DanhSachNhanVienForm.xaml
    /// </summary>
    public partial class DanhSachNhanVienForm : UserControl
    {
        private readonly NhanVienService _nhanVienService;

        public DanhSachNhanVienForm()
        {
            InitializeComponent();

            _nhanVienService = new NhanVienService();

            // Load dữ liệu khi control được khởi tạo
            Loaded += DanhSachNhanVienForm_Loaded;
        }

        private void DanhSachNhanVienForm_Loaded(object sender, RoutedEventArgs e)
        {
            // Load dữ liệu khi form được load
            LoadDanhSachNhanVienAsync();
            LoadPhongBan();
        }

        public async Task LoadDanhSachNhanVienAsync()
        {
            try
            {
                if (!QuyenNguoiDungHelper.HasPermission("btnNhanVien", "xem"))
                {
                    MessageBox.Show("Bạn không có quyền xem danh sách nhân viên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var nhanVienService = new NhanVienService();
                var danhSachNhanVien = await nhanVienService.LayTatCaNhanVienDtoAsync();

                
                foreach (var nhanVien in danhSachNhanVien)
                {
                    System.Diagnostics.Debug.WriteLine($"MaPB: {nhanVien.MaPb}, MaCV: {nhanVien.MaCv}, TenNV: {nhanVien.TenNv}");
                }

                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    dvDanhSachNhanVien.ItemsSource = danhSachNhanVien;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách nhân viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Error loading employees: {ex}");
            }
        }

        private async void btnThemNhanVien_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnNhanVien", "them"))
            {
                MessageBox.Show("Bạn không có quyền thêm nhân viên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!QuyenNguoiDungHelper.HasPermission("btnNhanVien", "xem"))
            {
                MessageBox.Show("Bạn không có quyền xem danh sách nhân viên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!QuyenNguoiDungHelper.HasPermission("btnNhanVien", "sua"))
            {
                MessageBox.Show("Bạn không có quyền sửa nhân viên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!QuyenNguoiDungHelper.HasPermission("btnNhanVien", "xoa"))
            {
                MessageBox.Show("Bạn không có quyền xóa nhân viên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!QuyenNguoiDungHelper.HasPermission("btnNhanVien", "them"))
            {
                MessageBox.Show("Bạn không có quyền thêm nhân viên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var themNhanVienWindow = new ThemNhanVienForm(this);
            themNhanVienWindow.ShowDialog();
           
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDanhSachNhanVienAsync();
        }

        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnNhanVien", "sua"))
            {
                MessageBox.Show("Bạn không có quyền sửa nhân viên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (dvDanhSachNhanVien.SelectedItem != null)
            {
                var selectedDto = dvDanhSachNhanVien.SelectedItem as NhanVienDto;
                if (selectedDto != null)
                {
                    // Debug the selected values
                    System.Diagnostics.Debug.WriteLine($"Selected DTO - MaPb: {selectedDto.MaPb}, MaCv: {selectedDto.MaCv}");

                    var nhanVienUpdate = new NhanVienModel
                    {
                        MaNV = selectedDto.MaNv,
                        MaPB = selectedDto.MaPb,
                        MaCV = selectedDto.MaCv,
                        TenNV = selectedDto.TenNv,
                        GioiTinh = selectedDto.GioiTinh,
                        DiaChi = selectedDto.DiaChi,
                        Email = selectedDto.Email,
                        SDT = selectedDto.Sdt,
                        NgayVaoLam = selectedDto.NgayVaoLam
                    };

                   
                    System.Diagnostics.Debug.WriteLine($"Created model - MaPB: {nhanVienUpdate.MaPB}, MaCV: {nhanVienUpdate.MaCV}");

                    
                    ThemNhanVienForm formSua = new ThemNhanVienForm(nhanVienUpdate, this);
                    formSua.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn nhân viên để sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!QuyenNguoiDungHelper.HasPermission("btnNhanVien", "xoa"))
                {
                    MessageBox.Show("Bạn không có quyền xóa nhân viên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (dvDanhSachNhanVien.SelectedItem != null)
                {
                    var selectedDto = dvDanhSachNhanVien.SelectedItem as NhanVienDto;
                    if (selectedDto != null)
                    {
                        MessageBoxResult result = MessageBox.Show($"Bạn có chắc chắn muốn xóa nhân viên {selectedDto.TenNv} không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (result == MessageBoxResult.Yes)
                        {
                            bool success = await _nhanVienService.XoaNhanVienAsync(selectedDto.MaNv);
                            if (success)
                            {
                                MessageBox.Show("Xóa nhân viên thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                                await LoadDanhSachNhanVienAsync(); 
                            }
                            else
                            {
                                MessageBox.Show("Xóa nhân viên thất bại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn nhân viên để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Error deleting employee: {ex}");
            }
        }

        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            await LoadDanhSachNhanVienAsync();
        }

        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string searchText = txtSearch.Text.Trim();

                int? selectedPhongBan = cboPhongBan.SelectedItem is PhongBan selectedPB && selectedPB.MaPhongBan != 0
                    ? selectedPB.MaPhongBan
                    : (int?)null;

                if (string.IsNullOrWhiteSpace(searchText) && selectedPhongBan == null)
                {
                    MessageBox.Show("Vui lòng nhập thông tin tìm kiếm hoặc chọn phòng ban để lọc.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var nhanVienDtos = await _nhanVienService.TimKiemNhanVienAsync(searchText, selectedPhongBan);

                dvDanhSachNhanVien.ItemsSource = nhanVienDtos;

                if (nhanVienDtos.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy kết quả nào.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi khi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Lỗi khi tìm kiếm: {ex}");
            }
        }

        private async void cboPhongBan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private async void LoadPhongBan()
        {
            try
            {
                var phongBans = await PhongBanService.LayDanhSachPhongBanAsync();

                if (phongBans != null && phongBans.Any())
                {
                    
                    var allPhongBan = new PhongBan
                    {
                        MaPhongBan = 0,
                        TenPhongBan = "Tất cả"
                    };

                    Application.Current.Dispatcher.Invoke(() => {
                        
                        phongBans.Insert(0, allPhongBan);
                        cboPhongBan.ItemsSource = phongBans;
                        cboPhongBan.DisplayMemberPath = "TenPhongBan";
                        cboPhongBan.SelectedValuePath = "MaPhongBan";
                        cboPhongBan.SelectedIndex = 0;
                    });
                }
                else
                {
                    MessageBox.Show("Không có phòng ban nào để hiển thị.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi load phòng ban: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Error loading departments: {ex}");
            }
        }
    }
}