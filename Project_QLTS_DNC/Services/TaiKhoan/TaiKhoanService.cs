using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.Models.NhanVien;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models;

namespace Project_QLTS_DNC.Services
{
    public class TaiKhoanService : SupabaseService
    {
        public TaiKhoanService() : base() { }

        public async Task<List<TaiKhoanDTO>> LayDanhSachTaiKhoanAsync()
        {
            var client = await SupabaseService.GetClientAsync();

            var danhSachTaiKhoan = await client.From<TaiKhoanModel>().Get();
            var danhSachLoaiTk = await client.From<LoaiTaiKhoanModel>().Get();
            var danhSachNhanVien = await client.From<NhanVienModel>().Get();

            // Tạo dictionary để tra nhanh
            var loaiTaiKhoanDict = danhSachLoaiTk.Models.ToDictionary(x => x.MaLoaiTk, x => x.TenLoaiTk);
            var nhanVienDict = danhSachNhanVien.Models.ToDictionary(x => x.MaNV, x => x.TenNV);

            
            var taiKhoanDTOs = new List<TaiKhoanDTO>();

            foreach (var taiKhoan in danhSachTaiKhoan.Models)
            {
                var tenLoaiTk = loaiTaiKhoanDict.GetValueOrDefault(taiKhoan.MaLoaiTk, "Không xác định");
                var tenNhanVien = taiKhoan.MaNv.HasValue
                    ? nhanVienDict.GetValueOrDefault(taiKhoan.MaNv.Value, "Không xác định")
                    : "Không có";

                var dto = new TaiKhoanDTO(taiKhoan, tenLoaiTk, tenNhanVien);
                taiKhoanDTOs.Add(dto);
            }

            return taiKhoanDTOs;
        }


    }
}

