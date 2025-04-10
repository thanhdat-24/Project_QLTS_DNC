using Project_QLTS_DNC.Models.QLTaiSan;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Services
{
    public class TaiSanService
    {
        /// <summary>
        /// Lấy danh sách Tài Sản từ Supabase
        /// </summary>
        public static async Task<ObservableCollection<TaiSanModel>> LayDanhSachTaiSanAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<TaiSanModel>().Get();

                // Chuyển đổi từ List<TaiSanModel> sang ObservableCollection<TaiSanModel>
                return new ObservableCollection<TaiSanModel>(response.Models);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy danh sách Tài Sản: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Đếm số lượng Tài Sản hiện có trong hệ thống
        /// </summary>
        public static async Task<int> DemSoLuongTaiSanAsync()
        {
            try
            {
                var dsTaiSan = await LayDanhSachTaiSanAsync();
                return dsTaiSan.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi đếm số lượng Tài Sản: {ex.Message}");
                throw;
            }
        }
    }
}