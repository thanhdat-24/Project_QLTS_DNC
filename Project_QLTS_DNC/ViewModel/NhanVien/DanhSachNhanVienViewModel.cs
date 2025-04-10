using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.ViewModels.NhanVien
{
    public class DanhSachNhanVienViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<NhanVienModel> _danhSachNhanVien;
        public ObservableCollection<NhanVienModel> DanhSachNhanVien
        {
            get => _danhSachNhanVien;
            set
            {
                _danhSachNhanVien = value;
                OnPropertyChanged();
            }
        }

        private readonly NhanVienService _nhanVienService;

        public DanhSachNhanVienViewModel()
        {
            _nhanVienService = new NhanVienService();
            _ = LoadNhanVienAsync();
        }

        public async Task LoadNhanVienAsync()
        {
            var danhSach = await _nhanVienService.LayDanhSachNhanVienAsync();
            DanhSachNhanVien = new ObservableCollection<NhanVienModel>(danhSach);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
