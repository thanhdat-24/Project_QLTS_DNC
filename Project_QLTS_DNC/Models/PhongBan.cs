using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Models
{
    public class PhongBan
    {
        // Mã phòng (khóa chính)
        [Key]
        public int MaPhongBan { get; set; }

        // Tên phòng ban
        public string TenPhongBan { get; set; }

       
        // Mô tả phòng ban
        public string MoTaPhongBan { get; set; }
    }
}
