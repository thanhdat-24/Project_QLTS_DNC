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
    [Table("nhomtaisan")]
    public class NhomTaiSan : BaseModel
    {
        [Column("ma_nhom_ts")]
        [PrimaryKey("ma_nhom_ts", false)]
        public int MaNhomTS { get; set; }

        [Column("ma_loai_ts")]
        public int ma_loai_ts { get; set; }

        [Column("ten_nhom_ts")]
        public string TenNhom { get; set; }

        [Column("mo_ta")]
        public string MoTa { get; set; }

        [JsonIgnore]
        public LoaiTaiSan LoaiTaiSan { get; set; }
    }
}