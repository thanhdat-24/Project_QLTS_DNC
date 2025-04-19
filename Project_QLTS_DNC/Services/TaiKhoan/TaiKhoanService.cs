using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.Models.NhanVien;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models;
using static Supabase.Postgrest.Constants;

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

        public async Task<TaiKhoanModel> ThemTaiKhoanAsync(string tenTaiKhoan, string matKhau, int maLoaiTk, int? maNv)
        {
            var client = await SupabaseService.GetClientAsync();

            
            var taiKhoan = new TaiKhoanModel
            {
                TenTaiKhoan = tenTaiKhoan,
                MatKhau = matKhau,
                MaLoaiTk = maLoaiTk,
                MaNv = maNv
            };

            try
            {
                
                var result = await client.From<TaiKhoanModel>().Order(x => x.MaTk, Ordering.Ascending)
                    .Insert(taiKhoan);
                return result.Models.FirstOrDefault(); 
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error while creating account: {ex.Message}");
                return null; 
            }
        }

        public async Task<bool> CapNhatTaiKhoanAsync(int maTk, string matKhau, int maLoaiTk, int? maNv)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Tìm tài khoản theo mã
                var taiKhoanResponse = await client
                    .From<TaiKhoanModel>().Order(x => x.MaTk, Ordering.Ascending)
                    .Where(t => t.MaTk == maTk)
                    .Get();

                var taiKhoan = taiKhoanResponse.Models.FirstOrDefault();
                if (taiKhoan == null)
                    return false;

                // Cập nhật thông tin
                taiKhoan.MatKhau = matKhau;
                taiKhoan.MaLoaiTk = maLoaiTk;
                taiKhoan.MaNv = maNv;

                // Lưu thay đổi vào cơ sở dữ liệu
                var response = await client
                    .From<TaiKhoanModel>()
                    .Update(taiKhoan);

                return response.Models.Any(); // Trả về true nếu cập nhật thành công
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi cập nhật tài khoản: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> KhoaTaiKhoanAsync(int maTk)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                var taiKhoanResponse = await client
                    .From<TaiKhoanModel>()
                    .Where(t => t.MaTk == maTk)
                    .Get();

                var taiKhoan = taiKhoanResponse.Models.FirstOrDefault();
                if (taiKhoan == null)
                    return false;

                taiKhoan.TrangThai = false;

                var response = await client
                    .From<TaiKhoanModel>()
                    .Update(taiKhoan);

                return response.Models.Any();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khóa tài khoản: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> MoKhoaTaiKhoanAsync(int maTk)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                var taiKhoanResponse = await client
                    .From<TaiKhoanModel>()
                    .Where(t => t.MaTk == maTk)
                    .Get();

                var taiKhoan = taiKhoanResponse.Models.FirstOrDefault();
                if (taiKhoan == null)
                    return false;

                taiKhoan.TrangThai = true;

                var response = await client
                    .From<TaiKhoanModel>()
                    .Update(taiKhoan);

                return response.Models.Any();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi mở khóa tài khoản: {ex.Message}");
                return false;
            }
        }

        public async Task<List<TaiKhoanDTO>> TimKiemTaiKhoanAsync(string tuKhoa = null, int? maLoaiTk = null)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                
                var danhSachTaiKhoan = await client.From<TaiKhoanModel>().Get();
                var danhSachLoaiTk = await client.From<LoaiTaiKhoanModel>().Get();
                var danhSachNhanVien = await client.From<NhanVienModel>().Get();

                
                var loaiTaiKhoanDict = danhSachLoaiTk.Models.ToDictionary(x => x.MaLoaiTk, x => x.TenLoaiTk);
                var nhanVienDict = danhSachNhanVien.Models.ToDictionary(x => x.MaNV, x => x.TenNV);

                
                Console.WriteLine($"Từ khóa tìm kiếm: {tuKhoa}");
                Console.WriteLine($"Mã loại tài khoản: {maLoaiTk}");

                
                var query = danhSachTaiKhoan.Models.AsEnumerable();

               
                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    tuKhoa = tuKhoa.ToLower().Trim();
                    query = query.Where(tk =>
                        tk.MaTk.ToString().Contains(tuKhoa) ||
                        tk.TenTaiKhoan.ToLower().Contains(tuKhoa) ||
                        (tk.MaNv.HasValue &&
                            nhanVienDict.ContainsKey(tk.MaNv.Value) &&
                            nhanVienDict[tk.MaNv.Value].ToLower().Contains(tuKhoa))
                    );
                }

                
                if (maLoaiTk.HasValue && maLoaiTk.Value != 0)
                {
                    query = query.Where(tk => tk.MaLoaiTk == maLoaiTk.Value);
                }

               
                var taiKhoanDTOs = new List<TaiKhoanDTO>();
                foreach (var taiKhoan in query)
                {
                    var tenLoaiTk = loaiTaiKhoanDict.GetValueOrDefault(taiKhoan.MaLoaiTk, "Không xác định");
                    var tenNhanVien = taiKhoan.MaNv.HasValue
                        ? nhanVienDict.GetValueOrDefault(taiKhoan.MaNv.Value, "Không xác định")
                        : "Không có";

                    var dto = new TaiKhoanDTO(taiKhoan, tenLoaiTk, tenNhanVien);
                    taiKhoanDTOs.Add(dto);
                }

                
                Console.WriteLine($"Số lượng tài khoản tìm được: {taiKhoanDTOs.Count}");

                return taiKhoanDTOs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tìm kiếm tài khoản: {ex.Message}");
                return new List<TaiKhoanDTO>();
            }
        }
    }
}

