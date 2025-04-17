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
        private ObservableCollection<LoaiTaiKhoanModel> _loaiTaiKhoans;

        private ObservableCollection<PhanQuyen> _danhSachPhanQuyen;
        public ObservableCollection<PhanQuyen> DanhSachPhanQuyen
        {
            get => _danhSachPhanQuyen;
            set { _danhSachPhanQuyen = value; OnPropertyChanged(); }
        }

        public ObservableCollection<LoaiTaiKhoanModel> LoaiTaiKhoans
        {
            get => _loaiTaiKhoans;
            set { _loaiTaiKhoans = value; OnPropertyChanged(); }
        }

        public LoaiTaiKhoanViewModel()
        {
            _ = LoadDataAsync();
        }


        private LoaiTaiKhoanModel _loaiTaiKhoanDuocChon;
        public LoaiTaiKhoanModel LoaiTaiKhoanDuocChon
        {
            get => _loaiTaiKhoanDuocChon;
            set { _loaiTaiKhoanDuocChon = value; OnPropertyChanged(); }
        }

        private ObservableCollection<LoaiTaiKhoanModel> _danhSachLoaiTaiKhoan;
        public ObservableCollection<LoaiTaiKhoanModel> DanhSachLoaiTaiKhoan
        {
            get => _danhSachLoaiTaiKhoan;
            set { _danhSachLoaiTaiKhoan = value; OnPropertyChanged(); }
        }



        public async Task<bool> XoaLoaiTaiKhoanAsync(int maLoaiTk)
        {
            var service = new LoaiTaiKhoanService();
            var ketQua = await service.XoaLoaiTaiKhoan(maLoaiTk);
            if (ketQua)
            {
                await LoadDataAsync();
                return true;
            }
            return false;
        }


        public async Task LoadDataAsync()
        {
            var service = new LoaiTaiKhoanService();
            var list = await service.LayDSLoaiTK();
            DanhSachLoaiTaiKhoan = new ObservableCollection<LoaiTaiKhoanModel>(list);

            DanhSachPhanQuyen = new ObservableCollection<PhanQuyen>(DanhSachQuyenMacDinh.LayDanhSach());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
