using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using Supabase;  // Thư viện chính của Supabase
using Supabase.Postgrest;  // Thư viện giúp thực hiện các truy vấn và thao tác với bảng Postgrest
using Supabase.Realtime;  // Thư viện giúp xử lý Realtime (nếu cần)


namespace Project_QLTS_DNC.Models.PhieuXuatKho
{
    [Table("xuatkho")]
    public class PhieuXuatInsert : BaseModel
    {
        [Column("ma_kho_xuat")]
        public long MaKhoXuat { get; set; }

        [Column("ma_kho_nhan")]
        public long MaKhoNhan { get; set; }

        [Column("ngay_xuat")]
        public DateTime NgayXuat { get; set; }

        [Column("ma_nv")]
        public long MaNV { get; set; }

        [Column("trang_thai")]
        public string? TrangThai { get; set; }

        [Column("ghi_chu")]
        public string? GhiChu { get; set; }
    }
}
