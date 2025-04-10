using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Models.DuyetPhieu
{
    public class DuyetPhieu
    {
        public class PhieuNhapKho
        {
            public string MaPhieu { get; set; }
            public string MaSP { get; set; }
            public DateTime NgayNhap { get; set; }
            public string MaNCC { get; set; }
            public int SoLuong { get; set; }
            public double DonGia { get; set; }
            public double TongTien => SoLuong * DonGia;
        }
        public class PhieuXuatKho
        {
            public string MaPhieu { get; set; }
            public DateTime NgayXuat { get; set; }
            public string MaNV { get; set; }
            public int SoLuong { get; set; }
            public string MoTa { get; set; }
        }
        public class PhieuBaoTri
        {
            public string MaPhieu { get; set; }
            public string MaKiemKe { get; set; }
            public DateTime NgayKiemKe { get; set; }
            public string TinhTrangSauKK { get; set; }
        }
        public class PhieuBaoHong
        {
            public string MaPhieu { get; set; }
            public string MaBaoCaoHong { get; set; }
            public string NguoiBaoCao { get; set; }
            public string HinhThucGhiNhan { get; set; }
            public string TinhTrang { get; set; }
        }

    }
}
