using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Helpers
{
    public static class PhamViHelper
    {
        public static IEnumerable<T> LocTheoKho<T>(this IEnumerable<T> danhSach)
        {
            var prop = typeof(T).GetProperty("MaKho");
            if (prop == null) return Enumerable.Empty<T>();

            var maKhoList = ThongTinDangNhap.DanhSachKhoTheoToa;
            return danhSach.Where(x =>
            {
                var value = prop.GetValue(x);
                return value != null && maKhoList.Contains((int)value);
            });
        }
    }

}
