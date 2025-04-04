using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Models
{
    public class NhomTaiSan
    {
        public int MaNhomTS { get; set; }
        public int? ma_loai_ts { get; set; }
        public string TenNhom { get; set; }
        public int? SoLuong { get; set; }
        public string MoTa { get; set; }

        // Navigation property
        public virtual LoaiTaiSan LoaiTaiSan { get; set; }
    }
}
