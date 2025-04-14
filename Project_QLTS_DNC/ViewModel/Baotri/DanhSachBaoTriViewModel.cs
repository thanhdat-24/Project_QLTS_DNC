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

        public DanhSachBaoTriViewModel()
        {
            _kiemKeTaiSanService = new KiemKeTaiSanService();
            _ = LoadDSKiemKeAsync();
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
                var danhSach = await _kiemKeTaiSanService.GetKiemKeTaiSanAsync();
                DsKiemKe = new ObservableCollection<KiemKeTaiSan>(danhSach);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
