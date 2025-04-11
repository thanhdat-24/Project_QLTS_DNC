using System;
using System.ComponentModel;

namespace Project_QLTS_DNC.DTOs
{
    public class TaiSanDTO : INotifyPropertyChanged
    {
        private int _maTaiSan;
        private int? _maChiTietPN;
        private string _tenTaiSan;
        private string _soSeri;
        private string _maQR;
        private DateTime? _ngaySuDung;
        private DateTime? _hanBH;
        private string _tinhTrangSP;
        private string _ghiChu;
        private int? _maPhong;
        private bool _isSelected;
        private string _tenPhong; // Tên phòng từ bảng phong
        private string _tenNhomTS; // Tên nhóm tài sản (có thể thêm nếu có)

        public event PropertyChangedEventHandler PropertyChanged;

        // Mã tài sản
        public int MaTaiSan
        {
            get { return _maTaiSan; }
            set
            {
                if (_maTaiSan != value)
                {
                    _maTaiSan = value;
                    OnPropertyChanged(nameof(MaTaiSan));
                }
            }
        }

        // Mã chi tiết phiếu nhập
        public int? MaChiTietPN
        {
            get { return _maChiTietPN; }
            set
            {
                if (_maChiTietPN != value)
                {
                    _maChiTietPN = value;
                    OnPropertyChanged(nameof(MaChiTietPN));
                }
            }
        }

        // Tên tài sản
        public string TenTaiSan
        {
            get { return _tenTaiSan; }
            set
            {
                if (_tenTaiSan != value)
                {
                    _tenTaiSan = value;
                    OnPropertyChanged(nameof(TenTaiSan));
                }
            }
        }

        // Số seri
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

        // Mã QR
        public string MaQR
        {
            get { return _maQR; }
            set
            {
                if (_maQR != value)
                {
                    _maQR = value;
                    OnPropertyChanged(nameof(MaQR));
                }
            }
        }

        // Ngày sử dụng
        public DateTime? NgaySuDung
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

        // Hạn bảo hành
        public DateTime? HanBH
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

        // Tình trạng sản phẩm
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

        // Ghi chú
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

        // Mã phòng
        public int? MaPhong
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

        // Thuộc tính bổ sung cho UI
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

        // Tên phòng (lấy từ bảng phong dựa vào MaPhong)
        public string TenPhong
        {
            get { return _tenPhong; }
            set
            {
                if (_tenPhong != value)
                {
                    _tenPhong = value;
                    OnPropertyChanged(nameof(TenPhong));
                }
            }
        }

        // Tên nhóm tài sản (nếu có)
        public string TenNhomTS
        {
            get { return _tenNhomTS; }
            set
            {
                if (_tenNhomTS != value)
                {
                    _tenNhomTS = value;
                    OnPropertyChanged(nameof(TenNhomTS));
                }
            }
        }

        // Phương thức thông báo thay đổi thuộc tính
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Phương thức chuyển đổi từ TaiSanModel sang TaiSanDTO
        public static TaiSanDTO FromModel(Models.QLTaiSan.TaiSanModel model)
        {
            return new TaiSanDTO
            {
                MaTaiSan = model.MaTaiSan,
                MaChiTietPN = model.MaChiTietPN,
                TenTaiSan = model.TenTaiSan,
                SoSeri = model.SoSeri,
                MaQR = model.MaQR,
                NgaySuDung = model.NgaySuDung,
                HanBH = model.HanBH,
                TinhTrangSP = model.TinhTrangSP,
                GhiChu = model.GhiChu,
                MaPhong = model.MaPhong,
                IsSelected = false
            };
        }

        // Phương thức chuyển đổi từ TaiSanDTO sang TaiSanModel
        public Models.QLTaiSan.TaiSanModel ToModel()
        {
            return new Models.QLTaiSan.TaiSanModel
            {
                MaTaiSan = this.MaTaiSan,
                MaChiTietPN = this.MaChiTietPN,
                TenTaiSan = this.TenTaiSan,
                SoSeri = this.SoSeri,
                MaQR = this.MaQR,
                NgaySuDung = this.NgaySuDung,
                HanBH = this.HanBH,
                TinhTrangSP = this.TinhTrangSP,
                GhiChu = this.GhiChu,
                MaPhong = this.MaPhong
            };
        }
    }
}