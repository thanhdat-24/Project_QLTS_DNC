using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Project_QLTS_DNC.Models.LichSu
{
    [Table("lichsudichuyentaisan")] // phải khớp tuyệt đối với Supabase
    public class LichSuDiChuyenTaiSan : BaseModel
    {
        [PrimaryKey("ma_lich_su", false)]
        public long MaLichSu { get; set; }

        [Column("ma_tai_san")]
        public long? MaTaiSan { get; set; }

        [Column("ma_phong_cu")]
        public long? MaPhongCu { get; set; }

        [Column("ma_phong_moi")]
        public long? MaPhongMoi { get; set; }

        [Column("ngay_ban_giao")]
        public DateTime? NgayBanGiao { get; set; }

        [Column("ma_nv")]
        public long? MaNhanVien { get; set; }

        [Column("trang_thai")]
        public bool? TrangThai { get; set; }

        [Column("ghi_chu")]
        public string GhiChu { get; set; }
    }
}
