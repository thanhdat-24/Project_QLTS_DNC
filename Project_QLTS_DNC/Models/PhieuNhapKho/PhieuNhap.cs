using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.PhieuNhapKho
{
    [Table("phieunhap")]
    public class PhieuNhap : BaseModel
    {
        private DateTime _ngaynhap;

        [PrimaryKey("ma_phieu_nhap", false)]
        [Column("ma_phieu_nhap")]
        public int MaPhieuNhap { get; set; }

        [Column("ma_kho")]
        public int MaKho { get; set; }

        [Column("ma_nv")]
        public int MaNV { get; set; }

        [Column("ma_ncc")]
        public int MaNCC { get; set; }

        [Column("ngay_nhap")]
        public DateTime NgayNhap
        {
            get => _ngaynhap;
            set
            {
                if (value == default || value.Year < 1900)
                    _ngaynhap = DateTime.UtcNow;
                else
                    _ngaynhap = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
        }

        [Column("tong_tien")]
        public decimal TongTien { get; set; }

        [Column("trang_thai")]
        public bool? TrangThai { get; set; }

       
    }
}
