using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

[Table("kho")]
public class Kho : BaseModel
{
    [PrimaryKey("ma_kho", false)]
    [Column("ma_kho")]
    public int MaKho { get; set; }

    [Column("ma_toa")]
    public int MaToaNha { get; set; }

    [Column("ten_kho")]
    public string TenKho { get; set; } = string.Empty;

    [Column("mo_ta")]
    public string MoTa { get; set; } = string.Empty;

 
}


namespace Project_QLTS_DNC.Models.Kho
{
    public class KhoViewModel
    {
        public int MaKho { get; set; }
        public string TenKho { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public int MaToaNha { get; set; }
        public string TenToaNha { get; set; } = "---"; // Hiển thị thay vì mã
    }
}
