
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.ToaNha
{
    [Table("toanha")]
    public class ToaNha : BaseModel
    {
        // PHẢI là int? và false
        [PrimaryKey("ma_toa", false)]
        public int? MaToaNha { get; set; }

        [Column("ten_toa")]
        public string TenToaNha { get; set; }

        [Column("dia_chi_tn")]
        public string DiaChiTN { get; set; }

        [Column("so_dien_thoai_tn")]
        public string SoDienThoaiTN { get; set; }

        [Column("mo_ta")]
        public string MoTaTN { get; set; }
    }
}
