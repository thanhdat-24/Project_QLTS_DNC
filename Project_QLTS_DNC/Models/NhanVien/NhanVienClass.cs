using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.NhanVien
{
    [Table("nhanvien")]
    public class NhanVienClass : BaseModel
    {
        [PrimaryKey("ma_nv", false)] // Đặt true nếu tự tăng
        public int MaNV { get; set; }  // Chuyển từ long sang int

        [Column("ten_nv")]
        public string TenNV { get; set; } = null!;

        [Column("gioi_tinh")]
        public string GioiTinh { get; set; } = null!;

        [Column("sdt")]
        public string SoDienThoai { get; set; } = null!;

        [Column("email")]
        public string Email { get; set; } = null!;

        [Column("dia_chi")]
        public string DiaChi { get; set; } = null!;

        [Column("chuc_vu")]
        public string ChucVu { get; set; } = null!;

        [Column("ngay_sinh")]
        public DateTime? NgaySinh { get; set; } // nullable là đúng
    }
}
