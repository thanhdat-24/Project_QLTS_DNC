using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.DTOs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Services
{
    public partial class NhomTaiSanService
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
        /// Sinh mã nhóm tài sản tự động
        /// </summary>
        private static async Task<int> SinhMaNhomTSAsync()
        {
            try
            {
                // Lấy danh sách nhóm tài sản hiện có
                var danhSachNhomTS = await LayDanhSachNhomTaiSanAsync();

                // Nếu không có dữ liệu, bắt đầu từ 1
                if (danhSachNhomTS == null || danhSachNhomTS.Count == 0)
                    return 1;

                // Tìm mã lớn nhất trong danh sách
                int maxMa = 0;
                foreach (var nhomTS in danhSachNhomTS)
                {
                    if (nhomTS.MaNhomTS > maxMa)
                        maxMa = nhomTS.MaNhomTS;
                }

                // Trả về mã mới = mã lớn nhất + 1
                return maxMa + 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi sinh mã nhóm tài sản: {ex.Message}");
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
                // Kiểm tra tính hợp lệ của dữ liệu
                if (string.IsNullOrWhiteSpace(nhomTaiSan.TenNhom))
                {
                    throw new ArgumentException("Tên nhóm tài sản không được để trống.");
                }

                // Kiểm tra xem loại tài sản có tồn tại không
                var client = await SupabaseService.GetClientAsync();
                var loaiTaiSan = await client.From<LoaiTaiSan>()
                    .Where(l => l.MaLoaiTaiSan == nhomTaiSan.MaLoaiTaiSan)
                    .Get();

                if (loaiTaiSan.Models.Count == 0)
                {
                    throw new ArgumentException("Loại tài sản không tồn tại.");
                }

                // Sinh mã mới nếu chưa có
                if (nhomTaiSan.MaNhomTS == 0)
                {
                    nhomTaiSan.MaNhomTS = await SinhMaNhomTSAsync();
                }

                // Thực hiện insert
                var response = await client.From<NhomTaiSan>().Insert(nhomTaiSan);

                // Kiểm tra kết quả insert
                if (response != null && response.Models.Count > 0)
                {
                    return response.Models[0];
                }

                throw new Exception("Không thể thêm mới nhóm tài sản!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi thêm Nhóm Tài Sản: {ex.Message}, {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Cập nhật thông tin một Nhóm Tài Sản
        /// </summary>
        public static async Task<NhomTaiSan> CapNhatNhomTaiSanAsync(NhomTaiSan nhomTaiSan)
        {
            try
            {
                // Kiểm tra tính hợp lệ của dữ liệu
                if (nhomTaiSan.MaNhomTS == 0)
                {
                    throw new ArgumentException("Mã nhóm tài sản không hợp lệ.");
                }

                if (string.IsNullOrWhiteSpace(nhomTaiSan.TenNhom))
                {
                    throw new ArgumentException("Tên nhóm tài sản không được để trống.");
                }

                var client = await SupabaseService.GetClientAsync();

                // Kiểm tra xem nhóm tài sản có tồn tại không
                var kiemTraTonTai = await client.From<NhomTaiSan>()
                    .Where(n => n.MaNhomTS == nhomTaiSan.MaNhomTS)
                    .Get();

                if (kiemTraTonTai.Models.Count == 0)
                {
                    throw new ArgumentException("Nhóm tài sản không tồn tại.");
                }

                // Kiểm tra xem loại tài sản có tồn tại không
                var loaiTaiSan = await client.From<LoaiTaiSan>()
                    .Where(l => l.MaLoaiTaiSan == nhomTaiSan.MaLoaiTaiSan)
                    .Get();

                if (loaiTaiSan.Models.Count == 0)
                {
                    throw new ArgumentException("Loại tài sản không tồn tại.");
                }

                // Cập nhật nhóm tài sản
                var response = await client.From<NhomTaiSan>()
                    .Where(n => n.MaNhomTS == nhomTaiSan.MaNhomTS)
                    .Update(nhomTaiSan);

                if (response.Models.Count > 0)
                {
                    return response.Models[0];
                }

                throw new Exception("Không thể cập nhật nhóm tài sản!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi cập nhật Nhóm Tài Sản: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Xóa một Nhóm Tài Sản khỏi Supabase
        /// </summary>
        public static async Task<bool> XoaNhomTaiSanAsync(int maNhomTaiSan)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Kiểm tra xem nhóm tài sản có tồn tại không
                var kiemTraTonTai = await client.From<NhomTaiSan>()
                    .Where(n => n.MaNhomTS == maNhomTaiSan)
                    .Get();

                if (kiemTraTonTai.Models.Count == 0)
                {
                    return false; // Nhóm tài sản không tồn tại
                }

                // Xóa nhóm tài sản
                await client.From<NhomTaiSan>()
                    .Where(n => n.MaNhomTS == maNhomTaiSan)
                    .Delete();

                // Kiểm tra xóa thành công
                var verifyResponse = await client.From<NhomTaiSan>()
                    .Where(n => n.MaNhomTS == maNhomTaiSan)
                    .Get();

                return verifyResponse.Models.Count == 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi xóa Nhóm Tài Sản: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Tạo danh sách NhomTaiSanDTO từ danh sách NhomTaiSan và LoaiTaiSan
        /// </summary>
        public static ObservableCollection<NhomTaiSanDTO> TaoDanhSachDTO(
            ObservableCollection<NhomTaiSan> dsNhomTaiSan,
            ObservableCollection<LoaiTaiSan> dsLoaiTaiSan)
        {
            ObservableCollection<NhomTaiSanDTO> danhSachDTO = new ObservableCollection<NhomTaiSanDTO>();

            foreach (var nhomTaiSan in dsNhomTaiSan)
            {
                // Tìm loại tài sản tương ứng
                var loaiTaiSan = dsLoaiTaiSan.FirstOrDefault(l => l.MaLoaiTaiSan == nhomTaiSan.MaLoaiTaiSan);

                // Tạo DTO với loại tài sản đã tìm được
                var nhomTaiSanDTO = new NhomTaiSanDTO(nhomTaiSan, loaiTaiSan);
                danhSachDTO.Add(nhomTaiSanDTO);
            }

            return danhSachDTO;
        }

        /// <summary>
        /// Phương thức tương thích ngược với code cũ
        /// Kết hợp dữ liệu Nhóm Tài Sản với Loại Tài Sản bằng cách tạo các DTO
        /// </summary>
        public static void KetHopDuLieu(ObservableCollection<LoaiTaiSan> dsLoaiTaiSan, ObservableCollection<NhomTaiSan> dsNhomTaiSan)
        {
            System.Diagnostics.Debug.WriteLine("Phương thức KetHopDuLieu đã được thay thế bằng TaoDanhSachDTO");
        }
    }
}