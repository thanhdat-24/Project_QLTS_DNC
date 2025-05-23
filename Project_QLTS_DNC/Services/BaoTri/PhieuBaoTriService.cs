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
using Project_QLTS_DNC.ViewModel.Baotri;

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
                if (phieuBaoTri.MaTaiSan == null || phieuBaoTri.MaTaiSan <= 0)
                {
                    System.Windows.MessageBox.Show("Mã tài sản không được để trống hoặc không hợp lệ", "Lỗi dữ liệu",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return false;
                }

                if (string.IsNullOrEmpty(phieuBaoTri.NoiDung))
                {
                    System.Windows.MessageBox.Show("Nội dung không được để trống", "Lỗi dữ liệu",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return false;
                }

                if (string.IsNullOrEmpty(phieuBaoTri.TrangThai))
                {
                    System.Windows.MessageBox.Show("Trạng thái không được để trống", "Lỗi dữ liệu",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return false;
                }

                Console.WriteLine($"Đang thêm phiếu bảo trì mới: MaTaiSan={phieuBaoTri.MaTaiSan}, NoiDung={phieuBaoTri.NoiDung}");

                // Đặt ngày bảo trì là ngày hiện tại nếu chưa có
                if (!phieuBaoTri.NgayBaoTri.HasValue)
                {
                    phieuBaoTri.NgayBaoTri = DateTime.Now;
                }

                // Lấy thông tin tài sản trước khi thêm phiếu
                Models.QLTaiSan.TaiSanModel taiSanInfo = null;
                if (phieuBaoTri.MaTaiSan.HasValue)
                {
                    try
                    {
                        taiSanInfo = await client.From<Models.QLTaiSan.TaiSanModel>()
                            .Where(t => t.MaTaiSan == phieuBaoTri.MaTaiSan.Value)
                            .Single();

                        // Cập nhật thông tin tài sản vào phiếu nếu chưa có
                        if (string.IsNullOrEmpty(phieuBaoTri.TenTaiSan) && taiSanInfo != null)
                        {
                            phieuBaoTri.TenTaiSan = taiSanInfo.TenTaiSan;
                            phieuBaoTri.SoSeri = taiSanInfo.SoSeri;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Cảnh báo: Không lấy được thông tin tài sản: {ex.Message}");
                        // Không cần ném lỗi, vẫn tiếp tục quy trình
                    }
                }

                // Chuẩn bị lưu thông tin lịch sử trước
                var lichSu = new LichSuBaoTri
                {
                    MaTaiSan = phieuBaoTri.MaTaiSan,
                    TenTaiSan = phieuBaoTri.TenTaiSan ?? taiSanInfo?.TenTaiSan ?? "Không xác định",
                    SoSeri = phieuBaoTri.SoSeri ?? taiSanInfo?.SoSeri ?? "",
                    NgayThucHien = DateTime.Now,
                    MaNguoiThucHien = phieuBaoTri.MaNV,
                    TenNguoiThucHien = await GetTenNhanVienAsync(phieuBaoTri.MaNV ?? 0),
                    TinhTrangTaiSan = phieuBaoTri.TrangThai ?? taiSanInfo?.TinhTrangSP ?? "Không xác định",
                    GhiChu = $"Thêm phiếu bảo trì mới: {phieuBaoTri.NoiDung}. Chi phí: {phieuBaoTri.ChiPhi ?? 0:N0} VNĐ"
                };

                // Thực hiện thêm mới phiếu bảo trì
                Console.WriteLine("Bắt đầu thêm phiếu bảo trì...");
                var response = await client.From<PhieuBaoTri>().Insert(phieuBaoTri);

                if (response == null || response.Models.Count == 0)
                {
                    System.Windows.MessageBox.Show("Không thể thêm phiếu bảo trì. Vui lòng kiểm tra lại dữ liệu!", "Lỗi",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return false;
                }

                var phieuDaLuu = response.Models[0];
                Console.WriteLine($"Đã thêm phiếu bảo trì thành công, mã phiếu: {phieuDaLuu.MaBaoTri}");

                // Nếu thêm thành công, thực hiện lưu lịch sử
                bool ketQuaLuuLichSu = false;

                // Phương pháp 1: Thử sử dụng trực tiếp Insert thông qua SQL
                try
                {
                    Console.WriteLine("Phương pháp 1: Lưu lịch sử bằng SQL...");
                    var sqlCommand = @"
                        INSERT INTO lichsu_baotri 
                        (ma_tai_san, ten_tai_san, so_seri, ngay_thuc_hien, ma_nv, ten_nv, tinh_trang_tai_san, ghi_chu) 
                        VALUES 
                        (@maTaiSan, @tenTaiSan, @soSeri, CURRENT_TIMESTAMP, @maNV, @tenNV, @tinhTrang, @ghiChu)
                    ";

                    var parameters = new Dictionary<string, object> {
                        { "maTaiSan", lichSu.MaTaiSan },
                        { "tenTaiSan", lichSu.TenTaiSan ?? "Không xác định" },
                        { "soSeri", lichSu.SoSeri ?? "" },
                        { "maNV", lichSu.MaNguoiThucHien ?? 0 },
                        { "tenNV", lichSu.TenNguoiThucHien ?? "Người dùng hệ thống" },
                        { "tinhTrang", lichSu.TinhTrangTaiSan ?? "Không xác định" },
                        { "ghiChu", lichSu.GhiChu }
                    };

                    await client.Rpc(sqlCommand, parameters);
                    Console.WriteLine("Phương pháp 1: Lưu lịch sử thành công");
                    ketQuaLuuLichSu = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Phương pháp 1 thất bại: {ex.Message}");
                    ketQuaLuuLichSu = false;
                }

                // Phương pháp 2: Dùng hàm lưu trữ RPC
                if (!ketQuaLuuLichSu)
                {
                    try
                    {
                        Console.WriteLine("Phương pháp 2: Thử sử dụng hàm insert_lichsu_baotri...");
                        var parameters = new Dictionary<string, object> {
                            { "ma_tai_san", lichSu.MaTaiSan },
                            { "ten_tai_san", lichSu.TenTaiSan ?? "Không xác định" },
                            { "so_seri", lichSu.SoSeri ?? "" },
                            { "ma_nv", lichSu.MaNguoiThucHien ?? 0 },
                            { "ten_nv", lichSu.TenNguoiThucHien ?? "Người dùng hệ thống" },
                            { "tinh_trang_tai_san", lichSu.TinhTrangTaiSan ?? "Không xác định" },
                            { "ghi_chu", lichSu.GhiChu }
                        };

                        await client.Rpc("insert_lichsu_baotri", parameters);
                        Console.WriteLine("Phương pháp 2: Lưu lịch sử thành công");
                        ketQuaLuuLichSu = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Phương pháp 2 thất bại: {ex.Message}");
                        ketQuaLuuLichSu = false;
                    }
                }

                // Phương pháp 3: Sử dụng Insert thông thường
                if (!ketQuaLuuLichSu)
                {
                    try
                    {
                        Console.WriteLine("Phương pháp 3: Thử sử dụng Insert trực tiếp...");
                        var lichSuResponse = await client.From<LichSuBaoTri>().Insert(lichSu);
                        ketQuaLuuLichSu = lichSuResponse != null && lichSuResponse.Models.Count > 0;
                        Console.WriteLine($"Phương pháp 3: {(ketQuaLuuLichSu ? "Thành công" : "Thất bại")}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Phương pháp 3 thất bại: {ex.Message}");
                        ketQuaLuuLichSu = false;
                    }
                }

                // Hiển thị thông báo dựa trên kết quả
                if (ketQuaLuuLichSu)
                {
                    System.Windows.MessageBox.Show("Đã thêm phiếu bảo trì và lưu lịch sử thành công!", "Thành công",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show("Đã thêm phiếu bảo trì nhưng không thể lưu lịch sử. Vui lòng kiểm tra kết nối mạng!",
                        "Cảnh báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chi tiết lỗi thêm phiếu: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                System.Windows.MessageBox.Show($"Lỗi khi thêm phiếu bảo trì: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }
        public async Task<bool> LuuLichSuPhieuMoiAsync(PhieuBaoTri phieuMoi)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    Console.WriteLine("Không thể kết nối đến cơ sở dữ liệu (client là null)");
                    return false;
                }


                
                // Lấy thông tin người dùng hiện tại
                var userInfo = await UserService.GetCurrentUserInfoAsync();
                if (userInfo == null)
                {
                    Console.WriteLine("Không thể lấy thông tin người dùng hiện tại");

                    // Sử dụng thông tin từ phiếu bảo trì nếu không lấy được thông tin người dùng
                    if (phieuMoi.MaNV.HasValue)
                    {
                        try
                        {
                            var nhanVienResponse = await client.From<NhanVienModel>()
                                .Where(nv => nv.MaNV == phieuMoi.MaNV.Value)
                                .Single();

                            if (nhanVienResponse != null)
                            {
                                userInfo = nhanVienResponse;
                            }
                            else
                            {
                                userInfo = new NhanVienModel
                                {
                                    MaNV = phieuMoi.MaNV.Value,
                                    TenNV = "Không xác định"
                                };
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Lỗi khi lấy thông tin nhân viên: {ex.Message}");
                            userInfo = new NhanVienModel
                            {
                                MaNV = phieuMoi.MaNV ?? 0,
                                TenNV = "Không xác định"
                            };
                        }
                    }
                    else
                    {
                        userInfo = new NhanVienModel
                        {
                            MaNV = 0,
                            TenNV = "Hệ thống"
                        };
                    }
                }

                // Lấy thông tin tài sản
                TaiSanModel taiSanInfo = null;
                if (phieuMoi.MaTaiSan.HasValue)
                {
                    try
                    {
                        var taiSanQuery = client.From<TaiSanModel>();
                        var taiSanFilter = taiSanQuery.Filter("ma_tai_san", Supabase.Postgrest.Constants.Operator.Equals, phieuMoi.MaTaiSan.Value);
                        var taiSanResponse = await taiSanFilter.Get();

                        if (taiSanResponse.Models.Count > 0)
                        {
                            taiSanInfo = taiSanResponse.Models[0];
                            Console.WriteLine($"Đã lấy thông tin tài sản: {taiSanInfo.TenTaiSan}");
                        }
                        else
                        {
                            Console.WriteLine($"Không tìm thấy thông tin tài sản có mã {phieuMoi.MaTaiSan.Value}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi khi lấy thông tin tài sản: {ex.Message}");
                        // Tiếp tục với thông tin sẵn có
                    }
                }

                // Tạo bản ghi lịch sử với đầy đủ thông tin nhất có thể
                var lichSu = new LichSuBaoTri
                {
                    MaTaiSan = phieuMoi.MaTaiSan,
                    TenTaiSan = taiSanInfo?.TenTaiSan ?? phieuMoi.TenTaiSan ?? "Không xác định",
                    SoSeri = taiSanInfo?.SoSeri ?? phieuMoi.SoSeri ?? "",
                    NgayThucHien = DateTime.Now,
                    MaNguoiThucHien = userInfo.MaNV,
                    TenNguoiThucHien = userInfo.TenNV,
                    TinhTrangTaiSan = phieuMoi.TrangThai ?? taiSanInfo?.TinhTrangSP ?? "Không xác định",
                    GhiChu = $"Thêm phiếu bảo trì mới: {phieuMoi.NoiDung}. Chi phí: {phieuMoi.ChiPhi ?? 0:N0} VNĐ"
                };

                // Log thông tin trước khi lưu
                Console.WriteLine($"Chuẩn bị lưu lịch sử: MaTaiSan={lichSu.MaTaiSan}, TenTaiSan={lichSu.TenTaiSan}, " +
                                $"NguoiThucHien={lichSu.TenNguoiThucHien}, TinhTrang={lichSu.TinhTrangTaiSan}");

                // Thêm cơ chế thử lại nhiều lần khi lưu lịch sử
                int maxRetries = 3;
                for (int retry = 0; retry < maxRetries; retry++)
                {
                    try
                    {
                        var lichSuTable = client.From<LichSuBaoTri>();
                        var lichSuResponse = await lichSuTable.Insert(lichSu);

                        if (lichSuResponse != null && lichSuResponse.Models.Count > 0)
                        {
                            Console.WriteLine($"Đã lưu lịch sử thành công, mã lịch sử: {lichSuResponse.Models[0].MaLichSu}");
                            return true;
                        }

                        Console.WriteLine($"Lần thử {retry + 1}: Lưu lịch sử không thành công - response không có dữ liệu");
                        await Task.Delay(500); // Chờ 500ms trước khi thử lại
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lần thử {retry + 1}: Lỗi khi thực hiện Insert lịch sử: {ex.Message}");
                        if (retry < maxRetries - 1)
                        {
                            await Task.Delay(1000); // Chờ 1 giây trước khi thử lại
                        }
                    }
                }

               
                try
                {
                    // Tạo một phiếu bảo trì trực tiếp
                    var lichSuTrucTiep = new LichSuBaoTri
                    {
                        MaTaiSan = phieuMoi.MaTaiSan,
                        TenTaiSan = lichSu.TenTaiSan,
                        SoSeri = lichSu.SoSeri,
                        NgayThucHien = DateTime.Now,
                        MaNguoiThucHien = lichSu.MaNguoiThucHien,
                        TenNguoiThucHien = lichSu.TenNguoiThucHien,
                        TinhTrangTaiSan = lichSu.TinhTrangTaiSan,
                        GhiChu = $"(Phương pháp thay thế) Thêm phiếu bảo trì: {phieuMoi.NoiDung}. Chi phí: {phieuMoi.ChiPhi ?? 0:N0} VNĐ"
                    };

                    // Lưu trực tiếp vào bảng lichsu_baotri
                    var lichSuTable = client.From<LichSuBaoTri>();
                    var response = await lichSuTable.Insert(lichSuTrucTiep);

                    if (response != null && response.Models.Count > 0)
                    {
                        Console.WriteLine("Đã lưu lịch sử thành công bằng phương pháp thay thế trực tiếp");
                        return true;
                    }

                    Console.WriteLine("Không thể lưu lịch sử bằng phương pháp thay thế trực tiếp");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi thử phương pháp thay thế trực tiếp: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tổng thể khi lưu lịch sử phiếu mới: {ex.Message}");
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

                // Lấy toàn bộ danh sách phiếu bảo trì
                var responsePhieuBaoTri = await client.From<PhieuBaoTri>().Get();
                var danhSachPhieu = responsePhieuBaoTri.Models;

                // Lấy danh sách tài sản để tìm kiếm theo tên tài sản
                var responseTaiSan = await client.From<TaiSanModel>().Get();
                var danhSachTaiSan = responseTaiSan.Models;
                var taiSanDict = danhSachTaiSan.ToDictionary(ts => ts.MaTaiSan, ts => ts.TenTaiSan?.ToLower() ?? "");

                // Lấy danh sách nhân viên để tìm kiếm theo tên người phụ trách
                var responseNhanVien = await client.From<NhanVienModel>().Get();
                var danhSachNhanVien = responseNhanVien.Models;
                var nhanVienDict = danhSachNhanVien.ToDictionary(nv => nv.MaNV, nv => nv.TenNV?.ToLower() ?? "");

                // Lấy danh sách loại bảo trì để tìm kiếm theo tên loại
                var responseLoaiBaoTri = await client.From<LoaiBaoTri>().Get();
                var danhSachLoaiBaoTri = responseLoaiBaoTri.Models;
                var loaiBaoTriDict = danhSachLoaiBaoTri.ToDictionary(lbt => lbt.MaLoaiBaoTri, lbt => lbt.TenLoai?.ToLower() ?? "");

                // Từ khóa tìm kiếm (chuyển về chữ thường)
                string lowerKeyword = keyword.ToLower();

                // Tìm các phiếu bảo trì có các tài sản phù hợp với từ khóa tìm kiếm
                var taiSanIdsMatch = danhSachTaiSan
                    .Where(ts => ts.TenTaiSan != null && ts.TenTaiSan.ToLower().Contains(lowerKeyword))
                    .Select(ts => ts.MaTaiSan)
                    .ToList();

                // Tìm các nhân viên phù hợp với từ khóa tìm kiếm
                var nhanVienIdsMatch = danhSachNhanVien
                    .Where(nv => nv.TenNV != null && nv.TenNV.ToLower().Contains(lowerKeyword))
                    .Select(nv => nv.MaNV)
                    .ToList();

                // Tìm các loại bảo trì phù hợp với từ khóa tìm kiếm
                var loaiBaoTriIdsMatch = danhSachLoaiBaoTri
                    .Where(lbt => lbt.TenLoai != null && lbt.TenLoai.ToLower().Contains(lowerKeyword))
                    .Select(lbt => lbt.MaLoaiBaoTri)
                    .ToList();

                // Lọc phiếu bảo trì theo từ khóa
                return danhSachPhieu.Where(p =>
                    // Tìm kiếm theo các trường có sẵn trong phiếu bảo trì
                    p.MaBaoTri.ToString().Contains(keyword) ||
                    (p.MaTaiSan != null && p.MaTaiSan.ToString().Contains(keyword)) ||
                    (p.MaNV != null && p.MaNV.ToString().Contains(keyword)) ||
                    (p.MaLoaiBaoTri != null && p.MaLoaiBaoTri.ToString().Contains(keyword)) ||
                    (p.NoiDung != null && p.NoiDung.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (p.GhiChu != null && p.GhiChu.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (p.TrangThai != null && p.TrangThai.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||

                    // Tìm kiếm theo tên tài sản
                    (p.MaTaiSan != null && taiSanIdsMatch.Contains(p.MaTaiSan.Value)) ||

                    // Tìm kiếm theo tên người phụ trách
                    (p.MaNV != null && nhanVienIdsMatch.Contains(p.MaNV.Value)) ||

                    // Tìm kiếm theo tên loại bảo trì
                    (p.MaLoaiBaoTri != null && loaiBaoTriIdsMatch.Contains(p.MaLoaiBaoTri.Value))
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

        // Xuất dữ liệu ra Excel với tên thay vì mã và thêm thông tin công ty
        public async Task<bool> ExportToExcel(List<PhieuBaoTri> danhSachPhieu, string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Phiếu Bảo Trì");

                    // Đọc thông tin công ty từ file JSON
                    var thongTinCongTy = ThongTinCongTyService.DocThongTinCongTy();

                    // Thêm thông tin công ty vào header
                    int currentRow = 1;
                    int logoHeightPx = 80;

                    if (!string.IsNullOrEmpty(thongTinCongTy.LogoPath) && File.Exists(thongTinCongTy.LogoPath))
                    {
                        var image = worksheet.AddPicture(thongTinCongTy.LogoPath)
                                             .MoveTo(worksheet.Cell(currentRow, 1))
                                             .WithSize(logoHeightPx * 2, logoHeightPx); // Width x Height (px)

                        // Đặt chiều cao hàng chứa logo để hình không bị cắt
                        worksheet.Row(currentRow).Height = 60;
                    }

                    // Tên công ty
                    worksheet.Cell(currentRow, 2).Value = thongTinCongTy.Ten;
                    worksheet.Cell(currentRow, 2).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, 2).Style.Font.FontSize = 14;
                    worksheet.Range(currentRow, 2, currentRow, 9).Merge();
                    worksheet.Range(currentRow, 2, currentRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    currentRow++;

                    // Địa chỉ
                    worksheet.Cell(currentRow, 2).Value = "Địa chỉ: " + thongTinCongTy.DiaChi;
                    worksheet.Range(currentRow, 2, currentRow, 9).Merge();
                    worksheet.Range(currentRow, 2, currentRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    currentRow++;

                    // Số điện thoại và Email
                    worksheet.Cell(currentRow, 2).Value = $"SĐT: {thongTinCongTy.SoDienThoai} - Email: {thongTinCongTy.Email}";
                    worksheet.Range(currentRow, 2, currentRow, 9).Merge();
                    worksheet.Range(currentRow, 2, currentRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    currentRow++;

                    // Mã số thuế
                    worksheet.Cell(currentRow, 2).Value = "Mã số thuế: " + thongTinCongTy.MaSoThue;
                    worksheet.Range(currentRow, 2, currentRow, 9).Merge();
                    worksheet.Range(currentRow, 2, currentRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    currentRow += 2; // Thêm khoảng trống


                    // Tạo tiêu đề báo cáo (giữ nguyên phần code của bạn)
                    worksheet.Cell($"A{currentRow}").Value = "DANH SÁCH PHIẾU BẢO TRÌ";
                    worksheet.Range($"A{currentRow}:I{currentRow}").Merge();
                    worksheet.Range($"A{currentRow}:I{currentRow}").Style.Font.Bold = true;
                    worksheet.Range($"A{currentRow}:I{currentRow}").Style.Font.FontSize = 16;
                    worksheet.Range($"A{currentRow}:I{currentRow}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    currentRow += 2; // Tăng currentRow để đến dòng header của bảng

                    // Tạo header cho bảng
                    worksheet.Cell($"A{currentRow}").Value = "STT";
                    worksheet.Cell($"B{currentRow}").Value = "Mã bảo trì";
                    worksheet.Cell($"C{currentRow}").Value = "Tên tài sản";
                    worksheet.Cell($"D{currentRow}").Value = "Loại bảo trì";
                    worksheet.Cell($"E{currentRow}").Value = "Ngày bảo trì";
                    worksheet.Cell($"F{currentRow}").Value = "Tên nhân viên";
                    worksheet.Cell($"G{currentRow}").Value = "Nội dung";
                    worksheet.Cell($"H{currentRow}").Value = "Trạng thái";
                    worksheet.Cell($"I{currentRow}").Value = "Chi phí";

                    // Định dạng header
                    var headerRange = worksheet.Range($"A{currentRow}:I{currentRow}");
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Điền dữ liệu vào bảng
                    for (int i = 0; i < danhSachPhieu.Count; i++)
                    {
                        var phieu = danhSachPhieu[i];
                        int row = i + currentRow + 1; // Bắt đầu từ dòng sau header

                        // STT và Mã bảo trì
                        worksheet.Cell(row, 1).Value = i + 1;
                        worksheet.Cell(row, 2).Value = phieu.MaBaoTri;

                        // Lấy tên tài sản thay vì mã
                        string tenTaiSan = "N/A";
                        if (phieu.MaTaiSan.HasValue)
                        {
                            tenTaiSan = await GetTenTaiSanAsync(phieu.MaTaiSan.Value);
                        }
                        worksheet.Cell(row, 3).Value = tenTaiSan;

                        // Chuyển đổi mã loại bảo trì thành tên
                        string loaiBaoTri = "Không xác định";
                        switch (phieu.MaLoaiBaoTri)
                        {
                            case 1: loaiBaoTri = "Định kỳ"; break;
                            case 2: loaiBaoTri = "Đột xuất"; break;
                            case 3: loaiBaoTri = "Bảo hành"; break;
                        }
                        worksheet.Cell(row, 4).Value = loaiBaoTri;

                        // Ngày bảo trì
                        worksheet.Cell(row, 5).Value = phieu.NgayBaoTri;
                        worksheet.Cell(row, 5).Style.DateFormat.Format = "dd/MM/yyyy";

                        // Lấy tên nhân viên thay vì mã
                        string tenNhanVien = "N/A";
                        if (phieu.MaNV.HasValue)
                        {
                            tenNhanVien = await GetTenNhanVienAsync(phieu.MaNV.Value);
                        }
                        worksheet.Cell(row, 6).Value = tenNhanVien;

                        // Các thông tin còn lại
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
                    int lastRow = danhSachPhieu.Count + currentRow + 2;
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


        // Thay đổi mức độ truy cập thành public
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
        // Lấy thông tin chi tiết của tất cả tài sản
        public async Task<Dictionary<int, PhieuBaoTriViewModel.TaiSanInfo>> GetAllTaiSanInfoAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                var response = await client.From<TaiSanModel>().Get();
                var danhSachTaiSan = response.Models;

                // Chuyển đổi danh sách tài sản sang Dictionary để truy vấn nhanh
                var taiSanDict = new Dictionary<int, PhieuBaoTriViewModel.TaiSanInfo>();

                foreach (var taiSan in danhSachTaiSan)
                {
                    taiSanDict[taiSan.MaTaiSan] = new PhieuBaoTriViewModel.TaiSanInfo
                    {
                        MaTaiSan = taiSan.MaTaiSan,
                        TenTaiSan = taiSan.TenTaiSan ?? "Không có tên",
                        SoSeri = taiSan.SoSeri ?? "Không có số sê-ri",
                        TinhTrangSP = taiSan.TinhTrangSP ?? "Không xác định"
                    };
                }

                Console.WriteLine($"Đã lấy thông tin chi tiết của {taiSanDict.Count} tài sản");
                return taiSanDict;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy thông tin tài sản: {ex.Message}");
                MessageBox.Show($"Lỗi khi lấy thông tin tài sản: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new Dictionary<int, PhieuBaoTriViewModel.TaiSanInfo>();
            }
        }
        // Thống kê tổng tài sản đã bảo trì
        public async Task<int> ThongKeTongTaiSanBaoTriAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    throw new Exception("Không thể kết nối Supabase Client");

                var response = await client.From<PhieuBaoTri>().Get();
                var danhSachPhieu = response.Models;

                // Lấy danh sách mã tài sản duy nhất đã từng được bảo trì
                var soLuongTaiSanBaoTri = danhSachPhieu
                    .Where(p => p.MaTaiSan.HasValue)
                    .Select(p => p.MaTaiSan.Value)
                    .Distinct()
                    .Count();

                return soLuongTaiSanBaoTri;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thống kê tài sản bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }
        // Thêm vào PhieuBaoTriService.cs
        public async Task<bool> ThemPhieuBaoTriVaLuuLichSuAsync(PhieuBaoTri phieuMoi)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null) return false;

                // Kiểm tra dữ liệu đầu vào
                if (!phieuMoi.MaTaiSan.HasValue || string.IsNullOrEmpty(phieuMoi.NoiDung) || string.IsNullOrEmpty(phieuMoi.TrangThai))
                {
                    Console.WriteLine("Thiếu thông tin bắt buộc cho phiếu bảo trì");
                    MessageBox.Show("Thiếu thông tin bắt buộc cho phiếu bảo trì", "Lỗi dữ liệu",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                Console.WriteLine($"Đang thêm phiếu bảo trì mới: MaTaiSan={phieuMoi.MaTaiSan}, NoiDung={phieuMoi.NoiDung}");

                // Thêm phiếu bảo trì mới
                var response = await client.From<PhieuBaoTri>().Insert(phieuMoi);

                if (response == null || response.Models.Count == 0)
                {
                    MessageBox.Show("Không thể thêm phiếu bảo trì. Vui lòng kiểm tra lại dữ liệu!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // Lưu lịch sử bảo trì
                var lichSuService = new LichSuBaoTriService();
                bool lichSuResult = await lichSuService.LuuLichSuPhieuMoiAsync(phieuMoi);

                // Hiển thị thông báo kết quả
                if (lichSuResult)
                {
                    MessageBox.Show("Đã thêm phiếu bảo trì và lưu lịch sử thành công!", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Đã thêm phiếu bảo trì nhưng không lưu được lịch sử. Vui lòng kiểm tra lại!",
                        "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thêm phiếu bảo trì và lưu lịch sử: {ex.Message}");
                MessageBox.Show($"Lỗi khi thêm phiếu bảo trì và lưu lịch sử: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}