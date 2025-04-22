using Project_QLTS_DNC.Models.PhanQuyen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Project_QLTS_DNC.Helpers
{
    public static class QuyenNguoiDungHelper
    {
        // Gốc dữ liệu: Danh sách quyền đầy đủ từ PhanQuyenService
        public static List<PhanQuyenModel> DanhSachQuyen { get; set; } = new();

        // Danh sách màn hình được hiển thị (dùng cho UI TreeView, Menu, ...)
        public static List<string> DanhSachMaManHinhDuocHienThi =>
            DanhSachQuyen.Where(q => q.HienThi).Select(q => q.MaManHinh).ToList();

        // Kiểm tra quyền theo mã màn hình và hành động
        public static bool HasPermission(string maManHinh, string hanhDong)
        {
            var q = DanhSachQuyen.FirstOrDefault(x => x.MaManHinh == maManHinh);
            if (q == null) return false;

            return hanhDong.ToLower() switch
            {
                "xem" => q.Xem,
                "them" => q.Them,
                "sua" => q.Sua,
                "xoa" => q.Xoa,
                "hien_thi" => q.HienThi,
                _ => false
            };
        }

        // Ghi log tất cả quyền (debug)
        public static void LogDanhSachQuyen()
        {
            Debug.WriteLine("==== QUYỀN NGƯỜI DÙNG ====");
            foreach (var q in DanhSachQuyen)
            {
                Debug.WriteLine($"{q.MaManHinh} | Xem: {q.Xem}, Thêm: {q.Them}, Sửa: {q.Sua}, Xóa: {q.Xoa}, Hiển thị: {q.HienThi}");
            }
        }
    }
}
