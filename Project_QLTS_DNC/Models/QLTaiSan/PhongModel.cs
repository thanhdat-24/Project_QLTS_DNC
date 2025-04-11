using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.QLTaiSan
{
    [Table("phong")]
    public class PhongModel : BaseModel
    {
        [PrimaryKey("ma_phong", true)]
        [Column("ma_phong")]
        public int MaPhong { get; set; }

        [Column("ten_phong")]
        public string TenPhong { get; set; }

        [Column("mo_ta")]
        public string MoTa { get; set; }

        [Column("ma_toa_nha")]
        public int? MaToaNha { get; set; }
    }
}