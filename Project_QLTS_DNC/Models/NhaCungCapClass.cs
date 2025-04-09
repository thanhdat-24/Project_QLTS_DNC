using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models
{
    [Table("nhacungcap")]
    public class NhaCungCapClass : BaseModel
    {
        [PrimaryKey("ma_ncc", false)] // Sửa thành true nếu dùng auto increment
        public int MaNCC { get; set; }  // Chuyển từ long sang int

        [Column("ten_ncc")]
        public string TenNCC { get; set; }

        [Column("dia_chi")]
        public string DiaChi { get; set; }

        [Column("sdt")]
        public string SoDienThoai { get; set; }

        [Column("email")]
        public string Email { get; set; }
    }
}
