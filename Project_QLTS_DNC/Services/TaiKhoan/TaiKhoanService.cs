using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Services
{
    public class TaiKhoanService
    {
        private Supabase.Client _client;

        public TaiKhoanService(Supabase.Client client)
        {
            _client = client;
        }

        private async Task EnsureClientAsync()
        {
            if (_client == null)
                _client = await SupabaseService.GetClientAsync();
        }

        public async Task<List<TaiKhoanDTO>> LayTatCaTaiKhoanNeuLaAdminAsync()
        {
            await EnsureClientAsync();

            var currentUser = _client.Auth.CurrentUser;
            if (currentUser == null) return null;

            var taiKhoanHienTai = await _client
                .From<TaiKhoanModel>()
                .Where(x => x.Uid == currentUser.Id)
                .Single();

            if (taiKhoanHienTai == null || taiKhoanHienTai.MaLoaiTk != 1)
                return null;

            var danhSachTaiKhoan = await _client.From<TaiKhoanModel>().Get();
            var danhSachLoaiTk = await _client.From<LoaiTaiKhoanModel>().Get();
            var danhSachNhanVien = await _client.From<NhanVienModel>().Get();

            var taiKhoanDTOs = new List<TaiKhoanDTO>();

            foreach (var taiKhoan in danhSachTaiKhoan.Models)
            {
                var loai = danhSachLoaiTk.Models.FirstOrDefault(x => x.MaLoaiTk == taiKhoan.MaLoaiTk);
                var nv = danhSachNhanVien.Models.FirstOrDefault(x => x.MaNV == taiKhoan.MaNv);

                var dto = new TaiKhoanDTO(
                    taiKhoan,
                    loai?.TenLoaiTk ?? "Unknown",
                    nv?.TenNV ?? "Unknown"
                );

                taiKhoanDTOs.Add(dto);
            }

            return taiKhoanDTOs;
        }
    }


}

