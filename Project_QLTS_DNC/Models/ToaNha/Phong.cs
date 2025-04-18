
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.ToaNha
{
    [Table("phong")]
    public class Phong : BaseModel
    {
        [PrimaryKey("ma_phong", false)] // Supabase sẽ tự sinh mã
        public int MaPhong { get; set; }

        [Column("ma_tang")]
        public int MaTang { get; set; }

        [Column("ten_phong")]
        public string TenPhong { get; set; }

        [Column("suc_chua")]
        public int SucChua { get; set; }

        [Column("mo_ta")]
        public string MoTaPhong { get; set; }
    }
}
