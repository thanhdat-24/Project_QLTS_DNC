using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Models.QLTaiSan;
using Supabase;

namespace Project_QLTS_DNC.Services.BaoTri
{
    public class DSBaoTriService
    {
        /// <summary>
        /// Lấy danh sách tài sản cần kiểm kê từ cơ sở dữ liệu
        /// </summary>
        public async Task<List<KiemKeTaiSan>> GetKiemKeTaiSanAsync(string tinhTrangFilter = null)
        {
            try
            {
                // Lấy client Supabase từ service
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    throw new Exception("Không thể kết nối Supabase Client");

                // Khởi tạo truy vấn
                var query = client.From<KiemKeTaiSan>().Select("*");

                // Lọc theo tình trạng nếu có
                if (!string.IsNullOrEmpty(tinhTrangFilter))
                {
                    query = query.Filter("tinh_trang", Supabase.Postgrest.Constants.Operator.Equals, tinhTrangFilter);
                }

                // Thực hiện truy vấn để lấy dữ liệu kiểm kê
                var response = await query
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
        public async Task<List<TaiSanModel>> GetDanhSachTaiSanCanKiemTraAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    throw new Exception("Không thể kết nối Supabase Client");

                // Thực hiện truy vấn để lấy tài sản có tình trạng "Cần kiểm tra"
                var response = await client
                    .From<TaiSanModel>()
                    .Select("*")
                    .Filter("tinh_trang_sp", Supabase.Postgrest.Constants.Operator.Equals, "Cần kiểm tra")
                    .Order("ma_tai_san", Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Get();

                System.Diagnostics.Debug.WriteLine($"Số lượng tài sản cần kiểm tra: {response.Models.Count}");
                return response.Models;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi truy vấn dữ liệu tài sản cần kiểm tra: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<TaiSanModel>();
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

                // Áp dụng các điều kiện lọc - bỏ qua lọc theo nhóm tài sản vì trường không tồn tại trong schema
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

                // Lọc theo nhóm tài sản (nếu được chỉ định) trong bộ nhớ sau khi truy vấn
                if (!string.IsNullOrEmpty(nhomTaiSan) && nhomTaiSan != "Tất cả nhóm")
                {
                    // Thực hiện lọc trên bộ nhớ nếu cần
                    // Lưu ý: Điều này yêu cầu thuộc tính TenNhomTS đã được điền từ trước
                    result = result.FindAll(item => item.TenNhomTS == nhomTaiSan);
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

      

        public async Task<bool> CapNhatTaiSanSauBaoTriAsync(KiemKeTaiSan taiSan)
        {
            try
            {
                // Lưu giá trị hiện tại để đề phòng mất dữ liệu - sửa kiểu dữ liệu thành int
                int viTriThucTeBanDau = taiSan.ViTriThucTe;

                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    throw new Exception("Không thể kết nối Supabase Client");

                // Chuyển đổi từ KiemKeTaiSan sang KiemKeTaiSanDb để loại bỏ các thuộc tính NotMapped
                var taiSanDb = KiemKeTaiSanDb.FromKiemKeTaiSan(taiSan);

                // Tiếp tục với các tham số chính xác - đã loại bỏ MaNhomTS
                var parameters = new Dictionary<string, object>
        {
            { "p_ma_kiem_ke_ts", taiSanDb.MaKiemKeTS },
            { "p_ma_dot_kiem_ke", taiSanDb.MaDotKiemKe },
            { "p_ma_tai_san", taiSanDb.MaTaiSan },
            { "p_ma_phong", taiSanDb.MaPhong },
            { "p_tinh_trang", taiSanDb.TinhTrang },
            { "p_vi_tri_thuc_te", taiSanDb.ViTriThucTe }, // Không cần chuyển đổi, đã đúng kiểu
            { "p_ghi_chu", taiSanDb.GhiChu }
        };

                var response = await client.Rpc("cap_nhat_tai_san", parameters);

                if (response.ResponseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Đã cập nhật tài sản sau bảo trì: {taiSan.MaTaiSan}");

                    // Sửa lại điều kiện kiểm tra cho kiểu int
                    // Tuy nhiên, với kiểu int, việc kiểm tra IsNullOrEmpty không còn phù hợp nữa
                    // Thay vào đó, chúng ta có thể kiểm tra giá trị không hợp lệ (ví dụ: 0 hoặc số âm)
                    if (taiSan.ViTriThucTe <= 0 && viTriThucTeBanDau > 0)
                    {
                        taiSan.ViTriThucTe = viTriThucTeBanDau;
                    }

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

                int successCount = 0;
                List<string> errorMessages = new List<string>();

                // Thử xóa trực tiếp từ bảng thay vì gọi procedure
                foreach (var taiSan in danhSachTaiSan)
                {
                    try
                    {
                        // Phương pháp 1: Không gán giá trị từ Delete()
                        await client
                            .From<KiemKeTaiSan>()
                            .Where(x => x.MaKiemKeTS == taiSan.MaKiemKeTS)
                            .Delete();

                        // Kiểm tra việc xóa bằng cách truy vấn lại
                        var checkResponse = await client
                            .From<KiemKeTaiSan>()
                            .Where(x => x.MaKiemKeTS == taiSan.MaKiemKeTS)
                            .Get();

                        if (checkResponse.Models.Count == 0)
                        {
                            successCount++;
                            Console.WriteLine($"Đã xóa tài sản: {taiSan.MaKiemKeTS}");
                        }
                        else
                        {
                            // Phương pháp 2: Nếu Delete không hoạt động, thử gọi RPC procedure
                            var parameters = new Dictionary<string, object>
                            {
                                { "p_ma_kiem_ke_ts", taiSan.MaKiemKeTS }
                            };

                            var rpcResponse = await client.Rpc("xoa_tai_san", parameters);

                            if (rpcResponse.ResponseMessage != null && rpcResponse.ResponseMessage.IsSuccessStatusCode)
                            {
                                // Kiểm tra lại sau khi gọi RPC
                                checkResponse = await client
                                    .From<KiemKeTaiSan>()
                                    .Where(x => x.MaKiemKeTS == taiSan.MaKiemKeTS)
                                    .Get();

                                if (checkResponse.Models.Count == 0)
                                {
                                    successCount++;
                                    Console.WriteLine($"Đã xóa tài sản (qua RPC): {taiSan.MaKiemKeTS}");
                                }
                                else
                                {
                                    string message = $"Không thể xóa tài sản {taiSan.MaKiemKeTS}";
                                    errorMessages.Add(message);
                                    Console.WriteLine(message);
                                }
                            }
                            else
                            {
                                string message = $"Không thể xóa tài sản {taiSan.MaKiemKeTS}";
                                if (rpcResponse.ResponseMessage != null)
                                {
                                    message += $" RPC Status: {rpcResponse.ResponseMessage.StatusCode}";
                                }
                                errorMessages.Add(message);
                                Console.WriteLine(message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = $"Lỗi khi xóa tài sản {taiSan.MaKiemKeTS}: {ex.Message}";
                        errorMessages.Add(message);
                        Console.WriteLine(message);
                    }
                }

                // Ghi log chi tiết để phân tích
                if (errorMessages.Count > 0)
                {
                    string allErrors = string.Join("\n", errorMessages);
                    Console.WriteLine($"Các lỗi xảy ra khi xóa:\n{allErrors}");
                }

                Console.WriteLine($"Đã xóa {successCount}/{danhSachTaiSan.Count} tài sản");

                // Nếu đã xóa ít nhất 1 tài sản, coi như thành công
                return successCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi nghiêm trọng khi xóa nhiều tài sản: {ex.Message}");
                return false;
            }
        }
    }
}