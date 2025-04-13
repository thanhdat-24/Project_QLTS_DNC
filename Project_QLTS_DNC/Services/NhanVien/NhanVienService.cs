using Project_QLTS_DNC.Models.NhanVien;
using Supabase;
using Supabase.Interfaces;
using Supabase.Postgrest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Services.NhanVien
{
    public class NhanVienService
    {
        public async Task<List<NhanVienModel>> GetNhanVienList()
        {
            var client = await SupabaseService.GetClientAsync();

            // Truy vấn nhân viên và tên phòng ban
            var response = await client.From<NhanVienModel>()
                                       .Select("*, phongban(ten_phong_ban)") // Phòng ban join vào nhân viên
                                       .Get();

            var nhanVienList = response.Models.ToList();

            // Gán tên phòng ban cho từng nhân viên
            foreach (var nv in nhanVienList)
            {
               // nv.TenPhongBan = nv.PhongBan?.TenPhongBan ?? "Không xác định";
            }

            return nhanVienList;
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
