using System;
using System.ComponentModel;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_QLTS_DNC.Models
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

        // Thuộc tính mở rộng để hiển thị tên
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        public string TenTaiSan { get; set; }

        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        public string TenPhong { get; set; }

        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        public string TenDotKiemKe { get; set; }

        // Thuộc tính IsSelected để hỗ trợ chọn dòng trong DataGrid
        private bool _isSelected;

        [NotMapped]
        [Supabase.Postgrest.Attributes.Column("IsSelected")] // Bỏ tham số true
        [Newtonsoft.Json.JsonIgnore] // Thêm dòng này để không serialize thuộc tính xuống JSON
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string tenThuocTinh = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(tenThuocTinh));
        }

        public override string ToString()
        {
            return $"KiemKe {MaKiemKeTS}: {TenTaiSan ?? $"Tài sản {MaTaiSan}"} - {TinhTrang}";
        }

        public void CapNhatThongTinMoRong(string tenTaiSan, string tenPhong, string tenDotKiemKe)
        {
            TenTaiSan = tenTaiSan;
            TenPhong = tenPhong;
            TenDotKiemKe = tenDotKiemKe;
        }
    }
}