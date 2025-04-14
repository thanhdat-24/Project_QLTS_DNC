using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models;
using Supabase;

namespace Project_QLTS_DNC.Services.BaoTri
{
    public class KiemKeTaiSanService
    {
        public async Task<List<KiemKeTaiSan>> GetKiemKeTaiSanAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    throw new Exception("Không thể kết nối Supabase Client");

                var response = await client.From<KiemKeTaiSan>().Get();

                Console.WriteLine("Số lượng tài sản kiểm kê: " + response.Models.Count);

                return response.Models;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi truy vấn dữ liệu: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<KiemKeTaiSan>();
            }
        }
    }
}
