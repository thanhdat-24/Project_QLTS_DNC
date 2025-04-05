using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using Project_QLTS_DNC.Models;

namespace Project_QLTS_DNC.Repositories
{
    public class PhieuBaoHongRepository
    {
        private readonly string _connectionString;

        public PhieuBaoHongRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Lấy danh sách phiếu báo hỏng
        public List<PhieuBaoHong> LayDanhSachPhieuBaoHong()
        {
            var danhSach = new List<PhieuBaoHong>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT 
                        b.MaBaoCao, 
                        b.MaSP, 
                        s.TenSanPham,
                        b.NguoiBaoCao, 
                        b.NgayBaoCao, 
                        b.MoTaLoi, 
                        b.TrangThai,
                        b.HinhThucGhiNhan,
                        nv.TenNV as NhanVienXuLy
                    FROM BaoCaoHongHoc b
                    LEFT JOIN SanPham s ON b.MaSP = s.MaSP
                    LEFT JOIN NhanVien nv ON b.MaNhanVien = nv.MaNV
                    ORDER BY b.NgayBaoCao DESC";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            danhSach.Add(new PhieuBaoHong
                            {
                                MaPhieu = reader["MaBaoCao"].ToString(),
                                MaTaiSan = reader["MaSP"].ToString(),
                                TenTaiSan = reader["TenSanPham"].ToString(),
                                NguoiLap = reader["NguoiBaoCao"].ToString(),
                                NgayLap = Convert.ToDateTime(reader["NgayBaoCao"]),
                                MoTa = reader["MoTaLoi"].ToString(),
                                TrangThai = reader["TrangThai"].ToString(),
                                MucDoHong = reader["HinhThucGhiNhan"].ToString(),
                                BoPhanQuanLy = reader["NhanVienXuLy"].ToString()
                            });
                        }
                    }
                }
            }
            return danhSach;
        }

        // Thêm mới phiếu báo hỏng
        public int ThemPhieuBaoHong(PhieuBaoHong phieu)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    INSERT INTO BaoCaoHongHoc 
                    (MaSP, NguoiBaoCao, NgayBaoCao, MoTaLoi, TrangThai, HinhThucGhiNhan, MaNhanVien) 
                    VALUES 
                    (@MaSP, @NguoiBaoCao, @NgayBaoCao, @MoTaLoi, @TrangThai, @HinhThucGhiNhan, @MaNhanVien)
                    RETURNING MaBaoCao";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MaSP", int.Parse(phieu.MaTaiSan));
                    command.Parameters.AddWithValue("@NguoiBaoCao", phieu.NguoiLap);
                    command.Parameters.AddWithValue("@NgayBaoCao", phieu.NgayLap);
                    command.Parameters.AddWithValue("@MoTaLoi", phieu.MoTa);
                    command.Parameters.AddWithValue("@TrangThai", phieu.TrangThai);
                    command.Parameters.AddWithValue("@HinhThucGhiNhan", phieu.MucDoHong);
                    command.Parameters.AddWithValue("@MaNhanVien", DBNull.Value); // Xử lý sau

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        // Cập nhật phiếu báo hỏng
        public bool CapNhatPhieuBaoHong(PhieuBaoHong phieu)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    UPDATE BaoCaoHongHoc 
                    SET 
                        MaSP = @MaSP, 
                        NguoiBaoCao = @NguoiBaoCao, 
                        NgayBaoCao = @NgayBaoCao, 
                        MoTaLoi = @MoTaLoi, 
                        TrangThai = @TrangThai,
                        HinhThucGhiNhan = @HinhThucGhiNhan
                    WHERE MaBaoCao = @MaBaoCao";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MaBaoCao", int.Parse(phieu.MaPhieu));
                    command.Parameters.AddWithValue("@MaSP", int.Parse(phieu.MaTaiSan));
                    command.Parameters.AddWithValue("@NguoiBaoCao", phieu.NguoiLap);
                    command.Parameters.AddWithValue("@NgayBaoCao", phieu.NgayLap);
                    command.Parameters.AddWithValue("@MoTaLoi", phieu.MoTa);
                    command.Parameters.AddWithValue("@TrangThai", phieu.TrangThai);
                    command.Parameters.AddWithValue("@HinhThucGhiNhan", phieu.MucDoHong);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        // Xóa phiếu báo hỏng
        public bool XoaPhieuBaoHong(string maPhieu)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = "DELETE FROM BaoCaoHongHoc WHERE MaBaoCao = @MaBaoCao";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MaBaoCao", int.Parse(maPhieu));
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        // Tìm kiếm phiếu báo hỏng
        public List<PhieuBaoHong> TimKiemPhieuBaoHong(string tuKhoa, string trangThai = null, string mucDoHong = null)
        {
            var danhSach = new List<PhieuBaoHong>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT 
                        b.MaBaoCao, 
                        b.MaSP, 
                        s.TenSanPham,
                        b.NguoiBaoCao, 
                        b.NgayBaoCao, 
                        b.MoTaLoi, 
                        b.TrangThai,
                        b.HinhThucGhiNhan
                    FROM BaoCaoHongHoc b
                    LEFT JOIN SanPham s ON b.MaSP = s.MaSP
                    WHERE 1=1 ";

                var parameters = new List<NpgsqlParameter>();

                if (!string.IsNullOrEmpty(tuKhoa))
                {
                    query += " AND (s.TenSanPham ILIKE @TuKhoa OR b.NguoiBaoCao ILIKE @TuKhoa)";
                    parameters.Add(new NpgsqlParameter("@TuKhoa", $"%{tuKhoa}%"));
                }

                if (!string.IsNullOrEmpty(trangThai))
                {
                    query += " AND b.TrangThai = @TrangThai";
                    parameters.Add(new NpgsqlParameter("@TrangThai", trangThai));
                }

                if (!string.IsNullOrEmpty(mucDoHong))
                {
                    query += " AND b.HinhThucGhiNhan = @MucDoHong";
                    parameters.Add(new NpgsqlParameter("@MucDoHong", mucDoHong));
                }

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            danhSach.Add(new PhieuBaoHong
                            {
                                MaPhieu = reader["MaBaoCao"].ToString(),
                                MaTaiSan = reader["MaSP"].ToString(),
                                TenTaiSan = reader["TenSanPham"].ToString(),
                                NguoiLap = reader["NguoiBaoCao"].ToString(),
                                NgayLap = Convert.ToDateTime(reader["NgayBaoCao"]),
                                MoTa = reader["MoTaLoi"].ToString(),
                                TrangThai = reader["TrangThai"].ToString(),
                                MucDoHong = reader["HinhThucGhiNhan"].ToString()
                            });
                        }
                    }
                }
            }
            return danhSach;
        }
    }
}