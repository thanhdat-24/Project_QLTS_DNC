using System;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace Project_QLTS_DNC.Models.BaoTri
{
    // Lớp phụ chỉ dùng cho thao tác với cơ sở dữ liệu
    [Supabase.Postgrest.Attributes.Table("kiemketaisan")]
    public class KiemKeTaiSanDb : BaseModel
    {
        [PrimaryKey("ma_kiem_ke_ts", false)]
        public int MaKiemKeTS { get; set; }

        [Supabase.Postgrest.Attributes.Column("ma_dot_kiem_ke")]
        public int? MaDotKiemKe { get; set; }

        [Supabase.Postgrest.Attributes.Column("ma_tai_san")]
        public int? MaTaiSan { get; set; }

        [Supabase.Postgrest.Attributes.Column("ma_phong")]
        public int? MaPhong { get; set; }

        [Supabase.Postgrest.Attributes.Column("tinh_trang")]
        public string TinhTrang { get; set; }

        [Supabase.Postgrest.Attributes.Column("vi_tri_thuc_te")]
        public string ViTriThucTe { get; set; }

        [Supabase.Postgrest.Attributes.Column("ghi_chu")]
        public string GhiChu { get; set; }

        // Phương thức chuyển đổi từ KiemKeTaiSan sang KiemKeTaiSanDb
        public static KiemKeTaiSanDb FromKiemKeTaiSan(KiemKeTaiSan taiSan)
        {
            return new KiemKeTaiSanDb
            {
                MaKiemKeTS = taiSan.MaKiemKeTS,
                MaDotKiemKe = taiSan.MaDotKiemKe,
                MaTaiSan = taiSan.MaTaiSan,
                MaPhong = taiSan.MaPhong,
                TinhTrang = taiSan.TinhTrang,
                ViTriThucTe = taiSan.ViTriThucTe,
                GhiChu = taiSan.GhiChu
            };
        }
    }
}