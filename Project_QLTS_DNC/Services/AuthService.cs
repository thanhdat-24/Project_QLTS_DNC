using Supabase.Gotrue;
using Project_QLTS_DNC.Services;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Services
{
    public class AuthService
    {
        public async Task<User> LoginAsync(string email, string password)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var session = await client.Auth.SignIn(email, password);
                return session?.User;
            }
            catch (Exception ex)
            {
                // Ghi log hoặc hiển thị lỗi cụ thể ra console
                System.Diagnostics.Debug.WriteLine("Lỗi đăng nhập: " + ex.Message);
                throw; // Ném lại exception để lớp gọi phía trên xử lý
            }
        }


        public async Task LogoutAsync()
        {
            var client = await SupabaseService.GetClientAsync();
            await client.Auth.SignOut();
        }

        public async Task<User> GetCurrentUserAsync()
        {
            var client = await SupabaseService.GetClientAsync();
            return client.Auth.CurrentUser;
        }
    }
}
