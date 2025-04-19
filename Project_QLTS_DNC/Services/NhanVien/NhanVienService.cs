using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services.QLToanNha;
using Supabase;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static Supabase.Postgrest.Constants;

namespace Project_QLTS_DNC.Services
{
    public class NhanVienService : SupabaseService
    {


        public NhanVienService() : base()
        {

        }

        public async Task<List<NhanVienDto>> LayTatCaNhanVienDtoAsync()
        {
            try
            {
                var _client = await SupabaseService.GetClientAsync();

                var danhSachNhanVien = await _client.From<NhanVienModel>().Get();
                var danhSachPhongBan = await _client.From<PhongBan>().Get();
                var danhSachChucVu = await _client.From<ChucVuModel>().Get();

                var phongBanDictionary = danhSachPhongBan.Models
                    .ToDictionary(p => p.MaPhongBan, p => p.TenPhongBan);

                var chucVuDictionary = danhSachChucVu.Models
                    .ToDictionary(c => c.MaChucVu, c => c.TenChucVu);

                var nhanVienDtos = new List<NhanVienDto>();

                var danhSachNhanVienSapXep = danhSachNhanVien.Models
                    .OrderBy(nv => nv.MaNV);

                foreach (var nhanVien in danhSachNhanVienSapXep)
                {
                    string tenPhongBan = phongBanDictionary.GetValueOrDefault(nhanVien.MaPB, "Không rõ");
                    string tenChucVu = chucVuDictionary.GetValueOrDefault(nhanVien.MaCV, "Không rõ");

                    var nhanVienDto = new NhanVienDto(nhanVien, tenPhongBan, tenChucVu);
                    nhanVienDtos.Add(nhanVienDto);
                }

                return nhanVienDtos;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy danh sách nhân viên: {ex.Message}");
                throw;
            }
        }



        public async Task<NhanVienModel> ThemNhanVienAsync(NhanVienModel nhanVien)
        {
            try
            {
                var _client = await SupabaseService.GetClientAsync();
                var response = await _client
                    .From<NhanVienModel>().Order(x => x.MaNV, Ordering.Ascending)
                    .Insert(nhanVien);


                if (response.Models.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("Thêm nhân viên thành công!");
                    return response.Models.First();
                }
                else
                {

                    System.Diagnostics.Debug.WriteLine("Insert trả về rỗng.");
                }

                return null;
            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.WriteLine("Lỗi khi thêm nhân viên: " + ex.Message);
                return null;
            }
        }

        public async Task<NhanVienModel> CapNhatNhanVienAsync(NhanVienModel nhanVien)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();


                var response = await client
                    .From<NhanVienModel>().Order(x => x.MaNV, Ordering.Ascending)
                    .Where(x => x.MaNV == nhanVien.MaNV)
                    .Update(nhanVien);

                var updatedModel = response.Models.FirstOrDefault();


                if (updatedModel == null)
                {
                    var getResponse = await client
                        .From<NhanVienModel>().Order(x => x.MaNV, Ordering.Ascending)
                        .Where(x => x.MaNV == nhanVien.MaNV)
                        .Get();

                    updatedModel = getResponse.Models.FirstOrDefault();
                }

                return updatedModel;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating NhanVien: {ex.Message}");
                throw new Exception("Có lỗi khi cập nhật nhân viên", ex);
            }
        }

        public async Task<bool> XoaNhanVienAsync(int maNV)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                await client
                        .From<NhanVienModel>()
                        .Where(x => x.MaNV == maNV)
                        .Delete();

                return true;


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa chức vụ: {ex.Message}");
                return false;
            }
        }


        public async Task<List<NhanVienDto>> TimKiemNhanVienAsync(string searchText, int? maPhongBan)
        {
            try
            {
                var _client = await SupabaseService.GetClientAsync();
                var danhSachNhanVien = await _client.From<NhanVienModel>().Get();
                var danhSachPhongBan = await _client.From<PhongBan>().Get();
                var danhSachChucVu = await _client.From<ChucVuModel>().Get();

                var phongBanDictionary = danhSachPhongBan.Models.ToDictionary(p => p.MaPhongBan, p => p.TenPhongBan);
                var chucVuDictionary = danhSachChucVu.Models.ToDictionary(c => c.MaChucVu, c => c.TenChucVu);

                var nhanVienDtos = new List<NhanVienDto>();

                foreach (var nhanVien in danhSachNhanVien.Models)
                {
                   
                    if ((maPhongBan == null || nhanVien.MaPB == maPhongBan) &&
                        (nhanVien.MaNV.ToString().Contains(searchText) ||
                         nhanVien.TenNV.ToLower().Contains(searchText.ToLower()) ||
                         nhanVien.Email.ToLower().Contains(searchText.ToLower())))
                    {
                        var phongBan = phongBanDictionary.ContainsKey(nhanVien.MaPB) ? phongBanDictionary[nhanVien.MaPB] : "Unknown";
                        var chucVu = chucVuDictionary.ContainsKey(nhanVien.MaCV) ? chucVuDictionary[nhanVien.MaCV] : "Unknown";

                        var nhanVienDto = new NhanVienDto(
                            nhanVien,
                            phongBan,
                            chucVu
                        );

                        nhanVienDtos.Add(nhanVienDto);
                    }
                }

                return nhanVienDtos.OrderBy(nv => nv.MaNv).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi tìm kiếm nhân viên: " + ex.Message);
                return new List<NhanVienDto>(); 
            }
        }


    }
}

