using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
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
        private List<TaiSanModel> _allTaiSan = new List<TaiSanModel>();
        private TaiSanModel _selectedTaiSan = null;
        private System.Timers.Timer _searchTimer;
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
            // Khởi tạo timer để delay tìm kiếm (tránh gọi quá nhiều khi đang nhập)
            _searchTimer = new System.Timers.Timer(300);
            _searchTimer.AutoReset = false;
            _searchTimer.Elapsed += (sender, e) =>
            {
                Dispatcher.Invoke(async () => await SearchTaiSanAsync());
            };
            // Load dữ liệu khi cửa sổ được tải
            Loaded += EditPhieuBaoTriWindow_Loaded;
        }
        private async void EditPhieuBaoTriWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await WarmUpSchemaAsync();
                // Tải dữ liệu cho các combobox
                await LoadAllTaiSanAsync();
                await LoadLoaiBaoTriAsync();
                await LoadNhanVienAsync();
                // Thiết lập giá trị mặc định cho combobox trạng thái
                SetDefaultTrangThai();
                // Hiển thị thông tin tài sản đã chọn nếu có
                if (PhieuBaoTri.MaTaiSan.HasValue)
                {
                    await LoadSelectedTaiSanInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Tải toàn bộ danh sách tài sản từ cơ sở dữ liệu với tình trạng "Cần kiểm tra"
        private async Task LoadAllTaiSanAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                {
                    throw new Exception("Không thể kết nối Supabase Client");
                }
                // Lấy danh sách tài sản từ Supabase với điều kiện TinhTrangSP là "Cần kiểm tra"
                var response = await client.From<TaiSanModel>()
                    .Where(t => t.TinhTrangSP == "Cần kiểm tra")
                    .Get();
                _allTaiSan = response.Models;
                // Log thông tin để debug
                Console.WriteLine($"Đã tải {_allTaiSan.Count} tài sản cần kiểm tra");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải danh sách tài sản: {ex.Message}");
            }
        }
        // Tải thông tin tài sản đã chọn
        private async Task LoadSelectedTaiSanInfo()
        {
            try
            {
                if (!PhieuBaoTri.MaTaiSan.HasValue)
                    return;
                var client = await SupabaseService.GetClientAsync();
                if (client == null)
                    return;
                var taiSan = await client.From<TaiSanModel>()
                    .Where(t => t.MaTaiSan == PhieuBaoTri.MaTaiSan.Value)
                    .Single();
                if (taiSan != null)
                {
                    _selectedTaiSan = taiSan;
                    DisplaySelectedTaiSan();
                    // Hiển thị thông tin tài sản
                    txtSearchTaiSan.Text = taiSan.TenTaiSan;

                    // Nếu tài sản không có tình trạng "Cần kiểm tra", hiển thị thông báo
                    if (taiSan.TinhTrangSP != "Cần kiểm tra")
                    {
                        Console.WriteLine("Lưu ý: Tài sản này không có tình trạng 'Cần kiểm tra'");
                        // Có thể thêm hiển thị thông báo cho người dùng nếu cần
                        // MessageBox.Show("Lưu ý: Tài sản này không có tình trạng 'Cần kiểm tra'", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tải thông tin tài sản đã chọn: {ex.Message}");
            }
        }
        // Hiển thị thông tin tài sản đã chọn
        private void DisplaySelectedTaiSan()
        {
            try
            {
                if (_selectedTaiSan != null)
                {
                    txtSelectedTaiSanName.Text = _selectedTaiSan.TenTaiSan ?? "Không có";
                    txtSelectedTaiSanSeri.Text = _selectedTaiSan.SoSeri ?? "Không có";
                    txtSelectedTaiSanStatus.Text = _selectedTaiSan.TinhTrangSP ?? "Không có";
                    txtSelectedTaiSanWarranty.Text = _selectedTaiSan.HanBH?.ToString("dd/MM/yyyy") ?? "Không có";

                    // Ẩn danh sách gợi ý và hiện thông tin chi tiết
                    lvTaiSanSuggestions.Visibility = Visibility.Collapsed;
                    selectedTaiSanInfo.Visibility = Visibility.Visible;

                    // Cập nhật giá trị MaTaiSan trong PhieuBaoTri
                    PhieuBaoTri.MaTaiSan = _selectedTaiSan.MaTaiSan;

                    // Hiển thị tên tài sản trên ô tìm kiếm
                    txtSearchTaiSan.Text = _selectedTaiSan.TenTaiSan;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi hiển thị thông tin tài sản: {ex.Message}");
            }
        }
        // Tìm kiếm tài sản dựa vào từ khóa
        private async Task SearchTaiSanAsync()
        {
            try
            {
                string keyword = txtSearchTaiSan.Text.Trim().ToLower();
                if (string.IsNullOrEmpty(keyword))
                {
                    lvTaiSanSuggestions.Visibility = Visibility.Collapsed;
                    return;
                }
                // Tìm kiếm tài sản dựa trên nhiều thuộc tính (chỉ trong danh sách đã lọc)
                var filteredTaiSan = _allTaiSan.Where(t =>
                    (t.TenTaiSan?.ToLower().Contains(keyword) ?? false) ||
                    (t.SoSeri?.ToLower().Contains(keyword) ?? false) ||
                    (t.TinhTrangSP?.ToLower().Contains(keyword) ?? false) ||
                    (t.MaQR?.ToLower().Contains(keyword) ?? false)
                )
                // Ưu tiên các kết quả có tên tài sản khớp với từ khóa
                .OrderByDescending(t => (t.TenTaiSan?.ToLower().Contains(keyword) ?? false) ? 1 : 0)
                .Take(20)
                .ToList();  // Giới hạn 20 kết quả để tránh quá nhiều

                if (filteredTaiSan.Count > 0)
                {
                    lvTaiSanSuggestions.ItemsSource = filteredTaiSan;
                    lvTaiSanSuggestions.Visibility = Visibility.Visible;
                    // Ẩn thông tin chi tiết tài sản nếu đang tìm kiếm và từ khóa khác với tên tài sản đang chọn
                    if (_selectedTaiSan != null &&
                        !_selectedTaiSan.TenTaiSan.ToLower().Contains(keyword) &&
                        !keyword.Contains(_selectedTaiSan.TenTaiSan.ToLower()))
                    {
                        selectedTaiSanInfo.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    lvTaiSanSuggestions.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tìm kiếm tài sản: {ex.Message}");
                lvTaiSanSuggestions.Visibility = Visibility.Collapsed;
            }
        }
        // Sự kiện khi nhập text vào ô tìm kiếm
        private void txtSearchTaiSan_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // Reset timer để tránh gọi quá nhiều lần SearchTaiSanAsync
                _searchTimer.Stop();
                _searchTimer.Start();

                // Nếu ô tìm kiếm trống, ẩn danh sách gợi ý
                if (string.IsNullOrEmpty(txtSearchTaiSan.Text))
                {
                    lvTaiSanSuggestions.Visibility = Visibility.Collapsed;

                    // Nếu đã xóa text trong khi đã có tài sản được chọn, hiển thị lại thông tin tài sản
                    if (_selectedTaiSan != null)
                    {
                        selectedTaiSanInfo.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        selectedTaiSanInfo.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong sự kiện TextChanged: {ex.Message}");
            }
        }
        // Sự kiện khi nhấn phím trong ô tìm kiếm
        private void txtSearchTaiSan_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down && lvTaiSanSuggestions.Items.Count > 0 &&
                    lvTaiSanSuggestions.Visibility == Visibility.Visible)
                {
                    // Di chuyển focus xuống danh sách gợi ý
                    lvTaiSanSuggestions.Focus();
                    lvTaiSanSuggestions.SelectedIndex = 0;
                }
                else if (e.Key == Key.Escape)
                {
                    // Ẩn danh sách gợi ý
                    lvTaiSanSuggestions.Visibility = Visibility.Collapsed;

                    // Nếu có tài sản đã chọn, đặt lại text về tên tài sản đã chọn
                    if (_selectedTaiSan != null)
                    {
                        txtSearchTaiSan.Text = _selectedTaiSan.TenTaiSan;
                        txtSearchTaiSan.SelectionStart = txtSearchTaiSan.Text.Length;
                    }
                }
                else if (e.Key == Key.Enter)
                {
                    // Xử lý khi nhấn Enter trong ô tìm kiếm
                    if (lvTaiSanSuggestions.Visibility == Visibility.Visible)
                    {
                        if (lvTaiSanSuggestions.Items.Count > 0)
                        {
                            // Nếu chưa có item được chọn, chọn item đầu tiên
                            if (lvTaiSanSuggestions.SelectedIndex == -1)
                            {
                                lvTaiSanSuggestions.SelectedIndex = 0;
                            }

                            // Lấy item đã chọn
                            if (lvTaiSanSuggestions.SelectedItem is TaiSanModel selectedTaiSan)
                            {
                                _selectedTaiSan = selectedTaiSan;
                                DisplaySelectedTaiSan();
                            }
                        }

                        // Ẩn danh sách gợi ý sau khi chọn
                        lvTaiSanSuggestions.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong sự kiện KeyUp: {ex.Message}");
            }
        }
        // Sự kiện khi chọn một tài sản từ danh sách gợi ý
        private void lvTaiSanSuggestions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (lvTaiSanSuggestions.SelectedItem is TaiSanModel selectedTaiSan)
                {
                    _selectedTaiSan = selectedTaiSan;
                    DisplaySelectedTaiSan();

                    // Đưa focus trở lại ô tìm kiếm
                    txtSearchTaiSan.Focus();
                    txtSearchTaiSan.SelectionStart = txtSearchTaiSan.Text.Length;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi chọn tài sản: {ex.Message}");
            }
        }
        // Sự kiện khi nhấn nút xóa lựa chọn
        private void btnClearTaiSan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _selectedTaiSan = null;
                PhieuBaoTri.MaTaiSan = null;
                txtSearchTaiSan.Text = string.Empty;
                selectedTaiSanInfo.Visibility = Visibility.Collapsed;

                // Đặt focus vào ô tìm kiếm
                txtSearchTaiSan.Focus();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa lựa chọn tài sản: {ex.Message}");
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
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thiết lập trạng thái mặc định: {ex.Message}");
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
                    txtSearchTaiSan.Focus();
                    return;
                }

                if (PhieuBaoTri.MaLoaiBaoTri == null)
                {
                    MessageBox.Show("Vui lòng chọn loại bảo trì!", "Lỗi dữ liệu",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    cboMaLoaiBaoTri.Focus();
                    return;
                }

                if (PhieuBaoTri.MaNV == null)
                {
                    MessageBox.Show("Vui lòng chọn nhân viên phụ trách!", "Lỗi dữ liệu",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    cboMaNV.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(PhieuBaoTri.NoiDung))
                {
                    MessageBox.Show("Vui lòng nhập nội dung bảo trì!", "Lỗi dữ liệu",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNoiDung.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(PhieuBaoTri.TrangThai))
                {
                    MessageBox.Show("Vui lòng chọn trạng thái!", "Lỗi dữ liệu",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    cboTrangThai.Focus();
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
            try
            {
                var client = await SupabaseService.GetClientAsync();
                if (client != null)
                {
                    // Gọi một truy vấn nhỏ để buộc Supabase cache lại schema
                    await client
                        .From<PhieuBaoTri>()
                        .Select("*")
                        .Limit(1)
                        .Get();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi khởi tạo schema: {ex.Message}");
            }
        }
    }
}