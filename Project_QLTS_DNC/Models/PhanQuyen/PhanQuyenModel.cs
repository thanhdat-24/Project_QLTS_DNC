using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Models.PhanQuyen
{
    public class PhanQuyenModel
    {
        [JsonProperty("ma_loai_tk")]
        public long MaLoaiTk { get; set; }

        [JsonProperty("ma_quyen")]
        public long MaQuyen { get; set; }

        [JsonProperty("xem")]
        public bool Xem { get; set; }

        [JsonProperty("them")]
        public bool Them { get; set; }

        [JsonProperty("sua")]
        public bool Sua { get; set; }

        [JsonProperty("xoa")]
        public bool Xoa { get; set; }

        [JsonProperty("hien_thi")]
        public bool HienThi { get; set; }

        [JsonProperty("ten_chuc_nang")] 
        public string TenChucNang { get; set; }

        [JsonProperty("ma_man_hinh")]   
        public string MaManHinh { get; set; }
    }

}
