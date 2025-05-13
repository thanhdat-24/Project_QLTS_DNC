using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.DuyetPhieu
{
    [Table("chitietdenghimua")]
    public class CTdenghimua : BaseModel
    {
        [PrimaryKey("ma_chi_tiet_denghi", false)]
        [Column("ma_chi_tiet_denghi")]
        public long MaChiTietDNM { get; set; }

        [Column("ma_phieu_denghi")]
        public long MaPhieuDeNghi { get; set; }

        [Column("ten_tai_san")] 
        public string TenTaiSan { get; set; }

        [Column("so_luong")]
        public int SoLuong { get; set; }

        [Column("don_vi_tinh")]
        public string DonViTinh { get; set; }

        [Column("du_kien_gia")]
        public int? DuKienGia { get; set; }

        [Column("mo_ta")]
        public string MoTa { get; set; }
    }
}
