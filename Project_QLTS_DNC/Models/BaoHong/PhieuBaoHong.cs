using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Project_QLTS_DNC.Models.BaoHong
{
    [Table("phieubaohong")]
    public class PhieuBaoHong : BaseModel, INotifyPropertyChanged
    {
        [PrimaryKey("ma_phieu_bao_hong", false)]
        public int MaPhieuBaoHong { get; set; }

        [Column("ma_nv")]
        public int? MaNV { get; set; }

        [Column("ma_phong")]
        public int? MaPhong { get; set; }

        [Column("ma_tai_san")]
        public int? MaTaiSan { get; set; }

        [Column("ngay_bao_hong")]
        public DateTime? NgayBaoHong { get; set; }

        [Column("hinh_thuc_ghi_nhan")]
        public string HinhThucGhiNhan { get; set; }

        [Column("mo_ta")]
        public string MoTa { get; set; }

        [Column("trang_thai")]
        public string TrangThai { get; set; } = "Chờ xử lý";

        // Các trường JsonIgnore để không lưu vào database
        [JsonIgnore]
        public string TenNhanVien { get; set; }

        [JsonIgnore]
        public string TenPhong { get; set; }

        [JsonIgnore]
        public string TenTaiSan { get; set; }

        [JsonIgnore]
        public bool IsSelected { get; set; }

        // Thông báo sự thay đổi thuộc tính
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}