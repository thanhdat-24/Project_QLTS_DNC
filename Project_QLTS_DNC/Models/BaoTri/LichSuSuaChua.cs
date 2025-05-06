using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Project_QLTS_DNC.Models.BaoTri
{
    [Supabase.Postgrest.Attributes.Table("lichsusuachua")]
    public class LichSuSuaChua : BaseModel, INotifyPropertyChanged
    {
        [PrimaryKey("ma_lich_su", true)]
        [Supabase.Postgrest.Attributes.Column("ma_lich_su")]
        public int MaLichSu { get; set; }

        [Supabase.Postgrest.Attributes.Column("ma_bao_tri")]
        public int? MaBaoTri { get; set; }

        [Supabase.Postgrest.Attributes.Column("ma_tai_san")]
        public int? MaTaiSan { get; set; }

        [Supabase.Postgrest.Attributes.Column("ma_nv")]
        public int? MaNV { get; set; }

        [Supabase.Postgrest.Attributes.Column("ngay_sua")]
        public DateTime? NgaySua { get; set; }

        [Supabase.Postgrest.Attributes.Column("loai_thao_tac")]
        public string LoaiThaoTac { get; set; }

        [Supabase.Postgrest.Attributes.Column("ket_qua")]
        public string KetQua { get; set; }

        [Supabase.Postgrest.Attributes.Column("chi_phi")]
        public decimal? ChiPhi { get; set; }

        [Supabase.Postgrest.Attributes.Column("trang_thai_truoc")]
        public string TrangThaiTruoc { get; set; }

        [Supabase.Postgrest.Attributes.Column("trang_thai_sau")]
        public string TrangThaiSau { get; set; }

        [Supabase.Postgrest.Attributes.Column("ghi_chu")]
        public string GhiChu { get; set; }

        [Supabase.Postgrest.Attributes.Column("noi_dung_bao_tri")]
        public string NoiDungBaoTri { get; set; }

        // Thuộc tính mở rộng để hiển thị thông tin từ bảng TaiSan
        [NotMapped]
        [JsonIgnore]
        public string TenTaiSan { get; set; }

        [NotMapped]
        [JsonIgnore]
        public string SoSeri { get; set; }

        [NotMapped]
        [JsonIgnore]
        public string TinhTrangTaiSan { get; set; }

        // Thuộc tính mở rộng để hiển thị thông tin từ bảng NhanVien
        [NotMapped]
        [JsonIgnore]
        public string TenNguoiThucHien { get; set; }

        // Thuộc tính mở rộng để hiển thị thông tin từ bảng NhomTaiSan
        [NotMapped]
        [JsonIgnore]
        public string TenNhomTaiSan { get; set; }

        // Thuộc tính mở rộng để hiển thị thông tin từ bảng Phong
        [NotMapped]
        [JsonIgnore]
        public string TenPhong { get; set; }

        // Thuộc tính mở rộng để đánh dấu chọn trong DataGrid
        [NotMapped]
        [JsonIgnore]
        public bool IsSelected { get; set; }

        // Triển khai interface INotifyPropertyChanged để hỗ trợ binding
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}