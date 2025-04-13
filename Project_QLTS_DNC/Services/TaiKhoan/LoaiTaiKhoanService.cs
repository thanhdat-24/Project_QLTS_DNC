using Project_QLTS_DNC.Models.TaiKhoan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Services.TaiKhoan
{
    public class LoaiTaiKhoanService
    {
        public async Task<List<LoaiTaiKhoanModel>> LayDSLoaiTK()
        {
            var client = await SupabaseService.GetClientAsync();
            var result = await client.From<LoaiTaiKhoanModel>().Get();
            return result.Models.ToList();
        }
    }
}
