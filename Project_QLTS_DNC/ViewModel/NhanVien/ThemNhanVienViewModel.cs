using Project_QLTS_DNC.Services.NhanVien;
using Project_QLTS_DNC.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Project_QLTS_DNC.Models.NhanVien;

namespace Project_QLTS_DNC.ViewModels.NhanVien
{
    public class ThemNhanVienViewModel : INotifyPropertyChanged
    {
        private NhanVienModel _nhanVien = new NhanVienModel();
        public NhanVienModel NhanVien
        {
            get => _nhanVien;
            set
            {
                _nhanVien = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ChucVuModel> _danhSachChucVu;
        public ObservableCollection<ChucVuModel> DanhSachChucVu
        {
            get => _danhSachChucVu;
            set
            {
                _danhSachChucVu = value;
                OnPropertyChanged();
            }
        }

        private long _selectedChucVu;
        public long SelectedChucVu
        {
            get => _selectedChucVu;
            set
            {
                _selectedChucVu = value;
                NhanVien.MaCV = (int)value; // Gán vào model nhân viên
                OnPropertyChanged();
            }
        }

        public ICommand ThemNhanVienCommand { get; }

        public ThemNhanVienViewModel()
        {
            ThemNhanVienCommand = new RelayCommand(async () => await ThemNhanVien());
            _ = LoadChucVu(); // Load danh sách chức vụ khi khởi tạo
        }

        private async Task ThemNhanVien()
        {
            try
            {
                var service = new NhanVienService();
                var result = await service.ThemNhanVien(NhanVien);

                if (result)
                {
                    MessageBox.Show("Thêm nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    NhanVien = new NhanVienModel(); // Reset form
                }
                else
                {
                    MessageBox.Show("Thêm nhân viên thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadChucVu()
        {
            try
            {
                var service = new NhanVienService();
                var chucvus = await service.GetChucVusAsync(); // Lấy danh sách chức vụ từ service
                DanhSachChucVu = new ObservableCollection<ChucVuModel>(chucvus);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi load chức vụ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
