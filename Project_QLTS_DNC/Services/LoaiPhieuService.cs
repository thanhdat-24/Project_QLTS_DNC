using Project_QLTS_DNC.Models.DuyetPhieu;
using Supabase;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace Project_QLTS_DNC.Services
{
    public class LoaiPhieuService
    {
        // Lấy danh sách theo thứ tự ma_loai_phieu tăng dần
        public static async Task<ObservableCollection<LoaiPhieu>> LayDanhSachLoaiPhieuAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                var response = await client
                    .From<LoaiPhieu>()
                    .Order(x => x.MaLoaiPhieu, Ordering.Ascending)
                    .Get();

                return new ObservableCollection<LoaiPhieu>(response.Models);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi lấy danh sách loại phiếu: {ex.Message}");
                return new ObservableCollection<LoaiPhieu>();
            }
        }

        // Thêm
        public static async Task<LoaiPhieu> ThemLoaiPhieuAsync(LoaiPhieu loaiPhieu)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loaiPhieu.TenLoaiPhieu))
                    throw new ArgumentException("Tên loại phiếu không được để trống.");

                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<LoaiPhieu>().Insert(loaiPhieu);

                if (response.Models.Count > 0)
                    return response.Models[0];

                throw new Exception("Không thể thêm loại phiếu!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi thêm loại phiếu: {ex.Message}");
                throw;
            }
        }

        // Cập nhật
        public static async Task<LoaiPhieu> CapNhatLoaiPhieuAsync(LoaiPhieu loaiPhieu)
        {
            try
            {
                if (loaiPhieu.MaLoaiPhieu == 0)
                    throw new ArgumentException("Mã loại phiếu không hợp lệ.");

                if (string.IsNullOrWhiteSpace(loaiPhieu.TenLoaiPhieu))
                    throw new ArgumentException("Tên loại phiếu không được để trống.");

                var client = await SupabaseService.GetClientAsync();

                var kiemTra = await client.From<LoaiPhieu>()
                    .Filter("ma_loai_phieu", Operator.Equals, loaiPhieu.MaLoaiPhieu)
                    .Get();

                if (kiemTra.Models.Count == 0)
                    throw new ArgumentException("Loại phiếu không tồn tại.");

                var response = await client.From<LoaiPhieu>()
                    .Filter("ma_loai_phieu", Operator.Equals, loaiPhieu.MaLoaiPhieu)
                    .Update(loaiPhieu);

                if (response.Models.Count > 0)
                    return response.Models[0];

                throw new Exception("Không thể cập nhật loại phiếu!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi cập nhật loại phiếu: {ex.Message}");
                throw;
            }
        }

        // Xoá
        public static async Task<bool> XoaLoaiPhieuAsync(int maLoaiPhieu)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                await client.From<LoaiPhieu>()
                    .Filter("ma_loai_phieu", Operator.Equals, maLoaiPhieu)
                    .Delete();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi xoá loại phiếu: {ex.Message}");
                return false;
            }
        }
    }
}
