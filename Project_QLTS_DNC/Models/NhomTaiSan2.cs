using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Project_QLTS_DNC.Models
{
    [Table("nhomtaisan")]  // Đảm bảo rằng bảng trong cơ sở dữ liệu có tên là 'nhomtaisan'
    public class NhomTaiSan2 : BaseModel
    {
        // Khóa chính
        [PrimaryKey("ma_nhom_ts", false)]
        public int MaNhomTS { get; set; }  // Mã nhóm tài sản

        // Mã loại tài sản (có thể null)
        [Column("ma_loai_ts")]
        public int? MaLoaiTS { get; set; }  // Mã loại tài sản

        // Tên nhóm tài sản
        [Column("ten_nhom_ts")]
        public string TenNhomTS { get; set; }  // Tên nhóm tài sản

        // Số lượng tài sản trong nhóm (có thể null)
        [Column("so_luong")]
        public int? SoLuong { get; set; }  // Số lượng tài sản trong nhóm

        // Mô tả nhóm tài sản
        [Column("mo_ta")]
        public string MoTa { get; set; }  // Mô tả nhóm tài sản
    }
}
