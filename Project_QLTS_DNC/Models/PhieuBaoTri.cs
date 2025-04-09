using System;
using System.ComponentModel;

namespace Project_QLTS_DNC.Models
{
    public class PhieuBaoTri : INotifyPropertyChanged
    {
        private int _maBaoTri;
        private int? _maTaiSanId;
        private int? _maLoaiBaoTri;
        private DateTime? _ngayBaoTri;
        private int? _maNv;
        private string _noiDung;
        private string _trangThaiSauBaoTri;
        private decimal? _chiPhi;
        private string _ghiChu;

        private string _maPhieu;
        private string _maTaiSan;
        private string _tenTaiSan;
        private string _loaiBaoTri;
        private DateTime? _ngayHoanThanh;
        private string _nguoiPhuTrach;
        private string _trangThai;
        private decimal _chiPhiDuKien;
        private string _noiDungBaoTri;
        private string _donViBaoTri;

        public event PropertyChangedEventHandler PropertyChanged;

        // Các thuộc tính chính từ cơ sở dữ liệu
        public int MaBaoTri
        {
            get => _maBaoTri;
            set
            {
                _maBaoTri = value;
                OnPropertyChanged(nameof(MaBaoTri));
            }
        }

        public int? MaTaiSanId
        {
            get => _maTaiSanId;
            set
            {
                _maTaiSanId = value;
                OnPropertyChanged(nameof(MaTaiSanId));
            }
        }

        public int? MaLoaiBaoTri
        {
            get => _maLoaiBaoTri;
            set
            {
                _maLoaiBaoTri = value;
                OnPropertyChanged(nameof(MaLoaiBaoTri));
            }
        }

        public DateTime? NgayBaoTri
        {
            get => _ngayBaoTri;
            set
            {
                _ngayBaoTri = value;
                OnPropertyChanged(nameof(NgayBaoTri));
            }
        }

        public int? MaNv
        {
            get => _maNv;
            set
            {
                _maNv = value;
                OnPropertyChanged(nameof(MaNv));
            }
        }

        public string NoiDung
        {
            get => _noiDung;
            set
            {
                _noiDung = value;
                OnPropertyChanged(nameof(NoiDung));
            }
        }

        public string TrangThaiSauBaoTri
        {
            get => _trangThaiSauBaoTri;
            set
            {
                _trangThaiSauBaoTri = value;
                OnPropertyChanged(nameof(TrangThaiSauBaoTri));
            }
        }

        public decimal? ChiPhi
        {
            get => _chiPhi;
            set
            {
                _chiPhi = value;
                OnPropertyChanged(nameof(ChiPhi));
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

        // Các thuộc tính hiển thị hoặc kết hợp từ bảng khác
        public string MaPhieu
        {
            get => _maPhieu;
            set
            {
                _maPhieu = value;
                OnPropertyChanged(nameof(MaPhieu));
            }
        }

        public string MaTaiSan
        {
            get => _maTaiSan;
            set
            {
                _maTaiSan = value;
                OnPropertyChanged(nameof(MaTaiSan));
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

        public string LoaiBaoTri
        {
            get => _loaiBaoTri;
            set
            {
                _loaiBaoTri = value;
                OnPropertyChanged(nameof(LoaiBaoTri));
            }
        }

        public DateTime? NgayHoanThanh
        {
            get => _ngayHoanThanh;
            set
            {
                _ngayHoanThanh = value;
                OnPropertyChanged(nameof(NgayHoanThanh));
            }
        }

        public string NguoiPhuTrach
        {
            get => _nguoiPhuTrach;
            set
            {
                _nguoiPhuTrach = value;
                OnPropertyChanged(nameof(NguoiPhuTrach));
            }
        }

        public string TrangThai
        {
            get => _trangThai;
            set
            {
                _trangThai = value;
                OnPropertyChanged(nameof(TrangThai));
            }
        }

        public decimal ChiPhiDuKien
        {
            get => _chiPhiDuKien;
            set
            {
                _chiPhiDuKien = value;
                OnPropertyChanged(nameof(ChiPhiDuKien));
            }
        }

        public string NoiDungBaoTri
        {
            get => _noiDungBaoTri;
            set
            {
                _noiDungBaoTri = value;
                OnPropertyChanged(nameof(NoiDungBaoTri));
            }
        }

        public string DonViBaoTri
        {
            get => _donViBaoTri;
            set
            {
                _donViBaoTri = value;
                OnPropertyChanged(nameof(DonViBaoTri));
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
