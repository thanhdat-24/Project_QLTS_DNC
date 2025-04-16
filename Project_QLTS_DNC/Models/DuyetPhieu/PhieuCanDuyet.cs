using System;

namespace Project_QLTS_DNC.Models.DuyetPhieu
{
    public class PhieuCanDuyet
    {
        public string MaPhieu { get; set; }
        public DateTime NgayTaoPhieu { get; set; }
        public bool? TrangThaiBool { get; set; }

        public string TrangThai => TrangThaiBool == true ? "Đã duyệt"
                            : TrangThaiBool == false ? "Từ chối duyệt"
                            : "Chưa duyệt";

        public string LoaiPhieu { get; set; }
    }

}
