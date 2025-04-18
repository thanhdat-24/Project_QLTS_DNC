using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;

namespace Project_QLTS_DNC.Models.BaoTri
{
    [Table("loaibaotri")]
    public class LoaiBaoTri : BaseModel
    {
        [PrimaryKey("ma_loai_bao_tri", true)] // true = identity, tự động tăng
        [Column("ma_loai_bao_tri")]
        [JsonProperty("ma_loai_bao_tri")]
        public int MaLoaiBaoTri { get; set; }

        [Column("ten_loai")]
        [JsonProperty("ten_loai")]
        public string TenLoai { get; set; }

        [Column("mo_ta")]
        [JsonProperty("mo_ta")]
        public string MoTa { get; set; }

        // Thuộc tính không lưu trong database
        [JsonIgnore]
        public bool IsSelected { get; set; }

        // Override ToString để hiển thị thông tin debug
        public override string ToString()
        {
            return $"LoaiBaoTri[MaLoaiBaoTri={MaLoaiBaoTri}, TenLoai={TenLoai}]";
        }
    }
}