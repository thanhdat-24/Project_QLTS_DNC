using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models
{
    [Table("phieunhap")]
    public class PhieuNhap : BaseModel
    {
        [PrimaryKey("ma_pn", false)]
        public int MaPN { get; set; }  // Chuyển từ long sang int

        [Column("ma_kho")]
        public int MaKho { get; set; }  // Chuyển từ long sang int

        [Column("ma_nv")]
        public int MaNV { get; set; }  // Chuyển từ long sang int

        [Column("ma_ncc")]
        public int MaNCC { get; set; }  // Chuyển từ long sang int

        [Column("ngay_nhap")]
        public DateTime NgayNhap { get; set; }

        [Column("tong_tien")]
        public decimal TongTien { get; set; }

        [Column("trang_thai")]
        public string? TrangThai { get; set; } // Cho phép null khi insert

        [PrimaryKey("ma_phieu_nhap", false)]
        [Column("ma_phieu_nhap")]
        public int MaPhieuNhap { get; set; }  // Chuyển từ long sang int
    }
}
