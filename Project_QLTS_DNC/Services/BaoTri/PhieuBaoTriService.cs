using System;
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

        // Thêm mới phiếu bảo trì
        public async Task<bool> AddPhieuBaoTriAsync(PhieuBaoTri phieuBaoTri)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                // Đảm bảo người dùng đã đăng nhập và token còn hiệu lực
                var session = client.Auth.CurrentSession;
                if (session == null || DateTime.Compare(session.ExpiresAt(), DateTime.UtcNow) < 0)
                {
                    // Thử làm mới token nếu hết hạn
                    try
                    {
                        await client.Auth.RefreshSession();
                    }
                    catch
                    {
                        throw new Exception("Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại");
                    }
                }

                // Kiểm tra các giá trị bắt buộc
                if (phieuBaoTri.MaTaiSan == null)
                {
                    throw new Exception("Mã tài sản không được để trống");
                }
                if (string.IsNullOrEmpty(phieuBaoTri.NoiDung))
                {
                    throw new Exception("Nội dung không được để trống");
                }
                if (string.IsNullOrEmpty(phieuBaoTri.TrangThai))
                {
                    throw new Exception("Trạng thái không được để trống");
                }

                Console.WriteLine($"Đang thêm phiếu bảo trì mới: {phieuBaoTri.MaBaoTri}");

                // Thực hiện thêm mới
                var response = await client.From<PhieuBaoTri>().Insert(phieuBaoTri);

                return response != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chi tiết lỗi thêm phiếu: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                MessageBox.Show($"Lỗi khi thêm phiếu bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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

                // Tạo phiên bản mới của phiếu bảo trì để cập nhật
                var updatePhieu = new PhieuBaoTri
                {
                    MaBaoTri = phieuBaoTri.MaBaoTri,
                    MaTaiSan = phieuBaoTri.MaTaiSan,
                    MaLoaiBaoTri = phieuBaoTri.MaLoaiBaoTri,
                    NgayBaoTri = phieuBaoTri.NgayBaoTri,
                    MaNV = phieuBaoTri.MaNV,
                    NoiDung = phieuBaoTri.NoiDung,
                    TrangThai = phieuBaoTri.TrangThai,
                    ChiPhi = phieuBaoTri.ChiPhi,
                    GhiChu = phieuBaoTri.GhiChu
                };

                Console.WriteLine($"Dữ liệu cập nhật: MaTaiSan={updatePhieu.MaTaiSan}, TrangThai={updatePhieu.TrangThai}");

                // Thực hiện cập nhật
                var response = await client.From<PhieuBaoTri>()
                    .Where(p => p.MaBaoTri == phieuBaoTri.MaBaoTri)
                    .Update(updatePhieu);

                Console.WriteLine($"Kết quả phản hồi: {(response != null ? "Thành công" : "Thất bại")}");
                return response != null;
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

                // Tìm kiếm phiếu bảo trì theo một số trường
                var response = await client.From<PhieuBaoTri>().Get();
                var danhSachPhieu = response.Models;

                // Lọc dữ liệu theo từ khóa
                return danhSachPhieu.Where(p =>
                    p.MaBaoTri.ToString().Contains(keyword) ||
                    (p.MaTaiSan != null && p.MaTaiSan.ToString().Contains(keyword)) ||
                    (p.NoiDung != null && p.NoiDung.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (p.GhiChu != null && p.GhiChu.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (p.TrangThai != null && p.TrangThai.Contains(keyword, StringComparison.OrdinalIgnoreCase))
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

        // Lấy mã phiếu bảo trì lớn nhất để tạo mã mới
        // Lấy mã phiếu bảo trì lớn nhất để tạo mã mới
        public async Task<int> GetMaxMaBaoTriAsync()
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
                if (response.Models.Count == 0)
                {
                    return 0; // Trả về 0 nếu không có phiếu bảo trì nào
                }
                // Tìm mã phiếu bảo trì lớn nhất
                int maxMaBaoTri = response.Models.Max(p => p.MaBaoTri);
                return maxMaBaoTri;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lấy mã phiếu bảo trì lớn nhất: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
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
                    worksheet.Cell("C3").Value = "Mã tài sản";
                    worksheet.Cell("D3").Value = "Loại bảo trì";
                    worksheet.Cell("E3").Value = "Ngày bảo trì";
                    worksheet.Cell("F3").Value = "Mã nhân viên";
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

                    // Điền dữ liệu vào bảng
                    for (int i = 0; i < danhSachPhieu.Count; i++)
                    {
                        var phieu = danhSachPhieu[i];
                        int row = i + 4;
                        worksheet.Cell(row, 1).Value = i + 1;
                        worksheet.Cell(row, 2).Value = phieu.MaBaoTri;
                        worksheet.Cell(row, 3).Value = phieu.MaTaiSan?.ToString() ?? "N/A";

                        // Chuyển đổi mã loại bảo trì thành tên
                        string loaiBaoTri = "Không xác định";
                        switch (phieu.MaLoaiBaoTri)
                        {
                            case 1: loaiBaoTri = "Định kỳ"; break;
                            case 2: loaiBaoTri = "Đột xuất"; break;
                            case 3: loaiBaoTri = "Bảo hành"; break;
                        }
                        worksheet.Cell(row, 4).Value = loaiBaoTri;

                        worksheet.Cell(row, 5).Value = phieu.NgayBaoTri;
                        worksheet.Cell(row, 5).Style.DateFormat.Format = "dd/MM/yyyy";
                        worksheet.Cell(row, 6).Value = phieu.MaNV?.ToString() ?? "N/A";
                        worksheet.Cell(row, 7).Value = phieu.NoiDung;
                        worksheet.Cell(row, 8).Value = phieu.TrangThai;
                        worksheet.Cell(row, 9).Value = phieu.ChiPhi;

                        // Định dạng ô
                        var rowRange = worksheet.Range(row, 1, row, 9);
                        rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    }

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