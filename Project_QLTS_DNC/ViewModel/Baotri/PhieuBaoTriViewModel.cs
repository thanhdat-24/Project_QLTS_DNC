using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Services;

namespace Project_QLTS_DNC.ViewModel.Baotri
{
    public class PhieuBaoTriViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<PhieuBaoTri> _dsBaoTri;
        public ObservableCollection<PhieuBaoTri> DsBaoTri
        {
            get => _dsBaoTri;
            set
            {
                _dsBaoTri = value;
                OnPropertyChanged(nameof(DsBaoTri));
            }
        }

        private readonly PhieuBaoTriService _phieuBaoTriService;

        public PhieuBaoTriViewModel()
        {
            _phieuBaoTriService = new PhieuBaoTriService();
            _ = LoadDSBaoTriAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task LoadDSBaoTriAsync()
        {
            try
            {
                var danhSachBaoTri = await _phieuBaoTriService.GetPhieuBaoTriAsync();
                DsBaoTri = new ObservableCollection<PhieuBaoTri>(danhSachBaoTri);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi tải dữ liệu từ ViewModel: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}