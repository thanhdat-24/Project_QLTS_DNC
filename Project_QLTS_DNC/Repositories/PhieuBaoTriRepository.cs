using System;
using System.Collections.Generic;
using Npgsql;
using Project_QLTS_DNC.Models.BaoTri;

namespace Project_QLTS_DNC.Repositories
{
    public class PhieuBaoTriRepository
    {
        private readonly string _connectionString;

        public PhieuBaoTriRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Lấy danh sách phiếu bảo trì
        public List<PhieuBaoTri> LayDanhSachPhieuBaoTri()
        {
            var danhSach = new List<PhieuBaoTri>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT 
                        MaBaoTri as MaPhieu,
                        MaSP as MaTaiSan,
                        s.TenSanPham as TenTaiSan,
                        NgayBaoTri,
                        NgayHoanThanh,
                        MaNhanVien as NguoiPhuTrach,
                        TinhTrang as TrangThai,
                        ChiPhi as ChiPhiDuKien,
                        NoiDung as NoiDungBaoTri
                    FROM BaoTri b
                    LEFT JOIN SanPham s ON b.MaSP = s.MaSP
                    ORDER BY NgayBaoTri DESC";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            danhSach.Add(new PhieuBaoTri
                            {
                                MaPhieu = reader["MaPhieu"].ToString(),
                                MaTaiSan = reader["MaTaiSan"].ToString(),
                                TenTaiSan = reader["TenTaiSan"].ToString(),
                                NgayBaoTri = Convert.ToDateTime(reader["NgayBaoTri"]),
                                NgayHoanThanh = reader["NgayHoanThanh"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["NgayHoanThanh"])
                                    : (DateTime?)null,
                                NguoiPhuTrach = reader["NguoiPhuTrach"].ToString(),
                                TrangThai = reader["TrangThai"].ToString(),
                                ChiPhiDuKien = Convert.ToDecimal(reader["ChiPhiDuKien"]),
                                NoiDungBaoTri = reader["NoiDungBaoTri"].ToString()
                            });
                        }
                    }
                }
            }
            return danhSach;
        }

        // Thêm phiếu bảo trì mới
        public int ThemPhieuBaoTri(PhieuBaoTri phieu)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    INSERT INTO BaoTri 
                    (MaSP, NgayBaoTri, NgayHoanThanh, MaNhanVien, TinhTrang, ChiPhi, NoiDung) 
                    VALUES 
                    (@MaSP, @NgayBaoTri, @NgayHoanThanh, @MaNhanVien, @TinhTrang, @ChiPhi, @NoiDung)
                    RETURNING MaBaoTri";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MaSP", int.Parse(phieu.MaTaiSan));
                    command.Parameters.AddWithValue("@NgayBaoTri", phieu.NgayBaoTri);
                    command.Parameters.AddWithValue("@NgayHoanThanh",
                        (object)phieu.NgayHoanThanh ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MaNhanVien", DBNull.Value); // Xử lý sau
                    command.Parameters.AddWithValue("@TinhTrang", phieu.TrangThai);
                    command.Parameters.AddWithValue("@ChiPhi", phieu.ChiPhiDuKien);
                    command.Parameters.AddWithValue("@NoiDung", phieu.NoiDungBaoTri);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        // Cập nhật phiếu bảo trì
        public bool CapNhatPhieuBaoTri(PhieuBaoTri phieu)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    UPDATE BaoTri 
                    SET 
                        MaSP = @MaSP, 
                        NgayBaoTri = @NgayBaoTri, 
                        NgayHoanThanh = @NgayHoanThanh, 
                        TinhTrang = @TinhTrang, 
                        ChiPhi = @ChiPhi, 
                        NoiDung = @NoiDung
                    WHERE MaBaoTri = @MaBaoTri";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MaBaoTri", int.Parse(phieu.MaPhieu));
                    command.Parameters.AddWithValue("@MaSP", int.Parse(phieu.MaTaiSan));
                    command.Parameters.AddWithValue("@NgayBaoTri", phieu.NgayBaoTri);
                    command.Parameters.AddWithValue("@NgayHoanThanh",
                        (object)phieu.NgayHoanThanh ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TinhTrang", phieu.TrangThai);
                    command.Parameters.AddWithValue("@ChiPhi", phieu.ChiPhiDuKien);
                    command.Parameters.AddWithValue("@NoiDung", phieu.NoiDungBaoTri);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        // Xóa phiếu bảo trì
        public bool XoaPhieuBaoTri(string maPhieu)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = "DELETE FROM BaoTri WHERE MaBaoTri = @MaBaoTri";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MaBaoTri", int.Parse(maPhieu));
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        // Tìm kiếm phiếu bảo trì
        public List<PhieuBaoTri> TimKiemPhieuBaoTri(string tuKhoa, string trangThai = null)
        {
            var danhSach = new List<PhieuBaoTri>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT 
                        MaBaoTri as MaPhieu,
                        MaSP as MaTaiSan,
                        s.TenSanPham as TenTaiSan,
                        NgayBaoTri,
                        NgayHoanThanh,
                        MaNhanVien as NguoiPhuTrach,
                        TinhTrang as TrangThai,
                        ChiPhi as ChiPhiDuKien,
                        NoiDung as NoiDungBaoTri
                    FROM BaoTri b
                    LEFT JOIN SanPham s ON b.MaSP = s.MaSP
                    WHERE 1=1 ";

                var parameters = new List<NpgsqlParameter>();

                if (!string.IsNullOrEmpty(tuKhoa))
                {
                    query += " AND (s.TenSanPham ILIKE @TuKhoa OR MaBaoTri::text ILIKE @TuKhoa)";
                    parameters.Add(new NpgsqlParameter("@TuKhoa", $"%{tuKhoa}%"));
                }

                if (!string.IsNullOrEmpty(trangThai))
                {
                    query += " AND TinhTrang = @TrangThai";
                    parameters.Add(new NpgsqlParameter("@TrangThai", trangThai));
                }

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            danhSach.Add(new PhieuBaoTri
                            {
                                MaPhieu = reader["MaPhieu"].ToString(),
                                MaTaiSan = reader["MaTaiSan"].ToString(),
                                TenTaiSan = reader["TenTaiSan"].ToString(),
                                NgayBaoTri = Convert.ToDateTime(reader["NgayBaoTri"]),
                                NgayHoanThanh = reader["NgayHoanThanh"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["NgayHoanThanh"])
                                    : (DateTime?)null,
                                NguoiPhuTrach = reader["NguoiPhuTrach"].ToString(),
                                TrangThai = reader["TrangThai"].ToString(),
                                ChiPhiDuKien = Convert.ToDecimal(reader["ChiPhiDuKien"]),
                                NoiDungBaoTri = reader["NoiDungBaoTri"].ToString()
                            });
                        }
                    }
                }
            }
            return danhSach;
        }
    }
}