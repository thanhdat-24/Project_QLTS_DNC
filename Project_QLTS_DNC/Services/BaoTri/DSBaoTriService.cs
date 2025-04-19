using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.BaoTri;
using Supabase;

namespace Project_QLTS_DNC.Services.BaoTri
{
    public class DSBaoTriService
    {
        /// <summary>
        /// Lấy danh sách tài sản cần kiểm kê từ cơ sở dữ liệu
        /// </summary>
        public async Task<List<KiemKeTaiSan>> GetKiemKeTaiSanAsync()
        {
            try
            {
                // Lấy client Supabase từ service
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    throw new Exception("Không thể kết nối Supabase Client");

                // Thực hiện truy vấn để lấy dữ liệu
                var response = await client
                    .From<KiemKeTaiSan>()
                    .Select("*")
                    .Order("ma_tai_san", Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Get();

                Console.WriteLine($"Số lượng tài sản kiểm kê: {response.Models.Count}");
                return response.Models;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi truy vấn dữ liệu kiểm kê tài sản: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<KiemKeTaiSan>();
            }
        }

        /// <summary>
        /// Lấy danh sách tài sản cần bảo trì dựa trên điều kiện lọc
        /// </summary>
        /// <param name="nhomTaiSan">Nhóm tài sản (null = tất cả)</param>
        /// <param name="loaiBaoTri">Loại bảo trì (null = tất cả)</param>
        /// <param name="tinhTrang">Tình trạng tài sản (null = tất cả)</param>
        public async Task<List<KiemKeTaiSan>> GetDSTaiSanCanBaoTriAsync(string nhomTaiSan = null, string loaiBaoTri = null, string tinhTrang = null)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    throw new Exception("Không thể kết nối Supabase Client");

                // Khởi tạo truy vấn
                var query = client.From<KiemKeTaiSan>().Select("*");

                // Áp dụng các điều kiện lọc
                if (!string.IsNullOrEmpty(nhomTaiSan) && nhomTaiSan != "Tất cả nhóm")
                {
                    query = query.Filter("nhom_tai_san", Supabase.Postgrest.Constants.Operator.Equals, nhomTaiSan);
                }

                if (!string.IsNullOrEmpty(loaiBaoTri) && loaiBaoTri != "Tất cả loại")
                {
                    query = query.Filter("loai_bao_tri", Supabase.Postgrest.Constants.Operator.Equals, loaiBaoTri);
                }

                // Thực hiện truy vấn
                var response = await query.Order("ma_tai_san", Supabase.Postgrest.Constants.Ordering.Ascending).Get();

                // Áp dụng bộ lọc tình trạng sau khi đã lấy dữ liệu (vì cần xử lý giá trị %)
                var result = response.Models;
                if (!string.IsNullOrEmpty(tinhTrang))
                {
                    List<KiemKeTaiSan> filteredList = new List<KiemKeTaiSan>();
                    foreach (var item in result)
                    {
                        if (int.TryParse(item.TinhTrang?.Replace("%", ""), out int tinhTrangValue))
                        {
                            if (tinhTrang == "Dưới 50%" && tinhTrangValue < 50)
                                filteredList.Add(item);
                            else if (tinhTrang == "Trên 50%" && tinhTrangValue >= 50)
                                filteredList.Add(item);
                        }
                    }
                    result = filteredList;
                }

                Console.WriteLine($"Số lượng tài sản cần bảo trì: {result.Count}");
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi truy vấn dữ liệu tài sản cần bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<KiemKeTaiSan>();
            }
        }

        /// <summary>
        /// Tạo phiếu bảo trì mới
        /// </summary>
        public async Task<bool> TaoPhieuBaoTriAsync(PhieuBaoTri phieuBaoTri)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    throw new Exception("Không thể kết nối Supabase Client");

                // Thêm phiếu bảo trì mới
                var response = await client.From<PhieuBaoTri>().Insert(phieuBaoTri);

                // Kiểm tra kết quả
                if (response.ResponseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Đã tạo phiếu bảo trì: {phieuBaoTri.MaBaoTri}");
                    return true;
                }
                else
                {
                    throw new Exception($"Lỗi khi tạo phiếu bảo trì: {response.ResponseMessage.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo phiếu bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Tạo nhiều phiếu bảo trì cùng lúc
        /// </summary>
        public async Task<bool> TaoNhieuPhieuBaoTriAsync(List<PhieuBaoTri> danhSachPhieu)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    throw new Exception("Không thể kết nối Supabase Client");

                // Đảm bảo không có trường ngoài schema trong danhSachPhieu
                foreach (var phieu in danhSachPhieu)
                {
                    // Nếu có các trường bổ sung không thuộc schema, hãy xóa chúng ở đây
                    // phieu.TruongBoSung = null;
                }

                // Thêm nhiều phiếu bảo trì
                var response = await client.From<PhieuBaoTri>().Insert(danhSachPhieu);

                // Kiểm tra kết quả
                if (response.ResponseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Đã tạo {danhSachPhieu.Count} phiếu bảo trì");
                    return true;
                }
                else
                {
                    throw new Exception($"Lỗi khi tạo phiếu bảo trì: {response.ResponseMessage.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo nhiều phiếu bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> CapNhatTaiSanSauBaoTriAsync(KiemKeTaiSan taiSan)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    throw new Exception("Không thể kết nối Supabase Client");

                // Chuyển đổi từ KiemKeTaiSan sang KiemKeTaiSanDb để loại bỏ các thuộc tính NotMapped
                var taiSanDb = KiemKeTaiSanDb.FromKiemKeTaiSan(taiSan);

                // Tiếp tục với các tham số chính xác
                var parameters = new Dictionary<string, object>
        {
            { "p_ma_kiem_ke_ts", taiSanDb.MaKiemKeTS },
            { "p_ma_dot_kiem_ke", taiSanDb.MaDotKiemKe },
            { "p_ma_tai_san", taiSanDb.MaTaiSan },
            { "p_ma_phong", taiSanDb.MaPhong },
            { "p_tinh_trang", taiSanDb.TinhTrang },
            { "p_vi_tri_thuc_te", taiSanDb.ViTriThucTe },
            { "p_ghi_chu", taiSanDb.GhiChu }
        };

                var response = await client.Rpc("cap_nhat_tai_san", parameters);

                if (response.ResponseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Đã cập nhật tài sản sau bảo trì: {taiSan.MaTaiSan}");
                    return true;
                }
                else
                {
                    throw new Exception($"Lỗi khi cập nhật tài sản: {response.ResponseMessage.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật tài sản sau bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách lịch sử bảo trì của một tài sản
        /// </summary>
        public async Task<List<PhieuBaoTri>> GetLichSuBaoTriAsync(int maTaiSan)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    throw new Exception("Không thể kết nối Supabase Client");

                // Thực hiện truy vấn để lấy lịch sử bảo trì
                var response = await client
                    .From<PhieuBaoTri>()
                    .Filter("ma_tai_san", Supabase.Postgrest.Constants.Operator.Equals, maTaiSan)
                    .Order("ngay_bao_tri", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();

                return response.Models;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi truy vấn lịch sử bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<PhieuBaoTri>();
            }
        }
        /// <summary>
        /// Xóa một tài sản kiểm kê
        /// </summary>
        public async Task<bool> XoaTaiSanAsync(KiemKeTaiSan taiSan)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    throw new Exception("Không thể kết nối Supabase Client");

                // Gọi hàm SQL trên Supabase
                var parameters = new Dictionary<string, object>
        {
            { "p_ma_kiem_ke_ts", taiSan.MaKiemKeTS }
        };

                var response = await client.Rpc("xoa_tai_san", parameters);

                // Kiểm tra trạng thái HTTP
                if (response.ResponseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Đã xóa tài sản: {taiSan.MaKiemKeTS}");
                    return true;
                }
                else
                {
                    throw new Exception($"Lỗi khi xóa tài sản: {response.ResponseMessage.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa tài sản: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        /// <summary>
        /// Xóa nhiều tài sản cùng lúc
        /// </summary>
        public async Task<bool> XoaNhieuTaiSanAsync(List<KiemKeTaiSan> danhSachTaiSan)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    throw new Exception("Không thể kết nối Supabase Client");

                bool allSuccess = true;
                foreach (var taiSan in danhSachTaiSan)
                {
                    try
                    {
                        var parameters = new Dictionary<string, object>
                {
                    { "p_ma_kiem_ke_ts", taiSan.MaKiemKeTS }
                };

                        var response = await client.Rpc("xoa_tai_san", parameters);

                        if (!response.ResponseMessage.IsSuccessStatusCode)
                        {
                            allSuccess = false;
                            Console.WriteLine($"Lỗi khi xóa tài sản {taiSan.MaKiemKeTS}: {response.ResponseMessage.ReasonPhrase}");
                        }
                        else
                        {
                            Console.WriteLine($"Đã xóa tài sản: {taiSan.MaKiemKeTS}");
                        }
                    }
                    catch (Exception ex)
                    {
                        allSuccess = false;
                        Console.WriteLine($"Lỗi khi xóa tài sản {taiSan.MaKiemKeTS}: {ex.Message}");
                    }
                }

                Console.WriteLine($"Đã xóa {danhSachTaiSan.Count} tài sản, trạng thái thành công: {allSuccess}");
                return allSuccess;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa nhiều tài sản: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}