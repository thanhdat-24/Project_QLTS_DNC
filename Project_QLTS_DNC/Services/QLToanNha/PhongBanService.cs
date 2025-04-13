using Project_QLTS_DNC.Models.ToaNha;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Supabase;

namespace Project_QLTS_DNC.Services.QLToanNha
{
    public class PhongBanService
    {
        public static async Task<ObservableCollection<PhongBan>> LayDanhSachPhongBanAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<PhongBan>().Get();
                return new ObservableCollection<PhongBan>(response.Models);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lấy danh sách phòng ban: {ex.Message}");
                throw;
            }
        }

        public static async Task<PhongBan> ThemPhongBanAsync(PhongBan pb)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<PhongBan>().Insert(pb);
                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi thêm phòng ban: {ex.Message}");
                throw;
            }
        }

        public static async Task<PhongBan> CapNhatPhongBanAsync(PhongBan pb)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client
                    .From<PhongBan>()
                    .Where(x => x.MaPhongBan == pb.MaPhongBan)
                    .Update(pb);

                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật phòng ban: {ex.Message}");
                throw;
            }
        }

        public static async Task<bool> XoaPhongBanAsync(int maPhongBan)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                await client
                    .From<PhongBan>()
                    .Filter("ma_pb", Supabase.Postgrest.Constants.Operator.Equals, maPhongBan)
                    .Delete();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xóa phòng ban: {ex.Message}");
                return false;
            }
        }
    }
}
