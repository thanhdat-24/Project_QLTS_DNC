using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Services.BaoTri;

namespace Project_QLTS_DNC.ViewModel.Baotri
{
    public class LoaiBaoTriViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<LoaiBaoTri> _dsLoaiBaoTri;
        public ObservableCollection<LoaiBaoTri> DSLoaiBaoTri
        {
            get => _dsLoaiBaoTri;
            set
            {
                _dsLoaiBaoTri = value;
                OnPropertyChanged(nameof(DSLoaiBaoTri));
            }
        }

        private readonly LoaiBaoTriService _loaiBaoTriService;

        public LoaiBaoTriViewModel()
        {
            _loaiBaoTriService = new LoaiBaoTriService();
            _ = LoadLoaiBaoTriAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task LoadLoaiBaoTriAsync()
        {
            DSLoaiBaoTri = new ObservableCollection<LoaiBaoTri>(await _loaiBaoTriService.LayDanhSachLoaiBT());
        }
    }
}
