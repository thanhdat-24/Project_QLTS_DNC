using Project_QLTS_DNC.Models.PhanQuyen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Helpers
{
    public static class DanhSachQuyenMacDinh
    {
        public static List<PhanQuyen> LayDanhSach()
        {
            return new List<PhanQuyen>
        {
            new PhanQuyen { TenChucNang = "Trang chủ", Xem = true, HienThi = true },
            new PhanQuyen { TenChucNang = "Danh sách tài khoản" },
            new PhanQuyen { TenChucNang = "Loại tài khoản" },
            new PhanQuyen { TenChucNang = "Phân quyền" },
            new PhanQuyen { TenChucNang = "Danh sách nhân viên" },
            new PhanQuyen { TenChucNang = "Chức vụ" },
            new PhanQuyen { TenChucNang = "Quản lý loại tài sản" },
            new PhanQuyen { TenChucNang = "Tòa nhà" },
            new PhanQuyen { TenChucNang = "Tầng" },
            new PhanQuyen { TenChucNang = "Phòng" },
            new PhanQuyen { TenChucNang = "Phòng ban" },
            new PhanQuyen { TenChucNang = "Quản lý kho" },
            new PhanQuyen { TenChucNang = "Nhập kho" },
            new PhanQuyen { TenChucNang = "Xuất kho" },
            new PhanQuyen { TenChucNang = "Tồn kho" },
            new PhanQuyen { TenChucNang = "Quản lý nhà cung cấp" },
            new PhanQuyen { TenChucNang = "Tra cứu tài sản" },
            new PhanQuyen { TenChucNang = "Loại bảo trì" },
            new PhanQuyen { TenChucNang = "Danh sách bảo trì" },
            new PhanQuyen { TenChucNang = "Báo cáo kiểm kê" },
            new PhanQuyen { TenChucNang = "Duyệt phiếu" },
            new PhanQuyen { TenChucNang = "Thông tin công ty" },
            new PhanQuyen { TenChucNang = "Cài đặt phiếu in" }
        };
        }
    }

}
