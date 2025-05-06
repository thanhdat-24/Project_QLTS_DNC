using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Models.NhanVien;
using Supabase.Postgrest;
using Supabase.Postgrest.Interfaces;

namespace Project_QLTS_DNC.Services
{
    public class LichSuSuaChuaService
    {
        /// <summary>
        /// Lấy tất cả lịch sử sửa chữa
        /// </summary>
        public async Task<List<LichSuSuaChua>> LayTatCaLichSuSuaChua()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<LichSuSuaChua>()
                    .Get();
                return response.Models;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy dữ liệu lịch sử sửa chữa: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy tất cả lịch sử sửa chữa với thông tin đầy đủ từ các bảng liên quan
        /// </summary>
        public async Task<List<LichSuSuaChua>> LayLichSuSuaChuaVoiThongTinTaiSan()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Lấy dữ liệu lịch sử sửa chữa
                var lichSuList = await client.From<LichSuSuaChua>().Get();
                var danhSachLichSu = lichSuList.Models;

                // Lấy danh sách tất cả mã tài sản từ lịch sử
                var maTaiSanList = danhSachLichSu
                    .Where(ls => ls.MaTaiSan.HasValue)
                    .Select(ls => ls.MaTaiSan.Value)
                    .Distinct()
                    .ToList();

                // Lấy thông tin tất cả tài sản liên quan
                if (maTaiSanList.Any())
                {
                    var taiSanList = await client.From<TaiSanModel>()
                        .Filter("ma_tai_san", Supabase.Postgrest.Constants.Operator.In, maTaiSanList)
                        .Get();
                    var danhSachTaiSan = taiSanList.Models;

                    // Bổ sung thông tin tài sản vào đối tượng lịch sử
                    foreach (var lichSu in danhSachLichSu)
                    {
                        if (lichSu.MaTaiSan.HasValue)
                        {
                            var taiSan = danhSachTaiSan.FirstOrDefault(ts => ts.MaTaiSan == lichSu.MaTaiSan.Value);
                            if (taiSan != null)
                            {
                                lichSu.TenTaiSan = taiSan.TenTaiSan;
                                lichSu.SoSeri = taiSan.SoSeri;
                                lichSu.TinhTrangTaiSan = taiSan.TinhTrangSP;

                                // Lưu mã phòng để lấy thông tin phòng sau
                                if (taiSan.MaPhong.HasValue)
                                {
                                    // Bạn có thể lấy tên phòng từ bảng Phong nếu cần
                                    // Ví dụ: lichSu.TenPhong = await LayTenPhong(taiSan.MaPhong.Value);
                                }
                            }
                        }
                    }
                }

                // Lấy thông tin tất cả phiếu bảo trì liên quan
                var maBaoTriList = danhSachLichSu
                    .Where(ls => ls.MaBaoTri.HasValue)
                    .Select(ls => ls.MaBaoTri.Value)
                    .Distinct()
                    .ToList();

                if (maBaoTriList.Any())
                {
                    var phieuBaoTriList = await client.From<PhieuBaoTri>()
                        .Filter("ma_bao_tri", Supabase.Postgrest.Constants.Operator.In, maBaoTriList)
                        .Get();
                    var danhSachPhieuBaoTri = phieuBaoTriList.Models;

                    // Bổ sung thông tin từ phiếu bảo trì
                    foreach (var lichSu in danhSachLichSu)
                    {
                        if (lichSu.MaBaoTri.HasValue)
                        {
                            var phieuBaoTri = danhSachPhieuBaoTri.FirstOrDefault(pbt => pbt.MaBaoTri == lichSu.MaBaoTri.Value);
                            if (phieuBaoTri != null)
                            {
                                // Nếu nội dung bảo trì trống, lấy từ phiếu bảo trì
                                if (string.IsNullOrEmpty(lichSu.NoiDungBaoTri))
                                {
                                    lichSu.NoiDungBaoTri = phieuBaoTri.NoiDung;
                                }

                                // Nếu chi phí không có, lấy từ phiếu bảo trì
                                if (!lichSu.ChiPhi.HasValue && phieuBaoTri.ChiPhi.HasValue)
                                {
                                    lichSu.ChiPhi = phieuBaoTri.ChiPhi;
                                }
                            }
                        }
                    }
                }

                // Lấy thông tin nhân viên
                var maNhanVienList = danhSachLichSu
                    .Where(ls => ls.MaNV.HasValue)
                    .Select(ls => ls.MaNV.Value)
                    .Distinct()
                    .ToList();

                if (maNhanVienList.Any())
                {
                    var nhanVienList = await client.From<NhanVienModel>()
                        .Filter("ma_nv", Supabase.Postgrest.Constants.Operator.In, maNhanVienList)
                        .Get();
                    var danhSachNhanVien = nhanVienList.Models;

                    // Bổ sung thông tin nhân viên
                    foreach (var lichSu in danhSachLichSu)
                    {
                        if (lichSu.MaNV.HasValue)
                        {
                            var nhanVien = danhSachNhanVien.FirstOrDefault(nv => nv.MaNV == lichSu.MaNV.Value);
                            if (nhanVien != null)
                            {
                                lichSu.TenNguoiThucHien = nhanVien.TenNV;
                            }
                        }
                    }
                }

                // Có thể bổ sung thêm việc lấy thông tin nhóm tài sản từ bảng chi tiết xuất kho nếu cần

                return danhSachLichSu;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy dữ liệu lịch sử sửa chữa: {ex.Message}");
            }
        }

        /// <summary>
        /// Tìm kiếm lịch sử sửa chữa theo các tiêu chí
        /// </summary>
        public async Task<List<LichSuSuaChua>> TimKiemLichSuSuaChua(string tuKhoa, DateTime? tuNgay, DateTime? denNgay)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Ghi log để debug
                System.Diagnostics.Debug.WriteLine($"Tìm kiếm với từ khóa: '{tuKhoa}', từ ngày: {tuNgay}, đến ngày: {denNgay}");

                // Bắt đầu với một truy vấn Select
                var query = client.From<LichSuSuaChua>().Select("*");

                // Tạo biến để theo dõi xem có áp dụng bất kỳ bộ lọc nào không
                bool coBoLoc = false;

                // Xử lý tìm kiếm theo từ khóa
                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    // Tạo danh sách các điều kiện với kiểu dữ liệu đúng
                    var filters = new List<IPostgrestQueryFilter>
                    {
                        // Tìm kiếm theo các trường văn bản
                        new QueryFilter("loai_thao_tac", Supabase.Postgrest.Constants.Operator.ILike, $"%{tuKhoa}%"),
                        new QueryFilter("ket_qua", Supabase.Postgrest.Constants.Operator.ILike, $"%{tuKhoa}%"),
                        new QueryFilter("ghi_chu", Supabase.Postgrest.Constants.Operator.ILike, $"%{tuKhoa}%"),
                        new QueryFilter("noi_dung_bao_tri", Supabase.Postgrest.Constants.Operator.ILike, $"%{tuKhoa}%"),
                        new QueryFilter("trang_thai_truoc", Supabase.Postgrest.Constants.Operator.ILike, $"%{tuKhoa}%"),
                        new QueryFilter("trang_thai_sau", Supabase.Postgrest.Constants.Operator.ILike, $"%{tuKhoa}%")
                    };

                    // Tìm kiếm theo mã nếu từ khóa có thể chuyển thành số
                    if (int.TryParse(tuKhoa, out int maKhoa))
                    {
                        filters.Add(new QueryFilter("ma_lich_su", Supabase.Postgrest.Constants.Operator.Equals, maKhoa));
                        filters.Add(new QueryFilter("ma_bao_tri", Supabase.Postgrest.Constants.Operator.Equals, maKhoa));
                        filters.Add(new QueryFilter("ma_tai_san", Supabase.Postgrest.Constants.Operator.Equals, maKhoa));
                        filters.Add(new QueryFilter("ma_nv", Supabase.Postgrest.Constants.Operator.Equals, maKhoa));
                    }

                    // Tìm kiếm theo chi phí nếu từ khóa có thể chuyển đổi thành decimal
                    if (decimal.TryParse(tuKhoa, out decimal chiPhiKhoa))
                    {
                        filters.Add(new QueryFilter("chi_phi", Supabase.Postgrest.Constants.Operator.Equals, chiPhiKhoa));
                    }

                    // Áp dụng điều kiện OR cho tìm kiếm
                    query = query.Or(filters);
                    coBoLoc = true;
                }

                // Xử lý điều kiện ngày tháng
                if (tuNgay.HasValue)
                {
                    // Chuẩn hóa giờ bắt đầu từ 00:00:00
                    DateTime ngayBatDau = new DateTime(tuNgay.Value.Year, tuNgay.Value.Month, tuNgay.Value.Day, 0, 0, 0);
                    query = query.Filter("ngay_sua", Supabase.Postgrest.Constants.Operator.GreaterThanOrEqual, ngayBatDau);
                    coBoLoc = true;
                }

                if (denNgay.HasValue)
                {
                    // Chuẩn hóa giờ kết thúc đến 23:59:59
                    DateTime ngayKetThuc = new DateTime(denNgay.Value.Year, denNgay.Value.Month, denNgay.Value.Day, 23, 59, 59);
                    query = query.Filter("ngay_sua", Supabase.Postgrest.Constants.Operator.LessThanOrEqual, ngayKetThuc);
                    coBoLoc = true;
                }

                // Nếu không có bộ lọc nào được áp dụng, lấy tất cả
                if (!coBoLoc)
                {
                    System.Diagnostics.Debug.WriteLine("Không có bộ lọc nào được áp dụng, lấy tất cả dữ liệu");
                }

                // Thực hiện truy vấn và trả về kết quả
                var response = await query.Get();

                System.Diagnostics.Debug.WriteLine($"Tìm được {response.Models.Count} kết quả");

                // Đối với tìm kiếm, cũng nên bổ sung thông tin đầy đủ cho các mục
                var ketQua = response.Models;
                await BoSungThongTinChiTiet(ketQua);

                return ketQua;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi tìm kiếm: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw new Exception($"Lỗi khi tìm kiếm lịch sử sửa chữa: {ex.Message}");
            }
        }

        /// <summary>
        /// Phương thức hỗ trợ để bổ sung thông tin chi tiết cho danh sách lịch sử
        /// </summary>
        private async Task BoSungThongTinChiTiet(List<LichSuSuaChua> danhSachLichSu)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Lấy danh sách tất cả mã tài sản từ lịch sử
                var maTaiSanList = danhSachLichSu
                    .Where(ls => ls.MaTaiSan.HasValue)
                    .Select(ls => ls.MaTaiSan.Value)
                    .Distinct()
                    .ToList();

                // Lấy thông tin tất cả tài sản liên quan
                if (maTaiSanList.Any())
                {
                    var taiSanList = await client.From<TaiSanModel>()
                        .Filter("ma_tai_san", Supabase.Postgrest.Constants.Operator.In, maTaiSanList)
                        .Get();
                    var danhSachTaiSan = taiSanList.Models;

                    // Bổ sung thông tin tài sản vào đối tượng lịch sử
                    foreach (var lichSu in danhSachLichSu)
                    {
                        if (lichSu.MaTaiSan.HasValue)
                        {
                            var taiSan = danhSachTaiSan.FirstOrDefault(ts => ts.MaTaiSan == lichSu.MaTaiSan.Value);
                            if (taiSan != null)
                            {
                                lichSu.TenTaiSan = taiSan.TenTaiSan;
                                lichSu.SoSeri = taiSan.SoSeri;
                                lichSu.TinhTrangTaiSan = taiSan.TinhTrangSP;
                            }
                        }
                    }
                }

                // Lấy thông tin nhân viên
                var maNhanVienList = danhSachLichSu
                    .Where(ls => ls.MaNV.HasValue)
                    .Select(ls => ls.MaNV.Value)
                    .Distinct()
                    .ToList();

                if (maNhanVienList.Any())
                {
                    var nhanVienList = await client.From<NhanVienModel>()
                        .Filter("ma_nv", Supabase.Postgrest.Constants.Operator.In, maNhanVienList)
                        .Get();
                    var danhSachNhanVien = nhanVienList.Models;

                    // Bổ sung thông tin nhân viên
                    foreach (var lichSu in danhSachLichSu)
                    {
                        if (lichSu.MaNV.HasValue)
                        {
                            var nhanVien = danhSachNhanVien.FirstOrDefault(nv => nv.MaNV == lichSu.MaNV.Value);
                            if (nhanVien != null)
                            {
                                lichSu.TenNguoiThucHien = nhanVien.TenNV;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi bổ sung thông tin chi tiết: {ex.Message}");
                // Không ném ngoại lệ để không làm ảnh hưởng đến luồng chính
            }
        }
    }
}