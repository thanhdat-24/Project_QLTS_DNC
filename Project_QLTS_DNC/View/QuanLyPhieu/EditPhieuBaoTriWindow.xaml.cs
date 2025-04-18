using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Services;
using Supabase;

namespace Project_QLTS_DNC.Views
{
    public partial class EditPhieuBaoTriWindow : Window
    {

        private readonly PhieuBaoTriService _phieuBaoTriService = new();
        public PhieuBaoTri PhieuBaoTri { get; private set; }

        public EditPhieuBaoTriWindow(PhieuBaoTri phieuBaoTri)
        {
            InitializeComponent();

            // Tạo bản sao của đối tượng để tránh thay đổi trực tiếp
            PhieuBaoTri = new PhieuBaoTri
            {
                MaBaoTri = phieuBaoTri.MaBaoTri,
                MaTaiSan = phieuBaoTri.MaTaiSan,
                MaLoaiBaoTri = phieuBaoTri.MaLoaiBaoTri,
                NgayBaoTri = phieuBaoTri.NgayBaoTri,
                MaNV = phieuBaoTri.MaNV,
                NoiDung = phieuBaoTri.NoiDung,
                TrangThai = phieuBaoTri.TrangThai,
                ChiPhi = phieuBaoTri.ChiPhi,
                GhiChu = phieuBaoTri.GhiChu
            };

            // Thiết lập DataContext cho binding
            DataContext = PhieuBaoTri;

            // Load dữ liệu khi cửa sổ được tải
            Loaded += EditPhieuBaoTriWindow_Loaded;
        }

        private async void EditPhieuBaoTriWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra xem đây là thêm mới hay chỉnh sửa
                if (PhieuBaoTri.MaBaoTri == 0)
                {
                    // Đây là trường hợp thêm mới
                    this.Title = "Thêm Phiếu Bảo Trì Mới";
                    // Không hiển thị và không cho chỉnh sửa MaBaoTri vì sẽ được tạo tự động
                    txtMaBaoTri.Text = "(Tự động tạo)";
                    txtMaBaoTri.IsEnabled = false;
                }
                else
                {
                    // Đây là trường hợp chỉnh sửa
                    this.Title = $"Chỉnh Sửa Phiếu Bảo Trì - Mã {PhieuBaoTri.MaBaoTri}";
                }

                await WarmUpSchemaAsync();
                // Tải dữ liệu cho các combobox
                await LoadTaiSanAsync();
                await LoadLoaiBaoTriAsync();
                await LoadNhanVienAsync();
                // Thiết lập giá trị mặc định cho combobox trạng thái
                SetDefaultTrangThai();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async Task LoadTaiSanAsync()
        {
            try
            {
                // Lấy client Supabase từ SupabaseService
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                // Lấy danh sách tài sản từ Supabase
                var response = await client.From<TaiSanModel>().Get();

                // Hiển thị danh sách lên ComboBox
                cboMaTaiSan.ItemsSource = response.Models;
                cboMaTaiSan.SelectedValuePath = "MaTaiSan";
                cboMaTaiSan.SelectedValue = PhieuBaoTri.MaTaiSan;

                // Thiết lập định dạng hiển thị nếu cần
                // cboMaTaiSan.ItemStringFormat = "{0}"; // Format nếu cần
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải danh sách tài sản: {ex.Message}");
            }
        }

        private async Task LoadLoaiBaoTriAsync()
        {
            try
            {
                // Lấy client Supabase từ SupabaseService
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                // Lấy danh sách loại bảo trì từ Supabase
                var response = await client.From<LoaiBaoTri>().Get();

                // Hiển thị danh sách lên ComboBox
                cboMaLoaiBaoTri.ItemsSource = response.Models;
                cboMaLoaiBaoTri.SelectedValuePath = "MaLoaiBaoTri";

                // Thiết lập định dạng hiển thị nếu cần
                // cboMaLoaiBaoTri.ItemStringFormat = "{0}"; // Format nếu cần

                // Đặt giá trị mặc định
                cboMaLoaiBaoTri.SelectedValue = PhieuBaoTri.MaLoaiBaoTri;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải danh sách loại bảo trì: {ex.Message}");
            }
        }

        private async Task LoadNhanVienAsync()
        {
            try
            {
                // Lấy client Supabase từ SupabaseService
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }

                // Lấy danh sách nhân viên từ Supabase
                var response = await client.From<NhanVienModel>().Get();

                // Hiển thị danh sách lên ComboBox
                cboMaNV.ItemsSource = response.Models;
                cboMaNV.SelectedValuePath = "MaNV";

                // Thiết lập định dạng hiển thị nếu cần
                // cboMaNV.ItemStringFormat = "{0}"; // Format nếu cần

                // Đặt giá trị mặc định
                cboMaNV.SelectedValue = PhieuBaoTri.MaNV;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải danh sách nhân viên: {ex.Message}");
            }
        }

        private void SetDefaultTrangThai()
        {
            // Thiết lập giá trị mặc định cho ComboBox trạng thái
            if (!string.IsNullOrEmpty(PhieuBaoTri.TrangThai))
            {
                foreach (ComboBoxItem item in cboTrangThai.Items)
                {
                    if (item.Content.ToString() == PhieuBaoTri.TrangThai)
                    {
                        cboTrangThai.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Lấy giá trị từ ComboBox trạng thái
                if (cboTrangThai.SelectedItem is ComboBoxItem selectedItem)
                {
                    PhieuBaoTri.TrangThai = selectedItem.Content.ToString();
                }

                // Kiểm tra dữ liệu hợp lệ
                if (PhieuBaoTri.MaTaiSan == null)
                {
                    MessageBox.Show("Vui lòng chọn tài sản!", "Lỗi dữ liệu",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (PhieuBaoTri.MaLoaiBaoTri == null)
                {
                    MessageBox.Show("Vui lòng chọn loại bảo trì!", "Lỗi dữ liệu",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (PhieuBaoTri.MaNV == null)
                {
                    MessageBox.Show("Vui lòng chọn nhân viên phụ trách!", "Lỗi dữ liệu",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(PhieuBaoTri.NoiDung))
                {
                    MessageBox.Show("Vui lòng nhập nội dung bảo trì!", "Lỗi dữ liệu",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(PhieuBaoTri.TrangThai))
                {
                    MessageBox.Show("Vui lòng chọn trạng thái!", "Lỗi dữ liệu",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Đóng form và trả về kết quả thành công
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu phiếu bảo trì: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Hủy thay đổi
            DialogResult = false;
            Close();
        }
        private async Task WarmUpSchemaAsync()
        {
            var client = await SupabaseService.GetClientAsync();

            // Gọi một truy vấn nhỏ để buộc Supabase cache lại schema
            await client
                .From<PhieuBaoTri>()
                .Select("*")
                .Limit(1)
                .Get();
        }
    }
}