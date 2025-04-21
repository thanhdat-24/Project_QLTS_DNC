using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.ViewModel.TaiKhoan;
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
                loadingIndicator.Visibility = Visibility.Visible;
                

                btnDangNhap.IsEnabled = false;

                

                var authService = new AuthService();
                var taiKhoan = await authService.DangNhapAsync(tenTaiKhoan, matKhau);

                if (taiKhoan != null)
                {
                    LoggedInTaiKhoan = taiKhoan;

                    UserProfileViewModel.SetCurrentUsername(tenTaiKhoan);


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

                    

                    this.Hide();

                    var mainWindow = new MainWindow(taiKhoan); 
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
            finally
            {
                
                loadingIndicator.Visibility = Visibility.Collapsed;
                btnDangNhap.IsEnabled = true;
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

        private void txtUsername_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                
                passwordBox.Focus();  
            }
        }

        private void passwordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                
                btnDangNhap_Click(sender,e);  
            }
        }
    }
}

