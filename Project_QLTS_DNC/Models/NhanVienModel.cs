using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Models
{
    [Table("nhanvien")]
    public class NhanVienModel : BaseModel
    {
        [PrimaryKey("ma_nv", false)]
        public int MaNV { get; set; }

        [Column("ma_pb")]
        public int MaPB { get; set; }

        [Column("ma_cv")]
        public int MaCV { get; set; }

        [Column("ten_nv")]
        public string TenNV { get; set; }

        [Column("gioi_tinh")]
        public string GioiTinh { get; set; }

        [Column("dia_chi")]
        public string DiaChi { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("sdt")]
        public string SDT { get; set; }

        [Column("ngay_vao_lam")]
        public DateTime NgayVaoLam { get; set; }
    }
}
