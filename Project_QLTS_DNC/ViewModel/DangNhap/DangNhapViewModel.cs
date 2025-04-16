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
        public string TenTaiKhoan { get; set; }
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
                var taiKhoan = await _authService.DangNhapAsync(TenTaiKhoan, MatKhau);

                if (taiKhoan != null)
                {
                    var tenLoai = await _authService.LayTenLoaiTaiKhoanTheoMaLoai(taiKhoan.MaLoaiTk);

                    MessageBox.Show($"Đăng nhập thành công với vai trò: {tenLoai.ToUpper()}");
                }
                else
                {
                    MessageBox.Show("Sai tên tài khoản hoặc mật khẩu!");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi đăng nhập: " + ex.Message);
            }
        }
    }
}
