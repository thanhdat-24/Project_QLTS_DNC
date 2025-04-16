using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models.BaoTri;
using Supabase.Postgrest.Responses;
using System.IO;
using System.IO;
using ClosedXML.Excel;
using System.Windows;


namespace Project_QLTS_DNC.Services
{
    public class PhieuBaoTriService
    {
        // Fix for CS0308: Remove the generic type argument from the Get() method
        // Fix for CS0246: Replace 'ModeledResponse<>' with the appropriate type or remove it if unnecessary

        public async Task<List<PhieuBaoTri>> GetPhieuBaoTriAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                // Fix for CS0815: Explicitly specify the type of the response
                var response = await client.From<PhieuBaoTri>().Get(); // Ensure the method returns a ModeledResponse<PhieuBaoTri>

                // Ensure the response is properly handled
                return response.Models;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi truy vấn dữ liệu: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
                System.Windows.MessageBox.Show($"Lỗi khi truy vấn dữ liệu: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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

                // Thực hiện thêm mới
                var response = await client.From<PhieuBaoTri>().Insert(phieuBaoTri);
                return response.ResponseMessage.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi thêm phiếu bảo trì: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> UpdatePhieuBaoTriAsync(PhieuBaoTri phieuBaoTri)
        {
            try
            {
                Console.WriteLine($"Bắt đầu cập nhật phiếu bảo trì: {phieuBaoTri.MaBaoTri}");
                var client = await SupabaseService.GetClientAsync();

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

                // Thực hiện cập nhật trực tiếp đối tượng PhieuBaoTri
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

                // Hiển thị thông báo lỗi chi tiết
                MessageBox.Show($"Lỗi cập nhật: {ex.Message}", "Chi tiết lỗi", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }
        }
        public async Task<List<PhieuBaoTri>> GetDanhSachCanBaoTriAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                // Thực hiện truy vấn SQL tùy chỉnh để lấy danh sách tài sản cần bảo trì
                // Giả sử có một cột tình trạng tài sản trong cơ sở dữ liệu
                // Đây là ví dụ, bạn cần điều chỉnh theo cấu trúc cơ sở dữ liệu thực tế của bạn
                var response = await client.Rpc("get_tai_san_can_bao_tri", new Dictionary<string, object>
                {
                    { "tinh_trang_threshold", 50 }
                });

                if (response == null)
                {
                    throw new Exception("Không có dữ liệu trả về");
                }

                // Replace the problematic line with the following:
                var result = JsonSerializer.Deserialize<List<PhieuBaoTri>>(response.Content);
                return result;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi lấy danh sách tài sản cần bảo trì: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<PhieuBaoTri>();
            }
        }

        // Fix for CS0266: Explicitly cast the query result to the appropriate type
        public async Task<List<PhieuBaoTri>> GetPhieuBaoTriTheoLoaiTrangThaiAsync(int? maLoaiBaoTri, string trangThai)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                // Explicitly cast the query to the appropriate type
                var query = (Supabase.Postgrest.Interfaces.IPostgrestTable<PhieuBaoTri>)client.From<PhieuBaoTri>();

                // Áp dụng bộ lọc theo loại bảo trì
                if (maLoaiBaoTri.HasValue)
                {
                    query = query.Where(p => p.MaLoaiBaoTri == maLoaiBaoTri);
                }

                // Áp dụng bộ lọc theo trạng thái
                if (!string.IsNullOrEmpty(trangThai) && trangThai != "Tất cả trạng thái")
                {
                    query = query.Where(p => p.TrangThai == trangThai);
                }

                // Thực hiện truy vấn
                var response = await query.Get();
                return response.Models;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi truy vấn dữ liệu: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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

                // Lấy tất cả dữ liệu và thực hiện lọc client-side nếu từ khóa được cung cấp
                var response = await client.From<PhieuBaoTri>().Get();

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return response.Models;
                }

                // Lọc dữ liệu theo từ khóa
                return response.Models.FindAll(p =>
                    p.MaBaoTri.ToString().Contains(keyword) ||
                    p.MaTaiSan?.ToString().Contains(keyword) == true ||
                    p.NoiDung?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true ||
                    p.GhiChu?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true
                );
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi tìm kiếm dữ liệu: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<PhieuBaoTri>();
            }
        }

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
                System.Windows.MessageBox.Show($"Lỗi khi lấy mã phiếu bảo trì lớn nhất: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return 0;
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

                // Fix for CS0815: Explicitly specify the type of the response
                var response = await client.From<PhieuBaoTri>().Get(); // Ensure the method returns a ModeledResponse<PhieuBaoTri>

                return response.ResponseMessage.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi xóa phiếu bảo trì: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }
      
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

                    System.Windows.MessageBox.Show("Xuất Excel thành công!", "Thông báo",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

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
                System.Windows.MessageBox.Show($"Lỗi khi xuất Excel: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

    }
}