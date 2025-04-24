using Project_QLTS_DNC.Models.LichSu;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Models.ToaNha;
using Supabase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Services
{
    public static class LichSuDiChuyenService
    {
        public static async Task<ObservableCollection<LichSuDTO>> LayDanhSachLichSuAsync()
        {
            var client = await SupabaseService.GetClientAsync();

            var dsLichSu = await client.From<LichSuDiChuyenTaiSan>().Get();
            var dsTaiSan = await client.From<TaiSanModel>().Get();
            var dsPhong = await client.From<Phong>().Get();
            var dsNhanVien = await client.From<NhanVienModel>().Get();

            var danhSach = dsLichSu.Models.Select(p =>
            {
                var ts = dsTaiSan.Models.FirstOrDefault(t => t.MaTaiSan == p.MaTaiSan);
                var phongCu = dsPhong.Models.FirstOrDefault(ph => ph.MaPhong == p.MaPhongCu);
                var phongMoi = dsPhong.Models.FirstOrDefault(ph => ph.MaPhong == p.MaPhongMoi);
                var nv = dsNhanVien.Models.FirstOrDefault(n => n.MaNV == p.MaNhanVien);

                return new LichSuDTO
                {
                    MaLichSu = p.MaLichSu,
                    TenTaiSan = ts?.TenTaiSan ?? "",
                    SoSeri = ts?.SoSeri ?? "",
                    TenPhongCu = phongCu?.TenPhong ?? "",
                    TenPhongMoi = phongMoi?.TenPhong ?? "",
                    TenNhanVien = nv?.TenNV ?? "",
                    NgayBanGiao = p.NgayBanGiao,
                    GhiChu = p.GhiChu,
                    TrangThai = p.TrangThai,
                    MaPhongCu = phongCu?.MaPhong
                };
            }).OrderByDescending(p => p.NgayBanGiao).ToList();

            return new ObservableCollection<LichSuDTO>(danhSach);
        }


        public static async Task<List<Phong>> LayDanhSachPhongAsync()
        {
            var client = await SupabaseService.GetClientAsync();
            var result = await client.From<Phong>().Get();
            return result.Models;
        }
        public static async Task<bool> XoaPhieuLichSuAsync(long maLichSu)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                await client
                    .From<LichSuDiChuyenTaiSan>()
                    .Where(x => x.MaLichSu == maLichSu)
                    .Delete();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi xóa phiếu: " + ex.Message);
                return false;
            }
        }





    }

    public class LichSuDTO
    {
        public long MaLichSu { get; set; }
        public string TenTaiSan { get; set; }
        public string SoSeri { get; set; }

        public string TenPhongCu { get; set; }
        public string TenPhongMoi { get; set; }
        public string TenNhanVien { get; set; }
        public DateTime? NgayBanGiao { get; set; }
        public string GhiChu { get; set; }
        public bool? TrangThai { get; set; }
        public int? MaPhongCu { get; set; }

        public string TrangThaiText => TrangThai == true ? "Đã duyệt" : TrangThai == false ? "Từ chối duyệt" : "Chờ duyệt";
    }
}