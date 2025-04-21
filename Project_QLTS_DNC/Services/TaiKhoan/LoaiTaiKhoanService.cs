using Project_QLTS_DNC.Models.TaiKhoan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase;
using static Supabase.Postgrest.Constants;

namespace Project_QLTS_DNC.Services.TaiKhoan
{
    public class LoaiTaiKhoanService : SupabaseService
    {
        public LoaiTaiKhoanService() : base()
        {
        }

        public async Task<List<LoaiTaiKhoanModel>> LayDSLoaiTK()
        {
            var client = await SupabaseService.GetClientAsync();
            var result = await client.From<LoaiTaiKhoanModel>().Order(x => x.MaLoaiTk, Ordering.Ascending)
                .Get();
            return result.Models.ToList();
        }

        public async Task<LoaiTaiKhoanModel> ThemLoaiTaiKhoan(LoaiTaiKhoanModel loaiTk)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrEmpty(loaiTk.TenLoaiTk))
                {
                    throw new Exception("Tên loại tài khoản không được để trống");
                }

                var client = await SupabaseService.GetClientAsync();

                // Kiểm tra tên trùng lặp
                var existingRecords = await client
                    .From<LoaiTaiKhoanModel>()
                    .Where(x => x.TenLoaiTk == loaiTk.TenLoaiTk)
                    .Get();

                if (existingRecords.Models.Count > 0)
                {
                    throw new Exception("Tên loại tài khoản đã tồn tại");
                }

                // Thêm mới bản ghi
                var newLoaiTk = new LoaiTaiKhoanModel
                {
                    TenLoaiTk = loaiTk.TenLoaiTk,
                    MoTa = loaiTk.MoTa ?? string.Empty
                };

                var result = await client
                    .From<LoaiTaiKhoanModel>()
                    .Insert(newLoaiTk);

                return result.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding LoaiTaiKhoan: {ex.Message}");
                throw new Exception($"Không thể lưu loại tài khoản: {ex.Message}", ex);
            }
        }

        public async Task<LoaiTaiKhoanModel> CapNhatLoaiTk(LoaiTaiKhoanModel _loaiTk)
        {
            try
            {
                if (string.IsNullOrEmpty(_loaiTk.TenLoaiTk))
                {
                    throw new Exception("Tên loại tài khoản không được để trống");
                }

                var client = await SupabaseService.GetClientAsync();

                // Kiểm tra tên trùng lặp (trừ chính bản ghi đang sửa)
                var existingRecords = await client
                    .From<LoaiTaiKhoanModel>()
                    .Where(x => x.TenLoaiTk == _loaiTk.TenLoaiTk && x.MaLoaiTk != _loaiTk.MaLoaiTk)
                    .Get();

                if (existingRecords.Models.Count > 0)
                {
                    throw new Exception("Tên loại tài khoản đã tồn tại");
                }

                // Tạo đối tượng cập nhật
                var updateModel = new LoaiTaiKhoanModel
                {
                    MaLoaiTk = _loaiTk.MaLoaiTk,
                    TenLoaiTk = _loaiTk.TenLoaiTk,
                    MoTa = _loaiTk.MoTa ?? string.Empty
                };

                // Cập nhật bản ghi
                var response = await client
                    .From<LoaiTaiKhoanModel>()
                    .Where(x => x.MaLoaiTk == _loaiTk.MaLoaiTk)
                    .Update(updateModel);

                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating LoaiTk: {ex.Message}");
                throw new Exception($"Có lỗi khi cập nhật loại tài khoản: {ex.Message}", ex);
            }
        }

        public async Task<bool> XoaLoaiTaiKhoan(int maLoaiTk)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                await client
                    .From<LoaiTaiKhoanModel>()
                    .Where(x => x.MaLoaiTk == maLoaiTk)
                    .Delete();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa loại tài khoản: {ex.Message}");
                return false;
            }
        }

        public async Task<LoaiTaiKhoanModel?> GetLoaiTaiKhoanByMaLoai(int maLoaiTk)
        {
            var client = await SupabaseService.GetClientAsync();
            var response = await client
                .From<LoaiTaiKhoanModel>()
                .Where(x => x.MaLoaiTk == maLoaiTk)
                .Single();

            return response;
        }

    }
}