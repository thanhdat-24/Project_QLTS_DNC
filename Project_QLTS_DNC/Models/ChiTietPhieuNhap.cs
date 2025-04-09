using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models
{
    [Table("chitietphieunhap")]
    public class ChiTietPhieuNhap : BaseModel
    {
        [PrimaryKey("ma_chi_tiet_pn", false)]
        public long MaChiTietPN { get; set; }

        [Column("ma_phieu_nhap")]
        public long MaPhieuNhap { get; set; }

        [Column("ma_nhom_ts")]
        public long MaNhomTS { get; set; }

        [Column("ten_tai_san")]
        public string TenTaiSan { get; set; }

        [Column("so_luong")]
        public int SoLuong { get; set; }

        [Column("don_gia")]
        public decimal DonGia { get; set; }

        [Column("thanh_tien")]
        public decimal ThanhTien { get; set; }
    }
}
