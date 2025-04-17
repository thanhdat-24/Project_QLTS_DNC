using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Project_QLTS_DNC.Models.PhanQuyen
{
    public class PhanQuyen : INotifyPropertyChanged
    {
        private string _tenChucNang;
        private string _iconKind;
        private bool _xem;
        private bool _them;
        private bool _sua;
        private bool _xoa;
        private bool _hienThi;

        public string TenChucNang
        {
            get => _tenChucNang;
            set
            {
                if (_tenChucNang != value)
                {
                    _tenChucNang = value;
                    OnPropertyChanged();
                }
            }
        }

        public string IconKind
        {
            get => _iconKind;
            set
            {
                if (_iconKind != value)
                {
                    _iconKind = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Xem
        {
            get => _xem;
            set
            {
                if (_xem != value)
                {
                    _xem = value;
                    OnPropertyChanged();

                    // Nếu bỏ chọn Xem, tự động bỏ chọn tất cả quyền khác
                    if (!value)
                    {
                        Them = false;
                        Sua = false;
                        Xoa = false;
                        HienThi = false;
                    }
                }
            }
        }

        public bool Them
        {
            get => _them;
            set
            {
                if (_them != value)
                {
                    _them = value;
                    OnPropertyChanged();

                    // Nếu chọn Thêm, tự động chọn Xem
                    if (value)
                    {
                        Xem = true;
                    }
                }
            }
        }

        public bool Sua
        {
            get => _sua;
            set
            {
                if (_sua != value)
                {
                    _sua = value;
                    OnPropertyChanged();

                    // Nếu chọn Sửa, tự động chọn Xem
                    if (value)
                    {
                        Xem = true;
                    }
                }
            }
        }

        public bool Xoa
        {
            get => _xoa;
            set
            {
                if (_xoa != value)
                {
                    _xoa = value;
                    OnPropertyChanged();

                    // Nếu chọn Xóa, tự động chọn Xem
                    if (value)
                    {
                        Xem = true;
                    }
                }
            }
        }

        public bool HienThi
        {
            get => _hienThi;
            set
            {
                if (_hienThi != value)
                {
                    _hienThi = value;
                    OnPropertyChanged();

                    // Nếu chọn Hiển thị, tự động chọn Xem
                    if (value)
                    {
                        Xem = true;
                    }
                }
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}