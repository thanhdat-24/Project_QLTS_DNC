
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("chitietdenghimua")]
public class ChiTietDeNghiMua : BaseModel
{
    [PrimaryKey("ma_chi_tiet_denghi", false)]
    public int MaChiTietDeNghi { get; set; }

    [Column("ma_phieu_denghi")]
    public int MaPhieuDeNghi { get; set; }

    [Column("ten_tai_san")]
    public string TenTaiSan { get; set; }

    [Column("so_luong")]
    public int SoLuong { get; set; }

    [Column("don_vi_tinh")]
    public string DonViTinh { get; set; }

    [Column("mo_ta")]
    public string MoTa { get; set; }

    [Column("du_kien_gia")]
    public int DuKienGia { get; set; }
}
