using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Models.PhanQuyen
{
    public class QuyenModel
    {
        [JsonProperty("ma_quyen")]
        public long MaQuyen { get; set; }

        [JsonProperty("ten_chuc_nang")]
        public string TenChucNang { get; set; }

        [JsonProperty("ma_man_hinh")]
        public string MaManHinh { get; set; }
    }
}
