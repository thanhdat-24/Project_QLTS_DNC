using System;
using Project_QLTS_DNC.Models.QLLoaiTS;
using Project_QLTS_DNC.Models.QLNhomTS;

namespace Project_QLTS_DNC.DTOs
{
    /// <summary>
    /// Data Transfer Object for NhomTaiSan with extended properties for display purposes
    /// </summary>
    public class NhomTaiSanDTO
    {
        // Basic properties from NhomTaiSan
        public int MaNhomTS { get; set; }
        public int MaLoaiTaiSan { get; set; }
        public string TenNhom { get; set; }
        public string MoTa { get; set; }

        // Extended properties for display
        public string TenLoaiTaiSan { get; set; }

        // Navigation property for UI operations - chỉ có trong DTO, không có trong entity
        public LoaiTaiSan LoaiTaiSan { get; set; }

        // Constructor from NhomTaiSan model
        public NhomTaiSanDTO(NhomTaiSan nhomTaiSan, LoaiTaiSan loaiTaiSan = null)
        {
            if (nhomTaiSan == null)
                throw new ArgumentNullException(nameof(nhomTaiSan));

            MaNhomTS = nhomTaiSan.MaNhomTS;
            MaLoaiTaiSan = nhomTaiSan.MaLoaiTaiSan;
            TenNhom = nhomTaiSan.TenNhom;
            MoTa = nhomTaiSan.MoTa;

            // Nếu có thông tin LoaiTaiSan từ tham số
            if (loaiTaiSan != null)
            {
                LoaiTaiSan = loaiTaiSan;
                TenLoaiTaiSan = loaiTaiSan.TenLoaiTaiSan;
            }
        }

        // Convert DTO back to entity model
        public NhomTaiSan ToEntity()
        {
            return new NhomTaiSan
            {
                MaNhomTS = this.MaNhomTS,
                MaLoaiTaiSan = this.MaLoaiTaiSan,
                TenNhom = this.TenNhom,
                MoTa = this.MoTa
            };
        }
    }
}