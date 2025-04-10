using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_QLTS_DNC.Models
{
    [Table("maintenance_types")]
    public class LoaiBaoTri : INotifyPropertyChanged
    {
        private int _maLoaiBaoTri;
        private string _tenLoai;
        private string _moTa;

        public event PropertyChangedEventHandler PropertyChanged;

        public int MaLoaiBaoTri
        {
            get => _maLoaiBaoTri;
            set
            {
                _maLoaiBaoTri = value;
                OnPropertyChanged(nameof(MaLoaiBaoTri));
            }
        }

        public string TenLoai
        {
            get => _tenLoai;
            set
            {
                _tenLoai = value;
                OnPropertyChanged(nameof(TenLoai));
            }
        }

        public string MoTa
        {
            get => _moTa;
            set
            {
                _moTa = value;
                OnPropertyChanged(nameof(MoTa));
            }
        }

        // Navigation property cho EF (nếu sử dụng)
        public virtual ICollection<PhieuBaoTri> PhieuBaoTris { get; set; } = new List<PhieuBaoTri>();

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
