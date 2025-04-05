using System;
using System.ComponentModel;

namespace Project_QLTS_DNC.Models
{
    public class PhieuBaoTri : INotifyPropertyChanged
    {
        private string _maPhieu;
        private string _maTaiSan;
        private string _tenTaiSan;
        private string _loaiBaoTri;
        private DateTime _ngayBaoTri;
        private DateTime? _ngayHoanThanh;
        private string _nguoiPhuTrach;
        private string _trangThai;
        private decimal _chiPhiDuKien;
        private string _noiDungBaoTri;
        private string _donViBaoTri;

        public event PropertyChangedEventHandler PropertyChanged;

        public string MaPhieu
        {
            get { return _maPhieu; }
            set
            {
                _maPhieu = value;
                OnPropertyChanged("MaPhieu");
            }
        }

        public string MaTaiSan
        {
            get { return _maTaiSan; }
            set
            {
                _maTaiSan = value;
                OnPropertyChanged("MaTaiSan");
            }
        }

        public string TenTaiSan
        {
            get { return _tenTaiSan; }
            set
            {
                _tenTaiSan = value;
                OnPropertyChanged("TenTaiSan");
            }
        }

        public string LoaiBaoTri
        {
            get { return _loaiBaoTri; }
            set
            {
                _loaiBaoTri = value;
                OnPropertyChanged("LoaiBaoTri");
            }
        }

        public DateTime NgayBaoTri
        {
            get { return _ngayBaoTri; }
            set
            {
                _ngayBaoTri = value;
                OnPropertyChanged("NgayBaoTri");
            }
        }

        public DateTime? NgayHoanThanh
        {
            get { return _ngayHoanThanh; }
            set
            {
                _ngayHoanThanh = value;
                OnPropertyChanged("NgayHoanThanh");
            }
        }

        public string NguoiPhuTrach
        {
            get { return _nguoiPhuTrach; }
            set
            {
                _nguoiPhuTrach = value;
                OnPropertyChanged("NguoiPhuTrach");
            }
        }

        public string TrangThai
        {
            get { return _trangThai; }
            set
            {
                _trangThai = value;
                OnPropertyChanged("TrangThai");
            }
        }

        public decimal ChiPhiDuKien
        {
            get { return _chiPhiDuKien; }
            set
            {
                _chiPhiDuKien = value;
                OnPropertyChanged("ChiPhiDuKien");
            }
        }

        public string NoiDungBaoTri
        {
            get { return _noiDungBaoTri; }
            set
            {
                _noiDungBaoTri = value;
                OnPropertyChanged("NoiDungBaoTri");
            }
        }

        public string DonViBaoTri
        {
            get { return _donViBaoTri; }
            set
            {
                _donViBaoTri = value;
                OnPropertyChanged("DonViBaoTri");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}