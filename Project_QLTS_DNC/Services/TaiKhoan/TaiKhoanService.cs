using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.Services;
using System.Collections.Generic;
using System.Linq;
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

        // Đảm bảo Supabase client đã được khởi tạo
        private async Task EnsureClientAsync()
        {
            if (_client == null)
                _client = await SupabaseService.GetClientAsync();
        }

        // Lấy tất cả tài khoản nếu người dùng hiện tại là admin
        public async Task<List<TaiKhoanDTO>> LayTatCaTaiKhoanNeuLaAdminAsync()
        {
            await EnsureClientAsync();

            var currentUser = _client.Auth.CurrentUser;
            if (currentUser == null) return null;  // Trả về null nếu không có người dùng đăng nhập

            // Lấy tài khoản của người dùng hiện tại
            var taiKhoanHienTai = await _client
                .From<TaiKhoanModel>()
                .Where(x => x.Uid == currentUser.Id)
                .Single();

            // Kiểm tra xem người dùng có phải là admin không (giả sử loại admin là 1)
            if (taiKhoanHienTai == null || taiKhoanHienTai.MaLoaiTk != 1)
                return null;

            // Lấy tất cả tài khoản, loại tài khoản và nhân viên
            var danhSachTaiKhoan = await _client.From<TaiKhoanModel>().Get();
            var danhSachLoaiTk = await _client.From<LoaiTaiKhoanModel>().Get();
            var danhSachNhanVien = await _client.From<NhanVienModel>().Get();

            // Danh sách chứa các DTO
            var taiKhoanDTOs = new List<TaiKhoanDTO>();

            foreach (var taiKhoan in danhSachTaiKhoan.Models)
            {
                // Lấy thông tin loại tài khoản và nhân viên tương ứng
                var loai = danhSachLoaiTk.Models.FirstOrDefault(x => x.MaLoaiTk == taiKhoan.MaLoaiTk);
                var nv = danhSachNhanVien.Models.FirstOrDefault(x => x.MaNV == taiKhoan.MaNv);

                // Tạo DTO cho tài khoản
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
