using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.DTOs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Project_QLTS_DNC.Models.NhanVien;

namespace Project_QLTS_DNC.ViewModels.NhanVien
{
    public class DanhSachNhanVienViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<NhanVienDto> _danhSachNhanVien;

        public ObservableCollection<NhanVienDto> DanhSachNhanVien
        {
            get => _danhSachNhanVien;
            set
            {
                _danhSachNhanVien = value;
                OnPropertyChanged(nameof(DanhSachNhanVien));
            }
        }

        private readonly NhanVienService _nhanVienService;

        public DanhSachNhanVienViewModel()
        {
            _nhanVienService = new NhanVienService();
            LoadNhanVienListAsync(); 
        }

        public async Task LoadNhanVienListAsync()
        {
            try
            {
                var danhSachDto = await _nhanVienService.LayTatCaNhanVienDtoAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DanhSachNhanVien = new ObservableCollection<NhanVienDto>(danhSachDto);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task ThemNhanVienAsync(NhanVienModel nhanVien)
        {
            var newNhanVien = await _nhanVienService.ThemNhanVienAsync(nhanVien);
            if (newNhanVien != null)
            {
                await LoadNhanVienListAsync();
            }
        }

        public async Task CapNhatNhanVienAsync(NhanVienModel nhanVien)
        {
            var updatedNhanVien = await _nhanVienService.CapNhatNhanVienAsync(nhanVien);
            if (updatedNhanVien != null)
            {
                await LoadNhanVienListAsync(); 
            }
        }

        public async Task XoaNhanVienAsync(int maNV)
        {
            var isSuccess = await _nhanVienService.XoaNhanVienAsync(maNV);
            if (isSuccess)
            {
                await LoadNhanVienListAsync(); 
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
