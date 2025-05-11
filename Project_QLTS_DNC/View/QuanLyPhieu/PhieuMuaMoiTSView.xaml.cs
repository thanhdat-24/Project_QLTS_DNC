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
using System.IO;
using Project_QLTS_DNC.Helpers;
using Project_QLTS_DNC.View.QuanLyKho;
namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    public partial class PhieuMuaMoiTSView : UserControl
    {
        private Supabase.Client _client;
        private Dictionary<int, string> _nvLookup = new Dictionary<int, string>();
        private List<MuaMoiTS> _danhSachPhieuGoc = new();  // Khai báo ở đầu class

        public PhieuMuaMoiTSView()
        {
            InitializeComponent();
            InitializeSupabaseAsync();
            LoadData();
            cbNgayDeNghiFilter.ItemsSource = new List<string> { "Tất cả ngày" };
            cbNgayDeNghiFilter.SelectedIndex = 0;
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

                // Lấy dữ liệu phiếu
                var result = await _client
                    .From<MuaMoiTS>()
                    .Get();

                _danhSachPhieuGoc = result.Models;

                // Chuyển đổi dữ liệu thành một danh sách có thể hiển thị
                var danhSachPhieu = result.Models
                    .OrderByDescending(p => p.MaPhieuDeNghi)
                    .Select(p => new
                    {
                        p.MaPhieuDeNghi,
                        p.NgayDeNghi,
                        p.DonViDeNghi,
                        p.LyDo,
                        p.GhiChu,
                        TrangThai = p.TrangThai switch
                        {
                            true => "Đã Duyệt",
                            false => "Từ Chối Duyệt",
                            null => "Chưa Duyệt"
                        },
                        TenNhanVien = _nvLookup.TryGetValue((int)p.MaNV, out var tenNV) ? tenNV : $"#{p.MaNV}"
                    })
                    .ToList();

                // Gán danh sách phiếu vào DataGrid
                dgPhieuXuatKho.ItemsSource = danhSachPhieu;

                // Cập nhật ComboBox với các ngày duy nhất từ NgayDeNghi
                await LoadNgayDeNghiFilter();
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

            // Hộp thoại lưu file
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = "DanhSachPhieuMuaMoi.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Đọc thông tin công ty
                var thongTinCongTy = ThongTinCongTyService.DocThongTinCongTy();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("DanhSachPhieu");

                    int currentRow = 1;

                    // Thêm logo công ty (nếu có)
                    if (!string.IsNullOrEmpty(thongTinCongTy.LogoPath) && File.Exists(thongTinCongTy.LogoPath))
                    {
                        var image = worksheet.AddPicture(thongTinCongTy.LogoPath)
                                             .MoveTo(worksheet.Cell(currentRow, 1))
                                             .WithSize(140, 70); // width, height in px
                        worksheet.Row(currentRow).Height = 60;
                    }

                    // Tên công ty
                    worksheet.Cell(currentRow, 2).Value = thongTinCongTy.Ten;
                    worksheet.Cell(currentRow, 2).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, 2).Style.Font.FontSize = 14;
                    worksheet.Range(currentRow, 2, currentRow, dt.Columns.Count).Merge();
                    worksheet.Range(currentRow, 2, currentRow, dt.Columns.Count).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    currentRow++;

                    // Địa chỉ
                    worksheet.Cell(currentRow, 2).Value = "Địa chỉ: " + thongTinCongTy.DiaChi;
                    worksheet.Range(currentRow, 2, currentRow, dt.Columns.Count).Merge();
                    worksheet.Range(currentRow, 2, currentRow, dt.Columns.Count).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    currentRow++;

                    // SĐT & Email
                    worksheet.Cell(currentRow, 2).Value = $"SĐT: {thongTinCongTy.SoDienThoai} - Email: {thongTinCongTy.Email}";
                    worksheet.Range(currentRow, 2, currentRow, dt.Columns.Count).Merge();
                    worksheet.Range(currentRow, 2, currentRow, dt.Columns.Count).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    currentRow++;

                    // Mã số thuế
                    worksheet.Cell(currentRow, 2).Value = "Mã số thuế: " + thongTinCongTy.MaSoThue;
                    worksheet.Range(currentRow, 2, currentRow, dt.Columns.Count).Merge();
                    worksheet.Range(currentRow, 2, currentRow, dt.Columns.Count).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    currentRow += 2;

                    // Tiêu đề
                    worksheet.Cell(currentRow, 1).Value = "DANH SÁCH PHIẾU MUA MỚI";
                    worksheet.Range(currentRow, 1, currentRow, dt.Columns.Count).Merge();
                    worksheet.Range(currentRow, 1, currentRow, dt.Columns.Count).Style.Font.Bold = true;
                    worksheet.Range(currentRow, 1, currentRow, dt.Columns.Count).Style.Font.FontSize = 16;
                    worksheet.Range(currentRow, 1, currentRow, dt.Columns.Count).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    currentRow += 2;

                    // Đổ dữ liệu từ DataTable vào sheet bắt đầu từ currentRow
                    worksheet.Cell(currentRow, 1).InsertTable(dt);

                    // Định dạng cột
                    worksheet.Columns().AdjustToContents();

                    // Thêm ngày xuất
                    int lastRow = currentRow + dt.Rows.Count + 2;
                    worksheet.Cell(lastRow, dt.Columns.Count - 1).Value = "Ngày xuất báo cáo:";
                    worksheet.Cell(lastRow, dt.Columns.Count).Value = DateTime.Now;
                    worksheet.Cell(lastRow, dt.Columns.Count).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
                    worksheet.Range(lastRow, dt.Columns.Count - 1, lastRow, dt.Columns.Count).Style.Font.Italic = true;

                    // Lưu file
                    workbook.SaveAs(saveFileDialog.FileName);
                    MessageBox.Show("Xuất Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ExportDataGridToExcel(dgPhieuXuatKho);
        }

        private void btnChiTiet_Click(object sender, RoutedEventArgs e)
        {
            {

                if (sender is Button btn && btn.Tag is int maPhieuMoi)
                {
                    var form = new ChiTietMuaMoiView(maPhieuMoi);
                    form.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Không thể lấy thông tin phiếu mua mới.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private async Task LoadNgayDeNghiFilter()
        {
            try
            {
                // Kiểm tra xem có dữ liệu không
                if (_danhSachPhieuGoc == null || !_danhSachPhieuGoc.Any())
                {
                    // Nếu không có dữ liệu, chỉ hiển thị tùy chọn "Tất cả ngày"
                    cbNgayDeNghiFilter.ItemsSource = new List<string> { "Tất cả ngày" };
                    cbNgayDeNghiFilter.SelectedIndex = 0;
                    return;
                }

                // Lấy các ngày duy nhất
                var ngayDuyNhat = _danhSachPhieuGoc
                    .Where(p => p.NgayDeNghi != default) // Đảm bảo không lấy ngày mặc định
                    .Select(p => p.NgayDeNghi.Date)
                    .Distinct()
                    .OrderByDescending(d => d)  // Sắp xếp ngày theo thứ tự giảm dần (mới nhất lên đầu)
                    .ToList();

                // Kiểm tra xem có ngày nào không
                if (!ngayDuyNhat.Any())
                {
                    cbNgayDeNghiFilter.ItemsSource = new List<string> { "Tất cả ngày" };
                    cbNgayDeNghiFilter.SelectedIndex = 0;
                    return;
                }

                // Tạo danh sách bao gồm mục "Tất cả ngày"
                var tuyChonLoc = new List<object> { "Tất cả ngày" };
                tuyChonLoc.AddRange(ngayDuyNhat.Cast<object>());

                // In log để debug
                Console.WriteLine($"Số lượng ngày: {ngayDuyNhat.Count}");
                foreach (var ngay in ngayDuyNhat)
                {
                    Console.WriteLine($"Ngày: {ngay:dd/MM/yyyy}");
                }

                // Thiết lập ItemsSource cho ComboBox
                cbNgayDeNghiFilter.ItemsSource = tuyChonLoc;
                cbNgayDeNghiFilter.SelectedIndex = 0; // Chọn "Tất cả ngày" mặc định
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải ngày: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void cbNgayDeNghiFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbNgayDeNghiFilter.SelectedItem != null)
            {
                if (cbNgayDeNghiFilter.SelectedItem is string &&
                    (string)cbNgayDeNghiFilter.SelectedItem == "Tất cả ngày")
                {
                    // Hiển thị tất cả bản ghi
                    HienThiTatCaBanGhi();
                }
                else if (cbNgayDeNghiFilter.SelectedItem is DateTime ngayDuocChon)
                {
                    // Lọc theo ngày được chọn
                    FilterDataByDate(ngayDuocChon);
                }
            }
        }

        private void HienThiTatCaBanGhi()
        {
            // Định dạng và hiển thị tất cả bản ghi
            var tatCaBanGhi = _danhSachPhieuGoc
                .Select(p => new
                {
                    p.MaPhieuDeNghi,
                    p.NgayDeNghi,
                    p.DonViDeNghi,
                    p.LyDo,
                    p.GhiChu,
                    TrangThai = p.TrangThai switch
                    {
                        true => "Đã Duyệt",
                        false => "Từ Chối Duyệt",
                        null => "Chưa Duyệt"
                    },
                    TenNhanVien = _nvLookup.TryGetValue((int)p.MaNV, out var tenNV) ? tenNV : $"#{p.MaNV}"
                })
                .ToList();

            dgPhieuXuatKho.ItemsSource = tatCaBanGhi;
        }

        private void FilterDataByDate(DateTime selectedDate)
        {
            // Lọc dữ liệu theo ngày được chọn
            var ketQuaLoc = _danhSachPhieuGoc
                .Where(p => p.NgayDeNghi.Date == selectedDate.Date)
                .Select(p => new
                {
                    p.MaPhieuDeNghi,
                    p.NgayDeNghi,
                    p.DonViDeNghi,
                    p.LyDo,
                    p.GhiChu,
                    TrangThai = p.TrangThai switch
                    {
                        true => "Đã Duyệt",
                        false => "Từ Chối Duyệt",
                        null => "Chưa Duyệt"
                    },
                    TenNhanVien = _nvLookup.TryGetValue((int)p.MaNV, out var tenNV) ? tenNV : $"#{p.MaNV}"
                })
                .ToList();

            dgPhieuXuatKho.ItemsSource = ketQuaLoc;
        }







    }
}
