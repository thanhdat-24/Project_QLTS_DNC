using Project_QLTS_DNC.Models.TaiKhoan;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models
{
    [Table("taikhoan")]
    public class TaiKhoanModel : BaseModel
    {
        [PrimaryKey("ma_tk", false)]
        public int MaTk { get; set; }

        [Column("ma_loai_tk")]
        public int MaLoaiTk { get; set; }

        [Column("ma_nv")]
        public int? MaNv { get; set; }

        [Column("ten_tai_khoan")]
        public string TenTaiKhoan { get; set; }

        [Column("mat_khau")]
        public string MatKhau { get; set; }

        [Column("uid")]
        public string Uid { get; set; }
        [Column("trang_thai")]
        public bool TrangThai { get; set; } = true;

        //[System.ComponentModel.DataAnnotations.Schema.NotMapped]
        //public LoaiTaiKhoanModel LoaiTaiKhoan { get; set; }

    }
}
