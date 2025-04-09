using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Project_QLTS_DNC.Commands;
using Project_QLTS_DNC.Models.DangNhap;

namespace Project_QLTS_DNC.ViewModels
{
    public class DangNhapViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService = new AuthService();
        public DangNhapModel ThongTinDangNhap { get; set; } = new DangNhapModel();

        public ICommand DangNhapCommand { get; }

        public DangNhapViewModel()
        {
            DangNhapCommand = new RelayCommand(async (_) => await DangNhapAsync());
        }

        private async Task DangNhapAsync()
        {
            try
            {
                var user = await _authService.LoginAsync(ThongTinDangNhap.Email, ThongTinDangNhap.MatKhau);
                if (user != null)
                {
                    MessageBox.Show($"Đăng nhập thành công! Xin chào {user.Email}");
                    // TODO: mở giao diện chính hoặc chuyển màn hình
                }
                else
                {
                    MessageBox.Show("Sai email hoặc mật khẩu.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đăng nhập: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
