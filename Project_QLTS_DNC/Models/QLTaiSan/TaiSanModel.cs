using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.QLTaiSan
{
    [Table("taisan")]
    public class TaiSanModel : BaseModel
    {
        [PrimaryKey("ma_tai_san", true)]
        [Column("ma_tai_san")]
        public int MaTaiSan { get; set; }

        [Column("ma_chi_tiet_pn")]
        public int? MaChiTietPN { get; set; }

        [Column("ten_tai_san")]
        public string TenTaiSan { get; set; }

        [Column("so_seri")]
        public string SoSeri { get; set; }

        [Column("ma_qr")]
        public string MaQR { get; set; }

        [Column("ngay_su_dung")]
        public DateTime? NgaySuDung { get; set; }

        [Column("han_bh")]
        public DateTime? HanBH { get; set; }

        [Column("tinh_trang_sp")]
        public string TinhTrangSP { get; set; }

        [Column("ghi_chu")]
        public string GhiChu { get; set; }

        [Column("ma_phong")]
        public int? MaPhong { get; set; }

      
       

    }
}