using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Models
{
    public class Phong
    {
        // Mã phòng (khóa chính)
        [Key]
        public int MaPhong { get; set; }

        // Tên phòng
        public string TenPhong { get; set; }

        // Sức chứa
        public int SucChua { get; set; }

        // Mô tả
        public string MoTaPhong { get; set; }
    }
}
