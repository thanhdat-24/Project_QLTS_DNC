using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Models.NhanVien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Supabase;
using Supabase.Postgrest;
using Supabase.Postgrest.Extensions;
using static Supabase.Postgrest.Constants;

namespace Project_QLTS_DNC.Services
{
    // Thêm UserService để lấy thông tin người dùng
    public static class UserService
    {
        public static async Task<NhanVienModel> GetCurrentUserInfoAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null) return null;

                // Lấy user ID từ session hiện tại
                var userId = client.Auth.CurrentUser?.Id;
                if (string.IsNullOrEmpty(userId)) return null;

                // Lấy thông tin nhân viên từ bảng NhanVien
                var query = client.Postgrest.Table<NhanVienModel>();
                var filter = query.Filter("user_id", Operator.Equals, userId);
                var response = await filter.Get();

                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy thông tin người dùng: {ex.Message}");
                return null;
            }
        }
    }

    // Tách LichSuBaoTriService ra khỏi UserService để trở thành class riêng
    public class LichSuBaoTriService
    {
        // Phương thức lưu lịch sử hoạt động bảo trì
        public async Task<bool> LuuLichSuHoatDongAsync(List<PhieuBaoTri> danhSachPhieu, string ghiChu = "")
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    MessageBox.Show("Không thể kết nối đến cơ sở dữ liệu!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // Lấy thông tin người dùng hiện tại
                var userInfo = await UserService.GetCurrentUserInfoAsync();
                if (userInfo == null)
                {
                    Console.WriteLine("Không thể lấy thông tin người dùng hiện tại");
                    return false;
                }

                // Tạo danh sách các bản ghi lịch sử
                var lichSuList = new List<LichSuBaoTri>();

                // Lấy thông tin tài sản để cập nhật tình trạng
                var taiSanIds = danhSachPhieu
                    .Where(p => p.MaTaiSan.HasValue)
                    .Select(p => p.MaTaiSan.Value)
                    .Distinct()
                    .ToList();

                var taiSanDict = new Dictionary<int, TaiSanModel>();
                if (taiSanIds.Any())
                {
                    var taiSanQuery = client.Postgrest.Table<TaiSanModel>();
                    var taiSanFilter = taiSanQuery.Filter("ma_tai_san", Operator.In, taiSanIds);
                    var response = await taiSanFilter.Get();

                    taiSanDict = response.Models.ToDictionary(t => t.MaTaiSan);
                }

                // Tạo nhóm các phiếu theo mã tài sản
                var phieuGroups = danhSachPhieu
                    .Where(p => p.MaTaiSan.HasValue)
                    .GroupBy(p => p.MaTaiSan.Value)
                    .ToList();

                foreach (var group in phieuGroups)
                {
                    var maTaiSan = group.Key;
                    var phieuDauTien = group.First();
                    string tinhTrangTaiSan = "Không xác định";

                    // Lấy tình trạng tài sản từ dictionary đã tạo
                    if (taiSanDict.ContainsKey(maTaiSan))
                    {
                        tinhTrangTaiSan = taiSanDict[maTaiSan].TinhTrangSP;
                    }

                    // Tạo bản ghi lịch sử cho nhóm tài sản này
                    var lichSu = new LichSuBaoTri
                    {
                        MaTaiSan = maTaiSan,
                        TenTaiSan = phieuDauTien.TenTaiSan,
                        SoSeri = phieuDauTien.SoSeri ?? "",
                        NgayThucHien = DateTime.Now,
                        MaNguoiThucHien = userInfo.MaNV,
                        TenNguoiThucHien = userInfo.TenNV,
                        TinhTrangTaiSan = tinhTrangTaiSan,
                        GhiChu = ghiChu
                    };

                    lichSuList.Add(lichSu);
                }

                // Nếu không có phiếu nào có mã tài sản, tạo một bản ghi tổng hợp
                if (lichSuList.Count == 0 && danhSachPhieu.Count > 0)
                {
                    var lichSu = new LichSuBaoTri
                    {
                        MaTaiSan = null,
                        TenTaiSan = "Nhiều tài sản",
                        SoSeri = "",
                        NgayThucHien = DateTime.Now,
                        MaNguoiThucHien = userInfo.MaNV,
                        TenNguoiThucHien = userInfo.TenNV,
                        TinhTrangTaiSan = "Nhiều tình trạng",
                        GhiChu = $"{ghiChu} ({danhSachPhieu.Count} phiếu)"
                    };

                    lichSuList.Add(lichSu);
                }

                // Lưu vào cơ sở dữ liệu
                if (lichSuList.Count > 0)
                {
                    var lichSuTable = client.Postgrest.Table<LichSuBaoTri>();
                    foreach (var lichSu in lichSuList)
                    {
                        await lichSuTable.Insert(lichSu);
                    }
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lưu lịch sử: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LuuLichSuPhieuBaoTriAsync(PhieuBaoTri phieu, string ghiChu = "")
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
                    userInfo = new NhanVienModel
                    {
                        MaNV = phieu.MaNV ?? 0,
                        TenNV = "Người dùng hệ thống"
                    };
                }

                // Lấy thông tin tài sản
                TaiSanModel taiSanInfo = null;
                if (phieu.MaTaiSan.HasValue)
                {
                    try
                    {
                        taiSanInfo = await client.From<TaiSanModel>()
                            .Where(t => t.MaTaiSan == phieu.MaTaiSan.Value)
                            .Single();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi khi lấy thông tin tài sản: {ex.Message}");
                    }
                }

                // Chuẩn bị ghi chú nếu không được cung cấp
                if (string.IsNullOrEmpty(ghiChu))
                {
                    ghiChu = $"Lưu phiếu bảo trì: {phieu.NoiDung}. Chi phí: {phieu.ChiPhi ?? 0:N0} VNĐ";
                }

                // Thực hiện thêm trực tiếp bằng SQL
                try
                {
                    var insertSql = @"
                INSERT INTO lichsu_baotri 
                (ma_tai_san, ten_tai_san, so_seri, ngay_thuc_hien, ma_nv, ten_nv, tinh_trang_tai_san, ghi_chu)
                VALUES 
                (@maTaiSan, @tenTaiSan, @soSeri, CURRENT_TIMESTAMP, @maNV, @tenNV, @tinhTrang, @ghiChu)
            ";

                    var parameters = new Dictionary<string, object>
            {
                { "maTaiSan", phieu.MaTaiSan },
                { "tenTaiSan", taiSanInfo?.TenTaiSan ?? phieu.TenTaiSan ?? "Không xác định" },
                { "soSeri", taiSanInfo?.SoSeri ?? phieu.SoSeri ?? "" },
                { "maNV", userInfo.MaNV },
                { "tenNV", userInfo.TenNV },
                { "tinhTrang", phieu.TrangThai ?? taiSanInfo?.TinhTrangSP ?? "Không xác định" },
                { "ghiChu", ghiChu }
            };

                    await client.Rpc(insertSql, parameters);
                    Console.WriteLine($"Đã lưu lịch sử thành công cho phiếu bảo trì {phieu.MaBaoTri}");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi thực thi SQL: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tổng thể: {ex.Message}");
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
                        taiSanInfo = await client.From<TaiSanModel>()
                            .Where(t => t.MaTaiSan == phieuMoi.MaTaiSan.Value)
                            .Single();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi khi lấy thông tin tài sản: {ex.Message}");
                        // Tiếp tục với thông tin sẵn có
                    }
                }

                // Thực hiện thêm trực tiếp bằng SQL thay vì sử dụng Insert()
                try
                {
                    // Chuẩn bị câu lệnh SQL
                    var insertSql = @"
                INSERT INTO lichsu_baotri 
                (ma_tai_san, ten_tai_san, so_seri, ngay_thuc_hien, ma_nv, ten_nv, tinh_trang_tai_san, ghi_chu)
                VALUES 
                (@maTaiSan, @tenTaiSan, @soSeri, CURRENT_TIMESTAMP, @maNV, @tenNV, @tinhTrang, @ghiChu)
            ";

                    // Chuẩn bị tham số
                    var parameters = new Dictionary<string, object>
            {
                { "maTaiSan", phieuMoi.MaTaiSan },
                { "tenTaiSan", taiSanInfo?.TenTaiSan ?? phieuMoi.TenTaiSan ?? "Không xác định" },
                { "soSeri", taiSanInfo?.SoSeri ?? phieuMoi.SoSeri ?? "" },
                { "maNV", userInfo.MaNV },
                { "tenNV", userInfo.TenNV },
                { "tinhTrang", phieuMoi.TrangThai ?? taiSanInfo?.TinhTrangSP ?? "Không xác định" },
                { "ghiChu", $"Thêm phiếu bảo trì mới: {phieuMoi.NoiDung}. Chi phí: {phieuMoi.ChiPhi ?? 0:N0} VNĐ" }
            };

                    Console.WriteLine($"Đang thực thi SQL để lưu lịch sử cho tài sản {phieuMoi.MaTaiSan}");

                    // Thực thi SQL
                    await client.Rpc(insertSql, parameters);
                    Console.WriteLine("SQL trực tiếp đã được thực thi thành công");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi thực thi SQL trực tiếp: {ex.Message}");

                    // Phương pháp thay thế: Tạo đối tượng LichSuBaoTri mới
                    try
                    {
                        Console.WriteLine("Thử sử dụng phương pháp tạo đối tượng LichSuBaoTri");

                        // Tạo đối tượng LichSuBaoTri
                        var lichSu = new LichSuBaoTri
                        {
                            MaTaiSan = phieuMoi.MaTaiSan,
                            TenTaiSan = taiSanInfo?.TenTaiSan ?? phieuMoi.TenTaiSan ?? "Không xác định",
                            SoSeri = taiSanInfo?.SoSeri ?? phieuMoi.SoSeri ?? "",
                            NgayThucHien = DateTime.Now,
                            MaNguoiThucHien = userInfo.MaNV,
                            TenNguoiThucHien = userInfo.TenNV,
                            TinhTrangTaiSan = phieuMoi.TrangThai ?? taiSanInfo?.TinhTrangSP ?? "Không xác định",
                            GhiChu = $"Thêm phiếu bảo trì (phương pháp thay thế): {phieuMoi.NoiDung}. Chi phí: {phieuMoi.ChiPhi ?? 0:N0} VNĐ"
                        };

                        // Sử dụng phương thức Insert với đối tượng
                        var lichSuTable = client.Postgrest.Table<LichSuBaoTri>();
                        var lichSuResponse = await lichSuTable.Insert(lichSu);

                        Console.WriteLine("Phương pháp thay thế với đối tượng LichSuBaoTri thành công");
                        return true;
                    }
                    catch (Exception innerEx)
                    {
                        Console.WriteLine($"Lỗi với phương pháp thay thế: {innerEx.Message}");

                        // Thử phương pháp cuối cùng: Rpc với câu lệnh SQL đã escape các ký tự đặc biệt
                        try
                        {
                            Console.WriteLine("Thử sử dụng phương pháp Rpc khác");

                            // Chuẩn bị dữ liệu an toàn
                            string tenTaiSan = (taiSanInfo?.TenTaiSan ?? phieuMoi.TenTaiSan ?? "Không xác định")
                                .Replace("'", "''"); // Escape dấu nháy đơn
                            string soSeri = (taiSanInfo?.SoSeri ?? phieuMoi.SoSeri ?? "")
                                .Replace("'", "''");
                            string tinhTrang = (phieuMoi.TrangThai ?? taiSanInfo?.TinhTrangSP ?? "Không xác định")
                                .Replace("'", "''");
                            string noiDung = (phieuMoi.NoiDung ?? "")
                                .Replace("'", "''");
                            decimal chiPhi = phieuMoi.ChiPhi ?? 0;
                            string tenNV = userInfo.TenNV.Replace("'", "''");

                            // SQL với các giá trị đã được escape
                            var altSql = $@"
                        INSERT INTO lichsu_baotri 
                        (ma_tai_san, ten_tai_san, so_seri, ngay_thuc_hien, ma_nv, ten_nv, tinh_trang_tai_san, ghi_chu)
                        VALUES 
                        ({phieuMoi.MaTaiSan}, '{tenTaiSan}', '{soSeri}', CURRENT_TIMESTAMP, {userInfo.MaNV}, '{tenNV}', '{tinhTrang}', 'Thêm phiếu bảo trì: {noiDung}. Chi phí: {chiPhi:N0} VNĐ')
                    ";

                            // Thực thi SQL không có tham số (vì đã nhúng trực tiếp vào chuỗi)
                            await client.Rpc(altSql, new Dictionary<string, object>());
                            Console.WriteLine("Phương pháp Rpc cuối cùng thành công");
                            return true;
                        }
                        catch (Exception innerEx2)
                        {
                            Console.WriteLine($"Lỗi với phương pháp cuối cùng: {innerEx2.Message}");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tổng thể khi lưu lịch sử: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }
        // Phương thức lấy danh sách lịch sử bảo trì
        public async Task<List<LichSuBaoTri>> GetLichSuBaoTriAsync(List<int> maTaiSanList = null)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    MessageBox.Show("Không thể kết nối đến cơ sở dữ liệu!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return new List<LichSuBaoTri>();
                }

                // Khởi tạo query
                var query = client.Postgrest.Table<LichSuBaoTri>();

                // Nếu có danh sách mã tài sản cần lọc
                if (maTaiSanList != null && maTaiSanList.Count > 0)
                {
                    query = query.Filter("ma_tai_san", Operator.In, maTaiSanList);
                }

                // Sắp xếp theo thời gian giảm dần (mới nhất trước)
                query = query.Order("ngay_thuc_hien", Ordering.Descending);

                // Thực hiện truy vấn
                var response = await query.Get();
                var lichSuList = response.Models;

                // Lấy danh sách mã nhân viên để truy vấn thông tin đầy đủ
                var maNVList = lichSuList
                    .Where(l => l.MaNguoiThucHien.HasValue)
                    .Select(l => l.MaNguoiThucHien.Value)
                    .Distinct()
                    .ToList();

                if (maNVList.Any())
                {
                    // Lấy thông tin nhân viên
                    var nhanVienQuery = client.Postgrest.Table<NhanVienModel>();
                    var nhanVienFilter = nhanVienQuery.Filter("ma_nv", Operator.In, maNVList);
                    var nhanVienResponse = await nhanVienFilter.Get();

                    var nhanVienDict = nhanVienResponse.Models.ToDictionary(nv => nv.MaNV);

                    // Cập nhật thông tin nhân viên đầy đủ
                    foreach (var lichSu in lichSuList)
                    {
                        if (lichSu.MaNguoiThucHien.HasValue && nhanVienDict.ContainsKey(lichSu.MaNguoiThucHien.Value))
                        {
                            var nhanVien = nhanVienDict[lichSu.MaNguoiThucHien.Value];
                            lichSu.TenNguoiThucHien = nhanVien.TenNV;
                        }
                    }
                }

                // Trả về danh sách kết quả
                return lichSuList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy lịch sử bảo trì: {ex.Message}");
                return new List<LichSuBaoTri>();
            }
        }

        // Phương thức thống kê số lần bảo trì của mỗi tài sản
        public async Task<Dictionary<int, int>> ThongKeSoLanBaoTriAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null) return new Dictionary<int, int>();

                // Lấy tất cả phiếu bảo trì từ cơ sở dữ liệu
                var query = client.Postgrest.Table<PhieuBaoTri>();
                var response = await query.Get();
                var allPhieuBaoTri = response.Models;

                // Nhóm theo mã tài sản và đếm số lần
                var thongKe = allPhieuBaoTri
                    .Where(p => p.MaTaiSan.HasValue)
                    .GroupBy(p => p.MaTaiSan.Value)
                    .ToDictionary(g => g.Key, g => g.Count());

                Console.WriteLine($"Đã thống kê được {thongKe.Count} tài sản với số lần bảo trì");
                foreach (var pair in thongKe)
                {
                    Console.WriteLine($"Tài sản #{pair.Key}: {pair.Value} lần bảo trì");
                }

                return thongKe;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thống kê số lần bảo trì: {ex.Message}");
                return new Dictionary<int, int>();
            }
        }

        // Kiểm tra xem một phiếu bảo trì đã được lưu vào lịch sử hay chưa
        public async Task<bool> KiemTraPhieuDaCoLichSuAsync(int maTaiSan, DateTime ngayBaoTri)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null) return false;

                // Truy vấn lịch sử bảo trì
                var query = client.Postgrest.Table<LichSuBaoTri>();

                // Lọc theo mã tài sản
                query = query.Filter("ma_tai_san", Operator.Equals, maTaiSan);

                // Lấy kết quả
                var response = await query.Get();
                var lichSuList = response.Models;

                // Kiểm tra xem có bản ghi lịch sử nào có thời gian gần với thời gian bảo trì không
                // (sử dụng khoảng thời gian 1 giờ trước và sau)
                DateTime ngayTruoc = ngayBaoTri.AddHours(-1);
                DateTime ngaySau = ngayBaoTri.AddHours(1);

                bool daCoLichSu = lichSuList.Any(ls =>
                    ls.NgayThucHien >= ngayTruoc &&
                    ls.NgayThucHien <= ngaySau);

                return daCoLichSu;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi kiểm tra lịch sử bảo trì: {ex.Message}");
                return false;
            }
        }

        // Phương thức kiểm tra và hiển thị thông báo cho người dùng
        public async Task HienThiThongBaoLichSuBaoTriAsync(PhieuBaoTri phieu)
        {
            try
            {
                // Kiểm tra xem phiếu có trong lịch sử không
                bool daLuu = false;
                if (phieu.MaTaiSan.HasValue)
                {
                    // Thêm điều kiện kiểm tra mã tài sản hợp lệ
                    if (phieu.MaTaiSan.Value > 0)
                    {
                        daLuu = await KiemTraPhieuDaCoLichSuAsync(phieu.MaTaiSan.Value, phieu.NgayBaoTri ?? DateTime.Now);
                    }
                }

                // Hiển thị thông báo tương ứng
                if (daLuu)
                {
                    MessageBox.Show($"Phiếu bảo trì cho tài sản #{phieu.MaTaiSan} đã được lưu vào lịch sử thành công!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Kiểm tra xem phiếu có đầy đủ thông tin cần thiết không
                    bool coTheLuuLichSu = phieu.MaTaiSan.HasValue && phieu.MaTaiSan.Value > 0 &&
                                          !string.IsNullOrEmpty(phieu.NoiDung) &&
                                          !string.IsNullOrEmpty(phieu.TrangThai);

                    if (coTheLuuLichSu)
                    {
                        var result = MessageBox.Show($"Phiếu bảo trì cho tài sản #{phieu.MaTaiSan} chưa được lưu vào lịch sử. " +
                            $"Bạn có muốn lưu ngay bây giờ không?",
                            "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                        if (result == MessageBoxResult.Yes)
                        {
                            // Thêm log để gỡ lỗi
                            Console.WriteLine($"Bắt đầu lưu lịch sử cho tài sản #{phieu.MaTaiSan}");

                            bool ketQua = await LuuLichSuPhieuMoiAsync(phieu);

                            // Thêm log sau khi lưu
                            Console.WriteLine($"Kết quả lưu lịch sử: {ketQua}");

                            if (ketQua)
                            {
                                MessageBox.Show("Đã lưu lịch sử thành công!", "Thành công",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Không thể lưu lịch sử. Vui lòng thử lại sau!", "Lỗi",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else
                    {
                        // Nếu thiếu thông tin, thông báo lỗi cụ thể
                        string thongBaoLoi = "Không thể lưu lịch sử vì thiếu thông tin: ";
                        if (!phieu.MaTaiSan.HasValue || phieu.MaTaiSan.Value <= 0)
                            thongBaoLoi += "Mã tài sản, ";
                        if (string.IsNullOrEmpty(phieu.NoiDung))
                            thongBaoLoi += "Nội dung, ";
                        if (string.IsNullOrEmpty(phieu.TrangThai))
                            thongBaoLoi += "Trạng thái, ";

                        thongBaoLoi = thongBaoLoi.TrimEnd(' ', ',');

                        MessageBox.Show(thongBaoLoi, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi chi tiết khi kiểm tra và hiển thị thông báo lịch sử: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Thêm PhieuBaoTriService cần thiết cho LichSuBaoTriViewModel
        public class PhieuBaoTriServicev2
        {
            private readonly LichSuBaoTriService _lichSuService = new LichSuBaoTriService();

            public async Task<List<PhieuBaoTri>> GetAllPhieuBaoTriAsync()
            {
                try
                {
                    var client = await SupabaseService.GetClientAsync();
                    if (client == null)
                    {
                        MessageBox.Show("Không thể kết nối đến cơ sở dữ liệu!", "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return new List<PhieuBaoTri>();
                    }

                    var query = client.Postgrest.Table<PhieuBaoTri>();
                    var response = await query.Get();
                    return response.Models;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi lấy danh sách phiếu bảo trì: {ex.Message}");
                    return new List<PhieuBaoTri>();
                }
            }

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

                    if (response == null)
                    {
                        Console.WriteLine("Thêm phiếu bảo trì thất bại");
                        MessageBox.Show("Thêm phiếu bảo trì thất bại", "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                    // Nếu thêm thành công, lưu lịch sử
                    bool lichSuResult = await _lichSuService.LuuLichSuPhieuMoiAsync(phieuMoi);

                    // Thêm thông báo hiển thị cho người dùng
                    if (lichSuResult)
                    {
                        MessageBox.Show("Đã thêm phiếu bảo trì và lưu lịch sử thành công!", "Thành công",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        Console.WriteLine("Đã lưu lịch sử bảo trì thành công");
                    }
                    else
                    {
                        MessageBox.Show("Đã thêm phiếu bảo trì nhưng không lưu được lịch sử. Vui lòng kiểm tra lại!",
                            "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Console.WriteLine("Lưu lịch sử bảo trì thất bại, nhưng phiếu đã được thêm");
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
}