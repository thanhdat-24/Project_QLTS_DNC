using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Services;
using Supabase;
using Supabase.Postgrest;   

namespace Project_QLTS_DNC.Services.BaoTri
{
    public class LoaiBaoTriService
    {
        public async Task<List<LoaiBaoTri>> LayDanhSachLoaiBT()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<LoaiBaoTri>().Get();
                Console.WriteLine("Số lượng loại bảo trì: " + response.Models.Count);
                return response.Models;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy danh sách loại bảo trì: {ex.Message}");
                throw;
            }
        }

        public async Task<LoaiBaoTri> LayLoaiBaoTriTheoMa(int maLoaiBaoTri)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<LoaiBaoTri>()
                    .Filter("ma_loai_bao_tri", Supabase.Postgrest.Constants.Operator.Equals, maLoaiBaoTri)
                    .Single();
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy loại bảo trì theo mã: {ex.Message}");
                throw;
            }
        }

        public async Task<LoaiBaoTri> ThemLoaiBaoTri(LoaiBaoTri loaiBaoTri)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Tạo dictionary với tham số
                var parameters = new Dictionary<string, object>
        {
            { "ten_loai_param", loaiBaoTri.TenLoai },
            { "mo_ta_param", loaiBaoTri.MoTa ?? (object)DBNull.Value }
        };

                // Gọi stored function - không cần đọc phản hồi chi tiết
                await client.Rpc("insert_loai_bao_tri", parameters);

                // Lấy dữ liệu vừa thêm bằng cách truy vấn theo tên
                var response = await client.From<LoaiBaoTri>()
                    .Filter("ten_loai", Supabase.Postgrest.Constants.Operator.Equals, loaiBaoTri.TenLoai)
                    .Order("ma_loai_bao_tri", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Limit(1)
                    .Get();

                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thêm loại bảo trì: {ex.Message}");
                throw;
            }
        }
        public async Task<LoaiBaoTri> CapNhatLoaiBaoTri(LoaiBaoTri loaiBaoTri)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Tạo dictionary với tham số cho stored function
                var parameters = new Dictionary<string, object>
        {
            { "ma_loai_param", loaiBaoTri.MaLoaiBaoTri },
            { "ten_loai_param", loaiBaoTri.TenLoai },
            { "mo_ta_param", loaiBaoTri.MoTa ?? (object)DBNull.Value }
        };

                // Gọi stored function update_loai_bao_tri (cần tạo trước trong PostgreSQL)
                await client.Rpc("update_loai_bao_tri", parameters);

                // Lấy dữ liệu đã cập nhật
                return await LayLoaiBaoTriTheoMa(loaiBaoTri.MaLoaiBaoTri);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật loại bảo trì: {ex.Message}");
                throw;
            }
        }
        public async Task<bool> XoaLoaiBaoTri(int maLoaiBaoTri)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                await client.From<LoaiBaoTri>()
                    .Filter("ma_loai_bao_tri", Supabase.Postgrest.Constants.Operator.Equals, maLoaiBaoTri)
                    .Delete();
                return true; // Nếu không có exception, coi là thành công
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa loại bảo trì: {ex.Message}");
                throw;
            }
        }

        public async Task<List<LoaiBaoTri>> TimKiemLoaiBaoTri(string keyword)
        {
            try
            {
                // Lấy tất cả dữ liệu và lọc phía client (do Supabase không hỗ trợ tốt cho search đa điều kiện)
                var allLoaiBaoTri = await LayDanhSachLoaiBT();
                if (string.IsNullOrWhiteSpace(keyword))
                    return allLoaiBaoTri;

                keyword = keyword.ToLower();
                // Tìm theo mã, tên hoặc mô tả
                return allLoaiBaoTri.Where(lbt =>
                    lbt.MaLoaiBaoTri.ToString().Contains(keyword) ||
                    lbt.TenLoai.ToLower().Contains(keyword) ||
                    (lbt.MoTa != null && lbt.MoTa.ToLower().Contains(keyword)))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tìm kiếm loại bảo trì: {ex.Message}");
                throw;
            }
        }
    }
}