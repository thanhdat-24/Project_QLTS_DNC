using Microsoft.IdentityModel.Tokens;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.PhieuNhapKho;
using Project_QLTS_DNC.Services;
using Supabase;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Microsoft.Win32;
using System.Data;
using System.Windows.Data;
namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    public partial class PhieuMuaMoiTSView : UserControl
    {
        private Supabase.Client _client;
        private Dictionary<int, string> _nvLookup = new Dictionary<int, string>();
        public PhieuMuaMoiTSView()
        {
            InitializeComponent();
            InitializeSupabaseAsync();
            LoadData();
        }
        private async Task LoadNhanVienLookupAsync()
        {
            try
            {
                var result = await _client.From<NhanVienModel>().Get();
                if (result.Models != null)
                {
                    _nvLookup = result.Models.ToDictionary(nv => nv.MaNV, nv => nv.TenNV);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách nhân viên: {ex.Message}");
            }
        }
        private async Task InitializeSupabaseAsync()
        {
            string supabaseUrl = "https://hoybfwnugefnpctgghha.supabase.co";
            string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhveWJmd251Z2VmbnBjdGdnaGhhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDQxMDQ4OTEsImV4cCI6MjA1OTY4MDg5MX0.KxNfiOUFXHGgqZf3b3xOk6BR4sllMZG_-W-y_OPUwCI";

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false,
                AutoRefreshToken = false
            };

            _client = new Supabase.Client(supabaseUrl, supabaseKey, options);
            await _client.InitializeAsync();
        }
        private async Task LoadData()
        {
            try
            {
                // Tải danh sách nhân viên trước (nếu chưa có)
                if (_nvLookup == null || !_nvLookup.Any())
                {
                    await LoadNhanVienLookupAsync();
                }

                var result = await _client
                    .From<MuaMoiTS>()
                    .Get();

                var danhSachPhieu = result.Models
                    .Select(p => new
                    {
                        p.MaPhieuDeNghi,
                        p.NgayDeNghi,
                        p.DonViDeNghi,
                        p.LyDo,
                        p.GhiChu,
                        TrangThai = p.TrangThai.HasValue && p.TrangThai.Value ? "Đã Duyệt" : "Chờ Duyệt",
                        // Thay vì dùng MaNV, tra ra tên
                        TenNhanVien = _nvLookup.TryGetValue((int)p.MaNV, out var tenNV) ? tenNV : $"#{p.MaNV}"
                    })
                    .ToList();

                dgPhieuXuatKho.ItemsSource = danhSachPhieu;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var form = new PhieuMuaMoiTS(); // Đây là cửa sổ tạo mới phiếu
            if (form.ShowDialog() == true)
            {
                LoadData(); // Sau khi thêm xong thì reload lại danh sách
            }
        }

        private async void btnSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var keyword = txtSearch.Text.Trim().ToLowerInvariant();

                var result = await _client
                    .From<MuaMoiTS>()
                    .Get();

                var filteredList = result.Models
                    .Where(p => string.IsNullOrEmpty(keyword) ||
                                p.MaPhieuDeNghi.ToString().ToLowerInvariant().Contains(keyword))
                    .Select(p => new
                    {
                        p.MaPhieuDeNghi,
                        p.NgayDeNghi,
                        p.DonViDeNghi,
                        p.LyDo,
                        p.GhiChu,
                        TrangThai = p.TrangThai.HasValue && p.TrangThai.Value ? "Đã Duyệt" : "Chờ Duyệt",
                        TenNhanVien = _nvLookup.TryGetValue((int)p.MaNV, out var tenNV) ? tenNV : $"#{p.MaNV}"
                    })
                    .ToList();

                dgPhieuXuatKho.ItemsSource = filteredList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

           private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if a row is selected
                var selectedItem = dgPhieuXuatKho.SelectedItem;
                if (selectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn phiếu để xóa", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Get the MaPhieuDeNghi from the selected item using reflection
                var prop = selectedItem.GetType().GetProperty("MaPhieuDeNghi");
                if (prop == null)
                {
                    MessageBox.Show("Không thể xác định mã phiếu", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int maPhieu = (int)prop.GetValue(selectedItem);

                // Confirm deletion
                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa phiếu {maPhieu}?",
                                            "Xác nhận xóa",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Delete from database
                    await _client
                        .From<MuaMoiTS>()
                        .Where(p => p.MaPhieuDeNghi == maPhieu)
                        .Delete();

                    MessageBox.Show("Xóa phiếu thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Reload data to refresh the view
                    await LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa phiếu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnSua_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                // Kiểm tra có dòng nào được chọn không
                var selectedItem = dgPhieuXuatKho.SelectedItem;
                if (selectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn phiếu để sửa", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Lấy mã phiếu
                var prop = selectedItem.GetType().GetProperty("MaPhieuDeNghi");
                if (prop == null)
                {
                    MessageBox.Show("Không thể xác định mã phiếu", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int maPhieu = (int)prop.GetValue(selectedItem);

                // Tải thông tin phiếu từ database
                var result = await _client
                    .From<MuaMoiTS>()
                    .Where(p => p.MaPhieuDeNghi == maPhieu)
                    .Get();

                var phieu = result.Models.FirstOrDefault();
                if (phieu == null)
                {
                    MessageBox.Show("Không tìm thấy thông tin phiếu", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Mở form chỉnh sửa và truyền phiếu
                var editForm = new PhieuMuaMoiTS();
                editForm.SetPhieuForEdit(phieu);

                if (editForm.ShowDialog() == true)
                {
                    // Tải lại dữ liệu
                    await LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi sửa phiếu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadPhieuForEdit(int maPhieu)
        {
            try
            {
                // Lấy thông tin phiếu từ database
                var result = await _client
                    .From<MuaMoiTS>()
                    .Where(p => p.MaPhieuDeNghi == maPhieu)
                    .Get();

                var phieu = result.Models.FirstOrDefault();
                if (phieu == null)
                {
                    MessageBox.Show("Không tìm thấy thông tin phiếu", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Mở form chỉnh sửa với constructor mặc định
                var editForm = new PhieuMuaMoiTS();

                // Sau khi tạo form, thiết lập chế độ chỉnh sửa
                await editForm.SetEditMode(maPhieu);

                if (editForm.ShowDialog() == true)
                {
                    // Tải lại dữ liệu sau khi chỉnh sửa hoàn tất
                    await LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin phiếu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadData(); // Giả sử bạn có hàm LoadData() để load lại danh sách
            txtSearch.Clear(); // Xóa ô tìm kiếm nếu cần
        }
        private void ExportDataGridToExcel(DataGrid dataGrid)
        {
            // Tạo DataTable từ DataGrid
            DataTable dt = new DataTable();

            foreach (DataGridColumn column in dataGrid.Columns)
            {
                // Header là tên cột
                dt.Columns.Add(column.Header.ToString());
            }

            foreach (var item in dataGrid.Items)
            {
                if (item == CollectionView.NewItemPlaceholder) continue;

                DataRow row = dt.NewRow();

                for (int i = 0; i < dataGrid.Columns.Count; i++)
                {
                    var column = dataGrid.Columns[i];

                    var cellContent = column.GetCellContent(item);
                    if (cellContent is TextBlock tb)
                        row[i] = tb.Text;
                }

                dt.Rows.Add(row);
            }

            // Mở hộp thoại lưu
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = "DanhSachPhieuMuaMoi.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(dt, "DanhSachPhieu");

                    // Format sheet (optional)
                    worksheet.Columns().AdjustToContents();

                    workbook.SaveAs(saveFileDialog.FileName);
                    MessageBox.Show("Xuất Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ExportDataGridToExcel(dgPhieuXuatKho);
        }
    }
}
