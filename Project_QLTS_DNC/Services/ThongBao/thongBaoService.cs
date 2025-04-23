using Project_QLTS_DNC.Models.ThongBao;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Services.ThongBao
{
    public class ThongBaoService : SupabaseService
    {
        public async Task<bool> ThemThongBaoAsync(ThongBaoModel tb)
        {
            try
            {
                var client = await GetClientAsync();
                var response = await client.From<ThongBaoModel>().Insert(tb);
                return response != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Lỗi thêm thông báo] {ex.Message}");
                return false;
            }
        }

        public async Task<List<ThongBaoModel>> LayThongBaoTheoNguoiDungAsync()
        {
            try
            {
                var client = await GetClientAsync();

                int maTaiKhoan = Helpers.ThongTinDangNhap.TaiKhoanDangNhap.MaTk;
                string? tenLoaiTk = Helpers.ThongTinDangNhap.LoaiTaiKhoanDangNhap?.TenLoaiTk?.Trim().ToLower();

                var builder = client
                    .From<ThongBaoModel>()
                    .Order(x => x.ThoiGian, Supabase.Postgrest.Constants.Ordering.Descending);

                // 🔐 Chỉ admin mới được xem tất cả
                if (tenLoaiTk != "admin")
                {
                    builder = builder.Where(x => x.MaTaiKhoan == maTaiKhoan);
                }

                var response = await builder.Get();
                return response.Models;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Lỗi lấy thông báo] {ex.Message}");
                return new List<ThongBaoModel>();
            }
        }



        public async Task<bool> DanhDauDaDocAsync(int id)
        {
            try
            {
                var client = await GetClientAsync();

                // 👇 Phải lấy đúng bản ghi rồi update!
                var response = await client
                    .From<ThongBaoModel>()
                    .Where(x => x.Id == id)
                    .Single();

                if (response == null)
                    return false;

                response.DaDoc = true;

                var updateResult = await client
                    .From<ThongBaoModel>()
                    .Update(response);

                return updateResult != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Lỗi cập nhật thông báo] {ex.Message}");
                return false;
            }
        }

        //public async Task<bool> XoaThongBaoDaDocAsync()
        //{
        //    try
        //    {
        //        var client = await GetClientAsync();
        //        int maTaiKhoan = Helpers.ThongTinDangNhap.TaiKhoanDangNhap.MaTk;

        //        var response = await client
        //            .From<ThongBaoModel>()
        //            .Where(x => x.DaDoc == true && x.MaTaiKhoan == maTaiKhoan)
        //            .Delete();

        //        return response != null;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[Lỗi xóa thông báo đã đọc] {ex.Message}");
        //        return false;
        //    }
        //}

    }
}
