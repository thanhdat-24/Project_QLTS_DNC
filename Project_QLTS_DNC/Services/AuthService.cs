using Supabase;
using Supabase.Gotrue;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.TaiKhoan;
using System.Collections.Generic;

namespace Project_QLTS_DNC.Services
{
    public class AuthService
    {
        private Supabase.Client _client;

        public AuthService()
        {
            // Chỉ khởi tạo khi cần thiết
        }

        private async Task<Supabase.Client> GetClientAsync()
        {
            if (_client == null)
                _client = await SupabaseService.GetClientAsync();
            return _client;
        }

        // Đăng nhập người dùng và trả về đối tượng User của Supabase
        public async Task<User> DangNhapAsync(string email, string password)
        {
            var client = await GetClientAsync();
            var session = await client.Auth.SignIn(email, password);  // Sử dụng SignIn thay vì SignInWithPasswordAsync

            if (session?.User == null)
                throw new System.Exception("Đăng nhập thất bại!");

            return session.User;
        }


        // Lấy loại tài khoản từ UID của người dùng
        public async Task<string> LayLoaiTaiKhoanTheoUid(string uid)
        {
            var client = await GetClientAsync();
            var taiKhoanResult = await client
                .From<TaiKhoanModel>()
                .Where(tk => tk.Uid == uid)
                .Single();

            if (taiKhoanResult == null)
                return null;

            var loaiTkResult = await client
                .From<LoaiTaiKhoanModel>()
                .Where(loai => loai.MaLoaiTk == taiKhoanResult.MaLoaiTk)
                .Single();

            return loaiTkResult?.TenLoaiTk;
        }

        // Kiểm tra xem người dùng hiện tại có phải là Admin không và lấy tất cả tài khoản
        public async Task<List<TaiKhoanModel>> LayTatCaTaiKhoanNeuLaAdminAsync()
        {
            var client = await GetClientAsync();
            var user = client.Auth.CurrentUser;

            if (user == null)
                return null;

            var taiKhoanHienTai = await client
                .From<TaiKhoanModel>()
                .Where(tk => tk.Uid == user.Id)
                .Single();

            // Kiểm tra quyền admin (MaLoaiTk == 1 là Admin)
            if (taiKhoanHienTai?.MaLoaiTk == 1)
            {
                var danhSach = await client
                    .From<TaiKhoanModel>()
                    .Get();
                return danhSach.Models;
            }

            return null;
        }
    }
}
