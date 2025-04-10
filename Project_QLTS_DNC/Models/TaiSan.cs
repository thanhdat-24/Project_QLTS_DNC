using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Project_QLTS_DNC.Models.BaoTri;

namespace Project_QLTS_DNC.Models
{
    [Table("assets")]
    public class TaiSan : INotifyPropertyChanged
    {
        private int _maTaiSan;
        private int? _maChiTietPn;
        private string _tenTaiSan;
        private string _soSeri;
        private string _maQr;
        private DateTime? _ngaySuDung;
        private DateTime? _hanBh;
        private string _tinhTrangSp;
        private string _ghiChu;
        private int? _maPhong;

        public event PropertyChangedEventHandler PropertyChanged;

        public int MaTaiSan
        {
            get => _maTaiSan;
            set
            {
                _maTaiSan = value;
                OnPropertyChanged(nameof(MaTaiSan));
            }
        }

        public int? MaChiTietPn
        {
            get => _maChiTietPn;
            set
            {
                _maChiTietPn = value;
                OnPropertyChanged(nameof(MaChiTietPn));
            }
        }

        public string TenTaiSan
        {
            get => _tenTaiSan;
            set
            {
                _tenTaiSan = value;
                OnPropertyChanged(nameof(TenTaiSan));
            }
        }

        public string SoSeri
        {
            get => _soSeri;
            set
            {
                _soSeri = value;
                OnPropertyChanged(nameof(SoSeri));
            }
        }

        public string MaQr
        {
            get => _maQr;
            set
            {
                _maQr = value;
                OnPropertyChanged(nameof(MaQr));
            }
        }

        public DateTime? NgaySuDung
        {
            get => _ngaySuDung;
            set
            {
                _ngaySuDung = value;
                OnPropertyChanged(nameof(NgaySuDung));
            }
        }

        public DateTime? HanBh
        {
            get => _hanBh;
            set
            {
                _hanBh = value;
                OnPropertyChanged(nameof(HanBh));
            }
        }

        public string TinhTrangSp
        {
            get => _tinhTrangSp;
            set
            {
                _tinhTrangSp = value;
                OnPropertyChanged(nameof(TinhTrangSp));
            }
        }

        public string GhiChu
        {
            get => _ghiChu;
            set
            {
                _ghiChu = value;
                OnPropertyChanged(nameof(GhiChu));
            }
        }

        public int? MaPhong
        {
            get => _maPhong;
            set
            {
                _maPhong = value;
                OnPropertyChanged(nameof(MaPhong));
            }
        }

        // Navigation Property nếu dùng Entity Framework
        public virtual ICollection<PhieuBaoTri> PhieuBaoTris { get; set; } = new List<PhieuBaoTri>();

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
