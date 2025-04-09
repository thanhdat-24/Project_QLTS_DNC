using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

namespace Project_QLTS_DNC.Models
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
    }
}