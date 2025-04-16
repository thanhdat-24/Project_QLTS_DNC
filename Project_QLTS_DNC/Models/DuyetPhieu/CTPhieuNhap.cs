using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Models.DuyetPhieu
{
    [Table("chitietphieunhap")]
    public class ChiTietPN : BaseModel
    {
        [PrimaryKey("ma_chi_tiet_pn", true)]
        [Column("ma_chi_tiet_pn")]
        public long? MaChiTietPN { get; set; }

        [Column("ma_phieu_nhap")]
        public long MaPhieuNhap { get; set; }

        [Column("ma_nhom_ts")]
        public long MaNhomTS { get; set; }

        [Column("ten_tai_san")]
        public string TenTaiSan { get; set; }

        [Column("so_luong")]
        public int SoLuong { get; set; }

        [Column("don_gia")]
        public decimal? DonGia { get; set; }

        [Column("can_quan_ly_rieng")]
        public bool? CanQuanLyRieng { get; set; }

        [Column("tong_tien")]
        public decimal? TongTien { get; set; }
    }
}