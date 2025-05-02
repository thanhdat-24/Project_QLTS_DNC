// ChiTietBanGiaoNhomDTO.cs
using System.Collections.Generic;

namespace Project_QLTS_DNC.DTOs
{
    public class ChiTietBanGiaoNhomDTO
    {
        public int MaNhomTS { get; set; }
        public string TenNhomTS { get; set; }
        public int SoLuong { get; set; }
        public List<ChiTietBanGiaoDTO> DanhSachTaiSan { get; set; }

        public ChiTietBanGiaoNhomDTO()
        {
            DanhSachTaiSan = new List<ChiTietBanGiaoDTO>();
        }
    }
}