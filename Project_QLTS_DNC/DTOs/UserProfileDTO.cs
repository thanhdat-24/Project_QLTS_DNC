using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.DTOs
{
    public class UserProfileDTO
    {
        public int ma_nv { get; set; }
        public string ten_pb { get; set; } = "Chưa xác định";
        public string ten_cv { get; set; } = "Chưa xác định";

        // Các thuộc tính khác
        public string ten_nv { get; set; } = "";
        public string gioi_tinh { get; set; } = "";
        public string email { get; set; } = "";
        public string sdt { get; set; } = "";
        public DateTime ngay_vao_lam { get; set; } = DateTime.Now;
        public string dia_chi { get; set; } = "";
        public string ten_tai_khoan { get; set; } = "";
        public string ten_loai_tk { get; set; } = "";
        public bool trang_thai { get; set; } = false;
    }
}
