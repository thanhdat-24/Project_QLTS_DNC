using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models.ThongSoKT;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Utils;
using Supabase;
using Supabase.Postgrest;

namespace Project_QLTS_DNC.Services.QLTaiSanService
{
    public static class ThongSoKyThuatService
    {
        public static async Task<ObservableCollection<ThongSoKyThuatDTO>> LayDanhSachThongSoAsync(int maNhomTS)
        {
            try
            {
                // Lấy Supabase client
                var client = await SupabaseService.GetClientAsync();

                // Truy vấn CSDL để lấy danh sách thông số kỹ thuật theo nhóm tài sản
                var response = await client
                    .From<ThongSoKyThuat>()
                    .Filter("ma_nhom_ts", Supabase.Postgrest.Constants.Operator.Equals, maNhomTS)
                    .Get();

                // Kiểm tra kết quả trả về
                if (response.Models == null)
                {
                    return new ObservableCollection<ThongSoKyThuatDTO>();
                }

                var dsThongSo = response.Models.ToList();

                // Chuyển đổi sang DTO và trả về
                return new ObservableCollection<ThongSoKyThuatDTO>(dsThongSo.ToListDTO());
            }
            catch (Exception ex)
            {
                // Log lỗi và ném ngoại lệ
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy danh sách thông số: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Thêm mới thông số kỹ thuật vào Supabase
        /// </summary>
        /// <param name="thongSoDTO">Thông tin thông số kỹ thuật cần thêm</param>
        /// <returns>Thông số kỹ thuật đã được thêm (có mã)</returns>
        public static async Task<ThongSoKyThuatDTO> ThemThongSoAsync(ThongSoKyThuatDTO thongSoDTO)
        {
            try
            {
                // Lấy Supabase client
                var client = await SupabaseService.GetClientAsync();

                // Chuyển đổi DTO sang model
                var thongSo = thongSoDTO.ToModel();

                // Thêm vào CSDL Supabase
                var response = await client
                    .From<ThongSoKyThuat>()
                    .Insert(thongSo);

                // Kiểm tra kết quả trả về
                if (response.Models == null || response.Models.Count == 0)
                {
                    throw new Exception("Không thể thêm thông số kỹ thuật");
                }

                // Lấy thông số đã thêm
                var thongSoDaThem = response.Models.First();

                // Chuyển đổi model đã thêm thành công sang DTO để trả về
                return thongSoDaThem.ToDTO();
            }
            catch (Exception ex)
            {
                // Log lỗi và ném ngoại lệ
                System.Diagnostics.Debug.WriteLine($"Lỗi khi thêm thông số: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cập nhật thông số kỹ thuật trong Supabase
        /// </summary>
        /// <param name="thongSoDTO">Thông tin thông số kỹ thuật cần cập nhật</param>
        /// <returns>Thông số kỹ thuật đã được cập nhật</returns>
        public static async Task<ThongSoKyThuatDTO> CapNhatThongSoAsync(ThongSoKyThuatDTO thongSoDTO)
        {
            try
            {
                // Lấy Supabase client
                var client = await SupabaseService.GetClientAsync();

                // Chuyển đổi DTO sang model
                var thongSo = thongSoDTO.ToModel();

                // Cập nhật vào CSDL Supabase
                var response = await client
                    .From<ThongSoKyThuat>()
                    .Filter("ma_thong_so", Supabase.Postgrest.Constants.Operator.Equals, thongSo.MaThongSo)
                    .Update(thongSo);

                // Kiểm tra kết quả trả về
                if (response.Models == null || response.Models.Count == 0)
                {
                    throw new Exception("Không thể cập nhật thông số kỹ thuật");
                }

                // Lấy thông số đã cập nhật
                var thongSoDaCapNhat = response.Models.First();

                // Chuyển đổi model đã cập nhật thành công sang DTO để trả về
                return thongSoDaCapNhat.ToDTO();
            }
            catch (Exception ex)
            {
                // Log lỗi và ném ngoại lệ
                System.Diagnostics.Debug.WriteLine($"Lỗi khi cập nhật thông số: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Xóa thông số kỹ thuật từ Supabase
        /// </summary>
        /// <param name="maThongSo">Mã thông số kỹ thuật cần xóa</param>
        /// <returns>True nếu xóa thành công, False nếu thất bại</returns>
        public static async Task<bool> XoaThongSoAsync(int maThongSo)
        {
            try
            {
                // Lấy Supabase client
                var client = await SupabaseService.GetClientAsync();

                // Xóa từ CSDL Supabase
                await client
                    .From<ThongSoKyThuat>()
                    .Filter("ma_thong_so", Supabase.Postgrest.Constants.Operator.Equals, maThongSo)
                    .Delete();

                // Trả về kết quả thành công (giống như cách xử lý trong TaiSanService)
                return true;
            }
            catch (Exception ex)
            {
                // Log lỗi và trả về false (giống như cách xử lý trong TaiSanService)
                System.Diagnostics.Debug.WriteLine($"Lỗi khi xóa thông số: {ex.Message}");
                return false;
            }
        }
    }
}