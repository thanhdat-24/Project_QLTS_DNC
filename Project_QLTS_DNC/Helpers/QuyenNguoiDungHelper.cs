using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Helpers
{
    public static class QuyenNguoiDungHelper
    {
        public static List<string> DanhSachMaManHinhDuocHienThi { get; set; } = new();

        public static void LogDanhSachQuyen()
        {
            Debug.WriteLine("==== [LOG] Danh sách mã màn hình được hiển thị ====");
            foreach (var item in DanhSachMaManHinhDuocHienThi)
            {
                Debug.WriteLine($"✔ {item}");
            }
            Debug.WriteLine("===============================================");
        }
    }
}
