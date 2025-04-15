using System;

namespace Project_QLTS_DNC.Models.DuyetPhieu
{
    public class PhieuCanDuyet
    {
        public string MaPhieu { get; set; }           // Ví dụ: PN12, PX03
        public DateTime NgayTaoPhieu { get; set; }    // Ngày tạo (ngay_nhap, ngay_xuat,...)
        public string TrangThai { get; set; }         // Chờ duyệt / Đã duyệt / Từ chối
        public string LoaiPhieu { get; set; }         // Phiếu nhập / Phiếu xuất / ...
        public long MaPhieuSo { get; set; }           // Mã số nguyên để lưu vào bảng phieuduyet (ma_phieu)
        public int MaLoaiPhieu { get; set; }          // 1: Nhập, 2: Xuất... dùng để phân biệt khi duyệt
    }
}
