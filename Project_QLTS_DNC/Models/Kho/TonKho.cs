using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using Project_QLTS_DNC.Models.Kho;



namespace Project_QLTS_DNC.Models.Kho
{ 
 [Table("tonkho")]
public class TonKho : BaseModel
{
    [PrimaryKey("ma_ton_kho", false)]
    public int MaTonKho { get; set; }

    [Column("ma_kho")]
    public int MaKho { get; set; }

    [Column("ma_nhom_ts")]
    public int MaNhomTS { get; set; }

    [Column("so_luong_nhap")]
    public int SoLuongNhap { get; set; }

    [Column("so_luong_xuat")]
    public int SoLuongXuat { get; set; }

    [Column("so_luong_ton")]
    public int SoLuongTon { get; set; } // Cột GENERATED vẫn có thể map được

    [Column("ngay_cap_nhat")]
    public DateTime NgayCapNhat { get; set; }

    [JsonIgnore]
    public string? TenKho { get; set; }

    [JsonIgnore]
    public string? TenNhomTS { get; set; }



    }



}