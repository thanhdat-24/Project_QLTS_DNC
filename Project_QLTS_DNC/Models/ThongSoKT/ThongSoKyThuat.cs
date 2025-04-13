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

        [Column("chi_tiet_thong_so")]
        public string ChiTietThongSo { get; set; }

        [Column("so_luong")]
        public int? SoLuong { get; set; }

        [Column("bao_hanh")]
        public int? BaoHanh { get; set; }

        // Constructor
        public ThongSoKyThuat()
        {
        }

        // Constructor với tham số
        public ThongSoKyThuat(int maNhomTS, string tenThongSo, string chiTietThongSo = null, int? soLuong = null, int? baoHanh = null)
        {
            MaNhomTS = maNhomTS;
            TenThongSo = tenThongSo;
            ChiTietThongSo = chiTietThongSo;
            SoLuong = soLuong;
            BaoHanh = baoHanh;
        }
    }
}