using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.ToaNha
{
    [Table("phongban")]
    public class PhongBan : BaseModel
    {
        [PrimaryKey("ma_pb", false)]
        public int? MaPhongBan { get; set; }

        [Column("ma_toa")]
        public int MaToa { get; set; }

        [Column("ten_pb")]
        public string TenPhongBan { get; set; }

        [Column("mo_ta")]
        public string MoTaPhongBan { get; set; }
    }
}
