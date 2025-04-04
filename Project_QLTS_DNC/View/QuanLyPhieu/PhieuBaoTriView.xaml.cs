﻿using System;
using ClosedXML.Excel;
using Microsoft.Win32;
using System.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Project_QLTS_DNC.Models; 

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for PhieuBaoTriView.xaml
    /// </summary>
    public partial class PhieuBaoTriView : Window, INotifyPropertyChanged
    {
        private ObservableCollection<PhieuBaoTri> _danhSachPhieuBaoTri;
        private PhieuBaoTri _phieuBaoTriHienTai;
        private string _searchText;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<PhieuBaoTri> DanhSachPhieuBaoTri
        {
            get { return _danhSachPhieuBaoTri; }
            set
            {
                _danhSachPhieuBaoTri = value;
                OnPropertyChanged("DanhSachPhieuBaoTri");
            }
        }

        public PhieuBaoTri PhieuBaoTriHienTai
        {
            get { return _phieuBaoTriHienTai; }
            set
            {
                _phieuBaoTriHienTai = value;
                OnPropertyChanged("PhieuBaoTriHienTai");
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

        public PhieuBaoTriView()
        {
            InitializeComponent();

            // Khởi tạo dữ liệu mẫu
            KhoiTaoDuLieuMau();

            // Thiết lập DataContext
            this.DataContext = this;

            // Gán dữ liệu cho DataGrid
            dgPhieuBaoTri.ItemsSource = DanhSachPhieuBaoTri;

            // Đăng ký sự kiện
            DangKySuKien();
        }

        private void KhoiTaoDuLieuMau()
        {
            DanhSachPhieuBaoTri = new ObservableCollection<PhieuBaoTri>
            {
                new PhieuBaoTri
                {
                    MaPhieu = "BT001",
                    MaTaiSan = "TS001",
                    TenTaiSan = "Máy tính văn phòng",
                    LoaiBaoTri = "Định kỳ",
                    NgayBaoTri = DateTime.Now.AddDays(-10),
                    NgayHoanThanh = DateTime.Now.AddDays(-8),
                    NguoiPhuTrach = "Nguyễn Văn A",
                    TrangThai = "Hoàn thành",
                    ChiPhiDuKien = 500000,
                    NoiDungBaoTri = "Vệ sinh máy tính, cập nhật phần mềm"
                },
                new PhieuBaoTri
                {
                    MaPhieu = "BT002",
                    MaTaiSan = "TS002",
                    TenTaiSan = "Máy in laser",
                    LoaiBaoTri = "Đột xuất",
                    NgayBaoTri = DateTime.Now.AddDays(-5),
                    NgayHoanThanh = DateTime.Now.AddDays(5),
                    NguoiPhuTrach = "Trần Thị B",
                    TrangThai = "Đang thực hiện",
                    ChiPhiDuKien = 1200000,
                    NoiDungBaoTri = "Thay thế linh kiện hỏng, tái cân chỉnh"
                },
                new PhieuBaoTri
                {
                    MaPhieu = "BT003",
                    MaTaiSan = "TS005",
                    TenTaiSan = "Điều hòa phòng họp",
                    LoaiBaoTri = "Bảo hành",
                    NgayBaoTri = DateTime.Now.AddDays(2),
                    NgayHoanThanh = DateTime.Now.AddDays(4),
                    NguoiPhuTrach = "Lê Văn C",
                    TrangThai = "Chờ thực hiện",
                    ChiPhiDuKien = 0,
                    NoiDungBaoTri = "Kiểm tra lỗi không làm lạnh, thay gas điều hòa"
                }
            };

            PhieuBaoTriHienTai = new PhieuBaoTri();
        }

        private void DangKySuKien()
        {
            // Sự kiện cho nút thêm
            btnThem.Click += BtnThem_Click;
            btnInPhieu.Click += BtnInPhieu_Click;
            btnIn.Click += BtnXuatExcel_Click;

            // Sự kiện tìm kiếm
            txtTimKiem.TextChanged += TxtTimKiem_TextChanged;

            cboTrangThai.SelectionChanged += LocDuLieu;
            cboLoaiBaoTri.SelectionChanged += LocDuLieu;

            // Sự kiện DataGrid
            dgPhieuBaoTri.MouseDoubleClick += DgPhieuBaoTri_MouseDoubleClick;

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
            if (dgPhieuBaoTri.ItemsSource == null) return;

            ICollectionView view = CollectionViewSource.GetDefaultView(dgPhieuBaoTri.ItemsSource);

            view.Filter = item =>
            {
                var phieuBaoTri = item as PhieuBaoTri;
                if (phieuBaoTri == null) return false;

                // Lọc theo từ khóa
                bool matchKeyword = string.IsNullOrEmpty(txtTimKiem.Text) ||
                    (phieuBaoTri.MaPhieu.Contains(txtTimKiem.Text, StringComparison.OrdinalIgnoreCase) ||
                     phieuBaoTri.MaTaiSan.Contains(txtTimKiem.Text, StringComparison.OrdinalIgnoreCase) ||
                     phieuBaoTri.TenTaiSan.Contains(txtTimKiem.Text, StringComparison.OrdinalIgnoreCase) ||
                     phieuBaoTri.NguoiPhuTrach.Contains(txtTimKiem.Text, StringComparison.OrdinalIgnoreCase));

                // Lọc theo trạng thái
                bool matchTrangThai = cboTrangThai.SelectedIndex == 0 ||
                    phieuBaoTri.TrangThai == cboTrangThai.Text;

                // Lọc theo loại bảo trì
                bool matchLoaiBaoTri = cboLoaiBaoTri.SelectedIndex == 0 ||
                    phieuBaoTri.LoaiBaoTri == cboLoaiBaoTri.Text;

                return matchKeyword && matchTrangThai && matchLoaiBaoTri;
            };

            view.Refresh();
        }

        private void BtnThem_Click(object sender, RoutedEventArgs e)
        {
            // Mở form nhập liệu
            PhieuBaoTriInputForm inputForm = new PhieuBaoTriInputForm(null);
            inputForm.Owner = this;
            if (inputForm.ShowDialog() == true)
            {
                // Nếu người dùng lưu thành công, thêm phiếu mới vào danh sách
                PhieuBaoTri phieuMoi = inputForm.PhieuBaoTri;
                DanhSachPhieuBaoTri.Add(phieuMoi);

                // Refresh DataGrid
                dgPhieuBaoTri.Items.Refresh();

                // Thông báo thành công
                MessageBox.Show("Thêm phiếu bảo trì thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnInPhieu_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = dgPhieuBaoTri.SelectedItem as PhieuBaoTri;
            if (selectedItem != null)
            {
                MessageBox.Show($"Đang in phiếu bảo trì {selectedItem.MaPhieu}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                // TODO: Thêm mã in phiếu tại đây
            }
            else
            {
                MessageBox.Show("Vui lòng chọn phiếu bảo trì để in!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra danh sách có dữ liệu không
                if (DanhSachPhieuBaoTri == null || DanhSachPhieuBaoTri.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Mở dialog lưu file
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = $"DanhSachPhieuBaoTri_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Tạo workbook mới
                    var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("Danh Sách Phiếu Bảo Trì");

                    // Tạo header
                    worksheet.Cell(1, 1).Value = "Mã Phiếu";
                    worksheet.Cell(1, 2).Value = "Mã Tài Sản";
                    worksheet.Cell(1, 3).Value = "Tên Tài Sản";
                    worksheet.Cell(1, 4).Value = "Loại Bảo Trì";
                    worksheet.Cell(1, 5).Value = "Ngày Bảo Trì";
                    worksheet.Cell(1, 6).Value = "Ngày Hoàn Thành";
                    worksheet.Cell(1, 7).Value = "Người Phụ Trách";
                    worksheet.Cell(1, 8).Value = "Trạng Thái";
                    worksheet.Cell(1, 9).Value = "Chi Phí Dự Kiến";

                    // Định dạng header
                    var headerRange = worksheet.Range(1, 1, 1, 9);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

                    // Điền dữ liệu
                    for (int i = 0; i < DanhSachPhieuBaoTri.Count; i++)
                    {
                        var phieu = DanhSachPhieuBaoTri[i];
                        worksheet.Cell(i + 2, 1).Value = phieu.MaPhieu;
                        worksheet.Cell(i + 2, 2).Value = phieu.MaTaiSan;
                        worksheet.Cell(i + 2, 3).Value = phieu.TenTaiSan;
                        worksheet.Cell(i + 2, 4).Value = phieu.LoaiBaoTri;
                        worksheet.Cell(i + 2, 5).Value = phieu.NgayBaoTri.ToString("dd/MM/yyyy");
                        worksheet.Cell(i + 2, 6).Value = phieu.NgayHoanThanh?.ToString("dd/MM/yyyy") ?? "";
                        worksheet.Cell(i + 2, 7).Value = phieu.NguoiPhuTrach;
                        worksheet.Cell(i + 2, 8).Value = phieu.TrangThai;
                        worksheet.Cell(i + 2, 9).Value = phieu.ChiPhiDuKien;
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

        private void DgPhieuBaoTri_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = dgPhieuBaoTri.SelectedItem as PhieuBaoTri;
            if (selectedItem != null)
            {
                // Mở form nhập liệu với phiếu đã chọn để chỉnh sửa
                PhieuBaoTriInputForm inputForm = new PhieuBaoTriInputForm(selectedItem);
                inputForm.Owner = this;
                if (inputForm.ShowDialog() == true)
                {
                    // Nếu người dùng lưu thành công, cập nhật phiếu trong danh sách
                    int index = TimChiSoPhieuBaoTri(selectedItem.MaPhieu);
                    if (index >= 0)
                    {
                        DanhSachPhieuBaoTri[index] = inputForm.PhieuBaoTri;
                    }

                    // Refresh DataGrid
                    dgPhieuBaoTri.Items.Refresh();

                    // Thông báo thành công
                    MessageBox.Show("Cập nhật phiếu bảo trì thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private int TimChiSoPhieuBaoTri(string maPhieu)
        {
            for (int i = 0; i < DanhSachPhieuBaoTri.Count; i++)
            {
                if (DanhSachPhieuBaoTri[i].MaPhieu == maPhieu)
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