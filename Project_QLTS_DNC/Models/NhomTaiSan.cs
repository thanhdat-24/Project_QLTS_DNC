using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Supabase;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Services;

namespace Project_QLTS_DNC.Models
{
    [Table("nhomtaisan")]
    public class NhomTaiSan : BaseModel
    {
        [PrimaryKey("ma_nhom_ts", true)]
        [Column("ma_nhom_ts")]
        public int MaNhomTS { get; set; }

        [Column("ma_loai_ts")]
        public int MaLoaiTaiSan { get; set; }

        [Column("ten_nhom_ts")]
        public string TenNhom { get; set; }

        [Column("mo_ta")]
        public string MoTa { get; set; }

        // Navigation property
        public LoaiTaiSan LoaiTaiSan { get; set; }
    }
}