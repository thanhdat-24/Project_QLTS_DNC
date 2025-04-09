using System;
using System.ComponentModel;

namespace Project_QLTS_DNC.Models
{
    public class PhieuBaoHong : INotifyPropertyChanged
    {
        private string _maPhieu;
        private string _maTaiSan;
        private string _tenTaiSan;
        private DateTime _ngayLap;
        private string _nguoiLap;
        private string _mucDoHong;
        private string _trangThai;
        private string _boPhanQuanLy;
        private string _moTa;

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

        public DateTime NgayLap
        {
            get { return _ngayLap; }
            set
            {
                _ngayLap = value;
                OnPropertyChanged("NgayLap");
            }
        }

        public string NguoiLap
        {
            get { return _nguoiLap; }
            set
            {
                _nguoiLap = value;
                OnPropertyChanged("NguoiLap");
            }
        }

        public string MucDoHong
        {
            get { return _mucDoHong; }
            set
            {
                _mucDoHong = value;
                OnPropertyChanged("MucDoHong");
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

        public string BoPhanQuanLy
        {
            get { return _boPhanQuanLy; }
            set
            {
                _boPhanQuanLy = value;
                OnPropertyChanged("BoPhanQuanLy");
            }
        }

        public string MoTa
        {
            get { return _moTa; }
            set
            {
                _moTa = value;
                OnPropertyChanged("MoTa");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}