using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Project_QLTS_DNC.Models.DuyetPhieu
{
    [Table("phieudenghimua")]
    public class denghimua : BaseModel
    {
        [PrimaryKey("ma_phieu_de_nghi", false)]
        [Column("ma_phieu_de_nghi")]
        public long MaPhieuDeNghi { get; set; }

        [Column("ngay_denghi")]
        public DateTime? NgayDeNghiMua { get; set; }

        [Column("ma_nv")]
        public long MaNV { get; set; }

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
