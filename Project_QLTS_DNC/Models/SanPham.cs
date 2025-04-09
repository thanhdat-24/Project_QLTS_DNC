using System;
using System.ComponentModel;

namespace Project_QLTS_DNC.Model
{
    public class SanPham : INotifyPropertyChanged
    {

        public class PhongFilter
        {
            public int MaPhong { get; set; }
        }

        public class NhomTSFilter
        {
            public int MaNhomTS { get; set; }
        }

        private int _maSP;
        private int _maPhong;
        private int _maNhomTS;
        private string _tenSanPham;
        private string _soSeri;
        private DateTime _ngaySuDung;
        private decimal _giaTriSP;
        private DateTime _hanBH;
        private string _tinhTrangSP;
        private string _ghiChu;

        public int MaSP
        {
            get { return _maSP; }
            set
            {
                if (_maSP != value)
                {
                    _maSP = value;
                    OnPropertyChanged(nameof(MaSP));
                }
            }
        }

        public int MaPhong
        {
            get { return _maPhong; }
            set
            {
                if (_maPhong != value)
                {
                    _maPhong = value;
                    OnPropertyChanged(nameof(MaPhong));
                }
            }
        }

        public int MaNhomTS
        {
            get { return _maNhomTS; }
            set
            {
                if (_maNhomTS != value)
                {
                    _maNhomTS = value;
                    OnPropertyChanged(nameof(MaNhomTS));
                }
            }
        }

        public string TenSanPham
        {
            get { return _tenSanPham; }
            set
            {
                if (_tenSanPham != value)
                {
                    _tenSanPham = value;
                    OnPropertyChanged(nameof(TenSanPham));
                }
            }
        }

        public string SoSeri
        {
            get { return _soSeri; }
            set
            {
                if (_soSeri != value)
                {
                    _soSeri = value;
                    OnPropertyChanged(nameof(SoSeri));
                }
            }
        }

        public DateTime NgaySuDung
        {
            get { return _ngaySuDung; }
            set
            {
                if (_ngaySuDung != value)
                {
                    _ngaySuDung = value;
                    OnPropertyChanged(nameof(NgaySuDung));
                }
            }
        }

        public decimal GiaTriSP
        {
            get { return _giaTriSP; }
            set
            {
                if (_giaTriSP != value)
                {
                    _giaTriSP = value;
                    OnPropertyChanged(nameof(GiaTriSP));
                }
            }
        }

        public DateTime HanBH
        {
            get { return _hanBH; }
            set
            {
                if (_hanBH != value)
                {
                    _hanBH = value;
                    OnPropertyChanged(nameof(HanBH));
                }
            }
        }

        public string TinhTrangSP
        {
            get { return _tinhTrangSP; }
            set
            {
                if (_tinhTrangSP != value)
                {
                    _tinhTrangSP = value;
                    OnPropertyChanged(nameof(TinhTrangSP));
                }
            }
        }

        public string GhiChu
        {
            get { return _ghiChu; }
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Thêm thuộc tính IsSelected vào class SanPham trong file SanPham.cs
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
    }
}