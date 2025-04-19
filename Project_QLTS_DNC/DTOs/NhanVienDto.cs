using Project_QLTS_DNC.Models.NhanVien;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.DTOs
{
    public class NhanVienDto: BaseModel
    {
        public int MaNv { get; set; }
        public int MaPb { get; set; }
        public int MaCv { get; set; }
        public string TenNv { get; set; }
        public string GioiTinh { get; set; }
        public string DiaChi { get; set; }
        public string Email { get; set; }
        public string Sdt { get; set; }
        public DateTime NgayVaoLam { get; set; }

        
        public string TenCv { get; set; }

        public string TenPb { get; set; }


        public NhanVienDto(NhanVienModel nhanVien, string tenPhongBan, string tenChucVu)
        {
            MaNv = nhanVien.MaNV;
            MaPb = nhanVien.MaPB;
            MaCv = nhanVien.MaCV;
            TenPb = tenPhongBan;
            TenCv = tenChucVu;
            TenNv = nhanVien.TenNV;
            GioiTinh = nhanVien.GioiTinh;
            DiaChi = nhanVien.DiaChi;
            Email = nhanVien.Email;
            Sdt = nhanVien.SDT;
            NgayVaoLam = nhanVien.NgayVaoLam;



        }
        public NhanVienDto()
        {
            // Constructor mặc định
        }

    }
}

