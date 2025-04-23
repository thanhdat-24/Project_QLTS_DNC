using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;
using System;

namespace Project_QLTS_DNC.Models.ThongBao
{
    [Table("thongbao")]
    public class ThongBaoModel : BaseModel
    {
        [PrimaryKey("id", false)]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("noi_dung")]
        [Column("noi_dung")]
        public string NoiDung { get; set; }

        [JsonProperty("da_doc")]
        [Column("da_doc")]
        public bool DaDoc { get; set; }

        [JsonProperty("thoi_gian")]
        [Column("thoi_gian")]
        public DateTime ThoiGian { get; set; }

        [JsonProperty("ma_tai_khoan")]
        [Column("ma_tai_khoan")]
        public int MaTaiKhoan { get; set; }

        [JsonProperty("loai_phieu")]
        [Column("loai_phieu")]
        public string? LoaiPhieu { get; set; }

        [JsonProperty("trang_thai")]
        [Column("trang_thai")]
        public string? TrangThai { get; set; }

        [JsonProperty("ma_phieu")]
        [Column("ma_phieu")]
        public int? MaPhieu { get; set; }

    }
}
