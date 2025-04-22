using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

[Table("chitietphieunhap")]
public class ChiTietPhieuNhap : BaseModel
{
    [PrimaryKey("ma_chi_tiet_pn", false)]
    [Column("ma_chi_tiet_pn")]
    public int MaChiTietPN { get; set; }

    [Column("ma_phieu_nhap")]
    public int MaPhieuNhap { get; set; }

    [Column("ma_nhom_ts")]
    public int MaNhomTS { get; set; }

    [Column("ten_tai_san")]
    public string TenTaiSan { get; set; } = string.Empty;

    [Column("so_luong")]
    public int? SoLuong { get; set; }

    [Column("can_quan_ly_rieng")]
    public bool CanQuanLyRieng { get; set; } = false;

    [Column("don_gia")]
    public decimal? DonGia { get; set; }

    [Column("tg_bao_hanh")]
    public int? ThoiGianBaoHanh { get; set; } 

    [JsonIgnore]
    public string TenNhomTS { get; set; }
}



[Table("chitietphieunhap")]
public class ChiTietPhieuNhapInsert : BaseModel
{
    [Column("ma_phieu_nhap")]
    public int MaPhieuNhap { get; set; }

    [Column("ma_nhom_ts")]
    public int MaNhomTS { get; set; }

    [Column("ten_tai_san")]
    public string TenTaiSan { get; set; } = string.Empty;

    [Column("so_luong")]
    public int? SoLuong { get; set; }

    [Column("can_quan_ly_rieng")]
    public bool CanQuanLyRieng { get; set; } = false;

    [Column("don_gia")]
    public decimal? DonGia { get; set; }

    [Column("tg_bao_hanh")]
    public int? ThoiGianBaoHanh { get; set; }
}