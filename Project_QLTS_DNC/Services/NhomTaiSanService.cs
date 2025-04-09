using Project_QLTS_DNC.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Services
{
    public class NhomTaiSanService
    {
        /// <summary>
        /// Lấy danh sách Nhóm Tài Sản từ Supabase
        /// </summary>
        public static async Task<ObservableCollection<NhomTaiSan>> LayDanhSachNhomTaiSanAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<NhomTaiSan>().Get();

                // Chuyển đổi từ List<NhomTaiSan> sang ObservableCollection<NhomTaiSan>
                return new ObservableCollection<NhomTaiSan>(response.Models);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy danh sách Nhóm Tài Sản: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Thêm mới một Nhóm Tài Sản vào Supabase
        /// </summary>
        public static async Task<NhomTaiSan> ThemNhomTaiSanAsync(NhomTaiSan nhomTaiSan)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<NhomTaiSan>().Insert(nhomTaiSan);

                if (response.Models.Count > 0)
                {
                    return response.Models[0];
                }

                throw new Exception("Không thể thêm mới nhóm tài sản!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi thêm Nhóm Tài Sản: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Kết hợp dữ liệu Nhóm Tài Sản với Loại Tài Sản
        /// </summary>
        public static void KetHopDuLieu(ObservableCollection<LoaiTaiSan> dsLoaiTaiSan, ObservableCollection<NhomTaiSan> dsNhomTaiSan)
        {
            foreach (var nhomTaiSan in dsNhomTaiSan)
            {
                nhomTaiSan.LoaiTaiSan = dsLoaiTaiSan.FirstOrDefault(l => l.MaLoaiTaiSan == nhomTaiSan.ma_loai_ts);
            }
        }
    }
}