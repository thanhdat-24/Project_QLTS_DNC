using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using Newtonsoft.Json; // ✅ THÊM NÀY
using System;

namespace Project_QLTS_DNC.Models.DuyetPhieu
{
    [Table("phieunhap")]
    public class PhieuNhapKhoInput : BaseModel
    {
        [PrimaryKey("ma_phieu_nhap", false)]
        [Column("ma_phieu_nhap")]
        public long MaPhieuNhap { get; set; }

        [Column("ma_kho")]
        public long MaKho { get; set; }

        [Column("ma_nv")]
        public long MaNV { get; set; }

        [Column("ma_ncc")]
        public long MaNCC { get; set; }

        [Column("ngay_nhap")]
        public DateTime NgayNhap { get; set; }

        [Column("tong_tien")]
        public decimal TongTien { get; set; }

        [Column("trang_thai")]
        public bool? TrangThai { get; set; }

        [JsonIgnore] // ✅ CHỈ DÙNG CHO UI
        public bool IsSelected { get; set; } = false;
   

        [JsonIgnore]
        public string TrangThaiHienThi => TrangThai == true
            ? "Đã duyệt"
            : TrangThai == false
                ? "Từ chối duyệt"
                : "Chưa duyệt";

    }
}
