using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Services.TaiKhoan;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.ViewModel.TaiKhoan
{
    public class DanhSachTaiKhoanViewModel : INotifyPropertyChanged
    {
        private readonly TaiKhoanService _taiKhoanService;

        private ObservableCollection<TaiKhoanDTO> _danhSachTaiKhoan;
        public ObservableCollection<TaiKhoanDTO> DanhSachTaiKhoan
        {
            get => _danhSachTaiKhoan;
            set
            {
                _danhSachTaiKhoan = value;
                OnPropertyChanged();
            }
        }

        
        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public DanhSachTaiKhoanViewModel(TaiKhoanService taiKhoanService)
        {
            _taiKhoanService = taiKhoanService;
            DanhSachTaiKhoan = new ObservableCollection<TaiKhoanDTO>();
            _ = LoadDataAsync(); 
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var taiKhoanDTOs = await _taiKhoanService.LayTatCaTaiKhoanNeuLaAdminAsync();
                if (taiKhoanDTOs != null)
                {
                    DanhSachTaiKhoan.Clear();
                    foreach (var item in taiKhoanDTOs)
                    {
                        DanhSachTaiKhoan.Add(item);
                    }
                }
                else
                {
                    ErrorMessage = "Không thể tải dữ liệu tài khoản.";
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Lỗi khi tải dữ liệu: {ex.Message}";
            }
        }

        // ================================
        // INotifyPropertyChanged support
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
