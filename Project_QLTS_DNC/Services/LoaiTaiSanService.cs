using Project_QLTS_DNC.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Services
{
    public class LoaiTaiSanService
    {
        /// <summary>
        /// Lấy danh sách Loại Tài Sản từ Supabase
        /// </summary>
        public static async Task<ObservableCollection<LoaiTaiSan>> LayDanhSachLoaiTaiSanAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<LoaiTaiSan>().Get();

                // Chuyển đổi từ List<LoaiTaiSan> sang ObservableCollection<LoaiTaiSan>
                return new ObservableCollection<LoaiTaiSan>(response.Models);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy danh sách Loại Tài Sản: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sinh mã loại tài sản tự động
        /// </summary>
        private static async Task<int> SinhMaLoaiTSAsync()
        {
            try
            {
                // Lấy danh sách loại tài sản hiện có
                var danhSachLoaiTS = await LayDanhSachLoaiTaiSanAsync();

                // Nếu không có dữ liệu, bắt đầu từ 1
                if (danhSachLoaiTS == null || danhSachLoaiTS.Count == 0)
                    return 1;

                // Tìm mã lớn nhất trong danh sách
                int maxMa = 0;
                foreach (var loaiTS in danhSachLoaiTS)
                {
                    if (loaiTS.MaLoaiTaiSan > maxMa)
                        maxMa = loaiTS.MaLoaiTaiSan;
                }

                // Trả về mã mới = mã lớn nhất + 1
                return maxMa + 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi sinh mã loại tài sản: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Thêm mới một Loại Tài Sản vào Supabase
        /// </summary>
        public static async Task<LoaiTaiSan> ThemLoaiTaiSanAsync(LoaiTaiSan loaiTaiSan)
        {
            try
            {
                // Sinh mã mới trước khi lấy client
                int maMoi = await SinhMaLoaiTSAsync();
                loaiTaiSan.MaLoaiTaiSan = maMoi;

                System.Diagnostics.Debug.WriteLine($"Thêm loại tài sản với mã: {loaiTaiSan.MaLoaiTaiSan}");

                var client = await SupabaseService.GetClientAsync();

                // Sử dụng một đối tượng mới để tránh các vấn đề với BaseModel
                var loaiTaiSanToInsert = new
                {
                    ma_loai_ts = loaiTaiSan.MaLoaiTaiSan,
                    ten_loai_ts = loaiTaiSan.TenLoaiTaiSan,
                    mo_ta = loaiTaiSan.MoTa
                };

                // Sử dụng RPC (gọi thủ tục từ xa) hoặc SQL trực tiếp nếu cần
                var response = await client.From<LoaiTaiSan>().Insert(loaiTaiSan);

                if (response.Models.Count > 0)
                {
                    return response.Models[0];
                }

                throw new Exception("Không thể thêm mới loại tài sản!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi thêm Loại Tài Sản: {ex.Message}, {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Cập nhật thông tin một Loại Tài Sản
        /// </summary>
        public static async Task<LoaiTaiSan> CapNhatLoaiTaiSanAsync(LoaiTaiSan loaiTaiSan)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<LoaiTaiSan>()
                    .Where(l => l.MaLoaiTaiSan == loaiTaiSan.MaLoaiTaiSan)
                    .Update(loaiTaiSan);

                if (response.Models.Count > 0)
                {
                    return response.Models[0];
                }

                throw new Exception("Không thể cập nhật loại tài sản!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi cập nhật Loại Tài Sản: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Tìm kiếm Loại Tài Sản theo mã
        /// </summary>
        public static async Task<LoaiTaiSan> TimLoaiTaiSanTheoMaAsync(int maLoaiTaiSan)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<LoaiTaiSan>()
                    .Where(l => l.MaLoaiTaiSan == maLoaiTaiSan)
                    .Get();

                if (response.Models.Count > 0)
                {
                    return response.Models[0];
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi tìm Loại Tài Sản theo mã: {ex.Message}");
                throw;
            }
        }
    }
}