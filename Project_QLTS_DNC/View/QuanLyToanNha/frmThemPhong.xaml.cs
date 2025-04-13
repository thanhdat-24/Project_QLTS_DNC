using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services.QLToanNha;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Project_QLTS_DNC.View.QuanLyToanNha
{
    public partial class frmThemPhong : Window
    {
        public Phong PhongMoi { get; private set; }
        private List<Tang> DanhSachTang = new();

        private readonly Phong _phongHienTai; // dùng để nhận biết đang sửa

        public frmThemPhong()
        {
            InitializeComponent();
            Title = "Thêm phòng mới";
            Loaded += async (s, e) => await LoadTang();
          
        }

        public frmThemPhong(Phong phongHienTai)
        {
            InitializeComponent();
            Title = "Chỉnh sửa phòng";
            _phongHienTai = phongHienTai;

            Loaded += async (s, e) =>
            {
                await LoadTang();

                // Gán dữ liệu phòng hiện tại lên form
                cboTenTang.SelectedValue = phongHienTai.MaTang;
                txtTenP.Text = phongHienTai.TenPhong;
                txtSucChuaP.Text = phongHienTai.SucChua.ToString();
                txtMoTaP.Text = phongHienTai.MoTaPhong;
            };
        }

        private async Task LoadTang()
        {
            try
            {
                DanhSachTang = (await TangService.LayDanhSachTangAsync()).ToList();
                cboTenTang.ItemsSource = DanhSachTang;
                cboTenTang.DisplayMemberPath = "TenTang";
                cboTenTang.SelectedValuePath = "MaTang";
                // ✅ Chỉ định rõ không chọn gì cả
                cboTenTang.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách tầng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (cboTenTang.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn tầng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTenP.Text))
            {
                MessageBox.Show("Vui lòng nhập tên phòng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtSucChuaP.Text, out int sucChua) || sucChua <= 0)
            {
                MessageBox.Show("Sức chứa phải là số nguyên dương!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PhongMoi = new Phong
            {
                MaPhong = _phongHienTai?.MaPhong, // nếu đang sửa
                MaTang = (int)cboTenTang.SelectedValue,
                TenPhong = txtTenP.Text.Trim(),
                SucChua = sucChua,
                MoTaPhong = txtMoTaP.Text.Trim()
            };

            // ✅ THÊM THÔNG BÁO
            if (_phongHienTai == null)
            {
                MessageBox.Show("Thêm phòng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Cập nhật phòng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            DialogResult = true;
            Close();
        }


        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
