using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.ToaNha
{
    [Table("toanha")] // Tên bảng trong PostgreSQL
    public class ToaNha : BaseModel
    {
        [PrimaryKey("ma_toa", false)] // Cột 'ma_toa' là khóa chính
        [Column("ma_toa")] // Đảm bảo rằng tên cột là 'ma_toa' trong cơ sở dữ liệu
        public int MaToaNha { get; set; }

        [Column("ten_toa")]
        public string TenToaNha { get; set; } = null!;

        [Column("dia_chi_tn")]
        public string DiaChiTN { get; set; } = null!;

        [Column("so_dien_thoai_tn")]
        public string SoDienThoaiTN { get; set; } = null!;

        [Column("mo_ta_tn")]
        public string MoTaTN { get; set; } = null!;
    }
}
