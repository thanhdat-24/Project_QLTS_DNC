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

        public ObservableCollection<LoaiTaiKhoanModel> LoaiTaiKhoans
        {
            get => _loaiTaiKhoans;
            set { _loaiTaiKhoans = value; OnPropertyChanged(); }
        }

        public LoaiTaiKhoanViewModel()
        {
            _ = LoadDataAsync();
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
            LoaiTaiKhoans = new ObservableCollection<LoaiTaiKhoanModel>(list);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
