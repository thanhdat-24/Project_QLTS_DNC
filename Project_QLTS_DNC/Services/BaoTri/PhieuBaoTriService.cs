﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Models.NhanVien;
using System.IO;
using ClosedXML.Excel;
using System.Windows;
using Supabase.Gotrue;
using Supabase;
using Newtonsoft.Json;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using Project_QLTS_DNC.Helpers;

namespace Project_QLTS_DNC.Services
{
    public class PhieuBaoTriService
    {
        // Trong PhieuBaoTriService.cs
        public async Task<List<PhieuBaoTri>> GetPhieuBaoTriAsync()
        {
            try
            {
                Console.WriteLine("Bắt đầu kết nối Supabase...");
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    Console.WriteLine("Lỗi: client là null");
                    throw new Exception("Không thể kết nối Supabase Client");
                }
                Console.WriteLine("Kết nối Supabase thành công, bắt đầu truy vấn...");

                // Lấy danh sách phiếu bảo trì
                var response = await client.From<PhieuBaoTri>().Get();
                Console.WriteLine($"Nhận được phản hồi: {response != null}");
                var danhSachPhieu = response.Models;
                Console.WriteLine($"Số lượng phiếu: {danhSachPhieu?.Count ?? 0}");

                return danhSachPhieu;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi chi tiết: {ex.Message}");
                Console.WriteLine($"Loại lỗi: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                MessageBox.Show($"Lỗi khi truy vấn dữ liệu: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<PhieuBaoTri>();
            }
        }

        // Lấy thông tin một phiếu bảo trì theo mã
        public async Task<PhieuBaoTri> GetPhieuBaoTriByIdAsync(int maBaoTri)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                var response = await client.From<PhieuBaoTri>()
                    .Where(p => p.MaBaoTri == maBaoTri)
                    .Single();

                return response;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi truy vấn dữ liệu: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
        // Thêm phiếu bảo trì mới
        public async Task<bool> AddPhieuBaoTriAsync(PhieuBaoTri phieuBaoTri)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                Console.WriteLine($"Service: Thêm mới phiếu bảo trì với mã {phieuBaoTri.MaBaoTri}");

                // Tạo object chỉ với các trường cần thêm
                var insertData = new
                {
                    ma_tai_san = phieuBaoTri.MaTaiSan,
                    ma_loai_bao_tri = phieuBaoTri.MaLoaiBaoTri,
                    ngay_bao_tri = phieuBaoTri.NgayBaoTri,
                    ma_nv = phieuBaoTri.MaNV,
                    noi_dung = phieuBaoTri.NoiDung,
                    trang_thai_sau_bao_tri = phieuBaoTri.TrangThai,
                    chi_phi = phieuBaoTri.ChiPhi,
                    ghi_chu = phieuBaoTri.GhiChu
                };

                // Chuyển đổi sang JSON
                string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(insertData);

                // Lấy thông tin kết nối từ ConfigHelper hoặc từ một nguồn khác
                var settings = ConfigHelper.GetSupabaseSettings();
                string apiUrl = settings.Url;
                string apiKey = settings.ApiKey;

                // Thực hiện HTTP request trực tiếp
                string url = $"{apiUrl}/rest/v1/baotri";
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                    httpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");

                    StringContent content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Thêm mới thành công");
                        return true;
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Lỗi thêm mới: {errorContent}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi thêm mới phiếu bảo trì: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        // Cập nhật phiếu bảo trì
        public async Task<bool> UpdatePhieuBaoTriAsync(PhieuBaoTri phieuBaoTri)
        {
            try
            {
                Console.WriteLine($"Bắt đầu cập nhật phiếu bảo trì: {phieuBaoTri.MaBaoTri}");
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                // Lấy thông tin kết nối từ ConfigHelper hoặc từ một nguồn khác
                var settings = ConfigHelper.GetSupabaseSettings();
                string apiUrl = settings.Url;
                string apiKey = settings.ApiKey;

                // Tạo object chỉ với các trường cần cập nhật
                var updateData = new
                {
                    ma_tai_san = phieuBaoTri.MaTaiSan,
                    ma_loai_bao_tri = phieuBaoTri.MaLoaiBaoTri,
                    ngay_bao_tri = phieuBaoTri.NgayBaoTri,
                    ma_nv = phieuBaoTri.MaNV,
                    noi_dung = phieuBaoTri.NoiDung,
                    trang_thai_sau_bao_tri = phieuBaoTri.TrangThai,
                    chi_phi = phieuBaoTri.ChiPhi,
                    ghi_chu = phieuBaoTri.GhiChu
                };

                // Chuyển đổi sang JSON
                string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(updateData);

                // Thực hiện HTTP request trực tiếp
                string url = $"{apiUrl}/rest/v1/baotri?ma_bao_tri=eq.{phieuBaoTri.MaBaoTri}";

                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                    httpClient.DefaultRequestHeaders.Add("Prefer", "return=minimal");

                    StringContent content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");

                    var response = await httpClient.PatchAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Cập nhật thành công");
                        return true;
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Lỗi cập nhật: {errorContent}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi cập nhật phiếu bảo trì: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                MessageBox.Show($"Lỗi cập nhật: {ex.Message}", "Chi tiết lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Xóa phiếu bảo trì
        public async Task<bool> DeletePhieuBaoTriAsync(int maBaoTri)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                // Thực hiện xóa mà không gán vào biến
                await client.From<PhieuBaoTri>()
                    .Where(p => p.MaBaoTri == maBaoTri)
                    .Delete();
                return true; // hoặc xác nhận xóa thành công bằng cách khác

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa phiếu bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Lấy danh sách tài sản cần bảo trì
        public async Task<List<PhieuBaoTri>> GetDanhSachCanBaoTriAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                // Lấy danh sách tài sản có tình trạng cần bảo trì
                var danhSachTaiSan = await client.From<TaiSanModel>()
                    .Where(t => t.TinhTrangSP.Contains("Cần sửa chữa") || t.TinhTrangSP.Contains("Kém") || t.TinhTrangSP.Contains("hỏng"))
                    .Get();

                // Chuyển đổi sang các phiếu bảo trì mới
                var danhSachPhieuDeXuat = new List<PhieuBaoTri>();
                foreach (var taiSan in danhSachTaiSan.Models)
                {
                    danhSachPhieuDeXuat.Add(new PhieuBaoTri
                    {
                        MaTaiSan = taiSan.MaTaiSan,
                        MaLoaiBaoTri = 2, // Đột xuất
                        NgayBaoTri = DateTime.Now,
                        TrangThai = taiSan.TinhTrangSP,
                        NoiDung = $"Bảo trì tài sản {taiSan.TenTaiSan} - Mã {taiSan.MaTaiSan}",
                        ChiPhi = 0,
                        GhiChu = $"Tình trạng hiện tại: {taiSan.TinhTrangSP}"
                    });
                }

                return danhSachPhieuDeXuat;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lấy danh sách tài sản cần bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<PhieuBaoTri>();
            }
        }
        // Tìm kiếm phiếu bảo trì theo từ khóa
        public async Task<List<PhieuBaoTri>> SearchPhieuBaoTriAsync(string keyword)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return await GetPhieuBaoTriAsync();
                }

                // Lấy tất cả phiếu bảo trì
                var responsePhieuBaoTri = await client.From<PhieuBaoTri>().Get();
                var danhSachPhieu = responsePhieuBaoTri.Models;

                // Lấy thông tin tài sản để tìm theo tên tài sản
                var responseTaiSan = await client.From<TaiSanModel>().Get();
                var danhSachTaiSan = responseTaiSan.Models;

                // Lấy thông tin loại bảo trì để tìm theo tên loại bảo trì
                var responseLoaiBaoTri = await client.From<LoaiBaoTri>().Get();
                var danhSachLoaiBaoTri = responseLoaiBaoTri.Models;

                // Lọc dữ liệu theo từ khóa
                return danhSachPhieu.Where(p =>
                    p.MaBaoTri.ToString().Contains(keyword) ||
                    (p.MaTaiSan != null && p.MaTaiSan.ToString().Contains(keyword)) ||
                    (p.MaLoaiBaoTri != null && p.MaLoaiBaoTri.ToString().Contains(keyword)) ||
                    (p.NoiDung != null && p.NoiDung.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (p.GhiChu != null && p.GhiChu.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (p.TrangThai != null && p.TrangThai.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    // Tìm theo tên tài sản
                    (p.MaTaiSan != null && danhSachTaiSan.Any(ts =>
                        ts.MaTaiSan == p.MaTaiSan &&
                        ts.TenTaiSan != null &&
                        ts.TenTaiSan.Contains(keyword, StringComparison.OrdinalIgnoreCase))) ||
                    // Tìm theo tên loại bảo trì
                    (p.MaLoaiBaoTri != null && danhSachLoaiBaoTri.Any(lbt =>
                        lbt.MaLoaiBaoTri == p.MaLoaiBaoTri &&
                        lbt.TenLoai != null &&
                        lbt.TenLoai.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                ).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm dữ liệu: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<PhieuBaoTri>();
            }
        }

        // Lọc phiếu bảo trì theo loại và trạng thái
        public async Task<List<PhieuBaoTri>> GetPhieuBaoTriTheoLoaiTrangThaiAsync(int? maLoaiBaoTri, string trangThai)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                // Lấy tất cả phiếu bảo trì
                var response = await client.From<PhieuBaoTri>().Get();
                var danhSachPhieu = response.Models;

                // Lọc theo loại bảo trì
                if (maLoaiBaoTri.HasValue)
                {
                    danhSachPhieu = danhSachPhieu.Where(p => p.MaLoaiBaoTri == maLoaiBaoTri).ToList();
                }

                // Lọc theo trạng thái
                if (!string.IsNullOrEmpty(trangThai) && trangThai != "Tất cả trạng thái")
                {
                    danhSachPhieu = danhSachPhieu.Where(p => p.TrangThai == trangThai).ToList();
                }

                return danhSachPhieu;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lọc dữ liệu: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<PhieuBaoTri>();
            }
        }

        
        public async Task<int> GetMaxMaBaoTriAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                Console.WriteLine("Service: Đang lấy mã bảo trì lớn nhất...");

                // Cách 1: Sử dụng Single() để lấy giá trị lớn nhất
                var response = await client.From<PhieuBaoTri>()
                                          .Select("ma_bao_tri")
                                          .Order("ma_bao_tri", Supabase.Postgrest.Constants.Ordering.Descending)
                                          .Limit(1)
                                          .Get();

                if (response?.Models != null && response.Models.Any())
                {
                    int maxId = response.Models.First().MaBaoTri;
                    Console.WriteLine($"Service: Mã bảo trì lớn nhất hiện tại: {maxId}");
                    return maxId;
                }

                Console.WriteLine("Service: Chưa có bản ghi nào, trả về mã 0");
                return 0; // Trường hợp không có phiếu bảo trì nào
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Service: Lỗi khi lấy mã bảo trì lớn nhất: {ex.Message}");
                Console.WriteLine($"Service: Stack Trace: {ex.StackTrace}");
                return 0; // Trả về 0 để không làm gián đoạn quá trình thêm
            }
        }

        // Xuất dữ liệu ra Excel
        public bool ExportToExcel(List<PhieuBaoTri> danhSachPhieu, string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Phiếu Bảo Trì");

                    // Tạo tiêu đề
                    worksheet.Cell("A1").Value = "DANH SÁCH PHIẾU BẢO TRÌ";
                    worksheet.Range("A1:I1").Merge();
                    worksheet.Range("A1:I1").Style.Font.Bold = true;
                    worksheet.Range("A1:I1").Style.Font.FontSize = 16;
                    worksheet.Range("A1:I1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Tạo header cho bảng
                    worksheet.Cell("A3").Value = "STT";
                    worksheet.Cell("B3").Value = "Mã bảo trì";
                    worksheet.Cell("C3").Value = "Tên tài sản";
                    worksheet.Cell("D3").Value = "Loại bảo trì";
                    worksheet.Cell("E3").Value = "Ngày bảo trì";
                    worksheet.Cell("F3").Value = "Nhân viên phụ trách";
                    worksheet.Cell("G3").Value = "Nội dung";
                    worksheet.Cell("H3").Value = "Trạng thái";
                    worksheet.Cell("I3").Value = "Chi phí";

                    // Định dạng header
                    var headerRange = worksheet.Range("A3:I3");
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Biến để tính tổng chi phí
                    decimal tongChiPhi = 0;

                    // Điền dữ liệu vào bảng
                    for (int i = 0; i < danhSachPhieu.Count; i++)
                    {
                        var phieu = danhSachPhieu[i];
                        int row = i + 4;
                        worksheet.Cell(row, 1).Value = i + 1;
                        worksheet.Cell(row, 2).Value = phieu.MaBaoTri;

                        // Sử dụng TenTaiSan thay vì MaTaiSan
                        worksheet.Cell(row, 3).Value = !string.IsNullOrEmpty(phieu.TenTaiSan) ? phieu.TenTaiSan : "N/A";

                        // Sử dụng TenLoaiBaoTri thay vì mã và phần switch
                        worksheet.Cell(row, 4).Value = !string.IsNullOrEmpty(phieu.TenLoaiBaoTri) ? phieu.TenLoaiBaoTri : "Không xác định";

                        worksheet.Cell(row, 5).Value = phieu.NgayBaoTri;
                        worksheet.Cell(row, 5).Style.DateFormat.Format = "dd/MM/yyyy";

                        // Sử dụng TenNhanVien thay vì MaNV
                        worksheet.Cell(row, 6).Value = !string.IsNullOrEmpty(phieu.TenNhanVien) ? phieu.TenNhanVien : "N/A";

                        worksheet.Cell(row, 7).Value = phieu.NoiDung;
                        worksheet.Cell(row, 8).Value = phieu.TrangThai;
                        worksheet.Cell(row, 9).Value = phieu.ChiPhi;

                        // Cộng dồn chi phí - kiểm tra và xử lý giá trị null
                        if (phieu.ChiPhi.HasValue)
                        {
                            tongChiPhi += phieu.ChiPhi.Value;
                        }

                        // Định dạng ô
                        var rowRange = worksheet.Range(row, 1, row, 9);
                        rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    }

                    // Thêm dòng tổng chi phí
                    int tongRow = danhSachPhieu.Count + 4;
                    worksheet.Cell(tongRow, 8).Value = "TỔNG CHI PHÍ:";
                    worksheet.Cell(tongRow, 8).Style.Font.Bold = true;
                    worksheet.Cell(tongRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    worksheet.Cell(tongRow, 9).Value = tongChiPhi;
                    worksheet.Cell(tongRow, 9).Style.Font.Bold = true;
                    worksheet.Cell(tongRow, 9).Style.NumberFormat.Format = "#,##0";

                    var totalRange = worksheet.Range(tongRow, 8, tongRow, 9);
                    totalRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    totalRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                    // Định dạng cột
                    worksheet.Columns().AdjustToContents();

                    // Thêm ngày xuất báo cáo
                    int lastRow = danhSachPhieu.Count + 4 + 2;
                    worksheet.Cell(lastRow, 7).Value = "Ngày xuất báo cáo:";
                    worksheet.Cell(lastRow, 8).Value = DateTime.Now;
                    worksheet.Cell(lastRow, 8).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
                    worksheet.Range(lastRow, 7, lastRow, 8).Style.Font.Italic = true;

                    // Lưu workbook
                    workbook.SaveAs(filePath);

                    MessageBox.Show("Xuất Excel thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Mở file sau khi xuất
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất Excel: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Lấy tên tài sản theo mã
        public async Task<string> GetTenTaiSanAsync(int maTaiSan)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                var taiSan = await client.From<TaiSanModel>()
                    .Where(t => t.MaTaiSan == maTaiSan)
                    .Single();

                return taiSan?.TenTaiSan ?? "Không xác định";
            }
            catch
            {
                return "Không xác định";
            }
        }

        // Lấy tên nhân viên theo mã
        public async Task<string> GetTenNhanVienAsync(int maNV)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                var nhanVien = await client.From<NhanVienModel>()
                    .Where(nv => nv.MaNV == maNV)
                    .Single();

                return nhanVien?.TenNV ?? "Không xác định";
            }
            catch
            {
                return "Không xác định";
            }
        }
    }
}