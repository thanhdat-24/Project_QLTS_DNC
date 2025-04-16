using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Services.ChucVu;
using Project_QLTS_DNC.Services.QLToanNha;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Supabase;

namespace Project_QLTS_DNC.ViewModels.NhanVien
{


    public class ThemNhanVienViewModel : INotifyPropertyChanged
    {

        private NhanVienService _nhanVienService;
        private Client _client;
        private ChucVuService _chucVuService;
        private ObservableCollection<PhongBan> _danhSachPhongBan;
        private ObservableCollection<ChucVuModel> _danhSachChucVu;
        public NhanVienModel NewNhanVien { get; set; } = new NhanVienModel();



        public ObservableCollection<PhongBan> DanhSachPhongBan
        {
            get { return _danhSachPhongBan; }
            set
            {
                if (_danhSachPhongBan != value)
                {
                    _danhSachPhongBan = value;
                    OnPropertyChanged(nameof(DanhSachPhongBan));  
                }
            }
        }

        public ObservableCollection<ChucVuModel> DanhSachChucVu
        {
            get { return _danhSachChucVu; }
            set
            {
                if (_danhSachChucVu != value)
                {
                    _danhSachChucVu = value;
                    OnPropertyChanged(nameof(DanhSachChucVu));
                }
            }
        }

        public ThemNhanVienViewModel()
        {
            _client = SupabaseService.GetClientAsync().Result; // Lưu ý khi sử dụng `.Result` cho các call async.

            _nhanVienService = new NhanVienService();
            _chucVuService = new ChucVuService();

            Task.Run(async () =>
            {
                await LoadPhongBan();
                await LoadChucVu();
            }).Wait();
        }



        public async Task LoadPhongBan()
        {
            try
            {
                DanhSachPhongBan = await PhongBanService.LayDanhSachPhongBanAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load phòng ban: {ex.Message}");
            }
        }

        public async Task LoadChucVu()
        {
            try
            {
                var chucVuList = await _chucVuService.GetAllChucVuAsync();
                DanhSachChucVu = new ObservableCollection<ChucVuModel>(chucVuList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load chức vụ: {ex.Message}");
            }
        }


        public async Task<bool> ThemNhanVienAsync(NhanVienModel nhanVien)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nhanVien.TenNV) ||
                    string.IsNullOrWhiteSpace(nhanVien.Email) ||
                    string.IsNullOrWhiteSpace(nhanVien.DiaChi))
                {
                    return false;
                }

                if (nhanVien.NgayVaoLam == default)
                    nhanVien.NgayVaoLam = DateTime.Now;

                var result = await _nhanVienService.ThemNhanVienAsync(nhanVien);
                return result != null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi thêm nhân viên: " + ex.Message);
                return false;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
