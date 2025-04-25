using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Services;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using System.Windows.Threading;
using System.Windows.Controls;

namespace Project_QLTS_DNC.View.DangNhap
{
    public partial class QuenMatKhauForm : Window
    {
        private DispatcherTimer otpTimer;
        private TimeSpan otpThoiGianConLai;

        public QuenMatKhauForm()
        {
            InitializeComponent();
            
            btnResendOTP.Click += BtnResendOTP_Click;
            btnBackLogin.Click += btnBackLogin_Click;
        }

        private void BatDauDemNguocOTP()
        {
            otpThoiGianConLai = TimeSpan.FromMinutes(5);

            if (otpTimer != null)
            {
                otpTimer.Stop();
                otpTimer = null;
            }

            otpTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            otpTimer.Tick += (s, e) =>
            {
                if (otpThoiGianConLai.TotalSeconds > 0)
                {
                    otpThoiGianConLai = otpThoiGianConLai.Subtract(TimeSpan.FromSeconds(1));
                    txtThoiGianOTP.Text = otpThoiGianConLai.ToString(@"mm\:ss");
                }
                else
                {
                    otpTimer.Stop();
                    txtThoiGianOTP.Text = "00:00";
                }
            };

            otpTimer.Start();
        }

        private async Task<bool> GuiOtpVaChuyenBuoc(string email)
        {
            var otpService = new OtpService();
            bool daGui = await otpService.SendOtpAsync(email);

            if (daGui)
            {
                //MessageBox.Show("Mã xác thực đã được gửi đến email.", "Thành công");
                tabResetPassword.SelectedIndex = 1;
                BatDauDemNguocOTP();
                return true;
            }
            else
            {
                MessageBox.Show("Gửi OTP thất bại.", "Lỗi");
                return false;
            }
        }

        private async void BtnSendOTP_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            var nhanVienService = new NhanVienService();
            txtEmailError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(email))
            {
                txtEmailError.Visibility = Visibility.Visible;
                txtEmailError.Text = "Vui lòng nhập email.";
                return;
            }

            var danhSachNhanVien = await nhanVienService.LayTatCaNhanVienDtoAsync();
            var nhanVien = danhSachNhanVien.FirstOrDefault(nv => nv.Email.ToLower() == email.ToLower());

            if (nhanVien == null)
            {
                txtEmailError.Visibility = Visibility.Visible;
                txtEmailError.Text = "Email không tồn tại trong hệ thống.";
                return;
            }

            await GuiOtpVaChuyenBuoc(email);
        }

        private async void BtnResendOTP_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if (!string.IsNullOrEmpty(email))
            {
                await GuiOtpVaChuyenBuoc(email);
            }
        }

        private async void BtnVerifyOTP_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string otp = txtOTP.Text.Trim();
            var otpService = new OtpService();

            txtOTPError.Visibility = Visibility.Collapsed;

            bool hopLe = await otpService.VerifyOtpAsync(email, otp);
            if (hopLe)
            {
                
                tabResetPassword.SelectedIndex = 2;
            }
            else
            {
                txtOTPError.Visibility = Visibility.Visible;
                txtOTPError.Text = "Mã OTP không đúng hoặc đã hết hạn.";
            }
        }

        private async void BtnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string newPass = txtNewPassword.Password;
            string confirmPass = txtConfirmPassword.Password;

            txtPasswordError.Visibility = Visibility.Collapsed;

            if (newPass != confirmPass || newPass.Length < 8)
            {
                txtPasswordError.Visibility = Visibility.Visible;
                txtPasswordError.Text = "Mật khẩu không khớp hoặc quá yếu.";
                return;
            }

            var taiKhoanService = new TaiKhoanService();
            var danhSach = await taiKhoanService.TimTaiKhoanTheoEmailNhanVienAsync(email);
            var taiKhoan = danhSach.FirstOrDefault();

            if (taiKhoan == null)
            {
                txtPasswordError.Visibility = Visibility.Visible;
                txtPasswordError.Text = "Không tìm thấy tài khoản liên kết với email này.";
                
                return;
            }

            bool doiThanhCong = await taiKhoanService.CapNhatTaiKhoanAsync(
                taiKhoan.MaTk, newPass, taiKhoan.MaLoaiTk, taiKhoan.MaNv
            );

            if (doiThanhCong)
            {
               
                tabResetPassword.SelectedIndex = 3;
            }
            else
            {
                //MessageBox.Show("Đổi mật khẩu thất bại!", "Lỗi");
            }
        }

        private void btnBackLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginForm = new DangNhapForm();
            loginForm.Show();
            this.Close();
        }
        

    }
}
