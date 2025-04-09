using Project_QLTS_DNC.Models;
using Supabase;
using Supabase.Postgrest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Services
{
    public class NhanVienService
    {
        public async Task<List<NhanVienModel>> LayDanhSachNhanVienAsync()
        {
            var client = await SupabaseService.GetClientAsync();
            var response = await client.From<NhanVienModel>().Get();

            // Kiểm tra dữ liệu trả về
            Console.WriteLine("Số lượng nhân viên: " + response.Models.Count);

            return response.Models;
        }

        public async Task<bool> ThemNhanVien(NhanVienModel nv)
        {
            var client = await SupabaseService.GetClientAsync();

            var result = await client.Rpc("them_nhan_vien", new Dictionary<string, object>
            {
                { "p_ma_pb", nv.MaPB },
                { "p_ma_cv", nv.MaCV },
                { "p_ten_nv", nv.TenNV },
                { "p_gioi_tinh", nv.GioiTinh },
                { "p_dia_chi", nv.DiaChi },
                { "p_email", nv.Email },
                { "p_sdt", nv.SDT },
                { "p_ngay_vao_lam", nv.NgayVaoLam }
            });

            return result != null;
        }

        // Lấy danh sách chức vụ
        public async Task<List<ChucVuModel>> GetChucVusAsync()
        {
            var client = await SupabaseService.GetClientAsync();

            // Truy vấn danh sách chức vụ từ Supabase
            var response = await client.From<ChucVuModel>().Get();

            // Kiểm tra dữ liệu trả về
            if (response != null && response.Models.Count > 0)
            {
                return response.Models;
            }
            else
            {
                return new List<ChucVuModel>(); // Trả về danh sách trống nếu không có dữ liệu
            }
        }
    }
}
