using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Services.BaoTri;

namespace Project_QLTS_DNC.ViewModel.Baotri
{
    public class DanhSachBaoTriViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<KiemKeTaiSan> _dsKiemKe;
        public ObservableCollection<KiemKeTaiSan> DsKiemKe
        {
            get => _dsKiemKe;
            set
            {
                _dsKiemKe = value;
                OnPropertyChanged(nameof(DsKiemKe));
            }
        }

        private readonly KiemKeTaiSanService _kiemKeTaiSanService;
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        // Thêm tham số autoLoad để kiểm soát việc tự động tải dữ liệu
        public DanhSachBaoTriViewModel(bool autoLoad = true)
        {
            _kiemKeTaiSanService = new KiemKeTaiSanService();
            DsKiemKe = new ObservableCollection<KiemKeTaiSan>();

            // Chỉ tự động tải dữ liệu nếu autoLoad = true
            if (autoLoad)
            {
                _ = LoadDSKiemKeAsync();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task LoadDSKiemKeAsync()
        {
            try
            {
                IsLoading = true;

                // Lấy dữ liệu từ service
                var danhSach = await _kiemKeTaiSanService.GetKiemKeTaiSanAsync();

                // Cập nhật danh sách
                DsKiemKe = new ObservableCollection<KiemKeTaiSan>(danhSach);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}