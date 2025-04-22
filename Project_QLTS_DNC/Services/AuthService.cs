using Supabase;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Helpers;
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services.TaiKhoan;

namespace Project_QLTS_DNC.Services
{
    public class AuthService
    {
        private Supabase.Client _client;

        public AuthService() { }

        private async Task<Supabase.Client> GetClientAsync()
        {
            if (_client == null)
                _client = await SupabaseService.GetClientAsync();
            return _client;
        }

        // Đăng nhập bằng bảng "taikhoan"


        //public async Task<TaiKhoanModel?> DangNhapAsync(string tenTaiKhoan, string matKhau)
        //{
        //    var client = await GetClientAsync();

        //    var taiKhoan = await client
        //        .From<TaiKhoanModel>()
        //        .Where(x => x.TenTaiKhoan == tenTaiKhoan && x.MatKhau == matKhau)
        //        .Single();

        //    if (taiKhoan == null || !taiKhoan.TrangThai)
        //        return null;

        //    var nhanVien = await client
        //        .From<NhanVienModel>()
        //        .Where(x => x.MaNV == taiKhoan.MaNv)
        //        .Single();

        //    var phongBan = await client
        //        .From<PhongBan>() // ← đúng tên class
        //        .Where(x => x.MaPhongBan == nhanVien.MaPB)
        //        .Single();


        //    // Gán vào biến toàn cục
        //    ThongTinDangNhap.TaiKhoanDangNhap = taiKhoan;
        //    ThongTinDangNhap.MaPhongBan = phongBan.MaPhongBan;
        //    ThongTinDangNhap.MaToaNha = phongBan.MaToa;

        //    //// Gán quyền
        //    //var phanQuyenService = new PhanQuyenService();
        //    //var danhSachQuyen = await phanQuyenService.LayDanhSachQuyenTheoLoaiTkAsync(taiKhoan.MaLoaiTk);
        //    //QuyenNguoiDungHelper.DanhSachQuyen = danhSachQuyen;
        //    // Lấy danh sách kho theo tòa nhà
        //    var dsKho = await client
        //        .From<Kho>()
        //        .Where(x => x.MaToaNha == phongBan.MaToa)
        //        .Get();

        //    ThongTinDangNhap.DanhSachKhoTheoToa = dsKho.Models.Select(k => k.MaKho).ToList();

        //    return taiKhoan;
        //}

        public async Task<TaiKhoanModel?> DangNhapAsync(string tenTaiKhoan, string matKhau)
        {
            var client = await GetClientAsync();

            var taiKhoan = await client
                .From<TaiKhoanModel>()
                .Where(x => x.TenTaiKhoan == tenTaiKhoan && x.MatKhau == matKhau)
                .Single();

            if (taiKhoan == null || !taiKhoan.TrangThai)
                return null;

            var nhanVien = await client
                .From<NhanVienModel>()
                .Where(x => x.MaNV == taiKhoan.MaNv)
                .Single();

            var phongBan = await client
                .From<PhongBan>()
                .Where(x => x.MaPhongBan == nhanVien.MaPB)
                .Single();

            // Gán thông tin toàn cục
            ThongTinDangNhap.TaiKhoanDangNhap = taiKhoan;
            ThongTinDangNhap.MaPhongBan = phongBan.MaPhongBan;
            ThongTinDangNhap.MaToaNha = phongBan.MaToa;

            // 🔐 Gán quyền phân quyền vào hệ thống
            var phanQuyenService = new PhanQuyenService();
            var danhSachQuyen = await phanQuyenService.LayDanhSachQuyenTheoLoaiTkAsync(taiKhoan.MaLoaiTk);
            QuyenNguoiDungHelper.DanhSachQuyen = danhSachQuyen;

            // (Tuỳ chọn) Log debug:
            QuyenNguoiDungHelper.LogDanhSachQuyen();

            // Gán danh sách kho theo tòa nhà
            var dsKho = await client
                .From<Kho>()
                .Where(x => x.MaToaNha == phongBan.MaToa)
                .Get();

            ThongTinDangNhap.DanhSachKhoTheoToa = dsKho.Models.Select(k => k.MaKho).ToList();

            return taiKhoan;
        }


        // Lấy tên loại tài khoản từ mã loại
        public async Task<string?> LayTenLoaiTaiKhoanTheoMaLoai(int maLoai)
        {
            var client = await GetClientAsync();
            var loaiTaiKhoan = await client
                .From<LoaiTaiKhoanModel>()
                .Where(x => x.MaLoaiTk == maLoai)
                .Single();

            return loaiTaiKhoan?.TenLoaiTk;
        }

       
    }
}
