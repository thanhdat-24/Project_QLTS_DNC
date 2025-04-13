using System;
using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Project_QLTS_DNC.Converters;

namespace Project_QLTS_DNC.Models.BaoTri
{
    [Table("baotri")]
    public class PhieuBaoTri : BaseModel
    {
        [PrimaryKey("ma_bao_tri", false)]
        public int MaBaoTri { get; set; }

        [Column("ma_tai_san")]
        public int? MaTaiSan { get; set; }

        [Column("ma_loai_bao_tri")]
        public int? MaLoaiBaoTri { get; set; }

        [Column("ngay_bao_tri")]
        public DateTime NgayBaoTri { get; set; }

        [Column("ma_nv")]
        public int? MaNV { get; set; }

        [Column("noi_dung")]
        public string NoiDung { get; set; }

        [Column("trang_thai_sau_bao_tri")]
        public string TrangThai { get; set; }

        // Sử dụng JsonConverter để xử lý kiểu tiền tệ khi nhận dữ liệu từ Supabase
        [Column("chi_phi")]
        [JsonConverter(typeof(CurrencyConverter))]
        public decimal? ChiPhi { get; set; }

        [Column("ghi_chu")]
        public string GhiChu { get; set; }
    }
}
