using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models.BaoTri;
using Supabase.Postgrest;
using Supabase.Postgrest.Interfaces;

namespace Project_QLTS_DNC.Services
{
    public class LichSuSuaChuaService
    {
        /// <summary>
        /// Lấy tất cả lịch sử sửa chữa
        /// </summary>
        public async Task<List<LichSuSuaChua>> LayTatCaLichSuSuaChua()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<LichSuSuaChua>()
                    .Get();
                return response.Models;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy dữ liệu lịch sử sửa chữa: {ex.Message}");
            }
        }

        // Trong LichSuSuaChuaService.cs
        public async Task<List<LichSuSuaChua>> TimKiemLichSuSuaChua(string tuKhoa, DateTime? tuNgay, DateTime? denNgay)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Ghi log để debug
                System.Diagnostics.Debug.WriteLine($"Tìm kiếm với từ khóa: '{tuKhoa}', từ ngày: {tuNgay}, đến ngày: {denNgay}");

                // Bắt đầu với một truy vấn Select
                var query = client.From<LichSuSuaChua>().Select("*");

                // Tạo biến để theo dõi xem có áp dụng bất kỳ bộ lọc nào không
                bool coBoLoc = false;

                // Xử lý tìm kiếm theo từ khóa
                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    // Tạo danh sách các điều kiện với kiểu dữ liệu đúng
                    var filters = new List<IPostgrestQueryFilter>
            {
                // Tìm kiếm theo các trường văn bản
                new QueryFilter("loai_thao_tac", Supabase.Postgrest.Constants.Operator.ILike, $"%{tuKhoa}%"),
                new QueryFilter("ket_qua", Supabase.Postgrest.Constants.Operator.ILike, $"%{tuKhoa}%"),
                new QueryFilter("ghi_chu", Supabase.Postgrest.Constants.Operator.ILike, $"%{tuKhoa}%"),
                new QueryFilter("noi_dung_bao_tri", Supabase.Postgrest.Constants.Operator.ILike, $"%{tuKhoa}%"),
                new QueryFilter("trang_thai_truoc", Supabase.Postgrest.Constants.Operator.ILike, $"%{tuKhoa}%"),
                new QueryFilter("trang_thai_sau", Supabase.Postgrest.Constants.Operator.ILike, $"%{tuKhoa}%")
            };

                    // Tìm kiếm theo mã nếu từ khóa có thể chuyển thành số
                    if (int.TryParse(tuKhoa, out int maKhoa))
                    {
                        filters.Add(new QueryFilter("ma_lich_su", Supabase.Postgrest.Constants.Operator.Equals, maKhoa));
                        filters.Add(new QueryFilter("ma_bao_tri", Supabase.Postgrest.Constants.Operator.Equals, maKhoa));
                        filters.Add(new QueryFilter("ma_tai_san", Supabase.Postgrest.Constants.Operator.Equals, maKhoa));
                        filters.Add(new QueryFilter("ma_nv", Supabase.Postgrest.Constants.Operator.Equals, maKhoa));
                    }

                    // Tìm kiếm theo chi phí nếu từ khóa có thể chuyển đổi thành decimal
                    if (decimal.TryParse(tuKhoa, out decimal chiPhiKhoa))
                    {
                        filters.Add(new QueryFilter("chi_phi", Supabase.Postgrest.Constants.Operator.Equals, chiPhiKhoa));
                    }

                    // Áp dụng điều kiện OR cho tìm kiếm
                    query = query.Or(filters);
                    coBoLoc = true;
                }

                // Xử lý điều kiện ngày tháng
                if (tuNgay.HasValue)
                {
                    // Chuẩn hóa giờ bắt đầu từ 00:00:00
                    DateTime ngayBatDau = new DateTime(tuNgay.Value.Year, tuNgay.Value.Month, tuNgay.Value.Day, 0, 0, 0);
                    query = query.Filter("ngay_sua", Supabase.Postgrest.Constants.Operator.GreaterThanOrEqual, ngayBatDau);
                    coBoLoc = true;
                }

                if (denNgay.HasValue)
                {
                    // Chuẩn hóa giờ kết thúc đến 23:59:59
                    DateTime ngayKetThuc = new DateTime(denNgay.Value.Year, denNgay.Value.Month, denNgay.Value.Day, 23, 59, 59);
                    query = query.Filter("ngay_sua", Supabase.Postgrest.Constants.Operator.LessThanOrEqual, ngayKetThuc);
                    coBoLoc = true;
                }

                // Nếu không có bộ lọc nào được áp dụng, lấy tất cả
                if (!coBoLoc)
                {
                    System.Diagnostics.Debug.WriteLine("Không có bộ lọc nào được áp dụng, lấy tất cả dữ liệu");
                }

                // Thực hiện truy vấn và trả về kết quả
                var response = await query.Get();

                System.Diagnostics.Debug.WriteLine($"Tìm được {response.Models.Count} kết quả");
                return response.Models;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi tìm kiếm: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw new Exception($"Lỗi khi tìm kiếm lịch sử sửa chữa: {ex.Message}");
            }
        }

    }
}