using Project_QLTS_DNC.Models.BaoTri;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Project_QLTS_DNC.Services
{
    public class LichSuBaoTriService
    {
        // Cập nhật phương thức trong LichSuBaoTriService.cs
        public async Task<bool> LuuLichSuHoatDongAsync(string loaiHoatDong, List<PhieuBaoTri> danhSachPhieu, string ghiChu = "")
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    Console.WriteLine("Không thể kết nối Supabase Client");
                    MessageBox.Show("Không thể kết nối đến máy chủ để lưu lịch sử", "Lỗi kết nối",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Lấy thông tin người dùng hiện tại
                string tenNguoiDung = "Người dùng hiện tại";
                int? maNguoiDung = null;

                // Kiểm tra session
                var session = client.Auth.CurrentSession;
                if (session != null)
                {
                    // Lấy thông tin từ session nếu có
                }

                // Lấy ngày giờ hiện tại
                DateTime ngayThucHien = DateTime.Now;

                // Debug: In thông tin phiếu đầu vào
                Console.WriteLine($"Bắt đầu lưu lịch sử cho {danhSachPhieu.Count} tài sản - loại: {loaiHoatDong}");
                Console.WriteLine($"Ghi chú: {ghiChu}");

                // Lưu từng phiếu riêng biệt
                int countSuccess = 0;
                List<Exception> errors = new List<Exception>();

                foreach (var phieu in danhSachPhieu)
                {
                    try
                    {
                        // Đảm bảo phiếu có mã tài sản
                        if (!phieu.MaTaiSan.HasValue)
                        {
                            Console.WriteLine("Bỏ qua phiếu không có mã tài sản");
                            continue;
                        }

                        // Thêm thông tin để dễ dàng tìm kiếm các tài sản thuộc cùng một lần in/xuất
                        string ghiChuTaiSan = string.IsNullOrEmpty(ghiChu)
                            ? $"Tổng số tài sản: {danhSachPhieu.Count}"
                            : $"{ghiChu} | Tổng số tài sản: {danhSachPhieu.Count}";

                        // Chuẩn bị dữ liệu đúng format
                        var lichSu = new LichSuBaoTri
                        {
                            MaTaiSan = phieu.MaTaiSan,
                            TenTaiSan = phieu.TenTaiSan ?? "Không xác định",
                            SoSeri = phieu.SoSeri ?? "Không xác định",
                            LoaiHoatDong = loaiHoatDong,
                            NgayThucHien = ngayThucHien,
                            MaNguoiThucHien = maNguoiDung,
                            TenNguoiThucHien = tenNguoiDung,
                            GhiChu = ghiChuTaiSan
                        };

                        // Ghi log trước khi insert
                        Console.WriteLine($"Chuẩn bị lưu: MaTaiSan={lichSu.MaTaiSan}, TenTaiSan={lichSu.TenTaiSan}, GhiChu={lichSu.GhiChu}");

                        try
                        {
                            // Sử dụng phương thức Insert không có Single
                            var insertResponse = await client.From<LichSuBaoTri>().Insert(lichSu);

                            if (insertResponse != null && insertResponse.Models.Count > 0)
                            {
                                countSuccess++;
                                Console.WriteLine($"Đã lưu lịch sử tài sản ID={phieu.MaTaiSan} thành công");
                            }
                        }
                        catch (Exception pex)
                        {
                            // Xử lý lỗi Postgrest cụ thể
                            Console.WriteLine($"Lỗi khi lưu tài sản {phieu.MaTaiSan}: {pex.Message}");
                            errors.Add(pex);
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex);
                        Console.WriteLine($"Lỗi khi lưu lịch sử tài sản {phieu.MaTaiSan}: {ex.Message}");
                        Console.WriteLine($"Chi tiết: {ex}");
                        // Không dừng vòng lặp, tiếp tục lưu các tài sản khác
                    }
                }

                Console.WriteLine($"Đã lưu thành công {countSuccess}/{danhSachPhieu.Count} tài sản");

                // Hiển thị thông báo lỗi nếu có tài sản không lưu được
                if (errors.Count > 0 && countSuccess < danhSachPhieu.Count)
                {
                    // Phân tích chi tiết lỗi để hiển thị thông báo cụ thể hơn
                    var errorsGrouped = errors.GroupBy(e => e.GetType().Name).ToDictionary(g => g.Key, g => g.Count());
                    string errorDetails = string.Join(", ", errorsGrouped.Select(kvp => $"{kvp.Key}: {kvp.Value}"));

                    string errorMessage = $"Lưu {countSuccess}/{danhSachPhieu.Count} tài sản thành công.\n" +
                                         $"Có {errors.Count} tài sản không lưu được.\n" +
                                         $"Chi tiết lỗi: {errorDetails}";

                    MessageBox.Show(errorMessage, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (countSuccess > 0)
                {
                    MessageBox.Show($"Đã lưu thành công {countSuccess}/{danhSachPhieu.Count} tài sản.",
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Nếu ít nhất một phiếu được lưu thành công
                return countSuccess > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lưu lịch sử bảo trì: {ex.Message}");
                Console.WriteLine($"Chi tiết: {ex}");
                MessageBox.Show($"Có lỗi xảy ra khi lưu lịch sử bảo trì: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Phần còn lại của class giữ nguyên
        // Lấy danh sách lịch sử bảo trì với thông tin số lượng tài sản
        public async Task<List<LichSuBaoTri>> GetLichSuBaoTriAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                var response = await client.From<LichSuBaoTri>()
                    .Order("ngay_thuc_hien", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();

                // Kiểm tra xem có dữ liệu không
                if (response.Models.Count == 0)
                {
                    Console.WriteLine("Không có dữ liệu lịch sử bảo trì nào");
                }
                else
                {
                    Console.WriteLine($"Đã lấy {response.Models.Count} bản ghi lịch sử bảo trì");
                }

                return response.Models;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy danh sách lịch sử bảo trì: {ex.Message}");
                MessageBox.Show($"Lỗi khi lấy lịch sử bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<LichSuBaoTri>();
            }
        }

        // Đếm số lượng tài sản cho mỗi hoạt động theo thời gian
        public async Task<Dictionary<DateTime, int>> CountTaiSanByNgayThucHienAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                var lichSuResponse = await client.From<LichSuBaoTri>().Get();
                var danhSachLichSu = lichSuResponse.Models;

                // Nhóm theo ngày thực hiện và đếm số lượng tài sản
                var result = new Dictionary<DateTime, int>();

                // Nhóm các bản ghi theo ngay_thuc_hien chính xác đến giây
                var groupedByTime = danhSachLichSu.GroupBy(ls => ls.NgayThucHien);

                foreach (var group in groupedByTime)
                {
                    result[group.Key] = group.Count();
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi đếm số lượng tài sản theo hoạt động: {ex.Message}");
                MessageBox.Show($"Lỗi khi đếm số lượng tài sản: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new Dictionary<DateTime, int>();
            }
        }

        // Đếm số lần bảo trì của mỗi tài sản
        public async Task<Dictionary<int, int>> CountBaoTriByTaiSanAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                var phieuBaoTriResponse = await client.From<PhieuBaoTri>().Get();
                var danhSachPhieuBaoTri = phieuBaoTriResponse.Models;

                // Đếm số lần bảo trì cho mỗi tài sản
                var result = new Dictionary<int, int>();

                foreach (var phieu in danhSachPhieuBaoTri)
                {
                    if (phieu.MaTaiSan.HasValue)
                    {
                        int maTaiSan = phieu.MaTaiSan.Value;
                        if (result.ContainsKey(maTaiSan))
                        {
                            result[maTaiSan]++;
                        }
                        else
                        {
                            result[maTaiSan] = 1;
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi đếm số lần bảo trì: {ex.Message}");
                MessageBox.Show($"Lỗi khi đếm số lần bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new Dictionary<int, int>();
            }
        }
    }
}