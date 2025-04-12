using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.ThongSoKT
{
    [Table("thongso")]
    public class ThongSoKyThuat : BaseModel
    {
        [PrimaryKey("ma_thong_so", false)]
        public int MaThongSo { get; set; }

        [Column("ma_nhom_ts")]
        public int MaNhomTS { get; set; }

        [Column("ten_thong_so")]
        public string TenThongSo { get; set; }

        // Constructor
        public ThongSoKyThuat()
        {
        }

        // Constructor với tham số
        public ThongSoKyThuat(int maNhomTS, string tenThongSo)
        {
            MaNhomTS = maNhomTS;
            TenThongSo = tenThongSo;
        }
    }
}