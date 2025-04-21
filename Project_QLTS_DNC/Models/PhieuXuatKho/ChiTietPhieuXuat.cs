using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.PhieuXuatKho
{
    [Table("chitietxuatkho")]
    public class ChiTietPhieuXuatModel : BaseModel
    {
        [PrimaryKey("ma_chi_tiet_xk", false)]
        [Column("ma_chi_tiet_xk")]
        public int MaChiTietXK { get; set; }

        [Column("ma_phieu_xuat")]
        public long MaPhieuXuat { get; set; }

        [Column("ma_tai_san")]
        public int MaTaiSan { get; set; }

        [Column("so_luong")]
        public int SoLuong { get; set; }

        [Column("ma_nhom_ts")]
        public long? MaNhomTS { get; set; }

    }

}