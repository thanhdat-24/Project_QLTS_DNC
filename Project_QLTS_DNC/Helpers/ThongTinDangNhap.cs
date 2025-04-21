using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.TaiKhoan;
using System.Collections.Generic;

namespace Project_QLTS_DNC.Helpers
{
    public static class ThongTinDangNhap
    {
        public static TaiKhoanModel TaiKhoanDangNhap { get; set; }

        public static LoaiTaiKhoanModel LoaiTaiKhoanDangNhap { get; set; }

        /// <summary>
        /// Phòng ban của nhân viên (gắn từ bảng nhanvien → phongban)
        /// </summary>
        public static int? MaPhongBan { get; set; }

        /// <summary>
        /// Tòa nhà của phòng ban nhân viên
        /// </summary>
        public static int? MaToaNha { get; set; }

        /// <summary>
        /// Danh sách mã kho mà tài khoản được phép truy cập (theo tòa nhà)
        /// </summary>
        public static List<int> DanhSachKhoTheoToa { get; set; } = new();
    }
}
