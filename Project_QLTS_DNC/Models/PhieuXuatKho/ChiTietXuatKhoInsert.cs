using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.PhieuXuatKho
{
    [Table("chitietxuatkho")]
    public class ChiTietPhieuXuatInsert : BaseModel
    {
        [Column("ma_phieu_xuat")]
        public long MaPhieuXuat { get; set; }

        [Column("ma_tai_san")]
        public long MaTaiSan { get; set; }

        [Column("so_luong")]
        public int SoLuong { get; set; }
    }
}
