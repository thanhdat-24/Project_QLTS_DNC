using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.ComponentModel;

namespace Project_QLTS_DNC.Models.BaoTri
{
    [Table("lichsu_baotri")]
    public class LichSuBaoTri : BaseModel, INotifyPropertyChanged
    {
        private int _maLichSu;
        private int? _maTaiSan;
        private string _tenTaiSan;
        private string _soSeri;
        private DateTime _ngayThucHien;
        private int? _maNguoiThucHien;
        private string _tenNguoiThucHien;
        private string _tinhTrangTaiSan;
        private string _ghiChu;

        [PrimaryKey("ma_lich_su", true)]
        public int MaLichSu
        {
            get => _maLichSu;
            set
            {
                if (_maLichSu != value)
                {
                    _maLichSu = value;
                    OnPropertyChanged(nameof(MaLichSu));
                }
            }
        }

        [Column("ma_tai_san")]
        public int? MaTaiSan
        {
            get => _maTaiSan;
            set
            {
                if (_maTaiSan != value)
                {
                    _maTaiSan = value;
                    OnPropertyChanged(nameof(MaTaiSan));
                }
            }
        }

        [Column("ten_tai_san")]
        public string TenTaiSan
        {
            get => _tenTaiSan;
            set
            {
                if (_tenTaiSan != value)
                {
                    _tenTaiSan = value;
                    OnPropertyChanged(nameof(TenTaiSan));
                }
            }
        }

        [Column("so_seri")]
        public string SoSeri
        {
            get => _soSeri;
            set
            {
                if (_soSeri != value)
                {
                    _soSeri = value;
                    OnPropertyChanged(nameof(SoSeri));
                }
            }
        }

        [Column("ngay_thuc_hien")]
        public DateTime NgayThucHien
        {
            get => _ngayThucHien;
            set
            {
                if (_ngayThucHien != value)
                {
                    _ngayThucHien = value;
                    OnPropertyChanged(nameof(NgayThucHien));
                }
            }
        }

        [Column("ma_nv")]
        public int? MaNguoiThucHien
        {
            get => _maNguoiThucHien;
            set
            {
                if (_maNguoiThucHien != value)
                {
                    _maNguoiThucHien = value;
                    OnPropertyChanged(nameof(MaNguoiThucHien));
                }
            }
        }

        [Column("ten_nv")]
        public string TenNguoiThucHien
        {
            get => _tenNguoiThucHien;
            set
            {
                if (_tenNguoiThucHien != value)
                {
                    _tenNguoiThucHien = value;
                    OnPropertyChanged(nameof(TenNguoiThucHien));
                }
            }
        }

        [Column("tinh_trang_tai_san")]
        public string TinhTrangTaiSan
        {
            get => _tinhTrangTaiSan;
            set
            {
                if (_tinhTrangTaiSan != value)
                {
                    _tinhTrangTaiSan = value;
                    OnPropertyChanged(nameof(TinhTrangTaiSan));
                }
            }
        }

        [Column("ghi_chu")]
        public string GhiChu
        {
            get => _ghiChu;
            set
            {
                if (_ghiChu != value)
                {
                    _ghiChu = value;
                    OnPropertyChanged(nameof(GhiChu));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}