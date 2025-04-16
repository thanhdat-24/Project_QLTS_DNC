using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("xuatkho")]
public class PhieuXuat : BaseModel
{
    [PrimaryKey("ma_phieu_xuat", false)]
    [Column("ma_phieu_xuat")]
    public long MaPhieuXuat { get; set; }  

    [Column("ma_kho_xuat")]
    public long MaKhoXuat { get; set; }

    [Column("ma_kho_nhan")]
    public long MaKhoNhan { get; set; }

    [Column("ngay_xuat")]
    public DateTime NgayXuat { get; set; }

    [Column("ma_nv")]
    public long MaNV { get; set; }

    [Column("trang_thai")]
    public string? TrangThai { get; set; }

    [Column("ghi_chu")]
    public string? GhiChu { get; set; }
}
