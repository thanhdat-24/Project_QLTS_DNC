using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Models
{
    public class ToaNha
    {
        // Mã toà nhà (khóa chính)
        [Key]
        public int MaToaNha { get; set; }

        // Tên phòng
        public string TenToaNha { get; set; }

        // Sức chứa
        public string DiaChiTN { get; set; }
        // Số điện thoại
        public string SoDienThoaiTN { get; set; }
        // Mô tả
        public string MoTaTN { get; set; }
    }
}
