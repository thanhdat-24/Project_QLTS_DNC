using Project_QLTS_DNC.Services.TaiKhoan;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Project_QLTS_DNC.View.TaiKhoan
{
    /// <summary>
    /// Interaction logic for ThemTaiKhoanForm.xaml
    /// </summary>
    public partial class ThemTaiKhoanForm : Window
    {
        private LoaiTaiKhoanService _loaiTaiKhoanService;
        private NhanVienService _nhanVienService;
        private TaiKhoanService _taiKhoanService;

        public ThemTaiKhoanForm()
        {
            InitializeComponent();
            _loaiTaiKhoanService = new LoaiTaiKhoanService();
            _nhanVienService = new NhanVienService();
            _taiKhoanService = new TaiKhoanService();
        }

        // Hàm được gọi khi cửa sổ được load
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadComboBoxLoaiTaiKhoanAsync();
                await LoadComboBoxNhanVienAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadComboBoxLoaiTaiKhoanAsync()
        {
            var dsLoaiTK = await _loaiTaiKhoanService.LayDSLoaiTK();
            if (dsLoaiTK != null && dsLoaiTK.Any())
            {
                cboLoaiTaiKhoan.ItemsSource = dsLoaiTK;
                cboLoaiTaiKhoan.DisplayMemberPath = "TenLoaiTk";
                cboLoaiTaiKhoan.SelectedValuePath = "MaLoaiTk";
                cboLoaiTaiKhoan.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("Không có loại tài khoản để hiển thị.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadComboBoxNhanVienAsync()
        {
            var dsNhanVien = await _nhanVienService.LayTatCaNhanVienDtoAsync();
            if (dsNhanVien != null && dsNhanVien.Any())
            {
                cboNhanVien.ItemsSource = dsNhanVien;

                // Kiểm tra kiểu đối tượng đầu tiên để xác định đúng tên thuộc tính
                var firstItem = dsNhanVien.FirstOrDefault();
                if (firstItem != null)
                {
                    // Log kiểu đối tượng (có thể xóa sau khi fix xong)
                    Console.WriteLine($"NhanVien Type: {firstItem.GetType().Name}");

                    // Thiết lập path dựa trên kiểu đối tượng thực tế
                    Type itemType = firstItem.GetType();
                    if (itemType.GetProperty("MaNV") != null)
                    {
                        cboNhanVien.SelectedValuePath = "MaNV";
                    }
                    else if (itemType.GetProperty("MaNv") != null)
                    {
                        cboNhanVien.SelectedValuePath = "MaNv";
                    }

                    if (itemType.GetProperty("TenNV") != null)
                    {
                        cboNhanVien.DisplayMemberPath = "TenNV";
                    }
                    else if (itemType.GetProperty("TenNv") != null)
                    {
                        cboNhanVien.DisplayMemberPath = "TenNv";
                    }
                }

                cboNhanVien.SelectedIndex = 0;
            }
        }

        // Hàm lưu tài khoản
        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tenTaiKhoan = txtTenTaiKhoan.Text.Trim();
                var matKhau = txtMatKhau.Password.Trim();
                var maLoaiTk = (cboLoaiTaiKhoan.SelectedItem as LoaiTaiKhoanModel)?.MaLoaiTk ?? 0;

                // Lấy MaNv hoặc MaNV từ SelectedValue trực tiếp
                int? maNv = null;

                if (cboNhanVien.SelectedValue != null)
                {
                    maNv = Convert.ToInt32(cboNhanVien.SelectedValue);
                }
                // Nếu không lấy được từ SelectedValue, thử lấy thông qua reflection
                else if (cboNhanVien.SelectedItem != null)
                {
                    var selectedItem = cboNhanVien.SelectedItem;
                    Type itemType = selectedItem.GetType();

                    var propMaNV = itemType.GetProperty("MaNV");
                    var propMaNv = itemType.GetProperty("MaNv");

                    if (propMaNV != null)
                    {
                        maNv = (int?)propMaNV.GetValue(selectedItem);
                    }
                    else if (propMaNv != null)
                    {
                        maNv = (int?)propMaNv.GetValue(selectedItem);
                    }
                }

                if (string.IsNullOrEmpty(tenTaiKhoan) || string.IsNullOrEmpty(matKhau))
                {
                    MessageBox.Show("Tên tài khoản và mật khẩu không thể để trống.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Kiểm tra nếu nhân viên vẫn là null
                if (maNv == null)
                {
                    MessageBox.Show("Vui lòng chọn nhân viên cho tài khoản này.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_isEditMode)
                {
                    // Cập nhật tài khoản hiện có
                    var result = await _taiKhoanService.CapNhatTaiKhoanAsync(_editingTaiKhoanId, matKhau, maLoaiTk, maNv);

                    if (result)
                    {
                        MessageBox.Show("Cập nhật tài khoản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Cập nhật tài khoản thất bại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // Tạo tài khoản mới - code hiện tại
                    var newTaiKhoan = new TaiKhoanModel
                    {
                        TenTaiKhoan = tenTaiKhoan,
                        MatKhau = matKhau,
                        MaLoaiTk = maLoaiTk,
                        MaNv = maNv
                    };

                    var result = await _taiKhoanService.ThemTaiKhoanAsync(newTaiKhoan.TenTaiKhoan, newTaiKhoan.MatKhau, newTaiKhoan.MaLoaiTk, newTaiKhoan.MaNv);

                    if (result != null)
                    {
                        MessageBox.Show("Tạo tài khoản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Tạo tài khoản thất bại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xử lý tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Thêm vào lớp ThemTaiKhoanForm
        public async void LoadTaiKhoanData(TaiKhoanDTO taiKhoan)
        {
            try
            {
                // Lưu thông tin tài khoản đang chỉnh sửa để sử dụng sau này
                _editingTaiKhoanId = taiKhoan.MaTk;

                // Thiết lập chế độ chỉnh sửa
                _isEditMode = true;

                // Load dữ liệu vào các controls
                txtTenTaiKhoan.Text = taiKhoan.TenTaiKhoan;
                txtMatKhau.Password = taiKhoan.MatKhau;

                // Load ComboBox dữ liệu nếu chưa được load
                if (cboLoaiTaiKhoan.ItemsSource == null)
                    await LoadComboBoxLoaiTaiKhoanAsync();

                if (cboNhanVien.ItemsSource == null)
                    await LoadComboBoxNhanVienAsync();

                // Chọn loại tài khoản hiện tại
                foreach (var item in cboLoaiTaiKhoan.Items)
                {
                    var loaiTK = item as LoaiTaiKhoanModel;
                    if (loaiTK != null && loaiTK.MaLoaiTk == taiKhoan.MaLoaiTk)
                    {
                        cboLoaiTaiKhoan.SelectedItem = item;
                        break;
                    }
                }

                // Chọn nhân viên hiện tại
                if (taiKhoan.MaNv.HasValue)
                {
                    foreach (var item in cboNhanVien.Items)
                    {
                        // Kiểm tra đúng kiểu dữ liệu của item
                        Type itemType = item.GetType();
                        var propMaNV = itemType.GetProperty("MaNV");
                        var propMaNv = itemType.GetProperty("MaNv");

                        int? maNv = null;
                        if (propMaNV != null)
                            maNv = (int?)propMaNV.GetValue(item);
                        else if (propMaNv != null)
                            maNv = (int?)propMaNv.GetValue(item);

                        if (maNv.HasValue && maNv.Value == taiKhoan.MaNv.Value)
                        {
                            cboNhanVien.SelectedItem = item;
                            break;
                        }
                    }
                }

                // Đổi nút "Lưu" thành "Cập nhật"
                this.Title = "Chỉnh sửa tài khoản";
                txtTieude.Text = "Chỉnh sửa tài khoản";
                btnLuu.Content = "Cập nhật";

                // Sửa sự kiện btnLuu_Click để hỗ trợ cả thêm mới và cập nhật
                // (điều này sẽ được thực hiện trong phần tiếp theo)
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Thêm các biến thành viên vào đầu lớp ThemTaiKhoanForm
        private int _editingTaiKhoanId;
        private bool _isEditMode = false;

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}