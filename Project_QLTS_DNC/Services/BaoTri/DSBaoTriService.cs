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

        /// <summary>
        /// Cập nhật tài sản sau khi bảo trì
        /// </summary>
        public async Task<bool> CapNhatTaiSanSauBaoTriAsync(KiemKeTaiSan taiSan)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    throw new Exception("Không thể kết nối Supabase Client");

                // Cập nhật thông tin tài sản sau khi bảo trì
                var response = await client
                    .From<KiemKeTaiSan>()
                    .Filter("ma_kiem_ke_ts", Supabase.Postgrest.Constants.Operator.Equals, taiSan.MaKiemKeTS)
                    .Update(taiSan);

                // Kiểm tra kết quả
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
    }
}