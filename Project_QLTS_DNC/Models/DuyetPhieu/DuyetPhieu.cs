using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Project_QLTS_DNC.Models.DuyetPhieu
{
    [Table("phieuduyet")]
    public class PhieuDuyet : BaseModel
    {
        [PrimaryKey("ma_phieu_duyet", true)] // TRUE => để Supabase tự sinh
        [Column("ma_phieu_duyet")]
        public long? MaPhieuDuyet { get; set; }

        [Column("ma_nv")]
        public long MaNV { get; set; }

        [Column("ma_loai_phieu")]
        public long MaLoaiPhieu { get; set; }

        [Column("ma_phieu")]
        public long MaPhieu { get; set; }

        [Column("ngay_duyet")]
        public DateTime? NgayDuyet { get; set; }

        [Column("ngay_tu_choi")]
        public DateTime? NgayTuChoi { get; set; }

        [Column("ly_do")]
        public string LyDo { get; set; }

        [Column("trang_thai")]
        public string TrangThai { get; set; }
    }
}
