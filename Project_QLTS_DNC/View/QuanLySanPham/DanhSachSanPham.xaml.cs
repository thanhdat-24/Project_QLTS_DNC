using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Project_QLTS_DNC.Model;
using static Project_QLTS_DNC.Model.SanPham;

namespace Project_QLTS_DNC.View.QuanLySanPham
{
    public partial class DanhSachSanPham : UserControl
    {
        private ObservableCollection<SanPham> _listSanPham;
        private CollectionViewSource _viewSource;
        private List<PhongFilter> _phongList;
        private List<NhomTSFilter> _nhomTSList;
        private UserControl _currentForm;

        public DanhSachSanPham()
        {
            InitializeComponent();
            LoadData();
            InitializeFilters();

            // Đăng ký các event
            btnSearch.Click += BtnSearch_Click;
            txtSearch.KeyDown += TxtSearch_KeyDown;
            cboPhong.SelectionChanged += Filter_SelectionChanged;
            cboNhomTS.SelectionChanged += Filter_SelectionChanged;
            btnExportQRCode.Click += BtnExportQRCode_Click; // Thêm dòng này

            this.Loaded += DanhSachSanPham_Loaded;
        }

        private void DanhSachSanPham_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateStatusBar();
        }

        private void LoadData()
        {
            // Mẫu dữ liệu - trong thực tế sẽ lấy từ database
            _listSanPham = new ObservableCollection<SanPham>
            {
                new SanPham { MaSP = 1, MaPhong = 101, MaNhomTS = 1, TenSanPham = "Máy tính Dell XPS 13", SoSeri = "XPS13-9310", NgaySuDung = DateTime.Parse("2023-01-15"), GiaTriSP = 30000000, HanBH = DateTime.Parse("2025-01-15"), TinhTrangSP = "Tốt", GhiChu = "Mới" },
                new SanPham { MaSP = 2, MaPhong = 102, MaNhomTS = 1, TenSanPham = "Máy tính HP Pavilion", SoSeri = "HP-PAV15", NgaySuDung = DateTime.Parse("2023-02-10"), GiaTriSP = 25000000, HanBH = DateTime.Parse("2025-02-10"), TinhTrangSP = "Tốt", GhiChu = "Mới" },
                new SanPham { MaSP = 3, MaPhong = 101, MaNhomTS = 2, TenSanPham = "Màn hình Dell 27 inch", SoSeri = "DELL-S2721QS", NgaySuDung = DateTime.Parse("2023-03-05"), GiaTriSP = 8000000, HanBH = DateTime.Parse("2025-03-05"), TinhTrangSP = "Tốt", GhiChu = "Mới" },
                new SanPham { MaSP = 4, MaPhong = 103, MaNhomTS = 3, TenSanPham = "Bàn làm việc", SoSeri = "TABLE-001", NgaySuDung = DateTime.Parse("2023-04-20"), GiaTriSP = 3500000, HanBH = DateTime.Parse("2026-04-20"), TinhTrangSP = "Tốt", GhiChu = "Mới" },
                new SanPham { MaSP = 5, MaPhong = 102, MaNhomTS = 2, TenSanPham = "Máy in HP LaserJet", SoSeri = "HP-LJ2055", NgaySuDung = DateTime.Parse("2023-05-18"), GiaTriSP = 7500000, HanBH = DateTime.Parse("2025-05-18"), TinhTrangSP = "Tốt", GhiChu = "Mới" }
            };

            _viewSource = new CollectionViewSource { Source = _listSanPham };
            dgSanPham.ItemsSource = _viewSource.View;
        }

        private void InitializeFilters()
        {
            // Load unique MaPhong values for filter
            _phongList = _listSanPham.Select(x => x.MaPhong).Distinct().OrderBy(x => x)
                .Select(x => new PhongFilter { MaPhong = x }).ToList();
            _phongList.Insert(0, new PhongFilter { MaPhong = 0 }); // Add "All" option
            cboPhong.ItemsSource = _phongList;
            cboPhong.SelectedIndex = 0;

            // Load unique MaNhomTS values for filter
            _nhomTSList = _listSanPham.Select(x => x.MaNhomTS).Distinct().OrderBy(x => x)
                .Select(x => new NhomTSFilter { MaNhomTS = x }).ToList();
            _nhomTSList.Insert(0, new NhomTSFilter { MaNhomTS = 0 }); // Add "All" option
            cboNhomTS.ItemsSource = _nhomTSList;
            cboNhomTS.SelectedIndex = 0;
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplyFilters();
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            _viewSource.View.Filter = item =>
            {
                var sanPham = item as SanPham;
                if (sanPham == null) return false;

                bool matchesSearch = string.IsNullOrEmpty(txtSearch.Text) ||
                                    sanPham.TenSanPham.ToLower().Contains(txtSearch.Text.ToLower());

                bool matchesPhong = cboPhong.SelectedIndex == 0 ||
                                    sanPham.MaPhong == ((PhongFilter)cboPhong.SelectedItem).MaPhong;

                bool matchesNhom = cboNhomTS.SelectedIndex == 0 ||
                                  sanPham.MaNhomTS == ((NhomTSFilter)cboNhomTS.SelectedItem).MaNhomTS;

                return matchesSearch && matchesPhong && matchesNhom;
            };

            UpdateStatusBar();
        }

        private void UpdateStatusBar()
        {
            txtStatus.Text = $"Tổng số sản phẩm: {_viewSource.View.Cast<SanPham>().Count()} / {_listSanPham.Count}";
        }
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Lấy sản phẩm được chọn
            Button button = sender as Button;
            SanPham selectedSanPham = button.DataContext as SanPham;

            if (selectedSanPham != null)
            {
                // Mở form chỉnh sửa sản phẩm
                EditSanPham editDialog = new EditSanPham(
                    this,
                    selectedSanPham,
                    _phongList.Where(p => p.MaPhong != 0).ToList(),
                    _nhomTSList.Where(n => n.MaNhomTS != 0).ToList()
                );

                // Hiển thị form chỉnh sửa
                editDialog.Owner = Application.Current.MainWindow;
                editDialog.ShowDialog();
            }
        }

        // Phương thức để lấy mã sản phẩm mới
        public int GetNewMaSP()
        {
            return _listSanPham.Count > 0 ? _listSanPham.Max(sp => sp.MaSP) + 1 : 1;
        }

        // Phương thức để thêm sản phẩm mới vào danh sách
        public void AddSanPham(SanPham sanPham)
        {
            _listSanPham.Add(sanPham);

            // Cập nhật lại bộ lọc nếu có phòng mới hoặc nhóm tài sản mới
            if (!_phongList.Any(p => p.MaPhong == sanPham.MaPhong && p.MaPhong != 0))
            {
                _phongList.Add(new PhongFilter { MaPhong = sanPham.MaPhong });
                _phongList = _phongList.OrderBy(p => p.MaPhong).ToList();
                cboPhong.ItemsSource = null;
                cboPhong.ItemsSource = _phongList;
            }

            if (!_nhomTSList.Any(n => n.MaNhomTS == sanPham.MaNhomTS && n.MaNhomTS != 0))
            {
                _nhomTSList.Add(new NhomTSFilter { MaNhomTS = sanPham.MaNhomTS });
                _nhomTSList = _nhomTSList.OrderBy(n => n.MaNhomTS).ToList();
                cboNhomTS.ItemsSource = null;
                cboNhomTS.ItemsSource = _nhomTSList;
            }

            // Cập nhật lại status bar
            UpdateStatusBar();
        }

        // Phương thức để cập nhật sản phẩm
        public void UpdateSanPham(SanPham sanPham)
        {
            // Sản phẩm đã được cập nhật từ EditSanPham, chỉ cần refresh DataGrid và cập nhật UI
            _viewSource.View.Refresh();
            UpdateStatusBar();
        }

        // Xử lý sự kiện khi nhấn nút Delete (xóa sản phẩm)
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Lấy sản phẩm được chọn để xóa
            Button button = sender as Button;
            SanPham selectedSanPham = button.DataContext as SanPham;

            if (selectedSanPham != null)
            {
                // Hiển thị hộp thoại xác nhận xóa
                MessageBoxResult result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa sản phẩm '{selectedSanPham.TenSanPham}'?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Xóa sản phẩm
                    _listSanPham.Remove(selectedSanPham);

                    // Cập nhật lại status bar
                    UpdateStatusBar();

                    MessageBox.Show("Đã xóa sản phẩm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        // Trong class DanhSachSanPham.xaml.cs, thêm xử lý cho nút Xuất QRCODE
        private void BtnExportQRCode_Click(object sender, RoutedEventArgs e)
        {
            // Tạo cửa sổ xuất QR code
            XuatQRCode xuatQRCodeDialog = new XuatQRCode(
                _listSanPham,
                _nhomTSList.Where(n => n.MaNhomTS != 0).ToList()
            );

            // Hiển thị dialog
            xuatQRCodeDialog.Owner = Application.Current.MainWindow;
            xuatQRCodeDialog.ShowDialog();
        }
    }
}