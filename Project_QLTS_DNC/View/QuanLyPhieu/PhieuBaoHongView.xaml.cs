using System;
using ClosedXML.Excel;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Project_QLTS_DNC.Models.BaoHong;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for PhieuBaoHongView.xaml
    /// </summary>
    public partial class PhieuBaoHongView : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<PhieuBaoHong> _danhSachPhieuBaoHong;
        private PhieuBaoHong _phieuBaoHongHienTai;
        private string _searchText;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<PhieuBaoHong> DanhSachPhieuBaoHong
        {
            get { return _danhSachPhieuBaoHong; }
            set
            {
                _danhSachPhieuBaoHong = value;
                OnPropertyChanged("DanhSachPhieuBaoHong");
            }
        }

        public PhieuBaoHong PhieuBaoHongHienTai
        {
            get { return _phieuBaoHongHienTai; }
            set
            {
                _phieuBaoHongHienTai = value;
                OnPropertyChanged("PhieuBaoHongHienTai");
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

        public PhieuBaoHongView()
        {
            InitializeComponent();

            // Khởi tạo dữ liệu mẫu
            KhoiTaoDuLieuMau();

            // Thiết lập DataContext
            this.DataContext = this;

            // Gán dữ liệu cho DataGrid
            dgPhieuBaoHong.ItemsSource = DanhSachPhieuBaoHong;

            // Đăng ký sự kiện
            DangKySuKien();
        }

        private void KhoiTaoDuLieuMau()
        {
            DanhSachPhieuBaoHong = new ObservableCollection<PhieuBaoHong>
            {
                new PhieuBaoHong
                {
                    MaPhieu = "PBH001",
                    MaTaiSan = "TS001",
                    TenTaiSan = "Máy tính xách tay Dell XPS 13",
                    NgayLap = DateTime.Now.AddDays(-5),
                    NguoiLap = "Nguyễn Văn A",
                    MucDoHong = "Nhẹ",
                    TrangThai = "Đã xử lý",
                    BoPhanQuanLy = "Phòng IT",
                    MoTa = "Máy tính bị hỏng bàn phím, một số phím không nhấn được"
                },
                new PhieuBaoHong
                {
                    MaPhieu = "PBH002",
                    MaTaiSan = "TS007",
                    TenTaiSan = "Máy in HP LaserJet Pro",
                    NgayLap = DateTime.Now.AddDays(-3),
                    NguoiLap = "Trần Thị B",
                    MucDoHong = "Trung bình",
                    TrangThai = "Đã duyệt",
                    BoPhanQuanLy = "Phòng Hành chính",
                    MoTa = "Máy in bị kẹt giấy thường xuyên, cần kiểm tra và sửa chữa"
                },
                new PhieuBaoHong
                {
                    MaPhieu = "PBH003",
                    MaTaiSan = "TS015",
                    TenTaiSan = "Máy chiếu Epson EB-X41",
                    NgayLap = DateTime.Now.AddDays(-1),
                    NguoiLap = "Lê Văn C",
                    MucDoHong = "Nặng",
                    TrangThai = "Mới tạo",
                    BoPhanQuanLy = "Phòng Đào tạo",
                    MoTa = "Máy chiếu không lên hình, đèn báo lỗi nhấp nháy đỏ"
                }
            };

            PhieuBaoHongHienTai = new PhieuBaoHong();
        }

        private void DangKySuKien()
        {
            // Sự kiện cho nút thêm và các nút khác
            btnThem.Click += BtnThem_Click;
            btnInPhieu.Click += BtnInPhieu_Click;
            btnIn.Click += BtnXuatExcel_Click;

            // Sự kiện tìm kiếm
            txtTimKiem.TextChanged += TxtTimKiem_TextChanged;
            btnTimKiem.Click += BtnTimKiem_Click; // Thêm xử lý cho nút tìm kiếm mới

            // Thêm sự kiện cho ComboBox
            cboTrangThai.SelectionChanged += LocDuLieu;
            cboMucDoHong.SelectionChanged += LocDuLieu;

            // Sự kiện DataGrid
            dgPhieuBaoHong.MouseDoubleClick += DgPhieuBaoHong_MouseDoubleClick;
        }
        private void BtnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Lấy phiếu được chọn để chỉnh sửa
            var button = sender as Button;
            var phieuBaoHong = button.DataContext as PhieuBaoHong;

            if (phieuBaoHong != null)
            {
                // Mở form nhập liệu với phiếu đã chọn để chỉnh sửa
                Window parentWindow = Window.GetWindow(this);
                PhieuBaoHongInputForm inputForm = new PhieuBaoHongInputForm(phieuBaoHong);
                inputForm.Owner = parentWindow;

                if (inputForm.ShowDialog() == true)
                {
                    // Nếu người dùng lưu thành công, cập nhật phiếu trong danh sách
                    int index = TimChiSoPhieuBaoHong(phieuBaoHong.MaPhieu);
                    if (index >= 0)
                    {
                        DanhSachPhieuBaoHong[index] = inputForm.PhieuBaoHong;
                    }

                    // Refresh DataGrid
                    dgPhieuBaoHong.Items.Refresh();

                    // Thông báo thành công
                    MessageBox.Show("Cập nhật phiếu báo hỏng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Lấy phiếu được chọn để xóa
            var button = sender as Button;
            var phieuBaoHong = button.DataContext as PhieuBaoHong;

            if (phieuBaoHong != null)
            {
                // Hiển thị hộp thoại xác nhận trước khi xóa
                MessageBoxResult result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa phiếu báo hỏng {phieuBaoHong.MaPhieu} không?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Thực hiện xóa phiếu
                    int index = TimChiSoPhieuBaoHong(phieuBaoHong.MaPhieu);
                    if (index >= 0)
                    {
                        DanhSachPhieuBaoHong.RemoveAt(index);
                    }

                    // Refresh DataGrid
                    dgPhieuBaoHong.Items.Refresh();

                    // Thông báo thành công
                    MessageBox.Show("Xóa phiếu báo hỏng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
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

        private void ApplyFilter()
        {
            if (dgPhieuBaoHong.ItemsSource == null) return;

            ICollectionView view = CollectionViewSource.GetDefaultView(dgPhieuBaoHong.ItemsSource);

            view.Filter = item =>
            {
                var phieuBaoHong = item as PhieuBaoHong;
                if (phieuBaoHong == null) return false;

                // Lọc theo từ khóa
                bool matchKeyword = string.IsNullOrEmpty(txtTimKiem.Text) ||
                    (phieuBaoHong.MaPhieu.Contains(txtTimKiem.Text, StringComparison.OrdinalIgnoreCase) ||
                     phieuBaoHong.MaTaiSan.Contains(txtTimKiem.Text, StringComparison.OrdinalIgnoreCase) ||
                     phieuBaoHong.TenTaiSan.Contains(txtTimKiem.Text, StringComparison.OrdinalIgnoreCase) ||
                     phieuBaoHong.NguoiLap.Contains(txtTimKiem.Text, StringComparison.OrdinalIgnoreCase));

                // Lọc theo trạng thái
                bool matchTrangThai = cboTrangThai.SelectedIndex == 0 ||
                    phieuBaoHong.TrangThai == (cboTrangThai.SelectedItem as ComboBoxItem)?.Content.ToString();

                // Lọc theo mức độ hỏng
                bool matchMucDoHong = cboMucDoHong.SelectedIndex == 0 ||
                    phieuBaoHong.MucDoHong == (cboMucDoHong.SelectedItem as ComboBoxItem)?.Content.ToString();

                return matchKeyword && matchTrangThai && matchMucDoHong;
            };

            view.Refresh();
        }


        private void BtnThem_Click(object sender, RoutedEventArgs e)
        {
            // Mở form nhập liệu
            Window parentWindow = Window.GetWindow(this);
            PhieuBaoHongInputForm inputForm = new PhieuBaoHongInputForm(null);
            inputForm.Owner = parentWindow;
            if (inputForm.ShowDialog() == true)
            {
                // Nếu người dùng lưu thành công, thêm phiếu mới vào danh sách
                PhieuBaoHong phieuMoi = inputForm.PhieuBaoHong;
                DanhSachPhieuBaoHong.Add(phieuMoi);

                // Refresh DataGrid
                dgPhieuBaoHong.Items.Refresh();

                // Thông báo thành công
                MessageBox.Show("Thêm phiếu báo hỏng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnInPhieu_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = dgPhieuBaoHong.SelectedItem as PhieuBaoHong;
            if (selectedItem != null)
            {
                MessageBox.Show($"Đang in phiếu báo hỏng {selectedItem.MaPhieu}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                // TODO: Thêm mã in phiếu tại đây
            }
            else
            {
                MessageBox.Show("Vui lòng chọn phiếu báo hỏng để in!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra danh sách có dữ liệu không
                if (DanhSachPhieuBaoHong == null || DanhSachPhieuBaoHong.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Mở dialog lưu file
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = $"DanhSachPhieuBaoHong_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Tạo workbook mới
                    var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("Danh Sách Phiếu Báo Hỏng");

                    // Tạo header
                    worksheet.Cell(1, 1).Value = "Mã Phiếu";
                    worksheet.Cell(1, 2).Value = "Mã Tài Sản";
                    worksheet.Cell(1, 3).Value = "Tên Tài Sản";
                    worksheet.Cell(1, 4).Value = "Ngày Lập";
                    worksheet.Cell(1, 5).Value = "Người Lập";
                    worksheet.Cell(1, 6).Value = "Mức Độ Hỏng";
                    worksheet.Cell(1, 7).Value = "Trạng Thái";
                    worksheet.Cell(1, 8).Value = "Bộ Phận Quản Lý";
                    worksheet.Cell(1, 9).Value = "Mô Tả";

                    // Định dạng header
                    var headerRange = worksheet.Range(1, 1, 1, 9);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

                    // Điền dữ liệu
                    for (int i = 0; i < DanhSachPhieuBaoHong.Count; i++)
                    {
                        var phieu = DanhSachPhieuBaoHong[i];
                        worksheet.Cell(i + 2, 1).Value = phieu.MaPhieu;
                        worksheet.Cell(i + 2, 2).Value = phieu.MaTaiSan;
                        worksheet.Cell(i + 2, 3).Value = phieu.TenTaiSan;
                        worksheet.Cell(i + 2, 4).Value = phieu.NgayLap.ToString("dd/MM/yyyy");
                        worksheet.Cell(i + 2, 5).Value = phieu.NguoiLap;
                        worksheet.Cell(i + 2, 6).Value = phieu.MucDoHong;
                        worksheet.Cell(i + 2, 7).Value = phieu.TrangThai;
                        worksheet.Cell(i + 2, 8).Value = phieu.BoPhanQuanLy;
                        worksheet.Cell(i + 2, 9).Value = phieu.MoTa;
                    }

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


        private void DgPhieuBaoHong_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = dgPhieuBaoHong.SelectedItem as PhieuBaoHong;
            if (selectedItem != null)
            {
                // Mở form nhập liệu với phiếu đã chọn để chỉnh sửa
                Window parentWindow = Window.GetWindow(this);
                PhieuBaoHongInputForm inputForm = new PhieuBaoHongInputForm(selectedItem);
                inputForm.Owner = parentWindow;
                if (inputForm.ShowDialog() == true)
                {
                    // Nếu người dùng lưu thành công, cập nhật phiếu trong danh sách
                    int index = TimChiSoPhieuBaoHong(selectedItem.MaPhieu);
                    if (index >= 0)
                    {
                        DanhSachPhieuBaoHong[index] = inputForm.PhieuBaoHong;
                    }

                    // Refresh DataGrid
                    dgPhieuBaoHong.Items.Refresh();

                    // Thông báo thành công
                    MessageBox.Show("Cập nhật phiếu báo hỏng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private int TimChiSoPhieuBaoHong(string maPhieu)
        {
            for (int i = 0; i < DanhSachPhieuBaoHong.Count; i++)
            {
                if (DanhSachPhieuBaoHong[i].MaPhieu == maPhieu)
                {
                    return i;
                }
            }
            return -1;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}