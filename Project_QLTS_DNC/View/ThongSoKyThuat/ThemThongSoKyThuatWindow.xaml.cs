using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Services.QLTaiSanService;

namespace Project_QLTS_DNC.View.ThongSoKyThuat
{
    public partial class ThemThongSoKyThuatWindow : Window
    {
        // Thông tin nhóm tài sản
        public NhomTaiSan NhomTaiSan { get; private set; }

        // Danh sách thông số kỹ thuật mới được tạo
        public ObservableCollection<ThongSoKyThuatDTO> DanhSachThongSoMoi { get; private set; }

        // Class cho preview thông số
        public class ThongSoPreview
        {
            public int Index { get; set; }
            public string TenThongSo { get; set; }
        }

        // Danh sách để hiển thị preview
        private ObservableCollection<ThongSoPreview> _previewList;

        // Thông tin thông số mới được tạo (giữ lại để tương thích với code cũ)
        public ThongSoKyThuatDTO ThongSoMoi { get; private set; }

        public ThemThongSoKyThuatWindow(NhomTaiSan nhomTaiSan)
        {
            InitializeComponent();

            // Lưu thông tin nhóm tài sản
            NhomTaiSan = nhomTaiSan;

            // Hiển thị tên nhóm tài sản
            txtTenNhom.Text = nhomTaiSan.TenNhom;

            // Khởi tạo danh sách thông số mới
            DanhSachThongSoMoi = new ObservableCollection<ThongSoKyThuatDTO>();
            _previewList = new ObservableCollection<ThongSoPreview>();

            // Gán danh sách preview vào ListView
            lvThongSoPreview.ItemsSource = _previewList;

            // Thêm sự kiện TextChanged để cập nhật danh sách preview khi nhập liệu
            txtNhieuThongSo.TextChanged += TxtNhieuThongSo_TextChanged;
        }

        /// <summary>
        /// Cập nhật danh sách preview khi text thay đổi
        /// </summary>
        private void TxtNhieuThongSo_TextChanged(object sender, TextChangedEventArgs e)
        {
            CapNhatPreview();
        }

        /// <summary>
        /// Cập nhật danh sách preview từ text trong TextBox
        /// </summary>
        private void CapNhatPreview()
        {
            _previewList.Clear();
            
            if (string.IsNullOrWhiteSpace(txtNhieuThongSo.Text))
                return;

            string[] danhSachTen = txtNhieuThongSo.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            int index = 1;
            foreach (string ten in danhSachTen)
            {
                string tenTrim = ten.Trim();
                if (!string.IsNullOrWhiteSpace(tenTrim))
                {
                    _previewList.Add(new ThongSoPreview
                    {
                        Index = index++,
                        TenThongSo = tenTrim
                    });
                }
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Hủy
        /// </summary>
        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            // Đóng cửa sổ và trả về kết quả false
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Lưu
        /// </summary>
        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra dữ liệu nhập
            if (!KiemTraDuLieu())
                return;

            try
            {
                // Hiển thị thông báo đang xử lý
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.Cursor = System.Windows.Input.Cursors.Wait;
                }

                // Lấy danh sách các thông số từ TextBox
                string[] danhSachTen = txtNhieuThongSo.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                List<string> danhSachTenHopLe = danhSachTen
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();

                // Tạo danh sách DTO
                List<ThongSoKyThuatDTO> dsThongSo = danhSachTenHopLe.Select(ten => new ThongSoKyThuatDTO
                {
                    MaNhomTS = NhomTaiSan.MaNhomTS,
                    TenThongSo = ten
                }).ToList();

                // Thêm từng thông số vào cơ sở dữ liệu
                DanhSachThongSoMoi.Clear();
                
                foreach (var thongSoDTO in dsThongSo)
                {
                    var thongSoDaThem = await ThongSoKyThuatService.ThemThongSoAsync(thongSoDTO);
                    DanhSachThongSoMoi.Add(thongSoDaThem);
                    
                    // Gán thông số đầu tiên cho ThongSoMoi để tương thích với code cũ
                    if (ThongSoMoi == null)
                    {
                        ThongSoMoi = thongSoDaThem;
                    }
                }

                // Khôi phục con trỏ
                if (window != null)
                {
                    window.Cursor = null;
                }

                // Đóng cửa sổ và trả về kết quả true
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                // Khôi phục con trỏ
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.Cursor = null;
                }

                MessageBox.Show($"Lỗi khi thêm thông số kỹ thuật: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Kiểm tra dữ liệu nhập
        /// </summary>
        /// <returns>True nếu dữ liệu hợp lệ, False nếu không hợp lệ</returns>
        private bool KiemTraDuLieu()
        {
            bool isValid = true;

            // Kiểm tra danh sách thông số
            if (string.IsNullOrWhiteSpace(txtNhieuThongSo.Text) || _previewList.Count == 0)
            {
                txtErrorThongSo.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                txtErrorThongSo.Visibility = Visibility.Collapsed;
            }

            return isValid;
        }
    }
}