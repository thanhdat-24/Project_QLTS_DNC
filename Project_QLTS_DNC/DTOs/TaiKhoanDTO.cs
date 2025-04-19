using Project_QLTS_DNC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Project_QLTS_DNC.DTOs
{
    public class TaiKhoanDTO
    {
        public int MaTk { get; set; }
        public string TenTaiKhoan { get; set; }
        public string MatKhau { get; set; }
        public string Uid { get; set; }
        public int MaLoaiTk { get; set; }
        public int? MaNv { get; set; }
        public bool TrangThai { get; set; } = true; 

        // Các thuộc tính mở rộng cho UI
        public string TenLoaiTk { get; set; }
        public string TenNV { get; set; }

        // Constructor từ TaiKhoanModel
        public TaiKhoanDTO(TaiKhoanModel taiKhoan, string tenLoaiTaiKhoan, string tenNhanVien)
        {
            MaTk = taiKhoan.MaTk;
            TenTaiKhoan = taiKhoan.TenTaiKhoan;
            MatKhau = taiKhoan.MatKhau;
            Uid = taiKhoan.Uid;
            MaLoaiTk = taiKhoan.MaLoaiTk;
            MaNv = taiKhoan.MaNv;
            TenLoaiTk = tenLoaiTaiKhoan;
            TenNV = tenNhanVien;
            TrangThai = taiKhoan.TrangThai; // Copy trạng thái từ model
        }
    }
}