using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.BaoTri
{
    [Table("loaibaotri")]
    public class LoaiBaoTri : BaseModel
    {
        [PrimaryKey("ma_loai_bao_tri", false)]
        public int MaLoaiBaoTri { get; set; }

        [Column("ten_loai")]
        public string TenLoai { get; set; }

        [Column("mo_ta")]
        public string MoTa { get; set; }
    }
}
