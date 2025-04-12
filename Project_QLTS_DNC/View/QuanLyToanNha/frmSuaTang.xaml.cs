using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services.QLToanNha;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Project_QLTS_DNC.View.QuanLyToanNha
{
    public partial class frmSuaTang : Window
    {
        public Tang TangDaSua { get; set; }
        private readonly Tang _tangCanSua;
        private List<ToaNha> DanhSachToaNha = new();

        public frmSuaTang(Tang tang)
        {
            InitializeComponent();
            _tangCanSua = tang;
            Loaded += frmSuaTang_Loaded;
        }

        private async void frmSuaTang_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DanhSachToaNha = (await ToaNhaService.LayDanhSachToaNhaAsync()).ToList();
                cboTenToa.ItemsSource = DanhSachToaNha;
                cboTenToa.DisplayMemberPath = "TenToaNha";
                cboTenToa.SelectedValuePath = "MaToaNha";

                // Gán giá trị ban đầu
                txtTenTang.Text = _tangCanSua.TenTang;
                txtMota.Text = _tangCanSua.MoTa;
                cboTenToa.SelectedValue = _tangCanSua.MaToa; // gán mã tòa đang có
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải tòa nhà: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnCapNhat_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenTang.Text))
            {
                MessageBox.Show("Vui lòng nhập tên tầng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cboTenToa.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn tòa nhà!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TangDaSua = new Tang
            {
                MaTang = _tangCanSua.MaTang,
                MaToa = (int)cboTenToa.SelectedValue,
                TenTang = txtTenTang.Text.Trim(),
                MoTa = txtMota.Text.Trim()
            };

            DialogResult = true;
            Close();
        }
    }
}
