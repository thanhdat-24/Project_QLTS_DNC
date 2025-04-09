using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Models
{
    public class LoaiTaiSan
    {
        public int MaLoaiTaiSan { get; set; }
        public string TenLoaiTaiSan { get; set; }
        public string MoTa { get; set; }

        // Navigation property
        public virtual ICollection<NhomTaiSan> NhomTaiSans { get; set; }
    }
}
