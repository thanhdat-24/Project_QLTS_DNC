using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.ViewModels.NhanVien;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Services.ChucVu;
using Project_QLTS_DNC.Services.QLToanNha;
using Project_QLTS_DNC.DTOs;

namespace Project_QLTS_DNC.View.NhanVien
{
    public partial class ThemNhanVienForm : Window
    {
        private NhanVienService _nhanVienService;
        private NhanVienModel _updateNV;
        private NhanVienDto _nhanVienDto;
        private PhongBanService _phongBanService;
        private ChucVuService _chucVuService;
        private readonly DanhSachNhanVienForm _danhSachNhanVienForm;
        private bool _isLoaded = false;

        private void InitializeForm()
        {
            _nhanVienService = new NhanVienService();
            _phongBanService = new PhongBanService();
            _chucVuService = new ChucVuService();
        }

        
        public ThemNhanVienForm(DanhSachNhanVienForm danhSachNhanVienForm = null)
        {
            InitializeComponent();
            InitializeForm();
            _danhSachNhanVienForm = danhSachNhanVienForm;

            
            this.Loaded += ThemNhanVienForm_Loaded;
        }

        
        public ThemNhanVienForm(NhanVienModel nhanVienUpdate, DanhSachNhanVienForm danhSachNhanVienForm = null)
        {
            InitializeComponent();
            InitializeForm();
            _updateNV = nhanVienUpdate;
            _danhSachNhanVienForm = danhSachNhanVienForm;

           
            System.Diagnostics.Debug.WriteLine($"ThemNhanVienForm constructor - MaPB: {nhanVienUpdate.MaPB}, MaCV: {nhanVienUpdate.MaCV}");

            
            this.txtTieude.Text = "Cập nhật nhân viên";
            this.btnLuu.Content = "Cập nhật";

           
            this.Loaded += async (s, e) =>
            {
                
                await LoadPhongBanandChucVu();
            };
        }

        private async void ThemNhanVienForm_Loaded(object sender, RoutedEventArgs e)
        {
           
            if (!_isLoaded)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Loading ComboBox data...");
                    await LoadPhongBanandChucVu();
                    _isLoaded = true;

                    System.Diagnostics.Debug.WriteLine("ComboBox data loaded successfully.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading ComboBox data: {ex.Message}");
                    MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CheckThongTinNhanVien(string tenNV, string email, string sdt)
        {
            if (string.IsNullOrEmpty(tenNV))
            {
                MessageBox.Show("Tên nhân viên không được để trống.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
            {
                MessageBox.Show("Email không hợp lệ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(sdt) || !IsValidPhoneNumber(sdt))
            {
                MessageBox.Show("Số điện thoại không hợp lệ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            return phoneNumber.All(char.IsDigit) && phoneNumber.Length == 10;
        }

        async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tenNV = txtTenNV.Text.Trim();
                string email = txtEmail.Text.Trim();
                string sdt = txtSDT.Text.Trim();

                if (!CheckThongTinNhanVien(tenNV, email, sdt)) return;

                string diaChi = txtDiaChi.Text.Trim();
                string gioiTinh = rdoNam.IsChecked == true ? "Nam" : "Nữ";

                var phongBanSelected = cboPhongBan.SelectedItem as PhongBan;
                var chucVuSelected = cboChucVu.SelectedItem as ChucVuModel;

                if (phongBanSelected == null || chucVuSelected == null)
                {
                    MessageBox.Show("Vui lòng chọn phòng ban và chức vụ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var nhanVien = new NhanVienModel
                {
                    TenNV = tenNV,
                    GioiTinh = gioiTinh,
                    DiaChi = diaChi,
                    Email = email,
                    SDT = sdt,
                    MaPB = phongBanSelected.MaPhongBan ?? 0,
                    MaCV = chucVuSelected.MaChucVu
                };

                bool result = false;

                if (_updateNV != null)
                {
                    nhanVien.MaNV = _updateNV.MaNV;
                    var updatedNhanVien = await _nhanVienService.CapNhatNhanVienAsync(nhanVien);
                    result = updatedNhanVien != null;
                }
                else
                {
                    var addedNhanVien = await _nhanVienService.ThemNhanVienAsync(nhanVien);
                    result = addedNhanVien != null;
                }

                if (result)
                {
                    
                    if (_danhSachNhanVienForm != null)
                    {
                        await _danhSachNhanVienForm.LoadDanhSachNhanVienAsync();
                    }

                    MessageBox.Show(_updateNV != null ? "Cập nhật nhân viên thành công." : "Thêm nhân viên thành công.",
                                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
                System.Diagnostics.Debug.WriteLine($"Error saving employee: {ex}");
                MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadNhanVienData()
        {
            if (_updateNV == null) return;

            System.Diagnostics.Debug.WriteLine($"Loading employee data: MaPB={_updateNV.MaPB}, MaCV={_updateNV.MaCV}");

            txtMaNV.Text = _updateNV.MaNV.ToString();
            txtTenNV.Text = _updateNV.TenNV;
            txtDiaChi.Text = _updateNV.DiaChi;
            txtEmail.Text = _updateNV.Email;
            txtSDT.Text = _updateNV.SDT;
            dpNgayVaoLam.SelectedDate = _updateNV.NgayVaoLam;
            rdoNam.IsChecked = _updateNV.GioiTinh == "Nam";
            rdoNu.IsChecked = _updateNV.GioiTinh == "Nữ";


            this.Title = _updateNV != null ? "Cập nhật nhân viên" : "Thêm nhân viên";
            this.txtTieude.Text = _updateNV != null ? "Cập nhật nhân viên" : "Thêm nhân viên";
            this.btnLuu.Content = _updateNV != null ? "Cập nhật" : "Thêm";

            foreach (PhongBan pb in cboPhongBan.Items)
            {
                if (pb.MaPhongBan == _updateNV.MaPB)
                {
                    cboPhongBan.SelectedItem = pb;
                    System.Diagnostics.Debug.WriteLine($"Selected PhongBan: {pb.TenPhongBan}, MaPB: {pb.MaPhongBan}");
                    break;
                }
            }


            foreach (ChucVuModel cv in cboChucVu.Items)
            {
                if (cv.MaChucVu == _updateNV.MaCV)
                {
                    cboChucVu.SelectedItem = cv;
                    System.Diagnostics.Debug.WriteLine($"Selected ChucVu: {cv.TenChucVu}, MaCV: {cv.MaChucVu}");
                    break;
                }
            }
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cboPhongBan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboPhongBan.SelectedItem is PhongBan selectedPhongBan)
            {
                int maPhongBan = selectedPhongBan.MaPhongBan ?? 0;
                System.Diagnostics.Debug.WriteLine($"Phòng ban đã chọn: {selectedPhongBan.TenPhongBan}, ID: {maPhongBan}");
            }
        }

        private void cboChucVu_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                
                var danhSachPhongBan = await PhongBanService.LayDanhSachPhongBanAsync();
                System.Diagnostics.Debug.WriteLine($"Loaded {danhSachPhongBan?.Count ?? 0} PhongBan items");

               
                var danhSachChucVu = await _chucVuService.GetAllChucVuAsync();
                System.Diagnostics.Debug.WriteLine($"Loaded {danhSachChucVu?.Count ?? 0} ChucVu items");

                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    
                    cboPhongBan.ItemsSource = danhSachPhongBan;
                    cboPhongBan.DisplayMemberPath = "TenPhongBan";
                    cboPhongBan.SelectedValuePath = "MaPhongBan";

                    
                    cboChucVu.ItemsSource = danhSachChucVu;
                    cboChucVu.DisplayMemberPath = "TenChucVu";
                    cboChucVu.SelectedValuePath = "MaChucVu";

                    
                    if (_updateNV != null)
                    {
                       
                        PhongBan selectedPhongBan = null;
                        foreach (var pb in danhSachPhongBan)
                        {
                            if (pb.MaPhongBan == _updateNV.MaPB)
                            {
                                selectedPhongBan = pb;
                                break;
                            }
                        }

                        ChucVuModel selectedChucVu = null;
                        foreach (var cv in danhSachChucVu)
                        {
                            if (cv.MaChucVu == _updateNV.MaCV)
                            {
                                selectedChucVu = cv;
                                break;
                            }
                        }

                        
                        if (selectedPhongBan != null)
                        {
                            cboPhongBan.SelectedItem = selectedPhongBan;
                            System.Diagnostics.Debug.WriteLine($"Đã chọn phòng ban: {selectedPhongBan.TenPhongBan}");
                        }

                        if (selectedChucVu != null)
                        {
                            cboChucVu.SelectedItem = selectedChucVu;
                            System.Diagnostics.Debug.WriteLine($"Đã chọn chức vụ: {selectedChucVu.TenChucVu}");
                        }

                        
                        LoadNhanVienData();
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading data for ComboBoxes: {ex.Message}");
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}