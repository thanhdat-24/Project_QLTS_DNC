using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Services;

namespace Project_QLTS_DNC.Services
{
    public class TheoDoiHoatDongService
    {
        /// <summary>
        /// Lưu hoạt động xuất Excel vào lịch sử
        /// </summary>
        public async Task<bool> LuuHoatDongXuatExcel(List<PhieuBaoTri> danhSachPhieu, string ghiChu = "")
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    return false;

                foreach (var phieu in danhSachPhieu)
                {
                    // Tạo bản ghi lịch sử mới
                    var lichSuMoi = new LichSuSuaChua
                    {
                        MaBaoTri = phieu.MaBaoTri,
                        MaTaiSan = phieu.MaTaiSan,
                        MaNV = phieu.MaNV,
                        NgaySua = DateTime.Now,
                        LoaiThaoTac = "Xuất Excel",
                        KetQua = "Đã xuất dữ liệu phiếu bảo trì ra Excel",
                        ChiPhi = phieu.ChiPhi,
                        TrangThaiTruoc = phieu.TrangThai,
                        TrangThaiSau = phieu.TrangThai,
                        GhiChu = string.IsNullOrEmpty(ghiChu) ? "Xuất Excel bởi người dùng" : ghiChu,
                        NoiDungBaoTri = phieu.NoiDung
                    };

                    // Lưu vào cơ sở dữ liệu
                    var response = await client.From<LichSuSuaChua>().Insert(lichSuMoi);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lưu hoạt động xuất Excel: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lưu hoạt động in phiếu vào lịch sử
        /// </summary>
        public async Task<bool> LuuHoatDongInPhieu(List<PhieuBaoTri> danhSachPhieu, string ghiChu = "")
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    return false;

                foreach (var phieu in danhSachPhieu)
                {
                    // Tạo bản ghi lịch sử mới
                    var lichSuMoi = new LichSuSuaChua
                    {
                        MaBaoTri = phieu.MaBaoTri,
                        MaTaiSan = phieu.MaTaiSan,
                        MaNV = phieu.MaNV,
                        NgaySua = DateTime.Now,
                        LoaiThaoTac = "In phiếu",
                        KetQua = "Đã in phiếu bảo trì",
                        ChiPhi = phieu.ChiPhi,
                        TrangThaiTruoc = phieu.TrangThai,
                        TrangThaiSau = phieu.TrangThai,
                        GhiChu = string.IsNullOrEmpty(ghiChu) ? "In phiếu bởi người dùng" : ghiChu,
                        NoiDungBaoTri = phieu.NoiDung
                    };

                    // Lưu vào cơ sở dữ liệu
                    var response = await client.From<LichSuSuaChua>().Insert(lichSuMoi);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lưu hoạt động in phiếu: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lưu hoạt động xuất Excel danh sách tài sản vào lịch sử
        /// </summary>
        public async Task<bool> LuuHoatDongXuatExcelDanhSach(List<KiemKeTaiSan> danhSachTaiSan, string ghiChu = "")
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    return false;

                foreach (var taiSan in danhSachTaiSan)
                {
                    // Tạo bản ghi lịch sử mới
                    var lichSuMoi = new LichSuSuaChua
                    {
                        MaTaiSan = taiSan.MaTaiSan,
                        NgaySua = DateTime.Now,
                        LoaiThaoTac = "Xuất Excel danh sách",
                        KetQua = "Đã xuất danh sách tài sản cần bảo trì ra Excel",
                        TrangThaiTruoc = taiSan.TinhTrang,
                        TrangThaiSau = taiSan.TinhTrang,
                        GhiChu = string.IsNullOrEmpty(ghiChu) ? $"Xuất Excel danh sách: {taiSan.GhiChu}" : ghiChu,
                        NoiDungBaoTri = "Danh sách tài sản cần bảo trì"
                    };

                    // Lưu vào cơ sở dữ liệu
                    var response = await client.From<LichSuSuaChua>().Insert(lichSuMoi);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lưu hoạt động xuất Excel danh sách: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lưu hoạt động in phiếu danh sách tài sản vào lịch sử
        /// </summary>
        public async Task<bool> LuuHoatDongInPhieuDanhSach(List<KiemKeTaiSan> danhSachTaiSan, string ghiChu = "")
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    return false;

                foreach (var taiSan in danhSachTaiSan)
                {
                    // Tạo bản ghi lịch sử mới
                    var lichSuMoi = new LichSuSuaChua
                    {
                        MaTaiSan = taiSan.MaTaiSan,
                        NgaySua = DateTime.Now,
                        LoaiThaoTac = "In phiếu danh sách",
                        KetQua = "Đã in phiếu danh sách tài sản cần bảo trì",
                        TrangThaiTruoc = taiSan.TinhTrang,
                        TrangThaiSau = taiSan.TinhTrang,
                        GhiChu = string.IsNullOrEmpty(ghiChu) ? $"In phiếu danh sách: {taiSan.GhiChu}" : ghiChu,
                        NoiDungBaoTri = "Danh sách tài sản cần bảo trì"
                    };

                    // Lưu vào cơ sở dữ liệu
                    var response = await client.From<LichSuSuaChua>().Insert(lichSuMoi);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lưu hoạt động in phiếu danh sách: {ex.Message}");
                return false;
            }
        }
    }
}