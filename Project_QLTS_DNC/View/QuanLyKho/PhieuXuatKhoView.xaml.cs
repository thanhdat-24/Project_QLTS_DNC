using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Microsoft.Win32;
using Project_QLTS_DNC.Models;
using System.IO; // Add this namespace for IOException

namespace Project_QLTS_DNC.View.QuanLyKho
{

    public partial class PhieuXuatKhoView : UserControl
    {
        public ObservableCollection<PhieuXuatKho> PhieuXuatKhoList { get; set; }

        public PhieuXuatKhoView()
        {
            InitializeComponent();

            // Sample data
            PhieuXuatKhoList = new ObservableCollection<PhieuXuatKho>
            {
                new PhieuXuatKho { MaKhoXuat = "KX001", MaSanPham = "SP001", MaPhieuXuat = "PX001", MaKhoNhan = "KN001", NgayXuat = DateTime.Now, SoLuong = 10, GhiChu = "Ghi chú 1" },
                new PhieuXuatKho { MaKhoXuat = "KX002", MaSanPham = "SP002", MaPhieuXuat = "PX002", MaKhoNhan = "KN002", NgayXuat = DateTime.Now.AddDays(-1), SoLuong = 20, GhiChu = "Ghi chú 2" },
                new PhieuXuatKho { MaKhoXuat = "KX003", MaSanPham = "SP003", MaPhieuXuat = "PX003", MaKhoNhan = "KN003", NgayXuat = DateTime.Now.AddDays(-2), SoLuong = 15, GhiChu = "Ghi chú 3" }
            };

            // Bind the data to the DataGrid
            dgPhieuXuatKho.ItemsSource = PhieuXuatKhoList;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle edit button click
            if (dgPhieuXuatKho.SelectedItem is PhieuXuatKho selectedItem)
            {
                // Open edit dialog or perform edit operation
                MessageBox.Show($"Editing item: {selectedItem.MaPhieuXuat}");
            }
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle delete button click
            if (dgPhieuXuatKho.SelectedItem is PhieuXuatKho selectedItem)
            {
                // Remove the selected item from the collection
                PhieuXuatKhoList.Remove(selectedItem);
            }
        }
        private void XuatExcelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra danh sách có dữ liệu không
                if (PhieuXuatKhoList == null || PhieuXuatKhoList.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Mở dialog lưu file
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = $"DanhSachPhieuXuatKho_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Sử dụng using để đảm bảo giải phóng tài nguyên
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Danh Sách Phiếu Xuất Kho");

                        // Tạo header
                        var headers = new[]
                        {
                    "Mã Kho Xuất", "Mã Sản Phẩm", "Mã Phiếu Xuất", "Mã Kho Nhận",
                    "Ngày Xuất", "Số Lượng", "Ghi Chú"
                };

                        for (int col = 0; col < headers.Length; col++)
                        {
                            worksheet.Cell(1, col + 1).Value = headers[col];
                        }

                        // Định dạng header
                        var headerRange = worksheet.Range(1, 1, 1, headers.Length);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Điền dữ liệu
                        for (int i = 0; i < PhieuXuatKhoList.Count; i++)
                        {
                            var phieu = PhieuXuatKhoList[i];
                            worksheet.Cell(i + 2, 1).Value = phieu.MaKhoXuat;
                            worksheet.Cell(i + 2, 2).Value = phieu.MaSanPham;
                            worksheet.Cell(i + 2, 3).Value = phieu.MaPhieuXuat;
                            worksheet.Cell(i + 2, 4).Value = phieu.MaKhoNhan;
                            worksheet.Cell(i + 2, 5).Value = phieu.NgayXuat.ToString("dd/MM/yyyy");
                            worksheet.Cell(i + 2, 6).Value = phieu.SoLuong;
                            worksheet.Cell(i + 2, 7).Value = phieu.GhiChu;
                        }

                        // Tự động điều chỉnh độ rộng cột
                        worksheet.Columns().AdjustToContents();

                        // Lưu file
                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show("Xuất Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (IOException ioEx)
            {
                MessageBox.Show($"Lỗi khi lưu file: {ioEx.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public class PhieuXuatKho
        {
            public string MaKhoXuat { get; set; }
            public string MaSanPham { get; set; }
            public string MaPhieuXuat { get; set; }
            public string MaKhoNhan { get; set; }
            public DateTime NgayXuat { get; set; }
            public int SoLuong { get; set; }
            public string GhiChu { get; set; }
        }

    }
    }
