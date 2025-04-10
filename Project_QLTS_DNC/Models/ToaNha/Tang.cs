using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Models.ToaNha
{
    public class Tang
    {
        // Mã tầng (khóa chính)
        [Key]
        public int MaTang { get; set; }

        // Tên tầng
        public string TenTang { get; set; }
    }
}
