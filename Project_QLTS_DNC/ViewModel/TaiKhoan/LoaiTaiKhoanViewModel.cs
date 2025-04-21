using Project_QLTS_DNC.Helpers;
using Project_QLTS_DNC.Models.PhanQuyen;
using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.Services.TaiKhoan;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Project_QLTS_DNC.ViewModels.TaiKhoan
{
    public class LoaiTaiKhoanViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<LoaiTaiKhoanModel> _danhSachLoaiTaiKhoan;
        private LoaiTaiKhoanModel _loaiTaiKhoanDuocChon;
        private readonly LoaiTaiKhoanService _service = new LoaiTaiKhoanService();
        public ICommand TimKiemCommand { get; }
        public ObservableCollection<LoaiTaiKhoanModel> DanhSachLoaiTaiKhoan
        {
            get => _danhSachLoaiTaiKhoan;
            set { _danhSachLoaiTaiKhoan = value; OnPropertyChanged(); }
        }

        public LoaiTaiKhoanModel LoaiTaiKhoanDuocChon
        {
            get => _loaiTaiKhoanDuocChon;
            set { _loaiTaiKhoanDuocChon = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Tìm kiếm
        /// </summary>
        private string _tuKhoa;
        public string TuKhoa
        {
            get => _tuKhoa;
            set
            {
                _tuKhoa = value;
                OnPropertyChanged();
                TimKiem(); 
            }
        }
        public void TimKiem()
        {
            if (_danhSachGoc == null) return;

            var keyword = TuKhoa?.Trim().ToLower();

            if (string.IsNullOrEmpty(keyword))
            {
                DanhSachLoaiTaiKhoan = new ObservableCollection<LoaiTaiKhoanModel>(_danhSachGoc);
                return;
            }

            var ketQua = _danhSachGoc.Where(x =>
                x.MaLoaiTk.ToString().Contains(keyword) || // ← thêm dòng này
                x.TenLoaiTk.ToLower().Contains(keyword) ||
                (!string.IsNullOrEmpty(x.MoTa) && x.MoTa.ToLower().Contains(keyword))
            ).ToList();

            DanhSachLoaiTaiKhoan = new ObservableCollection<LoaiTaiKhoanModel>(ketQua);
        }


        private List<LoaiTaiKhoanModel> _danhSachGoc = new();


        private LoaiTaiKhoanModel _loaiTaiKhoanFilter;
        public LoaiTaiKhoanModel LoaiTaiKhoanFilter
        {
            get => _loaiTaiKhoanFilter;
            set
            {
                _loaiTaiKhoanFilter = value;
                OnPropertyChanged();
                TimKiem(); // gọi lại tìm kiếm khi chọn loại mới
            }
        }


        /// <summary>
        /// Load data
        /// </summary>
        public LoaiTaiKhoanViewModel()
        {
            TimKiemCommand = new RelayCommand<string>(TuKhoaTimKiem =>
            {
                TuKhoa = TuKhoaTimKiem;
                TimKiem();
            });
            _ = LoadDataAsync();
        }

        public async Task<bool> XoaLoaiTaiKhoanAsync(int maLoaiTk)
        {
            var ketQua = await _service.XoaLoaiTaiKhoan(maLoaiTk);
            if (ketQua)
            {
                await LoadDataAsync();
                return true;
            }
            return false;
        }

        public async Task LoadDataAsync()
        {
            _danhSachGoc = await _service.LayDSLoaiTK();
            DanhSachLoaiTaiKhoan = new ObservableCollection<LoaiTaiKhoanModel>(_danhSachGoc);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}