using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Project_QLTS_DNC.Model;

namespace Project_QLTS_DNC.View.QuanLySanPham
{
    public partial class DanhSachSanPham : Window
    {
        private ObservableCollection<SanPham> _listSanPham;
        private CollectionViewSource _viewSource;

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
            btnAdd.Click += BtnAdd_Click;

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
            var uniquePhongs = _listSanPham.Select(x => x.MaPhong).Distinct().OrderBy(x => x)
                .Select(x => new { MaPhong = x }).ToList();
            uniquePhongs.Insert(0, new { MaPhong = 0 }); // Add "All" option
            cboPhong.ItemsSource = uniquePhongs;
            cboPhong.SelectedIndex = 0;

            // Load unique MaNhomTS values for filter
            var uniqueNhoms = _listSanPham.Select(x => x.MaNhomTS).Distinct().OrderBy(x => x)
                .Select(x => new { MaNhomTS = x }).ToList();
            uniqueNhoms.Insert(0, new { MaNhomTS = 0 }); // Add "All" option
            cboNhomTS.ItemsSource = uniqueNhoms;
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
                                    sanPham.MaPhong == ((dynamic)cboPhong.SelectedItem).MaPhong;

                bool matchesNhom = cboNhomTS.SelectedIndex == 0 ||
                                  sanPham.MaNhomTS == ((dynamic)cboNhomTS.SelectedItem).MaNhomTS;

                return matchesSearch && matchesPhong && matchesNhom;
            };

            UpdateStatusBar();
        }

        private void UpdateStatusBar()
        {
            txtStatus.Text = $"Tổng số sản phẩm: {_viewSource.View.Cast<SanPham>().Count()} / {_listSanPham.Count}";
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Mở form thêm mới sản phẩm
            MessageBox.Show("Chức năng thêm sản phẩm sẽ được triển khai sau.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}