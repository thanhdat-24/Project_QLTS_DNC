using Project_QLTS_DNC.Helpers;
using Project_QLTS_DNC.Models.PhanQuyen;
using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.Services.TaiKhoan;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.ViewModels.TaiKhoan
{
    public class LoaiTaiKhoanViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<LoaiTaiKhoanModel> _danhSachLoaiTaiKhoan;
        private LoaiTaiKhoanModel _loaiTaiKhoanDuocChon;
        private readonly LoaiTaiKhoanService _service = new LoaiTaiKhoanService();

        public ObservableCollection<LoaiTaiKhoanModel> DanhSachLoaiTaiKhoan
        {
            get => _danhSachLoaiTaiKhoan;
            set { _danhSachLoaiTaiKhoan = value; OnPropertyChanged(); }
        }

        public LoaiTaiKhoanModel LoaiTaiKhoanDuocChon
        {
            get => _loaiTaiKhoanDuocChon;
            set { _loaiTaiKhoanDuocChon = value; OnPropertyChanged(); }
        }

        public LoaiTaiKhoanViewModel()
        {
            _ = LoadDataAsync();
        }

        public async Task<bool> XoaLoaiTaiKhoanAsync(int maLoaiTk)
        {
            var ketQua = await _service.XoaLoaiTaiKhoan(maLoaiTk);
            if (ketQua)
            {
                await LoadDataAsync();
                return true;
            }
            return false;
        }

        public async Task LoadDataAsync()
        {
            var list = await _service.LayDSLoaiTK();
            DanhSachLoaiTaiKhoan = new ObservableCollection<LoaiTaiKhoanModel>(list);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}