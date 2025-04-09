using Project_QLTS_DNC.Services;
using System;
using System.Windows;
using Supabase.Gotrue;

namespace Project_QLTS_DNC.View.DangNhap
{
    public partial class DangNhapForm : Window
    {
        public User LoggedInUser { get; private set; }
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
            string email = txtUsername.Text;
            string password = isPasswordVisible ? txtVisiblePassword.Text : passwordBox.Password;

            try
            {
                var authService = new AuthService();
                var user = await authService.LoginAsync(email, password);
                if (user != null)
                {
                    LoggedInUser = user;

                    // Ghi nhớ tài khoản nếu được chọn
                    if (unchkRemember.IsChecked == true)
                    {
                        Properties.Settings.Default.SavedUsername = email;
                        Properties.Settings.Default.SavedPassword = password;
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
                    // Sau đăng nhập thành công
                    this.Hide();

                    var mainWindow = new MainWindow(user);
                    Application.Current.MainWindow = mainWindow; 
                    mainWindow.Show();

                    this.Close();

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
