using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Models.NhanVien;

namespace Project_QLTS_DNC.Services.BaoTri
{
    public class LoaiBaoTriService
    {
        public async Task<List<LoaiBaoTri>> LayDanhSachLoaiBT()
        {
            var client = await SupabaseService.GetClientAsync();
            var response = await client.From<LoaiBaoTri>().Get();

            // Kiểm tra dữ liệu trả về
            Console.WriteLine("Số lượng nhân viên: " + response.Models.Count);

            return response.Models;
        }
    }
}
