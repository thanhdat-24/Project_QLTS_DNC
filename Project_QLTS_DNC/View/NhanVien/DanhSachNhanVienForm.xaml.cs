using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.DTOs;
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

            var client = SupabaseService.GetClientAsync().Result;
            _nhanVienService = new NhanVienService();
            _ = LoadDanhSachNhanVienAsync();
            LoadPhongBan();
        }

        private async Task LoadDanhSachNhanVienAsync()
        {

            var danhSach = await _nhanVienService.LayTatCaNhanVienDtoAsync();


            if (danhSach != null)
            {
                dvDanhSachNhanVien.ItemsSource = danhSach;
            }
        }

        private void btnThemNhanVien_Click(object sender, RoutedEventArgs e)
        {
            var themNhanVienWindow = new ThemNhanVienForm();
            themNhanVienWindow.Show();
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDanhSachNhanVienAsync();
        }

        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            if (dvDanhSachNhanVien.SelectedItem != null)
            {
                var selectedDto = dvDanhSachNhanVien.SelectedItem as NhanVienDto;
                if (selectedDto != null)
                {
                    var nhanVienUpdate = new NhanVienModel
                    {
                        MaNV = selectedDto.MaNv,
                        TenNV = selectedDto.TenNv,
                        DiaChi = selectedDto.DiaChi,
                        Email = selectedDto.Email,
                        SDT = selectedDto.Sdt,
                        MaPB = selectedDto.MaPb,
                        MaCV = selectedDto.MaCv
                    };

                    ThemNhanVienForm formSua = new ThemNhanVienForm(nhanVienUpdate);
                    formSua.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn nhân viên để sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            if(dvDanhSachNhanVien.SelectedItem != null)
            {
                var selectedDto = dvDanhSachNhanVien.SelectedItem as NhanVienDto;
                if (selectedDto != null)
                {
                    MessageBoxResult result = MessageBox.Show($"Bạn có chắc chắn muốn xóa nhân viên {selectedDto.TenNv} không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        _nhanVienService.XoaNhanVienAsync(selectedDto.MaNv);
                        MessageBox.Show("Xóa nhân viên thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        _ = LoadDanhSachNhanVienAsync();
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn nhân viên để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            var selectedPhongBan = cboPhongBan.SelectedItem as PhongBan;

            
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

                    phongBans.Insert(0, allPhongBan); 

                    
                    cboPhongBan.ItemsSource = phongBans;

                    
                    cboPhongBan.DisplayMemberPath = "TenPhongBan";
                    cboPhongBan.SelectedValuePath = "MaPhongBan";

                    
                    cboPhongBan.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("Không có phòng ban nào để hiển thị.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi load phòng ban: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



    }
}
