using System.Windows;
using Project_QLTS_DNC.View;
using Project_QLTS_DNC.View.DangNhap;

namespace Project_QLTS_DNC
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var loginForm = new DangNhapForm();
            Application.Current.MainWindow = loginForm; // Gán luôn form đăng nhập
            loginForm.Show();
        }




        private void Application_Startup(object sender, StartupEventArgs e)
        {

        }
    }
}
