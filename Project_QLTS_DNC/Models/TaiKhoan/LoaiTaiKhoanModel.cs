using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Models.TaiKhoan
{
    [Table("loaitaikhoan")]
    public class LoaiTaiKhoanModel: BaseModel
    {
        [PrimaryKey("ma_loai_tk", false)]

        public int MaLoaiTk { get; set; }
        [Column("ten_loai_tk")]
        public string TenLoaiTk { get; set; }
        [Column("mo_ta")]
        public string MoTa { get; set; }

    }
}
