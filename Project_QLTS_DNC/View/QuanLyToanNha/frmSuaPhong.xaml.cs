using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services.QLToanNha;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Project_QLTS_DNC.View.QuanLyToanNha
{
    public partial class frmSuaPhong : Window
    {
        public Phong PhongDaSua { get; private set; }
        private readonly Phong _phongHienTai;
        private List<Tang> DanhSachTang = new();

        public frmSuaPhong(Phong phongHienTai)
        {
            InitializeComponent();
            _phongHienTai = phongHienTai;
            Loaded += frm_Loaded;
        }

        private async void frm_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DanhSachTang = (await TangService.LayDanhSachTangAsync()).ToList();
                cboTenTang.ItemsSource = DanhSachTang;
                cboTenTang.DisplayMemberPath = "TenTang";
                cboTenTang.SelectedValuePath = "MaTang";

                // 🟦 Gán lại giá trị đang sửa
                cboTenTang.SelectedValue = _phongHienTai.MaTang;
                txtTenP.Text = _phongHienTai.TenPhong;
                txtSucChuaP.Text = _phongHienTai.SucChua.ToString();
                txtMoTaP.Text = _phongHienTai.MoTaPhong;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải tầng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCapNhat_Click(object sender, RoutedEventArgs e)
        {
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

            if (cboTenTang.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn tầng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PhongDaSua = new Phong
            {
                MaPhong = _phongHienTai.MaPhong,
                MaTang = (int)cboTenTang.SelectedValue,
                TenPhong = txtTenP.Text.Trim(),
                SucChua = sucChua,
                MoTaPhong = txtMoTaP.Text.Trim()
            };

            MessageBox.Show("Cập nhật phòng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

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
