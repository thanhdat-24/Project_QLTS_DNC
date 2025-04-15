using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.ViewModels.NhanVien;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Project_QLTS_DNC.Services.ChucVu;
using Project_QLTS_DNC.Services.QLToanNha;


namespace Project_QLTS_DNC.View.NhanVien
{
    public partial class ThemNhanVienForm : Window
    {
        private NhanVienService _nhanVienService;
        private NhanVienModel _updateNV;
        private PhongBanService _phongBanService;
        private ChucVuService _chucVuService;

        public ThemNhanVienForm()
        {
            InitializeComponent();
            _nhanVienService = new NhanVienService();
            _phongBanService = new PhongBanService();
            _chucVuService = new ChucVuService();

            _ = LoadPhongBanandChucVu(); 
        }



        public ThemNhanVienForm(NhanVienModel nhanVienUpdate)
        {
            InitializeComponent();
            _updateNV = nhanVienUpdate;
            _nhanVienService = new NhanVienService();
            _phongBanService = new PhongBanService();
            _chucVuService = new ChucVuService();

            _ = LoadPhongBanandChucVu(); 
            LoadNhanVienData();
        }



        private async Task InitializeAsync(NhanVienModel nhanVienUpdate)
        {
            _updateNV = nhanVienUpdate;
            _nhanVienService = new NhanVienService();
            LoadNhanVienData();
        }


        async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tenNV = txtTenNV.Text.Trim();
                string diaChi = txtDiaChi.Text.Trim();
                string email = txtEmail.Text.Trim();
                string sdt = txtSDT.Text.Trim();

                if (string.IsNullOrEmpty(tenNV))
                {
                    MessageBox.Show("Tên nhân viên không được để trống.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                string gioiTinh = rdoNam.IsChecked == true ? "Nam" : "Nữ";
                var nhanVien = new NhanVienModel
                {
                    TenNV = tenNV,
                    GioiTinh = gioiTinh,
                    DiaChi = diaChi,
                    Email = email,
                    SDT = sdt,
                    MaPB = (cboPhongBan.SelectedItem as PhongBan)?.MaPhongBan ?? 0,
                    MaCV = (cboChucVu.SelectedItem as ChucVuModel)?.MaChucVu ?? 0
                };

                var result = false;

                if (_updateNV != null)
                {
                    try
                    {
                        nhanVien.MaNV = _updateNV.MaNV;
                        var updatedNhanVien = await _nhanVienService.CapNhatNhanVienAsync(nhanVien);
                        result = updatedNhanVien != null;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Update exception: {ex.Message}");
                        MessageBox.Show($"Lỗi khi cập nhật: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    try
                    {
                        var addedNhanVien = await _nhanVienService.ThemNhanVienAsync(nhanVien);
                        result = addedNhanVien != null;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Add exception: {ex.Message}");
                        MessageBox.Show($"Lỗi khi thêm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                if (result)
                {
                    MessageBox.Show(_updateNV != null ? "Cập nhật nhân viên thành công." : "Thêm nhân viên thành công.",
                                   "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);


                    try
                    {
                        var parentWindow = System.Windows.Window.GetWindow(this);
                        if (parentWindow != null)
                        {
                            var danhSachNhanVienViewModel = parentWindow.DataContext as DanhSachNhanVienViewModel;
                            if (danhSachNhanVienViewModel != null)
                            {
                                await danhSachNhanVienViewModel.LoadNhanVienListAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"LoadNhanVienAsync exception: {ex.Message}");
                    }

                    this.Close();
                }
                else
                {
                    MessageBox.Show(_updateNV != null ? "Cập nhật nhân viên thất bại." : "Thêm nhân viên thất bại.",
                                   "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General exception: {ex.Message}");
                MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Load dữ liệu nhân viên khi sửa
        private void LoadNhanVienData()
        {
            if (_updateNV != null)
            {
                txtTenNV.Text = _updateNV.TenNV;
                txtDiaChi.Text = _updateNV.DiaChi;
                txtEmail.Text = _updateNV.Email;
                txtSDT.Text = _updateNV.SDT;

                
                if (_updateNV.GioiTinh == "Nam")
                {
                    rdoNam.IsChecked = true;
                }
                else
                {
                    rdoNu.IsChecked = true;
                }

                this.txtTieude.Text = "Cập nhật nhân viên";
                this.Title = "Cập nhật nhân viên";
                btnLuu.Content = "Cập nhật";
            }
        }


        // Hủy thao tác
        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cboPhongBan_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cboPhongBan.SelectedItem is PhongBan selectedPhongBan)
            {

                int maPhongBan = selectedPhongBan.MaPhongBan ?? 0;
                System.Diagnostics.Debug.WriteLine($"Phòng ban đã chọn: {selectedPhongBan.TenPhongBan}, ID: {maPhongBan}");
            }
        }

        private void cboChucVu_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cboChucVu.SelectedItem is ChucVuModel selectedChucVu)
            {

                int maChucVu = selectedChucVu.MaChucVu;
                System.Diagnostics.Debug.WriteLine($"Chức vụ đã chọn: {selectedChucVu.TenChucVu}, ID: {maChucVu}");
            }
        }

        private async Task LoadPhongBanandChucVu()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                
                var danhSachPhongBan = await PhongBanService.LayDanhSachPhongBanAsync();
                var danhSachChucVu = await _chucVuService.GetAllChucVuAsync();

                if (danhSachPhongBan != null)
                {
                    cboPhongBan.ItemsSource = danhSachPhongBan;
                    cboPhongBan.SelectedIndex = 0;
                }
                if (danhSachChucVu != null)
                {
                    cboChucVu.ItemsSource = danhSachChucVu;
                    cboChucVu.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadPhongBanandChucVu exception: {ex.Message}");
            }
        }
    }
}
