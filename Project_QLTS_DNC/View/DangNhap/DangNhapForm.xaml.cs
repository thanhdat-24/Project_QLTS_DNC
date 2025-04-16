using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Services;
using System;
using System.Windows;

namespace Project_QLTS_DNC.View.DangNhap
{
    public partial class DangNhapForm : Window
    {
        public TaiKhoanModel LoggedInTaiKhoan { get; private set; }
        private bool isPasswordVisible = false;

        public DangNhapForm()
        {
            InitializeComponent();
            LoadSavedCredentials();
        }

        private void LoadSavedCredentials()
        {
            txtUsername.Text = Properties.Settings.Default.SavedUsername;

            if (Properties.Settings.Default.RememberMe)
            {
                string savedPassword = Properties.Settings.Default.SavedPassword;
                passwordBox.Password = savedPassword;
                txtVisiblePassword.Text = savedPassword;
                unchkRemember.IsChecked = true;
            }
        }

        private async void btnDangNhap_Click(object sender, RoutedEventArgs e)
        {
            string tenTaiKhoan = txtUsername.Text;
            string matKhau = isPasswordVisible ? txtVisiblePassword.Text : passwordBox.Password;

            try
            {
                var authService = new AuthService();
                var taiKhoan = await authService.DangNhapAsync(tenTaiKhoan, matKhau);

                if (taiKhoan != null)
                {
                    LoggedInTaiKhoan = taiKhoan;

                   

                    // Lưu thông tin nếu người dùng chọn ghi nhớ
                    if (unchkRemember.IsChecked == true)
                    {
                        Properties.Settings.Default.SavedUsername = tenTaiKhoan;
                        Properties.Settings.Default.SavedPassword = matKhau;
                        Properties.Settings.Default.RememberMe = true;
                    }
                    else
                    {
                        Properties.Settings.Default.SavedUsername = "";
                        Properties.Settings.Default.SavedPassword = "";
                        Properties.Settings.Default.RememberMe = false;
                    }
                    Properties.Settings.Default.Save();

                    MessageBox.Show("Đăng nhập thành công!");

                    this.Hide();

                    var mainWindow = new MainWindow(taiKhoan); // Truyền cả danh sách nếu cần
                    Application.Current.MainWindow = mainWindow;
                    mainWindow.Show();

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Sai tên tài khoản hoặc mật khẩu!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đăng nhập: " + ex.Message);
            }
        }


        private void btnShowPassword_Click(object sender, RoutedEventArgs e)
        {
            if (isPasswordVisible)
            {
                txtVisiblePassword.Visibility = Visibility.Collapsed;
                passwordBox.Visibility = Visibility.Visible;
                TogglePasswordVisibility.Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOff;

                passwordBox.Password = txtVisiblePassword.Text;
                passwordBox.Focus();
            }
            else
            {
                txtVisiblePassword.Text = passwordBox.Password;
                txtVisiblePassword.Visibility = Visibility.Visible;
                passwordBox.Visibility = Visibility.Collapsed;
                TogglePasswordVisibility.Kind = MaterialDesignThemes.Wpf.PackIconKind.Eye;
                txtVisiblePassword.Focus();
                txtVisiblePassword.CaretIndex = txtVisiblePassword.Text.Length;
            }
            isPasswordVisible = !isPasswordVisible;
        }

        private void btnForgotPass_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            var quenMatKhauForm = new QuenMatKhauForm();
            quenMatKhauForm.ShowDialog();
            this.Show();
        }
    }
}

