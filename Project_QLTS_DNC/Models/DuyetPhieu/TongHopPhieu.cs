using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("tong_hop_phieu")]
public class TongHopPhieu : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("ma_phieu_nhap")]
    public long? MaPhieuNhap { get; set; }

    [Column("ma_phieu_xuat")]
    public long? MaPhieuXuat { get; set; }

    [Column("ma_phieu_de_nghi_mua")]
    public long? MaPhieuDeNghiMua { get; set; }

    [Column("ma_phieu_bao_hong")]
    public long? MaPhieuBaoHong { get; set; }

    [Column("ma_lich_su")]
    public long? MaLichSuDiChuyen { get; set; }
}
