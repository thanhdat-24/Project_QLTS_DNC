using ClosedXML.Excel;
using Microsoft.Win32;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.View.QuanLyKho;
using Supabase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for ChiTietMuaMoi.xaml
    /// </summary>
    public partial class ChiTietMuaMoiView : UserControl
    {
        private Supabase.Client _client;
        private Dictionary<int, string> _phieumuaMoiLookup = new Dictionary<int, string>();
        public ChiTietMuaMoiView()
        {
            InitializeComponent();
            InitializeSupabaseAsync();
            LoadData();
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

                var result = await _client
                    .From<ChiTietDeNghiMua>()
                    .Get();

                var danhSachPhieu = result.Models
                    .Select(p => new
                    {   
                        p.MaChiTietDeNghi,
                        p.MaPhieuDeNghi,
                        p.TenTaiSan,
                        p.SoLuong, 
                        p.DonViTinh,
                        p.MoTa,
                        p.DuKienGia,
                        // Thay vì dùng MaNV, tra ra tên

                    })
                .ToList();

                dgPhieuCTMuaMoi.ItemsSource = danhSachPhieu;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var optionWindow = new MuaMoiOption(); // Tên class của window mới
            optionWindow.ShowDialog(); // Hiển thị cửa sổ dưới dạng modal
        }

        private async void btnSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var keyword = txtSearch.Text.Trim().ToLowerInvariant();

                var result = await _client
                    .From<ChiTietDeNghiMua>()
                    .Get();

                var filteredList = result.Models
                    .Where(p => string.IsNullOrEmpty(keyword) ||
                                p.MaChiTietDeNghi.ToString().ToLowerInvariant().Contains(keyword))
                    .Select(p => new
                    {
                        p.MaChiTietDeNghi,
                        p.MaPhieuDeNghi,
                        p.TenTaiSan,
                        p.SoLuong,
                        p.DonViTinh,
                        p.MoTa,
                        p.DuKienGia,
                    })
                    .ToList();

                dgPhieuCTMuaMoi.ItemsSource = filteredList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                FileName = "DanhSachCTPhieuMuaMoi.xlsx"
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
            ExportDataGridToExcel(dgPhieuCTMuaMoi);
        }

        private void btnSua_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if a row is selected
                var selectedItem = dgPhieuCTMuaMoi.SelectedItem;
                if (selectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn phiếu để xóa", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Get the MaPhieuDeNghi from the selected item using reflection
                var prop = selectedItem.GetType().GetProperty("MaChiTietDeNghi");
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
                        .From<ChiTietDeNghiMua>()
                        .Where(p => p.MaChiTietDeNghi == maPhieu)
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

    }
}
