using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Services.TaiKhoan;
using Project_QLTS_DNC.ViewModel.TaiKhoan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Project_QLTS_DNC.View.TaiKhoan
{
    /// <summary>
    /// Interaction logic for UserProfileForm.xaml
    /// </summary>
    public partial class UserProfileForm : UserControl
    {
        private UserProfileService _profileService;
        private UserProfileViewModel _viewModel;
        private readonly TaiKhoanService _taiKhoanService;
        public UserProfileForm()
        {
            InitializeComponent();
            _profileService = new UserProfileService();
            _taiKhoanService = new TaiKhoanService();

            _viewModel = new UserProfileViewModel(
                _profileService,  
                null,             
                _taiKhoanService  
            );

            this.DataContext = _viewModel;
            this.Loaded += UserProfileForm_Loaded;
            SetupEventHandlers();
        }

        private async void UserProfileForm_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("Bắt đầu tải thông tin người dùng...");
                string username = _viewModel.GetCurrentUsername();
                Console.WriteLine($"Username hiện tại: {username}");

                await _viewModel.LoadUserProfile();

                if (_viewModel.UserProfile != null)
                {
                    Console.WriteLine($"Đã tải thông tin: {_viewModel.UserProfile.ten_nv}");
                    UpdateUI();
                }
                else
                {
                    Console.WriteLine("ERROR: UserProfile là null sau khi load");
                    
                    MessageBox.Show("Không thể tải thông tin người dùng. Vui lòng đăng nhập lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LỖI TRONG UserProfileForm_Loaded: {ex.Message}");
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
        private void UpdateUI()
        {
            if (_viewModel.UserProfile != null)
            {
               
                txtMaNV.Text = _viewModel.UserProfile.ma_nv.ToString();
                txtPhongBan.Text = _viewModel.UserProfile.ten_pb;
                txtChucVu.Text = _viewModel.UserProfile.ten_cv;
                txtFullName.Text = _viewModel.UserProfile.ten_nv;
                txtGender.Text = _viewModel.UserProfile.gioi_tinh;
                txtEmail.Text = _viewModel.UserProfile.email;
                txtAddress.Text = _viewModel.UserProfile.dia_chi;
                txtPhone.Text = _viewModel.UserProfile.sdt;
                txtHireDate.Text = _viewModel.UserProfile.ngay_vao_lam.ToString("dd/MM/yyyy");
                txtUsername.Text = _viewModel.UserProfile.ten_tai_khoan;
                txtAccountType.Text = _viewModel.UserProfile.ten_loai_tk;

                
                txtUserName.Text = _viewModel.UserProfile.ten_nv;
            }
        }

        private void LoadDataToEditForm()
        {
            if (_viewModel.UserProfile != null)
            {
                
                var hoVaTenTextBox = FindName("txtHoVaTen") as TextBox;
                if (hoVaTenTextBox != null)
                {
                    hoVaTenTextBox.Text = _viewModel.UserProfile.ten_nv;
                    Console.WriteLine($"Đã set họ tên: {_viewModel.UserProfile.ten_nv}");
                }
                else
                {
                    Console.WriteLine("Không tìm thấy control txtHoVaTen");
                }

               
                var gioiTinhComboBox = FindName("cboGioiTinh") as ComboBox;
                if (gioiTinhComboBox != null)
                {
                    gioiTinhComboBox.Text = _viewModel.UserProfile.gioi_tinh;
                    Console.WriteLine($"Đã set giới tính: {_viewModel.UserProfile.gioi_tinh}");
                }
                else
                {
                    Console.WriteLine("Không tìm thấy control cboGioiTinh");
                }

                
                var emailTextBox = FindName("txtEmail") as TextBox;
                if (emailTextBox != null)
                {
                    emailTextBox.Text = _viewModel.UserProfile.email;
                    Console.WriteLine($"Đã set email: {_viewModel.UserProfile.email}");
                }
                else
                {
                    Console.WriteLine("Không tìm thấy control txtEmail");
                }

                
                var sdtTextBox = FindName("txtSoDienThoai") as TextBox;
                if (sdtTextBox != null)
                {
                    sdtTextBox.Text = _viewModel.UserProfile.sdt;
                    Console.WriteLine($"Đã set số điện thoại: {_viewModel.UserProfile.sdt}");
                }
                else
                {
                    Console.WriteLine("Không tìm thấy control txtSoDienThoai");
                }

                
                var ngayVaoLamDatePicker = FindName("dpNgayVaoLam") as DatePicker;
                if (ngayVaoLamDatePicker != null)
                {
                    ngayVaoLamDatePicker.SelectedDate = _viewModel.UserProfile.ngay_vao_lam;
                    Console.WriteLine($"Đã set ngày vào làm: {_viewModel.UserProfile.ngay_vao_lam}");
                }
                else
                {
                    Console.WriteLine("Không tìm thấy control dpNgayVaoLam");
                }

               
                var diaChiTextBox = FindName("txtDiaChi") as TextBox;
                if (diaChiTextBox != null)
                {
                    diaChiTextBox.Text = _viewModel.UserProfile.dia_chi;
                    Console.WriteLine($"Đã set địa chỉ: {_viewModel.UserProfile.dia_chi}");
                }
                else
                {
                    Console.WriteLine("Không tìm thấy control txtDiaChi");
                }

                Console.WriteLine("Đã hoàn thành việc điền dữ liệu vào form chỉnh sửa");
            }
            else
            {
                Console.WriteLine("Không thể load dữ liệu vì UserProfile là null");
            }
        }

        private void SetupEventHandlers()
        {
            
            var editButton = FindName("btnDoiThongTin") as Button;
            if (editButton != null)
            {
                editButton.Click += btnDoiThongTin_Click;
            }

            
            var saveChangesButton = FindName("btnLuuThongTin") as Button;
            var cancelEditButton = FindName("btnHuyDoiThongTin") as Button;

            if (saveChangesButton != null)
            {
                saveChangesButton.Click += btnDoiThongTin_Click;
            }

            if (cancelEditButton != null)
            {
                cancelEditButton.Click += btnDoiThongTin_Click;
            }

            
            var changePasswordButton = FindName("btnDoiMatKhau") as Button;
            if (changePasswordButton != null)
            {
                changePasswordButton.Click += btnDoiMatKhau_Click;
            }
        }
        private bool ValidateProfileData()
        {
            var profile = _viewModel.UserProfile;

           
            if (string.IsNullOrWhiteSpace(profile.ten_nv))
            {
                MessageBox.Show("Vui lòng nhập họ và tên", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            
            if (string.IsNullOrWhiteSpace(profile.email) || !IsValidEmail(profile.email))
            {
                MessageBox.Show("Email không hợp lệ", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            
            if (string.IsNullOrWhiteSpace(profile.sdt) || !IsValidPhoneNumber(profile.sdt))
            {
                MessageBox.Show("Số điện thoại không hợp lệ", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            
            return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^(0)(3|5|7|8|9)([0-9]{8})$");
        }

        private void btnDoiMatKhau_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordOverlay.Visibility = Visibility.Visible;
            //var doiMK = new DoiMatKhauForm();
            //doiMK.ShowDialog();
        }

        private void btnDoiThongTin_Click(object sender, RoutedEventArgs e)
        {
            EditProfileOverlay.Visibility = Visibility.Visible;
            var panel = FindName("EditProfileOverlay") as Panel;
            if (panel != null)
            {
                Console.WriteLine("Danh sách control trong form chỉnh sửa:");
                FindAllChildControls(panel);
            }

            LoadDataToEditForm();
        }


        private void FindAllChildControls(DependencyObject parent)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement element && !string.IsNullOrEmpty(element.Name))
                {
                    Console.WriteLine($"- {element.Name} ({element.GetType().Name})");
                }
                FindAllChildControls(child);
            }
        }

        private void btnHuyDoiThongTin_Click(object sender, RoutedEventArgs e)
        {
            EditProfileOverlay.Visibility = Visibility.Collapsed;
        }

        private async void btnUpdatePassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string matKhauCu = pwdCurrent.Password;
                string matKhauMoi = pwdNew.Password;
                string xacNhanMatKhau = pwdConfirm.Password;

                if (string.IsNullOrEmpty(matKhauCu) ||
                    string.IsNullOrEmpty(matKhauMoi) ||
                    string.IsNullOrEmpty(xacNhanMatKhau))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (matKhauMoi != xacNhanMatKhau)
                {
                    MessageBox.Show("Mật khẩu mới và xác nhận mật khẩu không khớp!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!IsStrongPassword(matKhauMoi))
                {
                    MessageBox.Show("Mật khẩu phải chứa ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Thêm các kiểm tra chi tiết
                if (_viewModel == null)
                {
                    MessageBox.Show("Lỗi: ViewModel không tồn tại", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (_viewModel.UserProfile == null)
                {
                    MessageBox.Show("Lỗi: Không có thông tin người dùng", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Log thông tin để debug
                Console.WriteLine($"Tên tài khoản: {_viewModel.UserProfile.ten_tai_khoan}");
                Console.WriteLine($"Mật khẩu cũ: {matKhauCu}");
                Console.WriteLine($"Mật khẩu mới: {matKhauMoi}");

                bool ketQua = await _viewModel.DoiMatKhauAsync(matKhauCu, matKhauMoi);

                if (ketQua)
                {
                    MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    ChangePasswordOverlay.Visibility = Visibility.Collapsed;

                    pwdCurrent.Clear();
                    pwdNew.Clear();
                    pwdConfirm.Clear();
                }
                else
                {
                    MessageBox.Show("Đổi mật khẩu thất bại. Vui lòng kiểm tra lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi chi tiết
                Console.WriteLine($"Lỗi khi đổi mật khẩu: {ex.Message}");
                Console.WriteLine($"Chi tiết: {ex.StackTrace}");

                MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsStrongPassword(string password)
        {
            
            return System.Text.RegularExpressions.Regex.IsMatch(password,
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordOverlay.Visibility = Visibility.Collapsed;
            pwdCurrent.Clear();
            pwdNew.Clear();
            pwdConfirm.Clear();
        }
        private async void btnLuuThayDoi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ValidateProfileData())
                {
                    
                    Console.WriteLine("Bắt đầu cập nhật thông tin người dùng");

                    var result = await Task.Run(() => {
                        try
                        {
                            return _viewModel.CapNhatThongTin();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Lỗi trong CapNhatThongTin: {ex.Message}");
                            Console.WriteLine($"Stack trace: {ex.StackTrace}");
                            return false;
                        }
                    });

                    if (result)
                    {
                        MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        EditProfileOverlay.Visibility = Visibility.Collapsed;

                        
                        UpdateUI();
                    }
                    else
                    {
                        MessageBox.Show("Cập nhật thông tin thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lưu thay đổi: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
