using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Services.NhanVien;
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

        // Constructor
        public DanhSachNhanVienViewModel()
        {
            _nhanVienService = new NhanVienService();
            _ = LoadNhanVienListAsync(); // Load dữ liệu ngay khi khởi tạo ViewModel
        }

        // Phương thức bất đồng bộ để tải danh sách nhân viên
        public async Task LoadNhanVienListAsync()
        {
            try
            {
                var danhSach = await _nhanVienService.GetNhanVienList(); // Sử dụng phương thức GetNhanVienList từ NhanVienService
                DanhSachNhanVien = new ObservableCollection<NhanVienModel>(danhSach);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                Console.WriteLine($"Lỗi khi tải danh sách nhân viên: {ex.Message}");
            }
        }

        // Sự kiện thông báo PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
