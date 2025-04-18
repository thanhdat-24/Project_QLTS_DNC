using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

namespace Project_QLTS_DNC.Models.ToaNha
{
    [Table("suc_chua_phong_nhom")] 
    public class PhongNhomTS : BaseModel
    {
        [PrimaryKey("ma_phong", true)]
        public int MaPhong { get; set; }

        [Column("ma_nhom_ts")]
        public int MaNhomTS { get; set; }

        [Column("suc_chua")]
        public int SucChua { get; set; }

        // Thuộc tính hiển thị thêm (nếu muốn)
       

    }
}

