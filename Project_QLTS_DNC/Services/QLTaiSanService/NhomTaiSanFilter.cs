using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Models.ThongSoKT;

namespace Project_QLTS_DNC.Services.QLTaiSanService
{
    public class NhomTaiSanFilter
    {
        public int? MaNhomTS { get; set; }
        public string TenNhomTS { get; set; }

        public override string ToString()
        {
            return TenNhomTS;
        }
    }

    public partial class NhomTaiSanService
    {
        /// <summary>
        /// Lấy danh sách thông số kỹ thuật của một nhóm tài sản
        /// </summary>
        public static async Task<List<ThongSoKyThuat>> LayThongSoTheoNhomTaiSanAsync(int maNhomTS)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<ThongSoKyThuat>()
                    .Where(x => x.MaNhomTS == maNhomTS)
                    .Get();

                return response.Models;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy thông số theo nhóm tài sản: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Lấy danh sách filter nhóm tài sản để sử dụng trong combobox
        /// </summary>
        public static async Task<List<NhomTaiSanFilter>> GetNhomTaiSanFilterListAsync()
        {
            try
            {
                // Lấy danh sách nhóm tài sản
                var nhomTSCollection = await LayDanhSachNhomTaiSanAsync();

                // Tạo danh sách filter
                var nhomTSFilterList = new List<NhomTaiSanFilter>
                {
                    new NhomTaiSanFilter { MaNhomTS = null, TenNhomTS = "Tất cả" }
                };

                // Thêm các nhóm tài sản vào danh sách
                foreach (var nhomTS in nhomTSCollection)
                {
                    nhomTSFilterList.Add(new NhomTaiSanFilter
                    {
                        MaNhomTS = nhomTS.MaNhomTS,
                        TenNhomTS = nhomTS.TenNhom
                    });
                }

                return nhomTSFilterList;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy danh sách filter nhóm tài sản: {ex.Message}");
                throw;
            }
        }
    }
}