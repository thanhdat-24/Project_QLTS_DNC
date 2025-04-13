using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Project_QLTS_DNC.Helpers;
namespace Project_QLTS_DNC.ViewModels
{
    public class DangNhapViewModel : ViewModelBase
    {
        public string Email { get; set; }
        public string MatKhau { get; set; }

        public ObservableCollection<TaiKhoanModel> DanhSachTaiKhoan { get; set; }

        public ICommand DangNhapCommand { get; set; }

        private readonly AuthService _authService;

        public DangNhapViewModel()
        {
            _authService = new AuthService();
            DanhSachTaiKhoan = new ObservableCollection<TaiKhoanModel>();
            DangNhapCommand = new RelayCommand(async () => await DangNhapAsync());
        }

        private async Task DangNhapAsync()
        {
            try
            {
                var session = await _authService.DangNhapAsync(Email, MatKhau);

                if (session != null)
                {
                    var loaiTk = await _authService.LayLoaiTaiKhoanTheoUid(session.Id);

                    if (loaiTk == "admin")
                    {
                        var danhSach = await _authService.LayTatCaTaiKhoanNeuLaAdminAsync();
                        DanhSachTaiKhoan.Clear();
                        foreach (var tk in danhSach)
                            DanhSachTaiKhoan.Add(tk);

                        MessageBox.Show("Đăng nhập thành công với vai trò ADMIN!");
                    }
                    else
                    {
                        MessageBox.Show($"Đăng nhập thành công với vai trò: {loaiTk.ToUpper()}");
                    }
                }
                else
                {
                    MessageBox.Show("Sai email hoặc mật khẩu!");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi đăng nhập: " + ex.Message);
            }
        }
    }
}
