using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

namespace Project_QLTS_DNC.Models.KiemKe
{
    [Table("dotkiemke")]
    public class DotKiemKe : BaseModel
    {
        [PrimaryKey("ma_dot_kiem_ke", false)]
        public int MaDotKiemKe { get; set; }

        [Column("ten_dot")]
        public string? TenDot { get; set; }

        [Column("ngay_bat_dau")]
        public DateTime? NgayBatDau { get; set; }

        [Column("ngay_ket_thuc")]
        public DateTime? NgayKetThuc { get; set; }

        [Column("ma_nv")]
        public int MaNV { get; set; }

        [Column("ghi_chu")]
        public string? GhiChu { get; set; }

        [JsonIgnore]
        public string? TenNhanVien { get; set; }
    }
}

