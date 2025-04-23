using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Services.TaiKhoan;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.ViewModel.TaiKhoan
{
    public class UserProfileViewModel : INotifyPropertyChanged
    {
        private readonly UserProfileService _profileService;
        private UserProfileDTO _userProfile;
        private readonly AuthService _authService;
        private static string _currentUsername;
        private readonly TaiKhoanService _taiKhoanService;

        public UserProfileDTO UserProfile
        {
            get => _userProfile;
            set
            {
                _userProfile = value;
                OnPropertyChanged(nameof(UserProfile));
                OnPropertyChanged(nameof(TenHienThi));
            }
        }
        public string TenHienThi
        {
            get
            {
                if (UserProfile != null)
                {
                    return !string.IsNullOrEmpty(UserProfile.ten_nv) ? UserProfile.ten_nv : UserProfile.ten_tai_khoan;
                }
                return "Chưa đăng nhập";
            }
        }


        public UserProfileViewModel(
            UserProfileService profileService,
            AuthService authService = null,
            TaiKhoanService taiKhoanService = null)
        {
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _authService = authService;
            _taiKhoanService = taiKhoanService ?? new TaiKhoanService();
            _ = LoadUserProfile();
        }

    

        public static void SetCurrentUsername(string username)
        {
            _currentUsername = username;
        }

        public static void Logout()
        {
            _currentUsername = null;
        }

        public string GetCurrentUsername()
        {
            // Kiểm tra username đã được set chưa
            if (string.IsNullOrEmpty(_currentUsername))
            {
                // Fallback to default if not set explicitly
                return "admin"; // Temporary hardcode as fallback
            }
            return _currentUsername;
        }

        public async Task LoadUserProfile()
        {
            try
            {
                string tenTaiKhoan = GetCurrentUsername();
                Console.WriteLine($"Đang tải thông tin cho user: {tenTaiKhoan}");

                var profile = await _profileService.LayThongTinCaNhanAsync(tenTaiKhoan);
                if (profile != null)
                {
                    UserProfile = profile; 
                    Console.WriteLine($"Đã tải thành công thông tin cho: {profile.ten_nv}");
                }
                else
                {
                    Console.WriteLine("Không thể tải thông tin người dùng");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tải thông tin cá nhân: {ex.Message}");
            }
        }

        public bool CapNhatThongTin()
        {
            try
            {
                var ketQua = _profileService.CapNhatThongTinCaNhanAsync(UserProfile).Result;
                return ketQua;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi cập nhật thông tin: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DoiMatKhauAsync(string matKhauCu, string matKhauMoi)
        {
            try
            {
                // Kiểm tra UserProfile và TenTaiKhoan
                if (UserProfile == null || string.IsNullOrEmpty(UserProfile.ten_tai_khoan))
                {
                    Console.WriteLine("Lỗi: UserProfile hoặc TenTaiKhoan không tồn tại");
                    return false;
                }

                // Kiểm tra _taiKhoanService
                if (_taiKhoanService == null)
                {
                    Console.WriteLine("Lỗi: TaiKhoanService không được khởi tạo");
                    return false;
                }

                // Gọi phương thức đổi mật khẩu
                return await _taiKhoanService.DoiMatKhauAsync(
                    UserProfile.ten_tai_khoan,
                    matKhauCu,
                    matKhauMoi
                );
            }
            catch (Exception ex)
            {
                // Ghi log lỗi chi tiết
                Console.WriteLine($"Lỗi trong DoiMatKhau: {ex.Message}");
                Console.WriteLine($"Chi tiết: {ex.StackTrace}");
                return false;
            }
        }

        // Implement INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}