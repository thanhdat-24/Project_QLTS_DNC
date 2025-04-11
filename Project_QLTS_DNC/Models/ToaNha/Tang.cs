
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Project_QLTS_DNC.Models.ToaNha
{
    [Table("tang")] // tên bảng trong Supabase
    public class Tang : BaseModel
    {
        [PrimaryKey("ma_tang", false)] // true = auto increment (identity)
        public int? MaTang { get; set; }

        [Column("ma_toa")]
        public int MaToa { get; set; }

        [Column("ten_tang")]
        public string TenTang { get; set; }

        [Column("mo_ta")]
        public string MoTa { get; set; }
    }
}
