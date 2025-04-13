using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.PhieuNhapKho
{
    [Table("phieunhap")]
    public class PhieuNhap : BaseModel
    {
        [PrimaryKey("ma_phieu_nhap", false)]
        [Column("ma_phieu_nhap")]
        public int MaPhieuNhap { get; set; }

        [Column("ma_kho")]
        public int MaKho { get; set; }

        [Column("ma_nv")]
        public int MaNV { get; set; }

        [Column("ma_ncc")]
        public int MaNCC { get; set; }

        [Column("ngay_nhap")]
        public DateTime NgayNhap { get; set; }

        [Column("tong_tien")]
        public decimal TongTien { get; set; }

        [Column("trang_thai")]
        public string? TrangThai { get; set; }
    }
}
