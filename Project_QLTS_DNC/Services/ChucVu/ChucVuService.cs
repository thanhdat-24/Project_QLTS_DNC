using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.Models.ToaNha;
using Supabase;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace Project_QLTS_DNC.Services.ChucVu
{
    public class ChucVuService: SupabaseService
    {
        
        public ChucVuService(): base()
        {

        }
       

        public async Task<List<ChucVuModel>> GetAllChucVuAsync()
        {
            var client = await SupabaseService.GetClientAsync();
            var result = await client
                .From<ChucVuModel>().Order(x => x.MaChucVu,Ordering.Ascending)
                .Get();

            return result.Models.ToList();
        }
        public async Task<bool> AddChucVuAsync(ChucVuModel chucVu)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var result = await client
                    .From<ChucVuModel>()
                    .Insert(chucVu);

                return result.Models.Any(); 
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public async Task<ChucVuModel> CapNhatChucVuAsync(ChucVuModel chucVu)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                var response = await client
                    .From<ChucVuModel>()
                    .Update(chucVu);

                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update error: {ex.Message}");
                return null;
            }
        }


        public async Task<List<ChucVuModel>> TimKiemChucVuAsync(string keyword)
        {
            var client = await SupabaseService.GetClientAsync();

            
            var resultByName = await client
                .From<ChucVuModel>()
                .Filter("ten_cv", Supabase.Postgrest.Constants.Operator.ILike, $"%{keyword}%")
                .Order(x => x.MaChucVu, Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            var results = resultByName.Models.ToList();

           
            if (int.TryParse(keyword, out int numericKeyword)) 
            {
                var resultById = await client
                    .From<ChucVuModel>()
                    .Filter("ma_cv", Supabase.Postgrest.Constants.Operator.Equals, numericKeyword) 
                    .Order(x => x.MaChucVu, Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Get();

                foreach (var item in resultById.Models)
                {
                    if (!results.Any(x => x.MaChucVu == item.MaChucVu))
                    {
                        results.Add(item);
                    }
                }
            }

            return results;
        }




        public async Task<bool> XoaChucVuAsync(int maChucVu)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                await client
                        .From<ChucVuModel>()
                        .Where(x => x.MaChucVu == maChucVu)
                        .Delete();

                return true;


                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa chức vụ: {ex.Message}");
                return false;
            }
        }




    }
}
