using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Project_QLTS_DNC.Models.DuyetPhieu
{
    [Table("phieudenghimua")]
    public class phieudenghimua : BaseModel
    {
        [PrimaryKey("ma_phieu_denghi", false)]
        [Column("ma_phieu_denghi")]
        public long MaPhieuDeNghi { get; set; }

        [Column("ngay_denghi")]
        public DateTime NgayDeNghi { get; set; }

        [Column("ma_nv")]
        public long MaNhanVien { get; set; }

        [Column("don_vi_denghi")]
        public string DonViDeNghi { get; set; }

        [Column("ly_do")]
        public string LyDo { get; set; }

        [Column("trang_thai")]
        public bool? TrangThai { get; set; }

        [Column("ghi_chu")]
        public string GhiChu { get; set; }
    }
}
