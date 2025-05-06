using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.BaoTri
{
    [Table("lichsusuachua")]
    public class LichSuSuaChua : BaseModel
    {
        [PrimaryKey("ma_lich_su", true)]
        [Column("ma_lich_su")]
        public int MaLichSu { get; set; }

        [Column("ma_bao_tri")]
        public int? MaBaoTri { get; set; }

        [Column("ma_tai_san")]
        public int? MaTaiSan { get; set; }

        [Column("ma_nv")]
        public int? MaNV { get; set; }

        [Column("ngay_sua")]
        public DateTime? NgaySua { get; set; }

        [Column("loai_thao_tac")]
        public string LoaiThaoTac { get; set; }

        [Column("ket_qua")]
        public string KetQua { get; set; }

        [Column("chi_phi")]
        public decimal? ChiPhi { get; set; }

        [Column("trang_thai_truoc")]
        public string TrangThaiTruoc { get; set; }

        [Column("trang_thai_sau")]
        public string TrangThaiSau { get; set; }

        [Column("ghi_chu")]
        public string GhiChu { get; set; }

        [Column("noi_dung_bao_tri")]
        public string NoiDungBaoTri { get; set; }
    }
}