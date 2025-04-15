using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Project_QLTS_DNC.Models.TonKho
{
    [Table("tonkho")]
    public class TonKho : BaseModel
    {
        [PrimaryKey("ma_ton_kho", false)]
        public long? MaTonKho { get; set; }

        [Column("ma_kho")]
        public long MaKho { get; set; }

        [Column("ma_nhom_ts")]
        public long MaNhomTS { get; set; }

        [Column("so_luong_nhap")]
        public int SoLuongNhap { get; set; }

        [Column("so_luong_xuat")]
        public int SoLuongXuat { get; set; }

        // ❌ Không cần khai báo so_luong_ton vì DB tự tính

        [Column("ngay_cap_nhat")]
        public DateTime NgayCapNhat { get; set; }
    }
}
