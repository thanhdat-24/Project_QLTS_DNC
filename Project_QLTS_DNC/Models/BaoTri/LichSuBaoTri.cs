using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Project_QLTS_DNC.Models.BaoTri
{
    [Table("lichsu_baotri")]
    public class LichSuBaoTri : BaseModel
    {
        [PrimaryKey("ma_lich_su", true)]
        public int MaLichSu { get; set; }

        [Column("ma_tai_san")]
        public int? MaTaiSan { get; set; }

        [Column("ten_tai_san")]
        public string TenTaiSan { get; set; }

        [Column("so_seri")]
        public string SoSeri { get; set; }

        [Column("loai_hoat_dong")]
        public string LoaiHoatDong { get; set; } // "XuatExcel" hoặc "InPhieu"

        [Column("ngay_thuc_hien")]
        public DateTime NgayThucHien { get; set; }

        [Column("ma_nguoi_thuc_hien")]
        public int? MaNguoiThucHien { get; set; }

        [Column("ten_nguoi_thuc_hien")]
        public string TenNguoiThucHien { get; set; }

        [Column("ghi_chu")]
        public string GhiChu { get; set; }
    }
}