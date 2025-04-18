using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models.BanGiaoTaiSan;
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Models.QLTaiSan;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Supabase.Postgrest;
using static Supabase.Postgrest.Constants;
using Newtonsoft.Json.Linq;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Models.PhieuNhapKho;
using Project_QLTS_DNC.Services;
using static Supabase.Postgrest.Constants;
using Project_QLTS_DNC.Models.QLLoaiTS;
using Project_QLTS_DNC.View.QuanLyToanNha;
using System.Windows;

namespace Project_QLTS_DNC.Services.BanGiaoTaiSanService
{
    public class BanGiaoTaiSanService
    {
        /// <summary>
        /// Lấy danh sách phiếu bàn giao tài sản từ Supabase
        /// </summary>
        public static async Task<ObservableCollection<BanGiaoTaiSanDTO>> LayDanhSachPhieuBanGiaoAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<BanGiaoTaiSanModel>().Get();

                // Danh sách phiếu bàn giao
                var dsBanGiao = response.Models.ToList();

                // Lấy danh sách nhân viên để hiển thị tên
                var nhanVienResponse = await client.From<NhanVienModel>().Get();
                var dsNhanVien = nhanVienResponse.Models.ToList();

                // Lấy danh sách phòng để hiển thị tên
                var phongResponse = await client.From<Phong>().Get();
                var dsPhong = phongResponse.Models.ToList();

                // Lấy danh sách tầng để xác định tòa nhà
                var tangResponse = await client.From<Tang>().Get();
                var dsTang = tangResponse.Models.ToList();

                // Lấy danh sách tòa nhà
                var toaNhaResponse = await client.From<ToaNha>().Get();
                var dsToaNha = toaNhaResponse.Models.ToList();

                // Chuyển đổi từ model sang DTO
                var dsBanGiaoDTO = new ObservableCollection<BanGiaoTaiSanDTO>();

                foreach (var bangiao in dsBanGiao)
                {
                    var banGiaoDTO = new BanGiaoTaiSanDTO
                    {
                        MaBanGiaoTS = bangiao.MaBanGiaoTS,
                        NgayBanGiao = bangiao.NgayBanGiao,
                        MaNV = bangiao.MaNV,
                        TenNV = dsNhanVien.FirstOrDefault(nv => nv.MaNV == bangiao.MaNV)?.TenNV ?? $"#{bangiao.MaNV}",
                        MaPhong = bangiao.MaPhong,
                        NoiDung = bangiao.NoiDung,
                        TrangThai = bangiao.TrangThai
                    };

                    // Thêm thông tin phòng và tòa nhà nếu có
                    if (bangiao.MaPhong.HasValue)
                    {
                        var phong = dsPhong.FirstOrDefault(p => p.MaPhong == bangiao.MaPhong.Value);
                        if (phong != null)
                        {
                            banGiaoDTO.TenPhong = phong.TenPhong;

                            // Lấy thông tin tầng
                            var tang = dsTang.FirstOrDefault(t => t.MaTang == phong.MaTang);
                            if (tang != null)
                            {
                                // Lấy thông tin tòa nhà
                                var toaNha = dsToaNha.FirstOrDefault(tn => tn.MaToaNha == tang.MaToa);
                                if (toaNha != null)
                                {
                                    banGiaoDTO.MaToaNha = toaNha.MaToaNha;
                                    banGiaoDTO.TenToaNha = toaNha.TenToaNha;
                                }
                            }
                        }
                    }

                    dsBanGiaoDTO.Add(banGiaoDTO);
                }

                return dsBanGiaoDTO;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy danh sách phiếu bàn giao: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Lấy danh sách chi tiết phiếu bàn giao
        /// </summary>
        public static async Task<ObservableCollection<ChiTietBanGiaoDTO>> LayDanhSachChiTietBanGiaoAsync(int maBanGiao)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Lấy danh sách chi tiết bàn giao
                var response = await client.From<ChiTietBanGiaoModel>()
                    .Filter("ma_bang_giao_ts", Operator.Equals, maBanGiao)
                    .Get();

                // Danh sách chi tiết
                var dsChiTiet = response.Models.ToList();

                // Lấy thông tin tài sản
                var taiSanResponse = await client.From<TaiSanModel>().Get();
                var dsTaiSan = taiSanResponse.Models.ToList();

                // Lấy thông tin nhóm tài sản (từ chi tiết phiếu nhập)
                var chiTietPNResponse = await client.From<ChiTietPhieuNhap>().Get();
                var dsChiTietPN = chiTietPNResponse.Models.ToList();

                // Lấy thông tin nhóm tài sản
                var nhomTaiSanResponse = await client.From<NhomTaiSan>().Get();
                var dsNhomTaiSan = nhomTaiSanResponse.Models.ToList();

                // Chuyển đổi từ model sang DTO
                var dsChiTietDTO = new ObservableCollection<ChiTietBanGiaoDTO>();

                foreach (var chiTiet in dsChiTiet)
                {
                    var taiSan = dsTaiSan.FirstOrDefault(ts => ts.MaTaiSan == chiTiet.MaTaiSan);

                    var chiTietDTO = new ChiTietBanGiaoDTO
                    {
                        MaChiTietBG = chiTiet.MaChiTietBG,
                        MaBanGiaoTS = chiTiet.MaBanGiaoTS,
                        MaTaiSan = chiTiet.MaTaiSan,
                        TenTaiSan = taiSan?.TenTaiSan ?? $"#{chiTiet.MaTaiSan}",
                        SoSeri = taiSan?.SoSeri ?? string.Empty,
                        ViTriTS = chiTiet.ViTriTS,
                        GhiChu = chiTiet.GhiChu
                    };

                    // Lấy thông tin nhóm tài sản
                    if (taiSan?.MaChiTietPN != null)
                    {
                        var chiTietPN = dsChiTietPN.FirstOrDefault(ct => ct.MaChiTietPN == taiSan.MaChiTietPN);
                        if (chiTietPN != null)
                        {
                            chiTietDTO.MaNhomTS = chiTietPN.MaNhomTS;

                            var nhomTS = dsNhomTaiSan.FirstOrDefault(n => n.MaNhomTS == chiTietPN.MaNhomTS);
                            if (nhomTS != null)
                            {
                                chiTietDTO.TenNhomTS = nhomTS.TenNhom;
                            }
                        }
                    }

                    dsChiTietDTO.Add(chiTietDTO);
                }

                return dsChiTietDTO;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy danh sách chi tiết bàn giao: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Lấy danh sách phòng kèm thông tin tòa nhà
        /// </summary>
        public static async Task<ObservableCollection<PhongBanGiaoFilter>> LayDanhSachPhongBanGiaoAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Lấy danh sách phòng
                var phongResponse = await client.From<Phong>().Get();
                var dsPhong = phongResponse.Models.ToList();

                // Lấy danh sách tầng
                var tangResponse = await client.From<Tang>().Get();
                var dsTang = tangResponse.Models.ToList();

                // Lấy danh sách tòa nhà
                var toaNhaResponse = await client.From<ToaNha>().Get();
                var dsToaNha = toaNhaResponse.Models.ToList();

                // Chuyển đổi sang DTO
                var dsPhongFilter = new ObservableCollection<PhongBanGiaoFilter>();

                foreach (var phong in dsPhong)
                {
                    var tang = dsTang.FirstOrDefault(t => t.MaTang == phong.MaTang);
                    if (tang == null) continue;

                    var toaNha = dsToaNha.FirstOrDefault(tn => tn.MaToaNha == tang.MaToa);
                    if (toaNha == null) continue;

                    dsPhongFilter.Add(new PhongBanGiaoFilter
                    {
                        MaPhong = phong.MaPhong,
                        TenPhong = phong.TenPhong,
                        MaTang = phong.MaTang,
                        TenTang = tang.TenTang,
                        MaToaNha = toaNha.MaToaNha,
                        TenToaNha = toaNha.TenToaNha
                    });
                }

                return dsPhongFilter;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy danh sách phòng bàn giao: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Lấy danh sách kho kèm thông tin tòa nhà
        /// </summary>
        public static async Task<ObservableCollection<KhoBanGiaoFilter>> LayDanhSachKhoBanGiaoAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Lấy danh sách kho
                var khoResponse = await client.From<Kho>().Get();
                var dsKho = khoResponse.Models.ToList();

                // Lấy danh sách tòa nhà
                var toaNhaResponse = await client.From<ToaNha>().Get();
                var dsToaNha = toaNhaResponse.Models.ToList();

                // Chuyển đổi sang DTO
                var dsKhoFilter = new ObservableCollection<KhoBanGiaoFilter>();

                foreach (var kho in dsKho)
                {
                    var toaNha = dsToaNha.FirstOrDefault(tn => tn.MaToaNha == kho.MaToaNha);
                    if (toaNha == null) continue;

                    dsKhoFilter.Add(new KhoBanGiaoFilter
                    {
                        MaKho = kho.MaKho,
                        TenKho = kho.TenKho,
                        MaToaNha = toaNha.MaToaNha,
                        TenToaNha = toaNha.TenToaNha
                    });
                }

                return dsKhoFilter;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy danh sách kho bàn giao: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Lấy danh sách tài sản trong kho (mã phòng = null) và thuộc tòa nhà cụ thể
        /// </summary>
        public static async Task<ObservableCollection<TaiSanKhoBanGiaoDTO>> LayDanhSachTaiSanTrongKhoAsync(int maToaNha)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Lấy danh sách tài sản không có mã phòng (đang ở kho)
                var taiSanResponse = await client.From<TaiSanModel>()
                    .Filter("ma_phong", Operator.Is, "null")
                    .Get();
                var dsTaiSan = taiSanResponse.Models.ToList();

                // Lấy danh sách chi tiết phiếu nhập để biết mã nhóm tài sản
                var chiTietPNResponse = await client.From<ChiTietPhieuNhap>().Get();
                var dsChiTietPN = chiTietPNResponse.Models.ToList();

                // Lấy danh sách phiếu nhập để biết kho
                var phieuNhapResponse = await client.From<PhieuNhap>().Get();
                var dsPhieuNhap = phieuNhapResponse.Models.ToList();

                // Lấy danh sách kho để biết tòa nhà
                var khoResponse = await client.From<Kho>().Get();
                var dsKho = khoResponse.Models.ToList();

                // Lấy danh sách nhóm tài sản để biết tên nhóm
                var nhomTaiSanResponse = await client.From<NhomTaiSan>().Get();
                var dsNhomTaiSan = nhomTaiSanResponse.Models.ToList();

                // Chuyển đổi sang DTO
                var dsTaiSanKhoBanGiao = new ObservableCollection<TaiSanKhoBanGiaoDTO>();

                foreach (var taiSan in dsTaiSan)
                {
                    // Lấy thông tin chi tiết phiếu nhập
                    var chiTietPN = dsChiTietPN.FirstOrDefault(ct => ct.MaChiTietPN == taiSan.MaChiTietPN);
                    if (chiTietPN == null) continue;

                    // Lấy thông tin phiếu nhập
                    var phieuNhap = dsPhieuNhap.FirstOrDefault(pn => pn.MaPhieuNhap == chiTietPN.MaPhieuNhap);
                    if (phieuNhap == null) continue;

                    // Lấy thông tin kho
                    var kho = dsKho.FirstOrDefault(k => k.MaKho == phieuNhap.MaKho);
                    if (kho == null) continue;

                    // Kiểm tra kho có thuộc tòa nhà đã chọn không
                    if (kho.MaToaNha != maToaNha) continue;

                    // Lấy thông tin nhóm tài sản
                    var nhomTaiSan = dsNhomTaiSan.FirstOrDefault(n => n.MaNhomTS == chiTietPN.MaNhomTS);

                    // Tạo DTO
                    dsTaiSanKhoBanGiao.Add(new TaiSanKhoBanGiaoDTO
                    {
                        MaTaiSan = taiSan.MaTaiSan,
                        MaChiTietPN = taiSan.MaChiTietPN,
                        TenTaiSan = taiSan.TenTaiSan,
                        SoSeri = taiSan.SoSeri,
                        MaQR = taiSan.MaQR,
                        NgaySuDung = taiSan.NgaySuDung,
                        HanBH = taiSan.HanBH,
                        TinhTrangSP = taiSan.TinhTrangSP,
                        MaNhomTS = chiTietPN.MaNhomTS,
                        TenNhomTS = nhomTaiSan?.TenNhom ?? $"#{chiTietPN.MaNhomTS}",
                        MaKho = kho.MaKho,
                        TenKho = kho.TenKho,
                        IsSelected = false
                    });
                }

                return dsTaiSanKhoBanGiao;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy danh sách tài sản trong kho: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sinh mã phiếu bàn giao tự động
        /// </summary>
        private static async Task<int> SinhMaBanGiaoTSAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Lấy danh sách phiếu bàn giao hiện có
                var response = await client.From<BanGiaoTaiSanModel>().Get();
                var dsBanGiao = response.Models.ToList();

                // Nếu không có dữ liệu, bắt đầu từ 1
                if (dsBanGiao == null || dsBanGiao.Count == 0)
                    return 1;

                // Tìm mã lớn nhất trong danh sách
                int maxMa = dsBanGiao.Max(bg => bg.MaBanGiaoTS);

                // Trả về mã mới = mã lớn nhất + 1
                return maxMa + 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi sinh mã phiếu bàn giao: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Thêm mới phiếu bàn giao tài sản
        /// </summary>
        public static async Task<BanGiaoTaiSanModel> ThemPhieuBanGiaoAsync(BanGiaoTaiSanModel banGiao)
        {
            try
            {
                // Sinh mã tự động nếu chưa có
                if (banGiao.MaBanGiaoTS == 0)
                {
                    banGiao.MaBanGiaoTS = await SinhMaBanGiaoTSAsync();
                }

                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<BanGiaoTaiSanModel>().Insert(banGiao);

                if (response.Models.Count > 0)
                {
                    return response.Models.First();
                }
                else
                {
                    throw new Exception("Không thể thêm phiếu bàn giao mới.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi thêm phiếu bàn giao: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sinh mã chi tiết bàn giao tự động
        /// </summary>
        private static async Task<int> SinhMaChiTietBanGiaoAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Lấy danh sách chi tiết bàn giao hiện có
                var response = await client.From<ChiTietBanGiaoModel>().Get();
                var dsChiTietBanGiao = response.Models.ToList();

                // Nếu không có dữ liệu, bắt đầu từ 1
                if (dsChiTietBanGiao == null || dsChiTietBanGiao.Count == 0)
                    return 1;

                // Tìm mã lớn nhất trong danh sách
                int maxMa = dsChiTietBanGiao.Max(ct => ct.MaChiTietBG);

                // Trả về mã mới = mã lớn nhất + 1
                return maxMa + 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi sinh mã chi tiết bàn giao: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Thêm chi tiết bàn giao
        /// </summary>
        public static async Task<List<ChiTietBanGiaoModel>> ThemChiTietBanGiaoAsync(List<ChiTietBanGiaoModel> dsChiTiet)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Kiểm tra trùng lặp tài sản trong cùng một phiếu bàn giao
                var dsTaiSanTrung = dsChiTiet
                    .GroupBy(ct => ct.MaTaiSan)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (dsTaiSanTrung.Any())
                {
                    throw new Exception($"Các tài sản {string.Join(", ", dsTaiSanTrung)} bị trùng lặp trong phiếu bàn giao.");
                }

                // Kiểm tra tài sản đã tồn tại trong chi tiết bàn giao khác chưa
                var dsMaTaiSan = dsChiTiet.Select(ct => ct.MaTaiSan).ToList();
                var responseKiemTra = await client.From<ChiTietBanGiaoModel>()
                    .Filter("ma_tai_san", Operator.In, dsMaTaiSan)
                    .Get();

                if (responseKiemTra.Models.Any())
                {
                    var dsTaiSanDaBanGiao = responseKiemTra.Models
                        .Select(ct => ct.MaTaiSan)
                        .ToList();
                    throw new Exception($"Các tài sản {string.Join(", ", dsTaiSanDaBanGiao)} đã tồn tại trong phiếu bàn giao khác.");
                }

                // Danh sách kết quả trả về
                var dsChiTietResult = new List<ChiTietBanGiaoModel>();

                // Thêm từng chi tiết bàn giao riêng lẻ
                foreach (var chiTiet in dsChiTiet)
                {
                    // Sinh mã tự động cho chi tiết bàn giao
                    int maChiTietBG = await SinhMaChiTietBanGiaoAsync();

                    // Tạo đối tượng mới để insert
                    var chiTietInsert = new ChiTietBanGiaoModel
                    {
                        MaChiTietBG = maChiTietBG, // Thêm mã tự sinh
                        MaBanGiaoTS = chiTiet.MaBanGiaoTS,
                        MaTaiSan = chiTiet.MaTaiSan,
                        ViTriTS = chiTiet.ViTriTS,
                        GhiChu = chiTiet.GhiChu
                    };

                    // Thực hiện insert từng bản ghi
                    var response = await client.From<ChiTietBanGiaoModel>().Insert(chiTietInsert);

                    if (response.Models.Count > 0)
                    {
                        dsChiTietResult.Add(response.Models.First());
                    }
                    else
                    {
                        throw new Exception($"Không thể thêm chi tiết bàn giao cho tài sản {chiTiet.MaTaiSan}.");
                    }
                }

                return dsChiTietResult;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi thêm chi tiết bàn giao: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Kiểm tra tòa nhà của phòng và kho có trùng nhau không
        /// </summary>
        public static async Task<bool> KiemTraPhongVaKhoTrungToaNhaAsync(int maPhong, int maKho)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Lấy thông tin phòng
                var phongResponse = await client.From<Phong>()
                    .Filter("ma_phong", Operator.Equals, maPhong)
                    .Get();

                var phong = phongResponse.Models.FirstOrDefault();
                if (phong == null)
                    throw new Exception($"Không tìm thấy phòng với mã {maPhong}");

                // Lấy thông tin tầng của phòng
                var tangResponse = await client.From<Tang>()
                    .Filter("ma_tang", Operator.Equals, phong.MaTang)
                    .Get();

                var tang = tangResponse.Models.FirstOrDefault();
                if (tang == null)
                    throw new Exception($"Không tìm thấy tầng với mã {phong.MaTang}");

                // Lấy thông tin kho
                var khoResponse = await client.From<Kho>()
                    .Filter("ma_kho", Operator.Equals, maKho)
                    .Get();

                var kho = khoResponse.Models.FirstOrDefault();
                if (kho == null)
                    throw new Exception($"Không tìm thấy kho với mã {maKho}");

                // So sánh mã tòa nhà
                return tang.MaToa == kho.MaToaNha;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi kiểm tra tòa nhà: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Lấy thông tin sức chứa của phòng theo nhóm tài sản
        /// </summary>
        public static async Task<int> LaySucChuaTheoNhomAsync(int maPhong, int maNhomTS)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Lấy thông tin sức chứa theo nhóm
                var response = await client.Rpc("lay_suc_chua_phong_nhom", new Dictionary<string, object>
                {
                    { "p_ma_phong", maPhong },
                    { "p_ma_nhom_ts", maNhomTS }
                });

                var responseObj = JObject.Parse(response.Content.ToString());
                if (responseObj.TryGetValue("suc_chua", out JToken sucChuaToken))
                {
                    return sucChuaToken.Value<int>();
                }

                return 0; // Trả về 0 nếu không có thông tin sức chứa
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy sức chứa: {ex.Message}");
                return 0; // Trả về 0 nếu không tìm thấy
            }
        }

        /// <summary>
        /// Duyệt phiếu bàn giao
        /// </summary>
        public static async Task<bool> DuyetPhieuBanGiaoAsync(int maBanGiao, bool trangThai)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Lấy thông tin phiếu bàn giao hiện tại
                var banGiaoResponse = await client.From<BanGiaoTaiSanModel>()
                    .Filter("ma_bang_giao_ts", Operator.Equals, maBanGiao)
                    .Get();

                var banGiao = banGiaoResponse.Models.FirstOrDefault();

                if (banGiao == null)
                    throw new Exception($"Không tìm thấy phiếu bàn giao với mã {maBanGiao}");

                // Cập nhật trạng thái
                banGiao.TrangThai = trangThai;

                // Lưu thay đổi
                var response = await client.From<BanGiaoTaiSanModel>()
                    .Filter("ma_bang_giao_ts", Operator.Equals, maBanGiao)
                    .Update(banGiao);

                return response.Models.Count > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi duyệt phiếu bàn giao: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Xóa phiếu bàn giao
        /// </summary>
        public static async Task<bool> XoaPhieuBanGiaoAsync(int maBanGiao)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Xóa chi tiết bàn giao trước
                await client.From<ChiTietBanGiaoModel>()
                    .Filter("ma_bang_giao_ts", Operator.Equals, maBanGiao)
                    .Delete();

                // Xóa phiếu bàn giao
                await client.From<BanGiaoTaiSanModel>()
                    .Filter("ma_bang_giao_ts", Operator.Equals, maBanGiao)
                    .Delete();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi xóa phiếu bàn giao: {ex.Message}");
                throw;
            }
        }

        public static async Task<bool> KiemTraVaYeuCauCauHinhSucChuaAsync(int maPhong, List<TaiSanKhoBanGiaoDTO> dsTaiSan)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Lấy danh sách nhóm tài sản cần kiểm tra (từ tài sản được chọn)
                var dsNhomTS = dsTaiSan
                    .Where(ts => ts.IsSelected && ts.MaNhomTS.HasValue)
                    .Select(ts => ts.MaNhomTS.Value)
                    .Distinct()
                    .ToList();

                // Nếu không có nhóm tài sản nào cần kiểm tra
                if (!dsNhomTS.Any())
                {
                    return true;
                }

                // Lấy thông tin các nhóm tài sản
                var nhomTaiSanResponse = await client.From<NhomTaiSan>()
                    .Filter("ma_nhom_ts", Supabase.Postgrest.Constants.Operator.In, dsNhomTS)
                    .Get();
                var dsNhomTaiSan = nhomTaiSanResponse.Models.ToList();

                // Lấy mã loại tài sản từ các nhóm
                var dsLoaiTS = dsNhomTaiSan.Select(n => n.MaLoaiTaiSan).Distinct().ToList();

                // Lấy thông tin loại tài sản để biết thuộc tính quản lý riêng
                var loaiTaiSanResponse = await client.From<LoaiTaiSan>()
                    .Filter("ma_loai_ts", Supabase.Postgrest.Constants.Operator.In, dsLoaiTS)
                    .Get();
                var dsLoaiTaiSan = loaiTaiSanResponse.Models.ToList();

                // Lọc ra các nhóm tài sản thuộc loại quản lý riêng (quan_ly_rieng = true)
                var dsNhomTSQuanLyRieng = new List<int>();
                foreach (var nhomTS in dsNhomTaiSan)
                {
                    var loaiTS = dsLoaiTaiSan.FirstOrDefault(l => l.MaLoaiTaiSan == nhomTS.MaLoaiTaiSan);
                    if (loaiTS != null && loaiTS.QuanLyRieng)
                    {
                        dsNhomTSQuanLyRieng.Add(nhomTS.MaNhomTS);
                    }
                }

                // Nếu không có nhóm tài sản nào cần quản lý riêng
                if (!dsNhomTSQuanLyRieng.Any())
                {
                    return true;
                }

                // Lấy thông tin sức chứa đã cấu hình cho phòng
                var sucChuaResponse = await client.From<PhongNhomTS>()
                    .Filter("ma_phong", Supabase.Postgrest.Constants.Operator.Equals, maPhong)
                    .Get();
                var dsSucChua = sucChuaResponse.Models.ToList();

                // Kiểm tra từng nhóm tài sản cần quản lý riêng
                foreach (var maNhomTS in dsNhomTSQuanLyRieng)
                {
                    // Kiểm tra xem phòng đã cấu hình sức chứa cho nhóm này chưa
                    var sucChua = dsSucChua.FirstOrDefault(sc => sc.MaNhomTS == maNhomTS);
                    if (sucChua == null)
                    {
                        // Lấy tên nhóm tài sản để hiển thị
                        var nhomTS = dsNhomTaiSan.FirstOrDefault(n => n.MaNhomTS == maNhomTS);
                        string tenNhom = nhomTS?.TenNhom ?? $"Mã nhóm: {maNhomTS}";

                        // Hiện hộp thoại hỏi người dùng có muốn cấu hình sức chứa không
                        MessageBoxResult result = MessageBox.Show(
                            $"Phòng chưa được cấu hình sức chứa cho nhóm tài sản '{tenNhom}'.\nBạn có muốn cấu hình ngay không?",
                            "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                        if (result == MessageBoxResult.Yes)
                        {
                            // Mở form cấu hình sức chứa
                            var configWindow = new suc_chua_phong_nhom(maPhong);
                            configWindow.ShowDialog();

                            // Kiểm tra lại sau khi cấu hình
                            var checkAgain = await client.From<PhongNhomTS>()
                                .Filter("ma_phong", Supabase.Postgrest.Constants.Operator.Equals, maPhong)
                                .Filter("ma_nhom_ts", Supabase.Postgrest.Constants.Operator.Equals, maNhomTS)
                                .Get();

                            if (checkAgain.Models.Count == 0)
                            {
                                // Người dùng đã đóng form nhưng chưa cấu hình
                                MessageBox.Show(
                                    $"Phòng vẫn chưa được cấu hình sức chứa cho nhóm tài sản '{tenNhom}'.\nKhông thể tiếp tục bàn giao!",
                                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                return false;
                            }
                        }
                        else
                        {
                            // Người dùng không muốn cấu hình
                            MessageBox.Show(
                                $"Không thể bàn giao tài sản thuộc nhóm '{tenNhom}' vì phòng chưa được cấu hình sức chứa.",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi kiểm tra sức chứa: {ex.Message}");
                MessageBox.Show($"Lỗi khi kiểm tra sức chứa: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}