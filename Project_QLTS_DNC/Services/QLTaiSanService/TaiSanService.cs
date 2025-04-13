using Project_QLTS_DNC.Models.QLTaiSan;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Supabase.Postgrest;

namespace Project_QLTS_DNC.Services.QLTaiSanService
{
    public class TaiSanService
    {
        /// <summary>
        /// Lấy danh sách Tài Sản từ Supabase
        /// </summary>
        public static async Task<ObservableCollection<TaiSanModel>> LayDanhSachTaiSanAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<TaiSanModel>().Get();

                // Chuyển đổi từ List<TaiSanModel> sang ObservableCollection<TaiSanModel>
                return new ObservableCollection<TaiSanModel>(response.Models);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy danh sách Tài Sản: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Đếm số lượng Tài Sản hiện có trong hệ thống
        /// </summary>
        public static async Task<int> DemSoLuongTaiSanAsync()
        {
            try
            {
                var dsTaiSan = await LayDanhSachTaiSanAsync();
                return dsTaiSan.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi đếm số lượng Tài Sản: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Thêm mới một Tài Sản vào hệ thống
        /// </summary>
        public static async Task<TaiSanModel> ThemTaiSanAsync(TaiSanModel taiSan)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<TaiSanModel>().Insert(taiSan);

                if (response.Models.Count > 0)
                {
                    return response.Models.First();
                }
                else
                {
                    throw new Exception("Không thể thêm Tài Sản mới.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi thêm Tài Sản: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Cập nhật thông tin Tài Sản
        /// </summary>
        /// <summary>
        /// Cập nhật thông tin Tài Sản
        /// </summary>
        public static async Task<TaiSanModel> CapNhatTaiSanAsync(TaiSanModel taiSan)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Tải dữ liệu hiện tại của tài sản trước để đảm bảo tất cả dữ liệu được giữ nguyên
                var currentTaiSan = await LayTaiSanTheoMaAsync(taiSan.MaTaiSan);

                // Cập nhật các thuộc tính từ tài sản mới
                currentTaiSan.TenTaiSan = taiSan.TenTaiSan;
                currentTaiSan.SoSeri = taiSan.SoSeri;
                currentTaiSan.MaQR = taiSan.MaQR;
                currentTaiSan.NgaySuDung = taiSan.NgaySuDung;
                currentTaiSan.HanBH = taiSan.HanBH;
                currentTaiSan.TinhTrangSP = taiSan.TinhTrangSP;
                currentTaiSan.GhiChu = taiSan.GhiChu;
                currentTaiSan.MaPhong = taiSan.MaPhong;

                // Sử dụng currentTaiSan trong câu lệnh cập nhật
                var response = await client.From<TaiSanModel>()
                    .Filter("ma_tai_san", Supabase.Postgrest.Constants.Operator.Equals, taiSan.MaTaiSan)
                    .Update(currentTaiSan);

                if (response.Models.Count > 0)
                {
                    return response.Models.First();
                }
                else
                {
                    throw new Exception("Không thể cập nhật Tài Sản.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi cập nhật Tài Sản: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một Tài Sản
        /// </summary>
        public static async Task<TaiSanModel> LayTaiSanTheoMaAsync(int maTaiSan)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<TaiSanModel>()
                    .Where(x => x.MaTaiSan == maTaiSan)
                    .Get();

                if (response.Models.Count > 0)
                {
                    return response.Models.First();
                }
                else
                {
                    throw new Exception("Không tìm thấy Tài Sản với mã đã cho.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy thông tin Tài Sản: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Xóa một Tài Sản khỏi hệ thống
        /// </summary>
        public static async Task<bool> XoaTaiSanAsync(int maTaiSan)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                // Thay vì gán kết quả vào biến response
                await client.From<TaiSanModel>()
                    .Where(x => x.MaTaiSan == maTaiSan)
                    .Delete();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi xóa Tài Sản: {ex.Message}");
                return false;
            }
        }
    }
}