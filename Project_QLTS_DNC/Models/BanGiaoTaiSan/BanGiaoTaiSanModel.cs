using System;
using System.Collections.ObjectModel;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.BanGiaoTaiSan
{
    [Table("bangiaotaisan")]
    public class BanGiaoTaiSanModel : BaseModel
    {
        [PrimaryKey("ma_bang_giao_ts", false)]
        [Column("ma_bang_giao_ts")]
        public int MaBanGiaoTS { get; set; }

        [Column("ngay_bang_giao")]
        public DateTime NgayBanGiao { get; set; }

        [Column("ma_nv")]
        public int MaNV { get; set; }

        [Column("ma_phong")]
        public int? MaPhong { get; set; }

        [Column("noi_dung")]
        public string NoiDung { get; set; }

        [Column("trang_thai")]
        public bool? TrangThai { get; set; }

        // Thêm mới trường người tiếp nhận
        [Column("cb_tiep_nhan")]
        public string CbTiepNhan { get; set; }

        [Column("ma_kho")]
        public int MaKho { get; set; }
    }
}

namespace Project_QLTS_DNC.Models.BanGiaoTaiSan
{
    [Table("chitietbangiao")]
    public class ChiTietBanGiaoModel : BaseModel
    {
        [PrimaryKey("ma_chi_tiet_bg", false)]
        [Column("ma_chi_tiet_bg")]
        public int MaChiTietBG { get; set; }

        [Column("ma_bang_giao_ts")]
        public int MaBanGiaoTS { get; set; }

        [Column("ma_tai_san")]
        public int MaTaiSan { get; set; }

        [Column("vi_tri_ts")]
        public int ViTriTS { get; set; }

        [Column("ghi_chu")]
        public string GhiChu { get; set; }


    }
}

namespace Project_QLTS_DNC.DTOs
{

    public class BanGiaoTaiSanDTO
    {
        public int MaBanGiaoTS { get; set; }
        public DateTime NgayBanGiao { get; set; }
        public int MaNV { get; set; }
        public string TenNV { get; set; }
        public int? MaPhong { get; set; }
        public string TenPhong { get; set; }
        public string NoiDung { get; set; }
        public bool? TrangThai { get; set; }
        public string TrangThaiText => TrangThai switch
        {
            null => "Chờ duyệt",
            true => "Đã duyệt",
            false => "Từ chối duyệt"
        };

        // Thông tin tòa nhà của phòng
        public int? MaToaNha { get; set; }
        public string TenToaNha { get; set; }

        // Thêm mới người tiếp nhận
        public string CbTiepNhan { get; set; }

        public int MaKho { get; set; }
        public string TenKho { get; set; }
    }
    public class ChiTietBanGiaoDTO
    {
        public int MaChiTietBG { get; set; }
        public int MaBanGiaoTS { get; set; }
        public int MaTaiSan { get; set; }
        public string TenTaiSan { get; set; }
        public string SoSeri { get; set; }
        public int ViTriTS { get; set; }
        public string GhiChu { get; set; }
        public int? MaNhomTS { get; set; }
        public string TenNhomTS { get; set; }
        public bool IsSelected { get; set; }

        public string TinhTrang { get; set; } = "Tốt";
    }

    // Lớp hỗ trợ cho ComboBox phòng trong bàn giao
    public class PhongBanGiaoFilter
    {
        public int MaPhong { get; set; }
        public string TenPhong { get; set; }
        public int MaTang { get; set; }
        public string TenTang { get; set; }
        public int MaToaNha { get; set; }
        public string TenToaNha { get; set; }

        public string HienThi => $"{TenPhong} - {TenTang} - {TenToaNha}";

        public override string ToString() => HienThi;
    }

    // Lớp hỗ trợ cho ComboBox kho trong bàn giao
    public class KhoBanGiaoFilter
    {
        public int MaKho { get; set; }
        public string TenKho { get; set; }
        public int MaToaNha { get; set; }
        public string TenToaNha { get; set; }

        public string HienThi => $"{TenKho} - {TenToaNha}";

        public override string ToString() => HienThi;
    }

    // Lớp hỗ trợ cho việc hiển thị tài sản có thể bàn giao
    public class TaiSanKhoBanGiaoDTO
    {
        public int MaTaiSan { get; set; }
        public int? MaChiTietPN { get; set; }
        public string TenTaiSan { get; set; }
        public string SoSeri { get; set; }
        public string MaQR { get; set; }
        public DateTime? NgaySuDung { get; set; }
        public DateTime? HanBH { get; set; }
        public string TinhTrangSP { get; set; }
        public int? MaNhomTS { get; set; }
        public string TenNhomTS { get; set; }
        public int MaKho { get; set; }
        public string TenKho { get; set; }
        public bool IsSelected { get; set; }
        public int ViTriTS { get; set; } = 1; // Mặc định vị trí là 1
        public string GhiChu { get; set; }

        public ObservableCollection<ViTriTSItem> ViTriList { get; set; } = new ObservableCollection<ViTriTSItem>();
    }
}