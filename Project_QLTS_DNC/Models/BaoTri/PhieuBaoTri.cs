using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
namespace Project_QLTS_DNC.Models.BaoTri
{
    // Đảm bảo tên bảng được chỉ định chính xác
    [Table("baotri")]
    public class PhieuBaoTri : BaseModel
    {
        // Khóa chính với identity
        [PrimaryKey("ma_bao_tri", true)]  // true = identity
        [Column("ma_bao_tri")]
        [JsonProperty("ma_bao_tri")]
        public int MaBaoTri { get; set; } = default;

        [Column("ma_tai_san")]
        [JsonProperty("ma_tai_san")]
        public int? MaTaiSan { get; set; }

        [Column("ma_loai_bao_tri")]
        [JsonProperty("ma_loai_bao_tri")]
        public int? MaLoaiBaoTri { get; set; }

        [Column("ngay_bao_tri")]
        [JsonProperty("ngay_bao_tri")]
        public DateTime? NgayBaoTri { get; set; }

        [Column("ma_nv")]
        [JsonProperty("ma_nv")]
        public int? MaNV { get; set; }

        [Column("noi_dung")]
        [JsonProperty("noi_dung")]
        public string NoiDung { get; set; }

        [Column("trang_thai_sau_bao_tri")]
        [JsonProperty("trang_thai_sau_bao_tri")]
        public string TrangThai { get; set; }

        // Kiểu dữ liệu numeric trong PostgreSQL
        [Column("chi_phi")]
        [JsonProperty("chi_phi")]
        public decimal? ChiPhi { get; set; }

        [Column("ghi_chu")]
        [JsonProperty("ghi_chu")]
        public string GhiChu { get; set; }

        // Thuộc tính không lưu trong database
        [JsonIgnore]
        public bool IsSelected { get; set; }

        // Các thuộc tính bổ sung chỉ dùng để hiển thị (không lưu trong cơ sở dữ liệu)
        [JsonIgnore]
        public string TenTaiSan { get; set; }

        [JsonIgnore]
        public string TenLoaiBaoTri { get; set; }

        [JsonIgnore]
        public string TenNhanVien { get; set; }

        // Override ToString để hiển thị thông tin debug
        public override string ToString()
        {
            return $"PhieuBaoTri[MaBaoTri={MaBaoTri}, MaTaiSan={MaTaiSan}, NgayBaoTri={NgayBaoTri}]";
        }
    }
}