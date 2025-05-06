using Newtonsoft.Json;
using Project_QLTS_DNC.Models.ThongTinCongTy;
using System;
using System.IO;

namespace Project_QLTS_DNC.Services
{
    public class ThongTinCongTyService
    {
        private static readonly string filePath = "thongtincongty.json";

        /// <summary>
        /// Đọc thông tin công ty từ file JSON
        /// </summary>
        /// <returns>Thông tin công ty hoặc một đối tượng mặc định nếu không thể đọc</returns>
        public static ThongTinCongTy DocThongTinCongTy()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string jsonContent = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<ThongTinCongTy>(jsonContent) ?? new ThongTinCongTy();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi đọc thông tin công ty: {ex.Message}");
                // Không hiển thị lỗi với người dùng, chỉ ghi log
            }

            // Trả về đối tượng mặc định nếu không thể đọc file
            return new ThongTinCongTy();
        }
    }
}