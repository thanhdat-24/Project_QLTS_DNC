using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.BaoTri
{
    // Đảm bảo model này có mối quan hệ phù hợp với NhanVienModel
    [Table("phieubaotri")]
    public class PhieuBaoTri : BaseModel
    {
        [PrimaryKey("ma_phieu", false)]
        public string MaPhieu { get; set; }

        [Column("ma_tai_san")]
        public string MaTaiSan { get; set; }

        [Column("ten_tai_san")]
        public string TenTaiSan { get; set; }

        [Column("loai_bao_tri")]
        public string LoaiBaoTri { get; set; }

        [Column("ngay_bao_tri")]
        public DateTime NgayBaoTri { get; set; }

        [Column("ngay_hoan_thanh")]
        public DateTime? NgayHoanThanh { get; set; }

        [Column("nguoi_phu_trach")]
        public string NguoiPhuTrach { get; set; }

        // Có thể thêm trường để lưu mã nhân viên nếu cần
        [Column("ma_nv")]
        public int? MaNV { get; set; }

        [Column("trang_thai")]
        public string TrangThai { get; set; }

        [Column("chi_phi_du_kien")]
        public decimal ChiPhiDuKien { get; set; }

        [Column("noi_dung_bao_tri")]
        public string NoiDungBaoTri { get; set; }

        [Column("don_vi_bao_tri")]
        public string DonViBaoTri { get; set; }
    }
}