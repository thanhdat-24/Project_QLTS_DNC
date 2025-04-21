using Project_QLTS_DNC.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Project_QLTS_DNC.Models.OTP;
using MimeKit;
using MailKit.Net.Smtp;

namespace Project_QLTS_DNC.Services
{
    public class OtpService : SupabaseService
    {
        public OtpService() : base() { }

        /// <summary>
        /// Tạo và lưu mã OTP mới cho email, thời hạn 5 phút
        /// </summary>
        public async Task<bool> SendOtpAsync(string email)
        {
            var client = await SupabaseService.GetClientAsync();
            var otpCode = new Random().Next(100000, 999999).ToString();

            var otp = new OtpCodeModel
            {
                Email = email,
                OtpCode = otpCode,
                CreatedAt = DateTime.UtcNow,
                ExpireAt = DateTime.UtcNow.AddMinutes(5),
                Verified = false
            };

            var response = await client.From<OtpCodeModel>().Insert(otp);

            // === Gửi mail thực sự bằng MailKit ===
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("QLTS DNC System", "your_email@gmail.com"));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = "Mã xác thực OTP - QLTS DNC";

                message.Body = new TextPart("plain")
                {
                    Text = $"Mã xác thực OTP của bạn là: {otpCode}. Mã này sẽ hết hạn sau 5 phút."
                };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync("bao219689@student.nctu.edu.vn", "rjkz ywzv yczz mvoj");
                
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);

                Console.WriteLine($"OTP đã gửi tới {email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi mail: {ex.Message}");
                return false;
            }

            return response.Models.Any();
        }


        /// <summary>
        /// Xác minh OTP với email và mã
        /// </summary>
        public async Task<bool> VerifyOtpAsync(string email, string otpInput)
        {
            var client = await SupabaseService.GetClientAsync();

            var result = await client.From<OtpCodeModel>()
                .Where(x => x.Email == email && x.Verified == false)
                .Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();

            var latestOtp = result.Models.FirstOrDefault();

            if (latestOtp == null)
                return false;

            if (latestOtp.ExpireAt < DateTime.UtcNow)
                return false;

            if (latestOtp.OtpCode != otpInput)
                return false;

            // Đánh dấu đã xác minh
            latestOtp.Verified = true;
            await client.From<OtpCodeModel>().Update(latestOtp);

            return true;
        }
    }
}
