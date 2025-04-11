using Project_QLTS_DNC.Models.ToaNha;
using Supabase;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static Supabase.Postgrest.Constants;

namespace Project_QLTS_DNC.Services
{
    public class TangService
    {
        public static async Task<ObservableCollection<Tang>> LayDanhSachTangAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<Tang>().Get();

                return new ObservableCollection<Tang>(response.Models);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi lấy danh sách tầng: {ex.Message}");
                throw;
            }
        }

        public static async Task<Tang> CapNhatTangAsync(Tang tang)
        {
            try
            {
                if (tang.MaTang == null || tang.MaTang == 0)
                    throw new ArgumentException("Mã tầng không hợp lệ.");

                if (string.IsNullOrWhiteSpace(tang.TenTang))
                    throw new ArgumentException("Tên tầng không được để trống.");

                var client = await SupabaseService.GetClientAsync();

                var kiemTra = await client.From<Tang>()
                    .Where(t => t.MaTang == tang.MaTang)
                    .Get();

                if (kiemTra.Models.Count == 0)
                    throw new ArgumentException("Tầng không tồn tại.");

                var response = await client.From<Tang>()
                    .Where(t => t.MaTang == tang.MaTang)
                    .Update(tang);

                if (response.Models.Count > 0)
                    return response.Models[0];

                throw new Exception("Không thể cập nhật tầng!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi cập nhật tầng: {ex.Message}");
                throw;
            }
        }

        public static async Task<Tang> ThemTangAsync(Tang tang)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tang.TenTang))
                    throw new ArgumentException("Tên tầng không được để trống.");

                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<Tang>().Insert(tang);

                if (response.Models.Count > 0)
                    return response.Models[0];

                throw new Exception("Không thể thêm tầng!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi thêm tầng: {ex.Message}");
                throw;
            }
        }

        public static async Task<bool> XoaTangAsync(int maTang)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                await client
                    .From<Tang>()
                    .Filter("ma_tang", Operator.Equals, maTang)
                    .Delete();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LỖI xoá tầng: {ex.Message}");
                return false;
            }
        }
    }
}
