using Project_QLTS_DNC.Models.DuyetPhieu;
using System;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Services
{
    public class PhieuDuyetService
    {
        /// Ghi nhận phiếu đã được duyệt
        public static async Task LuuPhieuDuyetAsync(long maPhieu, long maLoaiPhieu, long maNV)
        {
            var client = await SupabaseService.GetClientAsync();

            var phieu = new PhieuDuyet
            {
                MaPhieu = maPhieu,
                MaLoaiPhieu = maLoaiPhieu,
                MaNV = maNV,
                NgayDuyet = DateTime.Now,
                TrangThai = "Đã duyệt"
            };

            await client.From<PhieuDuyet>().Insert(phieu);
        }

        /// Ghi nhận phiếu bị từ chối
        public static async Task LuuPhieuTuChoiAsync(long maPhieu, long maLoaiPhieu, long maNV, string lyDo)
        {
            var client = await SupabaseService.GetClientAsync();

            var phieu = new PhieuDuyet
            {
                MaPhieu = maPhieu,
                MaLoaiPhieu = maLoaiPhieu,
                MaNV = maNV,
                TrangThai = "Từ chối",
                LyDo = lyDo,
                NgayTuChoi = DateTime.Now
            };

            await client.From<PhieuDuyet>().Insert(phieu);
        }
    }
}
