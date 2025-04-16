using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.TaiKhoan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase;
namespace Project_QLTS_DNC.Services.TaiKhoan
{
    public class LoaiTaiKhoanService: SupabaseService
    {
        public LoaiTaiKhoanService() : base()
        { 

        }
        public async Task<List<LoaiTaiKhoanModel>> LayDSLoaiTK()
        {
            var client = await SupabaseService.GetClientAsync();
            var result = await client.From<LoaiTaiKhoanModel>().Get();
            return result.Models.ToList();
        }

        public async Task<LoaiTaiKhoanModel> ThemLoaiTaiKhoan(LoaiTaiKhoanModel loaiTk)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var result = await client
                    .From<LoaiTaiKhoanModel>().Order(x => x.MaLoaiTk,Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Insert(loaiTk);

                
                return result.Models.FirstOrDefault(); 
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error adding LoaiTaiKhoan: {ex.Message}");
                return null; 
            }
        }


        public async Task<LoaiTaiKhoanModel> CapNhatLoaiTk(LoaiTaiKhoanModel _loaiTk)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();


                var response = await client
                    .From<LoaiTaiKhoanModel>()
                    .Where(x => x.MaLoaiTk == _loaiTk.MaLoaiTk)
                    .Update(_loaiTk);

                var updatedModel = response.Models.FirstOrDefault();


                if (updatedModel == null)
                {
                    var getResponse = await client
                        .From<LoaiTaiKhoanModel>()
                        .Where(x => x.MaLoaiTk == _loaiTk.MaLoaiTk)
                        .Get();

                    updatedModel = getResponse.Models.FirstOrDefault();
                }

                return updatedModel;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating LoaiTk: {ex.Message}");
                throw new Exception("Có lỗi khi cập nhật loại tài khoản", ex);
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
                Console.WriteLine($"Lỗi khi xóa chức vụ: {ex.Message}");
               return false;
            }
        }
    }
}
