using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.ThongSoKT
{
    [Table("thongso_taisan")]
    public class ThongSoTaiSan : BaseModel
    {
        [PrimaryKey("ma_thong_so_ts", false)]
        [Column("ma_thong_so_ts")]
        public int MaThongSoTS { get; set; }

        [Column("ma_tai_san")]
        public int MaTaiSan { get; set; }

        [Column("ma_thong_so")]
        public int MaThongSo { get; set; }

        [Column("gia_tri")]
        public string GiaTri { get; set; }

        [Column("ngay_cap_nhat")]
        public DateTime? NgayCapNhat { get; set; }

        [Column("han_bao_hanh")]
        public string HanBaoHanh { get; set; }

        [Column("tinh_trang")]
        public string TinhTrang { get; set; }

        [Column("mo_ta")]
        public string MoTa { get; set; }
    }
}