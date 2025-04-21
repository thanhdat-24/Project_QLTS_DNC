using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models.ToaNha;

namespace Project_QLTS_DNC.Services.TaiKhoan
{
    public class UserProfileService : SupabaseService
    {
        public async Task<UserProfileDTO> LayThongTinCaNhanAsync(string tenTaiKhoan)
        {
            try
            {
                Console.WriteLine($"Bắt đầu tải thông tin cho tài khoản: {tenTaiKhoan}");
                var client = await GetClientAsync();

                // Kiểm tra client
                if (client == null)
                {
                    Console.WriteLine("ERROR: Không thể kết nối đến Supabase");
                    return null;
                }

                // Tìm tài khoản theo tên tài khoản
                var taiKhoanResult = await client.From<TaiKhoanModel>()
                    .Where(tk => tk.TenTaiKhoan == tenTaiKhoan)
                    .Get();

                if (taiKhoanResult == null || !taiKhoanResult.Models.Any())
                {
                    Console.WriteLine($"Không tìm thấy tài khoản: {tenTaiKhoan}");
                    return null;
                }

                var taiKhoan = taiKhoanResult.Models.First();
                Console.WriteLine($"Đã tìm thấy tài khoản: {taiKhoan.TenTaiKhoan}, Mã NV: {taiKhoan.MaNv}");

                // Nếu không có mã nhân viên
                if (taiKhoan.MaNv == null || taiKhoan.MaNv <= 0)
                {
                    Console.WriteLine("Tài khoản không có mã nhân viên hợp lệ");
                    return null;
                }

               
                var nhanVienResult = await client.From<NhanVienModel>()
                    .Where(nv => nv.MaNV == taiKhoan.MaNv)
                    .Get();

                if (nhanVienResult == null || !nhanVienResult.Models.Any())
                {
                    Console.WriteLine($"Không tìm thấy nhân viên với mã: {taiKhoan.MaNv}");
                    return null;
                }

                var nhanVien = nhanVienResult.Models.First();
                Console.WriteLine($"Đã tìm thấy nhân viên: {nhanVien.TenNV}");

                
                var phongBanResult = await client.From<PhongBan>()
                    .Where(pb => pb.MaPhongBan == nhanVien.MaPB)
                    .Get();

                string tenPhongBan = "Chưa xác định";
                if (phongBanResult != null && phongBanResult.Models.Any())
                {
                    tenPhongBan = phongBanResult.Models.First().TenPhongBan;
                    Console.WriteLine($"Đã tìm thấy phòng ban: {tenPhongBan}");
                }
                else
                {
                    Console.WriteLine($"Không tìm thấy phòng ban với mã: {nhanVien.MaPB}");
                }

                
                var chucVuResult = await client.From<ChucVuModel>()
                    .Where(cv => cv.MaChucVu == nhanVien.MaCV)
                    .Get();

                string tenChucVu = "Chưa xác định";
                if (chucVuResult != null && chucVuResult.Models.Any())
                {
                    tenChucVu = chucVuResult.Models.First().TenChucVu;
                    Console.WriteLine($"Đã tìm thấy chức vụ: {tenChucVu}");
                }
                else
                {
                    Console.WriteLine($"Không tìm thấy chức vụ với mã: {nhanVien.MaCV}");
                }

                
                var loaiTaiKhoanResult = await client.From<LoaiTaiKhoanModel>()
                    .Where(ltk => ltk.MaLoaiTk == taiKhoan.MaLoaiTk)
                    .Get();

                string tenLoaiTaiKhoan = "Chưa xác định";
                if (loaiTaiKhoanResult != null && loaiTaiKhoanResult.Models.Any())
                {
                    tenLoaiTaiKhoan = loaiTaiKhoanResult.Models.First().TenLoaiTk;
                    Console.WriteLine($"Đã tìm thấy loại tài khoản: {tenLoaiTaiKhoan}");
                }
                else
                {
                    Console.WriteLine($"Không tìm thấy loại tài khoản với mã: {taiKhoan.MaLoaiTk}");
                }

                
                var userProfile = new UserProfileDTO
                {
                    ma_nv = nhanVien.MaNV,
                    ten_nv = nhanVien.TenNV,
                    gioi_tinh = nhanVien.GioiTinh,
                    email = nhanVien.Email,
                    sdt = nhanVien.SDT,
                    ngay_vao_lam = nhanVien.NgayVaoLam,
                    dia_chi = nhanVien.DiaChi,
                    ten_tai_khoan = taiKhoan.TenTaiKhoan,
                    ten_loai_tk = tenLoaiTaiKhoan,
                    trang_thai = taiKhoan.TrangThai,
                    ten_pb = tenPhongBan,
                    ten_cv = tenChucVu
                };

                Console.WriteLine("Đã tạo thông tin hồ sơ người dùng thành công");
                return userProfile;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tải thông tin cá nhân: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        
        public async Task<bool> CapNhatThongTinCaNhanAsync(UserProfileDTO profile)
        {
            try
            {
                Console.WriteLine($"Bắt đầu cập nhật thông tin cho nhân viên mã: {profile.ma_nv}");
                var client = await GetClientAsync();

                if (client == null)
                {
                    Console.WriteLine("ERROR: Không thể kết nối đến Supabase");
                    return false;
                }

                
                var response = await client.From<NhanVienModel>()
                    .Where(nv => nv.MaNV == profile.ma_nv)
                    .Get();

                var nhanVien = response.Models.FirstOrDefault();

                if (nhanVien == null)
                {
                    Console.WriteLine("Không tìm thấy nhân viên để cập nhật");
                    return false;
                }

                
                nhanVien.TenNV = profile.ten_nv;
                nhanVien.GioiTinh = profile.gioi_tinh;
                nhanVien.Email = profile.email;
                nhanVien.SDT = profile.sdt;
                nhanVien.NgayVaoLam = profile.ngay_vao_lam;
                nhanVien.DiaChi = profile.dia_chi;

                
                var updated = await client.From<NhanVienModel>().Update(nhanVien);

                bool success = updated != null && updated.Models.Any();
                Console.WriteLine(success
                    ? $"Cập nhật thông tin nhân viên mã {profile.ma_nv} thành công"
                    : $"Cập nhật thông tin nhân viên mã {profile.ma_nv} thất bại");

                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật thông tin cá nhân: {ex.Message}");
                return false;
            }
        }

        
    }
}

