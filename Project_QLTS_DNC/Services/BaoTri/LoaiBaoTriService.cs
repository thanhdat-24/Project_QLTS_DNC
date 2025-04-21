using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Models.NhanVien;

namespace Project_QLTS_DNC.Services.BaoTri
{
    public class LoaiBaoTriService
    {
        public async Task<List<LoaiBaoTri>> LayDanhSachLoaiBT()
        {
            var client = await SupabaseService.GetClientAsync();
            var response = await client.From<LoaiBaoTri>().Get();
            // Kiểm tra dữ liệu trả về
            Console.WriteLine("Số lượng loại bảo trì: " + response.Models.Count);
            return response.Models;
        }

        // Thêm phương thức ThemLoaiBaoTri
        public async Task<bool> ThemLoaiBaoTri(LoaiBaoTri loaiBaoTri)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                // Đảm bảo rằng ID không được đặt khi thêm mới
                loaiBaoTri.MaLoaiBaoTri = 0;
                var response = await client.From<LoaiBaoTri>().Insert(loaiBaoTri);
                return response.Models.Count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thêm loại bảo trì: {ex.Message}");
                throw;
            }
        }

        // Thêm phương thức CapNhatLoaiBaoTri
        public async Task<bool> CapNhatLoaiBaoTri(LoaiBaoTri loaiBaoTri)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<LoaiBaoTri>()
                    .Where(x => x.MaLoaiBaoTri == loaiBaoTri.MaLoaiBaoTri)
                    .Update(loaiBaoTri);
                return response.Models.Count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật loại bảo trì: {ex.Message}");
                throw;
            }
        }

        // Thêm phương thức XoaLoaiBaoTri
        public async Task<bool> XoaLoaiBaoTri(int maLoaiBaoTri)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Gọi phương thức Delete mà không gán kết quả cho biến
                await client.From<LoaiBaoTri>()
                    .Where(x => x.MaLoaiBaoTri == maLoaiBaoTri)
                    .Delete();

                return true; // Trả về true nếu không có exception
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa loại bảo trì: {ex.Message}");
                throw;
            }
        }
        // Thêm phương thức TimKiemLoaiBaoTri
        public async Task<List<LoaiBaoTri>> TimKiemLoaiBaoTri(string searchText)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    return await LayDanhSachLoaiBT();
                }

                var client = await SupabaseService.GetClientAsync();

                // Sử dụng tìm kiếm đơn giản hơn
                var query = client.From<LoaiBaoTri>();

                // Thêm tham số cho phương thức tìm kiếm
                var formattedSearch = $"%{searchText}%";

                // Thực hiện truy vấn chỉ theo tên loại trước
                var response = await query.Get();

                // Lọc kết quả thủ công
                var results = response.Models
                    .Where(x => (x.TenLoai != null && x.TenLoai.ToLower().Contains(searchText.ToLower())) ||
                                (x.MoTa != null && x.MoTa.ToLower().Contains(searchText.ToLower())))
                    .ToList();

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tìm kiếm loại bảo trì: {ex.Message}");
                throw;
            }
        }
    }
}