using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Services.TaiKhoan;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Project_QLTS_DNC.Helpers;
namespace Project_QLTS_DNC.ViewModel.TaiKhoan
{
    public class DanhSachTaiKhoanViewModel : INotifyPropertyChanged
    {
        private readonly TaiKhoanService _taiKhoanService;
        private readonly LoaiTaiKhoanService _loaiTaiKhoanService;
        private ObservableCollection<TaiKhoanDTO> _danhSachTaiKhoan;
        private ObservableCollection<LoaiTaiKhoanModel> _danhSachLoaiTaiKhoan;
        private List<TaiKhoanDTO> _allTaiKhoan;
        private string _tuKhoa = string.Empty;
        private LoaiTaiKhoanModel _selectedLoaiTaiKhoan;
        private string _errorMessage;

        public ObservableCollection<TaiKhoanDTO> DanhSachTaiKhoan
        {
            get => _danhSachTaiKhoan;
            set
            {
                _danhSachTaiKhoan = value;
                OnPropertyChanged();
            }
        }

        // Thêm getter và setter cho DanhSachLoaiTaiKhoan
        public ObservableCollection<LoaiTaiKhoanModel> DanhSachLoaiTaiKhoan
        {
            get => _danhSachLoaiTaiKhoan;
            set
            {
                _danhSachLoaiTaiKhoan = value;
                OnPropertyChanged();
            }
        }

        public string TuKhoa
        {
            get => _tuKhoa;
            set
            {
                _tuKhoa = value;
                OnPropertyChanged();
            }
        }

        public LoaiTaiKhoanModel SelectedLoaiTaiKhoan
        {
            get => _selectedLoaiTaiKhoan;
            set
            {
                _selectedLoaiTaiKhoan = value;
                OnPropertyChanged();
                 TimKiem();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }
        public ICommand TimKiemCommand { get; }
        public DanhSachTaiKhoanViewModel(TaiKhoanService taiKhoanService)
        {
            _taiKhoanService = taiKhoanService;
            _loaiTaiKhoanService = new LoaiTaiKhoanService();
            DanhSachTaiKhoan = new ObservableCollection<TaiKhoanDTO>();
            DanhSachLoaiTaiKhoan = new ObservableCollection<LoaiTaiKhoanModel>();
            
            TimKiemCommand = new RelayCommand(() => TimKiem());
            _allTaiKhoan = new List<TaiKhoanDTO>();
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var taiKhoanDTOs = await _taiKhoanService.LayDanhSachTaiKhoanAsync();
                await LoadDanhSachLoaiTaiKhoanAsync();
                Console.WriteLine($"Số lượng tài khoản: {taiKhoanDTOs?.Count}");
                Console.WriteLine($"Số lượng loại tài khoản: {DanhSachLoaiTaiKhoan?.Count}");
                _allTaiKhoan = new List<TaiKhoanDTO>(DanhSachTaiKhoan);

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


        public async Task LoadDanhSachTaiKhoanAsync()
        {
            try
            {
                var danhSachTaiKhoan = await _taiKhoanService.LayDanhSachTaiKhoanAsync();

                
                _allTaiKhoan = new List<TaiKhoanDTO>(danhSachTaiKhoan);

                DanhSachTaiKhoan.Clear();

                
                foreach (var item in danhSachTaiKhoan)
                {
                    DanhSachTaiKhoan.Add(item);
                }

                
                OnPropertyChanged(nameof(DanhSachTaiKhoan));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tải danh sách tài khoản: {ex.Message}");
                ErrorMessage = $"Lỗi khi tải danh sách tài khoản: {ex.Message}";
            }
        }

        private async Task LoadDanhSachLoaiTaiKhoanAsync()
        {
            try
            {
                var loaiTaiKhoans = await _loaiTaiKhoanService.LayDSLoaiTK();
                if (loaiTaiKhoans != null)
                {
                    
                    var tatCa = new LoaiTaiKhoanModel
                    {
                        MaLoaiTk = 0,
                        TenLoaiTk = "Tất cả"
                    };

                    DanhSachLoaiTaiKhoan.Clear();
                    DanhSachLoaiTaiKhoan.Add(tatCa);

                    foreach (var item in loaiTaiKhoans)
                    {
                        DanhSachLoaiTaiKhoan.Add(item);
                    }

                    
                    SelectedLoaiTaiKhoan = tatCa;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi tải danh sách loại tài khoản: {ex.Message}";
            }
        }
        public async void TimKiem()
        {
            try
            {
                
                Console.WriteLine($"Từ khóa tìm kiếm: {TuKhoa}");
                Console.WriteLine($"Loại tài khoản: {SelectedLoaiTaiKhoan?.TenLoaiTk} (Mã: {SelectedLoaiTaiKhoan?.MaLoaiTk})");

                
                var ketQua = await _taiKhoanService.TimKiemTaiKhoanAsync(
                    tuKhoa: TuKhoa?.Trim(), 
                    maLoaiTk: SelectedLoaiTaiKhoan?.MaLoaiTk
                );

               
                Console.WriteLine($"Số lượng tài khoản tìm được: {ketQua.Count}");

                
                DanhSachTaiKhoan.Clear();
                foreach (var item in ketQua)
                {
                    DanhSachTaiKhoan.Add(item);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi tìm kiếm: {ex.Message}";
                Console.WriteLine($"Chi tiết lỗi: {ex}");
            }
        }
        // ================================
        // INotifyPropertyChanged support
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
