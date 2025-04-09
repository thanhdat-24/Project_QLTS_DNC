using System;
using ClosedXML.Excel;
using Microsoft.Win32;
using System.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Linq;
using Project_QLTS_DNC.Models;
using System.Collections.Generic;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for DanhSachBaoTri.xaml
    /// </summary>
    public partial class DanhSachBaoTri : Window, INotifyPropertyChanged
    {
        private ObservableCollection<TaiSanCanBaoTri> _danhSachTaiSan;
        private string _searchText;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TaiSanCanBaoTri> DanhSachTaiSan
        {
            get { return _danhSachTaiSan; }
            set
            {
                _danhSachTaiSan = value;
                OnPropertyChanged("DanhSachTaiSan");
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged("SearchText");
                ApplyFilter();
            }
        }

        public DanhSachBaoTri()
        {
            InitializeComponent();

            // Khởi tạo dữ liệu mẫu
            KhoiTaoDuLieuMau();

            // Thiết lập DataContext
            this.DataContext = this;

            // Đăng ký sự kiện
            DangKySuKien();
        }

        private void KhoiTaoDuLieuMau()
        {
            // Trong thực tế, dữ liệu này sẽ được lấy từ CSDL dựa trên điều kiện TinhTrangPhanTram <= 50
            DanhSachTaiSan = new ObservableCollection<TaiSanCanBaoTri>
            {
                new TaiSanCanBaoTri
                {
                    MaTaiSan = "TS001",
                    TenTaiSan = "Máy tính văn phòng Dell Optiplex 3080",
                    NhomTaiSan = "Máy tính",
                    ViTri = "Bàn làm việc số 1",
                    TenPhong = "Phòng Kế toán",
                    TinhTrangPhanTram = 30,
                    GhiChu = "Hỏng ổ cứng, chạy chậm",
                    IsSelected = false
                },
                new TaiSanCanBaoTri
                {
                    MaTaiSan = "TS002",
                    TenTaiSan = "Máy in laser HP LaserJet Pro M404dn",
                    NhomTaiSan = "Máy in",
                    ViTri = "Góc phòng",
                    TenPhong = "Phòng Hành chính",
                    TinhTrangPhanTram = 20,
                    GhiChu = "Kẹt giấy thường xuyên, mực ít",
                    IsSelected = false
                },
                new TaiSanCanBaoTri
                {
                    MaTaiSan = "TS005",
                    TenTaiSan = "Điều hòa Panasonic 1 chiều 12000BTU",
                    NhomTaiSan = "Điều hòa",
                    ViTri = "Tường phía Nam",
                    TenPhong = "Phòng họp",
                    TinhTrangPhanTram = 45,
                    GhiChu = "Làm lạnh kém, có tiếng ồn",
                    IsSelected = false
                },
                new TaiSanCanBaoTri
                {
                    MaTaiSan = "TS008",
                    TenTaiSan = "Switch Cisco SG350-28 28-Port",
                    NhomTaiSan = "Thiết bị mạng",
                    ViTri = "Tủ Rack",
                    TenPhong = "Phòng server",
                    TinhTrangPhanTram = 40,
                    GhiChu = "Một số cổng không hoạt động",
                    IsSelected = false
                },
                new TaiSanCanBaoTri
                {
                    MaTaiSan = "TS010",
                    TenTaiSan = "Bàn làm việc văn phòng 160x80cm",
                    NhomTaiSan = "Bàn ghế",
                    ViTri = "Cửa vào",
                    TenPhong = "Phòng Kế hoạch",
                    TinhTrangPhanTram = 50,
                    GhiChu = "Mặt bàn bị xước, chân bàn lung lay",
                    IsSelected = false
                }
            };
        }

        private void DangKySuKien()
        {
            // Sự kiện cho các nút
            btnTaoPhieuBaoTri.Click += BtnTaoPhieuBaoTri_Click;
            btnDong.Click += BtnDong_Click;
            btnXuatExcel.Click += BtnXuatExcel_Click;

            // Sự kiện tìm kiếm
            txtTimKiem.TextChanged += TxtTimKiem_TextChanged;
            btnTimKiem.Click += BtnTimKiem_Click;

            // Sự kiện lọc
            cboLoaiBaoTri.SelectionChanged += LocDuLieu;
            cboNhomTaiSan.SelectionChanged += LocDuLieu;
            cboTinhTrang.SelectionChanged += LocDuLieu;

            // Gán ItemsSource cho DataGrid
            dgDanhSachTaiSan.ItemsSource = DanhSachTaiSan;

            // Sự kiện DataGrid
            dgDanhSachTaiSan.MouseDoubleClick += DgDanhSachTaiSan_MouseDoubleClick;
        }

        private void BtnTaoPhieuBaoTri_Click(object sender, RoutedEventArgs e)
        {
            // Lấy danh sách các tài sản đã chọn
            var danhSachDaChon = DanhSachTaiSan.Where(x => x.IsSelected).ToList();

            if (danhSachDaChon.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một tài sản để tạo phiếu bảo trì!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Trong thực tế, tạo phiếu bảo trì từ danh sách đã chọn và lưu vào CSDL
                // Sau đó mở form phiếu bảo trì với danh sách đã chọn

                // Mở form nhập liệu phiếu bảo trì
                PhieuBaoTriInputForm inputForm = new PhieuBaoTriInputForm(null)
                {
                    Owner = this
                };

                // Có thể thiết lập thông tin mặc định cho phiếu bảo trì từ danh sách tài sản đã chọn
                inputForm.ThietLapDanhSachTaiSan(danhSachDaChon);

                if (inputForm.ShowDialog() == true)
                {
                    // Cập nhật lại danh sách tài sản sau khi tạo phiếu
                    // LoadDanhSachTaiSan();

                    // Thông báo thành công
                    MessageBox.Show("Tạo phiếu bảo trì thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo phiếu bảo trì: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            // Áp dụng bộ lọc khi nhấn nút tìm kiếm
            SearchText = txtTimKiem.Text;
            ApplyFilter();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Lấy dữ liệu từ hàng được chọn
            var selectedItem = (TaiSanCanBaoTri)((Button)sender).DataContext;

            // Hiển thị hộp thoại xác nhận
            MessageBoxResult result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa tài sản '{selectedItem.TenTaiSan}' khỏi danh sách không?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Xóa tài sản khỏi danh sách
                    DanhSachTaiSan.Remove(selectedItem);

                    // Refresh DataGrid
                    ICollectionView view = CollectionViewSource.GetDefaultView(dgDanhSachTaiSan.ItemsSource);
                    view.Refresh();

                    // Thông báo thành công
                    MessageBox.Show("Xóa tài sản khỏi danh sách thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa tài sản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Lấy dữ liệu từ hàng được chọn
            var selectedItem = (TaiSanCanBaoTri)((Button)sender).DataContext;

            try
            {
                // Mở form chỉnh sửa thông tin tài sản
                TaiSanCanBaoTriEditForm editForm = new TaiSanCanBaoTriEditForm(selectedItem)
                {
                    Owner = this
                };

                if (editForm.ShowDialog() == true)
                {
                    // Cập nhật dữ liệu sau khi sửa
                    // Trong thực tế, cần cập nhật vào CSDL

                    // Refresh DataGrid
                    ICollectionView view = CollectionViewSource.GetDefaultView(dgDanhSachTaiSan.ItemsSource);
                    view.Refresh();

                    // Thông báo thành công
                    MessageBox.Show("Cập nhật thông tin tài sản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật thông tin tài sản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchText = txtTimKiem.Text;
        }

        private void LocDuLieu(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ChkSelectAll_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = (chkSelectAll.IsChecked == true);

            // Áp dụng trạng thái chọn cho tất cả các mục trong danh sách
            foreach (var item in DanhSachTaiSan)
            {
                item.IsSelected = isChecked;
            }

            // Refresh DataGrid
            ICollectionView view = CollectionViewSource.GetDefaultView(dgDanhSachTaiSan.ItemsSource);
            view.Refresh();
        }

        private void ApplyFilter()
        {
            if (dgDanhSachTaiSan.ItemsSource == null) return;

            ICollectionView view = CollectionViewSource.GetDefaultView(dgDanhSachTaiSan.ItemsSource);

            view.Filter = item =>
            {
                var taiSan = item as TaiSanCanBaoTri;
                if (taiSan == null) return false;

                // Lọc theo từ khóa
                bool matchKeyword = string.IsNullOrEmpty(txtTimKiem.Text) ||
                    (taiSan.MaTaiSan.Contains(txtTimKiem.Text, StringComparison.OrdinalIgnoreCase) ||
                     taiSan.TenTaiSan.Contains(txtTimKiem.Text, StringComparison.OrdinalIgnoreCase) ||
                     taiSan.NhomTaiSan.Contains(txtTimKiem.Text, StringComparison.OrdinalIgnoreCase) ||
                     taiSan.TenPhong.Contains(txtTimKiem.Text, StringComparison.OrdinalIgnoreCase));

                // Lọc theo nhóm tài sản
                ComboBoxItem selectedNhomTaiSan = cboNhomTaiSan.SelectedItem as ComboBoxItem;
                bool matchNhomTaiSan = selectedNhomTaiSan == null ||
                    selectedNhomTaiSan.Content.ToString() == "Tất cả nhóm" ||
                    taiSan.NhomTaiSan == selectedNhomTaiSan.Content.ToString();

                // Lọc theo loại bảo trì (trong trường hợp thực tế cần thêm thông tin này vào model)
                bool matchLoaiBaoTri = true; // Giả định mặc định là phù hợp
                ComboBoxItem selectedLoaiBaoTri = cboLoaiBaoTri.SelectedItem as ComboBoxItem;
                if (selectedLoaiBaoTri != null && selectedLoaiBaoTri.Content.ToString() != "Tất cả loại")
                {
                    // Trong thực tế, cần kiểm tra loại bảo trì phù hợp
                    // matchLoaiBaoTri = taiSan.LoaiBaoTri == selectedLoaiBaoTri.Content.ToString();
                }

                // Lọc theo tình trạng
                bool matchTinhTrang = true; // Giả định mặc định là phù hợp
                ComboBoxItem selectedTinhTrang = cboTinhTrang.SelectedItem as ComboBoxItem;
                if (selectedTinhTrang != null)
                {
                    if (selectedTinhTrang.Content.ToString() == "Dưới 50%")
                    {
                        matchTinhTrang = taiSan.TinhTrangPhanTram <= 50;
                    }
                    else if (selectedTinhTrang.Content.ToString() == "Trên 50%")
                    {
                        matchTinhTrang = taiSan.TinhTrangPhanTram > 50;
                    }
                }

                return matchKeyword && matchNhomTaiSan && matchLoaiBaoTri && matchTinhTrang;
            };

            view.Refresh();
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra danh sách có dữ liệu không
                if (DanhSachTaiSan == null || DanhSachTaiSan.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Mở dialog lưu file
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = $"DanhSachTaiSanCanBaoTri_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Tạo workbook mới
                    var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("Danh Sách Tài Sản Cần Bảo Trì");

                    // Thiết lập tiêu đề báo cáo
                    worksheet.Cell("A1").Value = "DANH SÁCH TÀI SẢN CẦN BẢO TRÌ";
                    worksheet.Range("A1:H1").Merge();
                    worksheet.Cell("A1").Style.Font.Bold = true;
                    worksheet.Cell("A1").Style.Font.FontSize = 16;
                    worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Thêm ngày tháng xuất báo cáo
                    worksheet.Cell("A2").Value = $"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}";
                    worksheet.Range("A2:H2").Merge();
                    worksheet.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Tạo header cho bảng dữ liệu
                    worksheet.Cell("A4").Value = "STT";
                    worksheet.Cell("B4").Value = "Mã Tài Sản";
                    worksheet.Cell("C4").Value = "Tên Tài Sản";
                    worksheet.Cell("D4").Value = "Nhóm Tài Sản";
                    worksheet.Cell("E4").Value = "Vị Trí";
                    worksheet.Cell("F4").Value = "Phòng";
                    worksheet.Cell("G4").Value = "Tình Trạng (%)";
                    worksheet.Cell("H4").Value = "Ghi Chú";

                    // Định dạng header
                    var headerRange = worksheet.Range("A4:H4");
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Điền dữ liệu
                    for (int i = 0; i < DanhSachTaiSan.Count; i++)
                    {
                        var taiSan = DanhSachTaiSan[i];
                        worksheet.Cell(i + 5, 1).Value = i + 1; // STT
                        worksheet.Cell(i + 5, 2).Value = taiSan.MaTaiSan;
                        worksheet.Cell(i + 5, 3).Value = taiSan.TenTaiSan;
                        worksheet.Cell(i + 5, 4).Value = taiSan.NhomTaiSan;
                        worksheet.Cell(i + 5, 5).Value = taiSan.ViTri;
                        worksheet.Cell(i + 5, 6).Value = taiSan.TenPhong;
                        worksheet.Cell(i + 5, 7).Value = taiSan.TinhTrangPhanTram;
                        worksheet.Cell(i + 5, 8).Value = taiSan.GhiChu;
                    }

                    // Định dạng dữ liệu
                    var dataRange = worksheet.Range(5, 1, DanhSachTaiSan.Count + 4, 8);
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Căn giữa một số cột
                    worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // STT
                    worksheet.Column(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Mã tài sản
                    worksheet.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Tình trạng

                    // Tự động điều chỉnh độ rộng cột
                    worksheet.Columns().AdjustToContents();

                    // Lưu file
                    workbook.SaveAs(saveFileDialog.FileName);

                    MessageBox.Show("Xuất Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDong_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DgDanhSachTaiSan_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = dgDanhSachTaiSan.SelectedItem as TaiSanCanBaoTri;
            if (selectedItem != null)
            {
                BtnEdit_Click(new Button { DataContext = selectedItem }, new RoutedEventArgs());
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Để sử dụng form này từ form PhieuBaoTriView
        public void LoadDanhSachTaiSanCanBaoTri()
        {
            // Trong thực tế, bạn sẽ lấy dữ liệu từ cơ sở dữ liệu dựa trên điều kiện tình trạng <= 50%
            // Ví dụ:
            /*
            string query = @"SELECT ts.mataisan, ts.tentaisan, nts.tennhomts, 
                            kt.tinhtrang, kt.vitrithucte, p.tenphong, kt.ghichu
                            FROM kiemketaisan kt
                            JOIN taisan ts ON kt.mataisan = ts.mataisan
                            JOIN nhomtaisan nts ON ts.manhomts = nts.manhomts
                            JOIN phong p ON kt.maphong = p.maphong
                            WHERE CAST(SUBSTRING(kt.tinhtrang, 1, CHARINDEX('%', kt.tinhtrang) - 1) AS INT) <= 50
                            ORDER BY CAST(SUBSTRING(kt.tinhtrang, 1, CHARINDEX('%', kt.tinhtrang) - 1) AS INT) ASC";
            */

            // Sau khi lấy dữ liệu, cập nhật danh sách
            // DanhSachTaiSan = new ObservableCollection<TaiSanCanBaoTri>(danhSachTuDatabase);
        }
    }

    // Model cho tài sản cần bảo trì
    public class TaiSanCanBaoTri : INotifyPropertyChanged
    {
        private string _maTaiSan;
        private string _tenTaiSan;
        private string _nhomTaiSan;
        private string _viTri;
        private string _tenPhong;
        private int _tinhTrangPhanTram;
        private string _ghiChu;
        private bool _isSelected;

        public event PropertyChangedEventHandler PropertyChanged;

        public string MaTaiSan
        {
            get { return _maTaiSan; }
            set
            {
                _maTaiSan = value;
                OnPropertyChanged("MaTaiSan");
            }
        }

        public string TenTaiSan
        {
            get { return _tenTaiSan; }
            set
            {
                _tenTaiSan = value;
                OnPropertyChanged("TenTaiSan");
            }
        }

        public string NhomTaiSan
        {
            get { return _nhomTaiSan; }
            set
            {
                _nhomTaiSan = value;
                OnPropertyChanged("NhomTaiSan");
            }
        }

        public string ViTri
        {
            get { return _viTri; }
            set
            {
                _viTri = value;
                OnPropertyChanged("ViTri");
            }
        }

        public string TenPhong
        {
            get { return _tenPhong; }
            set
            {
                _tenPhong = value;
                OnPropertyChanged("TenPhong");
            }
        }

        public int TinhTrangPhanTram
        {
            get { return _tinhTrangPhanTram; }
            set
            {
                _tinhTrangPhanTram = value;
                OnPropertyChanged("TinhTrangPhanTram");
            }
        }

        public string GhiChu
        {
            get { return _ghiChu; }
            set
            {
                _ghiChu = value;
                OnPropertyChanged("GhiChu");
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}