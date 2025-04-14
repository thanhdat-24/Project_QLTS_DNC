using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Services;

namespace Project_QLTS_DNC.View.NhanVien
{
    /// <summary>
    /// Interaction logic for DanhSachNhanVienForm.xaml
    /// </summary>
    public partial class DanhSachNhanVienForm : UserControl
    {
        private readonly NhanVienService _nhanVienService;

        // Constructor
        public DanhSachNhanVienForm()
        {
            InitializeComponent();
            // Khởi tạo NhanVienService với client Supabase
            var client = SupabaseService.GetClientAsync().Result; // Lấy client đồng bộ
            _nhanVienService = new NhanVienService(client);
            _ = LoadDanhSachNhanVienAsync(); // Gọi hàm async trong constructor
        }

        private async Task LoadDanhSachNhanVienAsync()
        {
            // Lấy danh sách nhân viên DTO từ NhanVienService
            var danhSach = await _nhanVienService.LayTatCaNhanVienDtoAsync();

            // Bind dữ liệu vào DataGrid
            if (danhSach != null)
            {
                employeeDataGrid.ItemsSource = danhSach; // Cập nhật ItemSource của DataGrid
            }
        }

        private void btnThemNhanVien_Click(object sender, RoutedEventArgs e)
        {
            var themNhanVienWindow = new ThemNhanVienForm();
            themNhanVienWindow.Show();
        }
    }
}
