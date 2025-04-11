using Project_QLTS_DNC.Models.QLLoaiTS;
using Project_QLTS_DNC.Models.QLNhomTS;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Services.QLTaiSanService
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
                // Kiểm tra tính hợp lệ của dữ liệu
                if (string.IsNullOrWhiteSpace(loaiTaiSan.TenLoaiTaiSan))
                {
                    throw new ArgumentException("Tên loại tài sản không được để trống.");
                }

                // Sinh mã mới nếu chưa có
                if (loaiTaiSan.MaLoaiTaiSan == 0)
                {
                    loaiTaiSan.MaLoaiTaiSan = await SinhMaLoaiTSAsync();
                }

                var client = await SupabaseService.GetClientAsync();

                // Tạo đối tượng mới để insert vào Supabase
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
                // Kiểm tra tính hợp lệ của dữ liệu
                if (loaiTaiSan.MaLoaiTaiSan == 0)
                {
                    throw new ArgumentException("Mã loại tài sản không hợp lệ.");
                }

                if (string.IsNullOrWhiteSpace(loaiTaiSan.TenLoaiTaiSan))
                {
                    throw new ArgumentException("Tên loại tài sản không được để trống.");
                }

                var client = await SupabaseService.GetClientAsync();

                // Kiểm tra xem loại tài sản có tồn tại không
                var kiemTraTonTai = await client.From<LoaiTaiSan>()
                    .Where(l => l.MaLoaiTaiSan == loaiTaiSan.MaLoaiTaiSan)
                    .Get();

                if (kiemTraTonTai.Models.Count == 0)
                {
                    throw new ArgumentException("Loại tài sản không tồn tại.");
                }

                // Update the loại tài sản
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
        /// Xóa một Loại Tài Sản khỏi Supabase
        /// </summary>
        public static async Task<bool> XoaLoaiTaiSanAsync(int maLoaiTaiSan)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Kiểm tra xem loại tài sản có tồn tại không
                var kiemTraTonTai = await client.From<LoaiTaiSan>()
                    .Where(l => l.MaLoaiTaiSan == maLoaiTaiSan)
                    .Get();

                if (kiemTraTonTai.Models.Count == 0)
                {
                    return false; // Loại tài sản không tồn tại
                }

                // Kiểm tra xem có nhóm tài sản nào thuộc loại tài sản này không
                var kiemTraNhomTaiSan = await client.From<NhomTaiSan>()
                    .Where(n => n.MaLoaiTaiSan == maLoaiTaiSan)
                    .Get();

                if (kiemTraNhomTaiSan.Models.Count > 0)
                {
                    throw new InvalidOperationException("Không thể xóa loại tài sản đã có nhóm tài sản liên kết.");
                }

                // Xóa loại tài sản
                await client.From<LoaiTaiSan>()
                    .Where(l => l.MaLoaiTaiSan == maLoaiTaiSan)
                    .Delete();

                // Kiểm tra xóa thành công
                var verifyResponse = await client.From<LoaiTaiSan>()
                    .Where(l => l.MaLoaiTaiSan == maLoaiTaiSan)
                    .Get();

                return verifyResponse.Models.Count == 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi xóa Loại Tài Sản: {ex.Message}");
                throw;
            }
        }
    }
}