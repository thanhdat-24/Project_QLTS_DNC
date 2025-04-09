using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Models
{
    [Table("chucvu")]
    public class ChucVuModel : BaseModel
    {
        [PrimaryKey("ma_cv", false)]
        public long MaChucVu { get; set; }

        [Column("ten_cv")]
        public string TenChucVu { get; set; }

        [Column("mo_ta")]
        public string MoTa { get; set; }
    }
}
