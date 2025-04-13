using System;
using System.Linq;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models.QLTaiSan;

namespace Project_QLTS_DNC.Services.QLTaiSanService
{
    public class ChiTietPhieuNhapService
    {
        /// <summary>
        /// Lấy thông tin chi tiết phiếu nhập theo mã
        /// </summary>
        public static async Task<ChiTietPhieuNhap> LayChiTietPhieuNhapTheoMaAsync(int maChiTietPN)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<ChiTietPhieuNhap>()
                    .Where(x => x.MaChiTietPN == maChiTietPN)
                    .Get();

                if (response.Models.Count > 0)
                {
                    return response.Models.First();
                }
                else
                {
                    throw new Exception("Không tìm thấy chi tiết phiếu nhập với mã đã cho.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy thông tin chi tiết phiếu nhập: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Tìm nhóm tài sản của một tài sản thông qua mã chi tiết phiếu nhập
        /// </summary>
        public static async Task<int> TimNhomTaiSanTheoTaiSanAsync(int maTaiSan)
        {
            try
            {
                var taiSan = await TaiSanService.LayTaiSanTheoMaAsync(maTaiSan);

                if (taiSan.MaChiTietPN.HasValue)
                {
                    var chiTietPN = await LayChiTietPhieuNhapTheoMaAsync(taiSan.MaChiTietPN.Value);
                    return chiTietPN.MaNhomTS;
                }

                // Nếu không có chi tiết phiếu nhập, trả về -1 hoặc xử lý theo yêu cầu
                return -1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi tìm nhóm tài sản: {ex.Message}");
                return -1;
            }
        }
    }
}