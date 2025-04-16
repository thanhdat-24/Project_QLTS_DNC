using Supabase;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.NhanVien;

namespace Project_QLTS_DNC.Services
{
    public class AuthService
    {
        private Supabase.Client _client;

        public AuthService() { }

        private async Task<Supabase.Client> GetClientAsync()
        {
            if (_client == null)
                _client = await SupabaseService.GetClientAsync();
            return _client;
        }

        // Đăng nhập bằng bảng "taikhoan"
        public async Task<TaiKhoanModel?> DangNhapAsync(string tenTaiKhoan, string matKhau)
        {
            try
            {
                var client = await GetClientAsync();

                var response = await client
                    .From<TaiKhoanModel>()
                    .Select("*")
                    .Where(x => x.TenTaiKhoan == tenTaiKhoan && x.MatKhau == matKhau)
                    .Single();

                return response;
            }
            catch
            {
                return null;
            }
        }

        // Lấy tên loại tài khoản từ mã loại
        public async Task<string?> LayTenLoaiTaiKhoanTheoMaLoai(int maLoai)
        {
            var client = await GetClientAsync();
            var loaiTaiKhoan = await client
                .From<LoaiTaiKhoanModel>()
                .Where(x => x.MaLoaiTk == maLoai)
                .Single();

            return loaiTaiKhoan?.TenLoaiTk;
        }
    }
}
