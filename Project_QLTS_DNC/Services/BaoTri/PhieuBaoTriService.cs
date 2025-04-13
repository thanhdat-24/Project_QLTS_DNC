using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models.BaoTri;

namespace Project_QLTS_DNC.Services
{
    public class PhieuBaoTriService
    {
        public async Task<List<PhieuBaoTri>> GetPhieuBaoTriAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                var response = await client.From<PhieuBaoTri>().Get();

                // Kiểm tra dữ liệu trả về
                Console.WriteLine("Số lượng phiếu bảo trì: " + response.Models.Count);

                return response.Models;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi truy vấn dữ liệu: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<PhieuBaoTri>();
            }
        }
    }
}