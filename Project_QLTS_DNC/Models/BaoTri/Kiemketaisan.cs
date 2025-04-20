using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json; // Thêm namespace này

namespace Project_QLTS_DNC.Models.BaoTri
{
    [Supabase.Postgrest.Attributes.Table("kiemketaisan")]
    public class KiemKeTaiSan : BaseModel, INotifyPropertyChanged
    {
        [PrimaryKey("ma_kiem_ke_ts", false)]
        public int MaKiemKeTS { get; set; }

        [Supabase.Postgrest.Attributes.Column("ma_dot_kiem_ke")]
        public int? MaDotKiemKe { get; set; }

        [Supabase.Postgrest.Attributes.Column("ma_tai_san")]
        public int? MaTaiSan { get; set; }

        [Supabase.Postgrest.Attributes.Column("ma_phong")]
        public int? MaPhong { get; set; }

        [Supabase.Postgrest.Attributes.Column("tinh_trang")]
        public string TinhTrang { get; set; }

        [Supabase.Postgrest.Attributes.Column("vi_tri_thuc_te")]
        public string ViTriThucTe { get; set; }

        [Supabase.Postgrest.Attributes.Column("ghi_chu")]
        public string GhiChu { get; set; }

        [Supabase.Postgrest.Attributes.Column("ma_nhom_ts")]
        public int? MaNhomTS { get; set; }

        // Sửa các thuộc tính không có trong database
        [NotMapped]
        [JsonIgnore] // Thêm dòng này
        public string TenNhomTS { get; set; }

        [NotMapped]
        [JsonIgnore] // Thêm dòng này
        public string TenTaiSan { get; set; }

        [NotMapped]
        [JsonIgnore] // Thêm dòng này
        public string TenPhong { get; set; }

        [NotMapped]
        [JsonIgnore] // Thêm dòng này
        public string TenDotKiemKe { get; set; }

        private bool _isSelected;
        [NotMapped]
        [JsonIgnore] // Thêm dòng này
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}