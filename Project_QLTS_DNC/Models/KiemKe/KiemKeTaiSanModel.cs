using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.KiemKe
{
    [Table("kiemketaisan")]
    public class KiemKeTaiSanModel : BaseModel
    {
        [PrimaryKey("ma_kiem_ke_ts", false)]
        public int MaKiemKeTS { get; set; }

        [Column("ma_dot_kiem_ke")]
        public int MaDotKiemKe { get; set; }

        [Column("ma_tai_san")]
        public int MaTaiSan { get; set; }

        [Column("ma_phong")]
        public int MaPhong { get; set; }

        [Column("tinh_trang")]
        public string TinhTrang { get; set; } = string.Empty;

        [Column("vi_tri_thuc_te")]
        public string ViTriThucTe { get; set; } = string.Empty;

        [Column("ghi_chu")]
        public string? GhiChu { get; set; }  
    }
}





