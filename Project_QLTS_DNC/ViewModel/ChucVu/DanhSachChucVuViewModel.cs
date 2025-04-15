using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Services.ChucVu;
using Project_QLTS_DNC.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Project_QLTS_DNC.Helpers;
namespace Project_QLTS_DNC.ViewModels.NhanVien
{
    public class DanhSachChucVuViewModel : INotifyPropertyChanged
    {
        private ChucVuService _chucVuService;

        public ObservableCollection<ChucVuModel> DanhSachChucVu { get; set; } = new ObservableCollection<ChucVuModel>();
        public ICommand XoaChucVuCommand { get; }

        public DanhSachChucVuViewModel()
        {
            KhoiTaoAsync();
            XoaChucVuCommand = new RelayCommand<int>(async (maChucVu) => await XoaChucVuAsync(maChucVu));
        }

        private async void KhoiTaoAsync()
        {
            _chucVuService = new ChucVuService();
            await LoadChucVuAsync();
        }

        public async Task LoadChucVuAsync()
        {
            var list = await _chucVuService.GetAllChucVuAsync();
            DanhSachChucVu.Clear();
            foreach (var item in list)
                DanhSachChucVu.Add(item);
        }


        public async Task TimKiemChucVuAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                await LoadChucVuAsync();  
                return;
            }

            var result = await _chucVuService.TimKiemChucVuAsync(keyword);  
            DanhSachChucVu.Clear();
            foreach (var item in result)
                DanhSachChucVu.Add(item);
        }



        public async Task<bool> XoaChucVuAsync(int maChucVu)
        {
            var ketQua = await _chucVuService.XoaChucVuAsync(maChucVu);
            if (ketQua)
            {
                await LoadChucVuAsync();
            }
            return ketQua;
        }




        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
