using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.ToaNha;
using Supabase;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Services
{
    public class NhanVienService
    {
        private Supabase.Client _client;

        public NhanVienService(Supabase.Client client)
        {
            _client = client;
        }




        // Lấy danh sách nhân viên nếu người dùng là admin
        public async Task<List<NhanVienDto>> LayTatCaNhanVienDtoAsync()
        {

            var danhSachNhanVien = await _client.From<NhanVienModel>().Get();
            var danhSachPhongBan = await _client.From<PhongBan>().Get();
            var danhSachChucVu = await _client.From<ChucVuModel>().Get();


            var nhanVienDtos = new List<NhanVienDto>();

            


            foreach (var nhanVien in danhSachNhanVien.Models)
            {

                var phongBan = danhSachPhongBan.Models.FirstOrDefault(x => x.MaPhongBan == nhanVien.MaPB);
                var chucVu = danhSachChucVu.Models.FirstOrDefault(x => x.MaChucVu == nhanVien.MaCV);

                var nhanVienDto = new NhanVienDto(
                    nhanVien,
                    phongBan?.TenPhongBan ?? "Unknown",
                    chucVu?.TenChucVu ?? "Unknown"
                );

                nhanVienDtos.Add(nhanVienDto);
            }


            return nhanVienDtos;
        }



    }
}

