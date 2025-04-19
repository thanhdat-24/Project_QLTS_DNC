using System;
using System.Collections.Generic;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.QLLoaiTS
{
    [Table("loaitaisan")]
    public class LoaiTaiSan : BaseModel
    {
        [PrimaryKey("ma_loai_ts", true)]
        [Column("ma_loai_ts")]
        public int MaLoaiTaiSan { get; set; }

        [Column("ten_loai_ts")]
        public string TenLoaiTaiSan { get; set; }

        [Column("mo_ta")]
        public string MoTa { get; set; }

        [Column("quan_ly_rieng")]
        public bool QuanLyRieng { get; set; }
    }
}