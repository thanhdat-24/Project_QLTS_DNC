using Supabase.Gotrue;
using System;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Services
{
    public class AuthService
    {
        // Phương thức đăng nhập
        public async Task<User> LoginAsync(string email, string password)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();  // Lấy Supabase client
                var session = await client.Auth.SignIn(email, password); // Đăng nhập

                // Kiểm tra kết quả đăng nhập
                if (session != null && session.User != null)
                {
                    string userId = session.User.Id;  // Lấy UID của người dùng
                    Console.WriteLine($"UID của người dùng: {userId}");
                    return session.User;  // Trả về người dùng sau khi đăng nhập thành công
                }
                else
                {
                    throw new Exception("Đăng nhập không thành công.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi đăng nhập: " + ex.Message);
                throw; // Ném lại exception để lớp gọi phía trên xử lý
            }
        }


        // Phương thức đăng xuất
        public async Task LogoutAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();  // Lấy Supabase client
                await client.Auth.SignOut();  // Đăng xuất người dùng
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khi đăng xuất nếu có
                System.Diagnostics.Debug.WriteLine("Lỗi đăng xuất: " + ex.Message);
                throw;
            }
        }

        // Lấy người dùng hiện tại
        public async Task<User> GetCurrentUserAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();  // Lấy Supabase client
                return client.Auth.CurrentUser;  // Trả về người dùng hiện tại
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu không lấy được người dùng
                System.Diagnostics.Debug.WriteLine("Lỗi lấy thông tin người dùng: " + ex.Message);
                return null;
            }
        }
    }
}
