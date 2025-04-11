using Project_QLTS_DNC.Models.QLTaiSan;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Services.QLTaiSanService
{
    public class PhongService
    {
        /// <summary>
        /// Lấy danh sách Phòng từ Supabase
        /// </summary>
        public static async Task<ObservableCollection<PhongModel>> LayDanhSachPhongAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<PhongModel>().Get();

                // Chuyển đổi từ List<PhongModel> sang ObservableCollection<PhongModel>
                return new ObservableCollection<PhongModel>(response.Models);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy danh sách Phòng: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một Phòng
        /// </summary>
        public static async Task<PhongModel> LayPhongTheoMaAsync(int maPhong)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<PhongModel>()
                    .Where(x => x.MaPhong == maPhong)
                    .Get();

                if (response.Models.Count > 0)
                {
                    return response.Models[0];
                }
                else
                {
                    throw new Exception("Không tìm thấy Phòng với mã đã cho.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy thông tin Phòng: {ex.Message}");
                throw;
            }
        }
    }
}