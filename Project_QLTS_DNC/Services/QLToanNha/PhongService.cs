using Project_QLTS_DNC.Models.ToaNha;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Supabase;
using static Supabase.Postgrest.Constants;

namespace Project_QLTS_DNC.Services.QLToanNha
{
    public class PhongService
    {
        public static async Task<ObservableCollection<Phong>> LayDanhSachPhongAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var result = await client.From<Phong>().Get();

                return new ObservableCollection<Phong>(result.Models);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lấy danh sách phòng: {ex.Message}");
                throw;
            }
        }

        public static async Task<Phong> ThemPhongAsync(Phong phong)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phong.TenPhong))
                    throw new ArgumentException("Tên phòng không được để trống.");

                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<Phong>().Insert(phong);

                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi thêm phòng: {ex.Message}");
                throw;
            }
        }

        public static async Task<Phong> CapNhatPhongAsync(Phong phong)
        {
            try
            {
                if (phong.MaPhong == null || phong.MaPhong == 0)
                    throw new ArgumentException("Mã phòng không hợp lệ.");

                var client = await SupabaseService.GetClientAsync();
                var result = await client.From<Phong>()
                                         .Where(p => p.MaPhong == phong.MaPhong)
                                         .Update(phong);

                return result.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật phòng: {ex.Message}");
                throw;
            }
        }

        public static async Task<bool> XoaPhongAsync(int maPhong)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                await client.From<Phong>()
                            .Filter("ma_phong", Operator.Equals, maPhong)
                            .Delete();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xóa phòng: {ex.Message}");
                return false;
            }
        }
    }
}
