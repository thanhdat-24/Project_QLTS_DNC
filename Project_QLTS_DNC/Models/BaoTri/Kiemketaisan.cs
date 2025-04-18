using System;
using System.ComponentModel;
using Supabase.Postgrest.Models;  // Cần thêm reference đến package này
using Supabase.Postgrest.Attributes;

namespace Project_QLTS_DNC.Models
{
    [Table("kiemketaisan")]
    public class KiemKeTaiSan : BaseModel, INotifyPropertyChanged
    {
        [PrimaryKey("ma_kiem_ke_ts", false)]
        public int MaKiemKeTS { get; set; }

        [Column("ma_dot_kiem_ke")]
        public int? MaDotKiemKe { get; set; }

        [Column("ma_tai_san")]
        public int? MaTaiSan { get; set; }

        [Column("ma_phong")]
        public int? MaPhong { get; set; }

        [Column("tinh_trang")]
        public string TinhTrang { get; set; }

        [Column("vi_tri_thuc_te")]
        public string ViTriThucTe { get; set; }

        [Column("ghi_chu")]
        public string GhiChu { get; set; }

        // Thuộc tính IsSelected để hỗ trợ chọn dòng trong DataGrid
        private bool _isSelected;
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        [Column("loai_bao_tri")]
        public string LoaiBaoTri { get; set; }

        [Column("nhom_tai_san")]
        public string NhomTaiSan { get; set; }

        // Sự kiện PropertyChanged để hỗ trợ binding dữ liệu
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string tenThuocTinh = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(tenThuocTinh));
        }
    }
}