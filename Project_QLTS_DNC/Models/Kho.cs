using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("kho")]
public class Kho : BaseModel
{
    [PrimaryKey("ma_kho", false)] // Để PostgreSQL tự tạo giá trị
    [Column("ma_kho")]
    public int MaKho { get; set; }  // Giữ cột này lại

    [Column("ten_kho")]
    public string TenKho { get; set; } = null!;

    [Column("mo_ta")]
    public string MoTa { get; set; } = null!;

    [Column("ma_toa")]
    public int MaToaNha { get; set; }  // Gán mã tòa nhà vào đây
}


