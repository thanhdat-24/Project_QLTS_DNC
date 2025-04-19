using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.Services.TaiKhoan;
using Project_QLTS_DNC.ViewModels.TaiKhoan;
using System;
using System.Windows;

namespace Project_QLTS_DNC.View.TaiKhoan
{
    public partial class ThemLoaiTaiKhoanForm : Window
    {
        private LoaiTaiKhoanService _loaiTaiKhoanService;
        private LoaiTaiKhoanModel _updateLoaiTk;
        private readonly LoaiTaiKhoanForm _loaiTaiKhoanForm;

        public ThemLoaiTaiKhoanForm(LoaiTaiKhoanForm loaiTaiKhoanForm = null)
        {
            InitializeComponent();
            _loaiTaiKhoanService = new LoaiTaiKhoanService();
            _loaiTaiKhoanForm = loaiTaiKhoanForm; // Lưu reference đến form cha
        }

        public ThemLoaiTaiKhoanForm(LoaiTaiKhoanModel loaiTkUpdate, LoaiTaiKhoanForm loaiTaiKhoanForm = null)
        {
            InitializeComponent();
            _updateLoaiTk = loaiTkUpdate;
            _loaiTaiKhoanService = new LoaiTaiKhoanService();
            _loaiTaiKhoanForm = loaiTaiKhoanForm; // Lưu reference đến form cha
            LoadChucVuData();
        }

        public async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            string tenLoaiTk = txtTenLoaiTaiKhoan.Text.Trim();
            string moTa = txtMoTa.Text.Trim();

            if (string.IsNullOrEmpty(tenLoaiTk))
            {
                MessageBox.Show("Vui lòng nhập tên loại tài khoản!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool success;

                if (_updateLoaiTk != null)
                {
                    // Cập nhật loại tài khoản hiện có
                    _updateLoaiTk.TenLoaiTk = tenLoaiTk;
                    _updateLoaiTk.MoTa = moTa;

                    var result = await _loaiTaiKhoanService.CapNhatLoaiTk(_updateLoaiTk);
                    success = result != null;
                }
                else
                {
                    // Thêm mới loại tài khoản
                    var loaiTk = new LoaiTaiKhoanModel
                    {
                        TenLoaiTk = tenLoaiTk,
                        MoTa = moTa
                    };

                    var result = await _loaiTaiKhoanService.ThemLoaiTaiKhoan(loaiTk);
                    success = result != null;
                }

                if (success)
                {
                    // Gọi phương thức LoadDanhSachLoaiTaiKhoan() của form cha
                    if (_loaiTaiKhoanForm != null)
                    {
                        await _loaiTaiKhoanForm.LoadDanhSachLoaiTaiKhoan();
                    }

                    MessageBox.Show("Lưu thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Không thể lưu loại tài khoản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu loại tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadChucVuData()
        {
            if (_updateLoaiTk != null)
            {
                txtTenLoaiTaiKhoan.Text = _updateLoaiTk.TenLoaiTk;
                txtMoTa.Text = _updateLoaiTk.MoTa;
                this.Title = "Cập nhật loại tài khoản";
                this.txtTieude.Text = "Cập nhật loại tài khoản";
                this.btnLuu.Content = "Cập nhật";
            }
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}