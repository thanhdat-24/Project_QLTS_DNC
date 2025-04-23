using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.NhanVien;
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
                var thongBaos = response.Models;

                // 🔍 Nếu là admin, ẩn các dòng "📥 Bạn đã tạo..."
                if (tenLoaiTk == "admin")
                {
                    thongBaos = thongBaos
                        .Where(tb => tb.NoiDung == null || !tb.NoiDung.StartsWith("📥 Bạn đã tạo"))
                        .ToList();
                }

                return thongBaos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Lỗi lấy thông báo] {ex.Message}");
                return new List<ThongBaoModel>();
            }
        }


        public async Task<List<TaiKhoanModel>> LayDanhSachTaiKhoanTheoLoaiAsync(int loaiTk)
        {
            try
            {
                var client = await GetClientAsync();
                var result = await client
                    .From<TaiKhoanModel>()
                    .Where(x => x.MaLoaiTk == loaiTk)
                    .Get();

                return result.Models;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Lỗi lấy danh sách tài khoản theo loại] {ex.Message}");
                return new List<TaiKhoanModel>();
            }
        }
        public async Task ThongBaoTaoPhieuAsync(int maPhieu, string loaiPhieu, int maTaiKhoanNguoiTao)
        {
            try
            {
                var client = await GetClientAsync();

                // Lấy tài khoản người tạo
                var taiKhoan = await client.From<TaiKhoanModel>()
                                           .Where(x => x.MaTk == maTaiKhoanNguoiTao)
                                           .Single();

                var nhanVien = await client.From<NhanVienModel>()
                                           .Where(x => x.MaNV == taiKhoan.MaNv)
                                           .Single();

                string tenNguoiTao = nhanVien?.TenNV ?? "Không rõ";

                // 🔍 Lấy toàn bộ tài khoản admin
                var dsAdmin = await client.From<TaiKhoanModel>()
                                          .Where(x => x.MaLoaiTk == 1)
                                          .Get();


                var maAdminList = dsAdmin.Models.Select(a => a.MaTk).ToList();
                bool laAdmin = maAdminList.Contains(maTaiKhoanNguoiTao);

                Console.WriteLine($"✅ laAdmin = {laAdmin}, người tạo: {maTaiKhoanNguoiTao}");

                // 📨 Gửi thông báo cho chính người tạo nếu không phải admin
                if (!laAdmin)
                {
                    await client.From<ThongBaoModel>().Insert(new ThongBaoModel
                    {
                        NoiDung = $"📥 Bạn đã tạo {loaiPhieu} #{maPhieu}",
                        MaTaiKhoan = maTaiKhoanNguoiTao,
                        ThoiGian = DateTime.Now,
                        DaDoc = false
                    });
                }

                // 📢 Gửi thông báo cho admin khác
                foreach (var admin in dsAdmin.Models)
                {
                    if (admin.MaTk == maTaiKhoanNguoiTao)
                        continue;

                    await client.From<ThongBaoModel>().Insert(new ThongBaoModel
                    {
                        NoiDung = $"📢 Nhân viên {tenNguoiTao} đã tạo {loaiPhieu} #{maPhieu}",
                        MaTaiKhoan = admin.MaTk,
                        ThoiGian = DateTime.Now,
                        DaDoc = false
                    });
                    Console.WriteLine($"✅ laAdmin = {laAdmin}, người tạo: {maTaiKhoanNguoiTao}");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Lỗi gửi thông báo tạo phiếu] {ex.Message}");
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
