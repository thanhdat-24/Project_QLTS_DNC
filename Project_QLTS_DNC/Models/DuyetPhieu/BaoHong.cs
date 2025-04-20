using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Project_QLTS_DNC.Models.Phieu
{
    [Table("phieubaohong")]
    public class BaoHong : BaseModel
    {
        [PrimaryKey("ma_phieu_bao_hong", false)]
        [Column("ma_phieu_bao_hong")]
        public long MaPhieuBaoHong { get; set; }

        [Column("ngay_bao_hong")]
        public DateTime NgayBaoHong { get; set; }

        [Column("hinh_thuc_ghi_nhan")]
        public string HinhThucGhiNhan { get; set; }

        [Column("mo_ta")]
        public string MoTa { get; set; }

        [Column("trang_thai")]
        public bool? TrangThai { get; set; }

        [Column("ma_nv")]
        public long? MaNV { get; set; }

        [Column("ma_phong")]
        public long? MaPhong { get; set; }

        [Column("ma_tai_san")]
        public long? MaTaiSan { get; set; }
    }
}
