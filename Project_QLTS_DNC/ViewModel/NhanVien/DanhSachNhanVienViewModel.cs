using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Models.NhanVien;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.ViewModels.NhanVien
{
    public class DanhSachNhanVienViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<NhanVienModel> _danhSachNhanVien;

        public ObservableCollection<NhanVienModel> DanhSachNhanVien
        {
            get => _danhSachNhanVien;
            set
            {
                if (_danhSachNhanVien != value)
                {
                    _danhSachNhanVien = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly NhanVienService _nhanVienService;
        private readonly AuthService _authService;

        // Constructor
        public DanhSachNhanVienViewModel()
        {
            // Đảm bảo lấy đúng client từ SupabaseService
            var client = SupabaseService.GetClientAsync().Result; // Lấy client đồng bộ

            // Khởi tạo NhanVienService và AuthService với client
            _nhanVienService = new NhanVienService(client);
            _authService = new AuthService(client); // Truyền client vào constructor

            _ = LoadNhanVienListAsync(); // Gọi hàm async trong constructor
        }

        // Phương thức bất đồng bộ để tải danh sách nhân viên nếu người dùng là admin
        private async Task LoadNhanVienListAsync()
        {
            // Lấy danh sách DTO với thông tin nhân viên kèm tên phòng ban và chức vụ
            var danhSachDto = await _nhanVienService.LayTatCaNhanVienDtoAsync();

            if (danhSachDto != null)
            {
                // Chuyển từ danh sách DTO sang NhanVienModel (chỉ cần thêm TenChucVu và TenPhongBan)
                List<NhanVienModel> danhSach = danhSachDto.Select(dto => new NhanVienModel
                {
                    MaNV = dto.MaNv,
                    TenNV = dto.TenNv,
                    MaPB = dto.MaPb,
                    MaCV = dto.MaCv,
                    // Cập nhật tên chức vụ và phòng ban trong NhanVienModel
                }).ToList();

                // Sau đó, sử dụng ObservableCollection để cập nhật giao diện
                DanhSachNhanVien = new ObservableCollection<NhanVienModel>(danhSach);
            }
        }



        // Sự kiện thông báo PropertyChanged để giao diện tự động cập nhật khi danh sách thay đổi
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
