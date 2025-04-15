using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Models.NhanVien;
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
                if (_danhSachNhanVien != value)
                {
                    _danhSachNhanVien = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly NhanVienService _nhanVienService;
        private readonly AuthService _authService;

       
        public DanhSachNhanVienViewModel()
        {
            
            var client = SupabaseService.GetClientAsync().Result; 

           
            _nhanVienService = new NhanVienService(client);
            _authService = new AuthService(client); 

            _ = LoadNhanVienListAsync(); 
        }

        
        private async Task LoadNhanVienListAsync()
        {
           
            var danhSachDto = await _nhanVienService.LayTatCaNhanVienDtoAsync();

            if (danhSachDto != null)
            {
                
                List<NhanVienModel> danhSach = danhSachDto.Select(dto => new NhanVienModel
                {
                    MaNV = dto.MaNv,
                    TenNV = dto.TenNv,
                    MaPB = dto.MaPb,
                    MaCV = dto.MaCv,
                    
                }).ToList();

               
                DanhSachNhanVien = new ObservableCollection<NhanVienModel>(danhSach);
            }
        }



        // Sự kiện thông báo PropertyChanged để giao diện tự động cập nhật khi danh sách thay đổi
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
