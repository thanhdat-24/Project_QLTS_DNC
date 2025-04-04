using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Project_QLTS_DNC.Models
{
    public class Product : INotifyPropertyChanged
    {
        private string _maSP;
        private string _tenSP;
        private string _maPhong;
        private string _maNhom;
        private decimal _donGia;
        private int _soLuong;

        public string MaSP
        {
            get { return _maSP; }
            set { SetProperty(ref _maSP, value); }
        }

        public string TenSP
        {
            get { return _tenSP; }
            set { SetProperty(ref _tenSP, value); }
        }

        public string MaPhong
        {
            get { return _maPhong; }
            set { SetProperty(ref _maPhong, value); }
        }

        public string MaNhom
        {
            get { return _maNhom; }
            set { SetProperty(ref _maNhom, value); }
        }

        public decimal DonGia
        {
            get { return _donGia; }
            set { SetProperty(ref _donGia, value); }
        }

        public int SoLuong
        {
            get { return _soLuong; }
            set { SetProperty(ref _soLuong, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}