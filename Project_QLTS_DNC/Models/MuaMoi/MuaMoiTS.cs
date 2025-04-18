using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

[Table("phieudenghimua")]
public class MuaMoiTS : BaseModel
{
    [PrimaryKey("ma_phieu_denghi", false)]
    [Column("ma_phieu_denghi")]
    public int MaPhieuDeNghi { get; set; }

    [Column("ngay_denghi")]
    public DateTime NgayDeNghi{ get; set; }

    [Column("ma_nv")]
    public int MaNV { get; set; }

    [Column("trang_thai")]
    public bool? TrangThai { get; set; }

    [Column("don_vi_denghi")]
    public string? DonViDeNghi { get; set; }

    [Column("ly_do")]
    public string? LyDo { get; set; }

    [Column("ghi_chu")]
    public string? GhiChu { get; set; }

}
