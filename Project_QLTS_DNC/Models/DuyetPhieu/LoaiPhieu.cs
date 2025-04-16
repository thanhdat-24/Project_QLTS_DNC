using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.DuyetPhieu
{
    [Table("loaiphieu")] // tên bảng trong Supabase
    public class LoaiPhieu : BaseModel
    {
        [PrimaryKey("ma_loai_phieu", false)]
        public int MaLoaiPhieu { get; set; }

        [Column("ten_phieu")]
        public string TenLoaiPhieu { get; set; }

        [Column("mo_ta")]
        public string MoTaLP { get; set; }
    }
}
