using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project_QLTS_DNC.Models.NhaCungCap;

namespace Project_QLTS_DNC.View.NhaCungCap
{
    public partial class ThemNhaCungCapForm : Window
    {
        private ObservableCollection<NhaCungCapClass> danhSach;
        private NhaCungCapClass nccCu;

        public NhaCungCapClass NhaCungCapMoi { get; set; }

        public ThemNhaCungCapForm(ObservableCollection<NhaCungCapClass> danhSach)
        {
            InitializeComponent();
            this.danhSach = danhSach;
        }

        public ThemNhaCungCapForm(ObservableCollection<NhaCungCapClass> danhSach, NhaCungCapClass ncc)
        {
            InitializeComponent();
            this.danhSach = danhSach;
            this.nccCu = ncc;

            if (ncc != null)
            {
                txtTenNhaCungCap.Text = ncc.TenNCC;
                txtDiaChi.Text = ncc.DiaChi;
                txtSoDienThoai.Text = ncc.SoDienThoai;
                txtEmail.Text = ncc.Email;
                txtMoTa.Text = ncc.MoTa;
            }
        }

        private int SinhMaNCC()
        {
            // Lấy mã lớn nhất trong danh sách, +1
            return danhSach.Select(ncc => ncc.MaNCC).DefaultIfEmpty(0).Max() + 1;
        }

        private void btnCapNhat_Click(object sender, RoutedEventArgs e)
        {
            ResetError();

            string ten = txtTenNhaCungCap.Text.Trim();
            string diaChi = txtDiaChi.Text.Trim();
            string sdt = txtSoDienThoai.Text.Trim();
            string email = txtEmail.Text.Trim();
            string moTa = txtMoTa.Text.Trim();

            bool isValid = true;

            if (string.IsNullOrEmpty(ten)) { ShowError(txtTenNhaCungCap, "Tên không được trống."); isValid = false; }
            if (string.IsNullOrEmpty(diaChi)) { ShowError(txtDiaChi, "Địa chỉ không được trống."); isValid = false; }
            if (!Regex.IsMatch(sdt, @"^\d{1,11}$")) { ShowError(txtSoDienThoai, "SĐT chỉ chứa số và tối đa 11 chữ số."); isValid = false; }
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) { ShowError(txtEmail, "Email không hợp lệ."); isValid = false; }

            if (!isValid) return;

            NhaCungCapMoi = new NhaCungCapClass
            {
                TenNCC = ten,
                DiaChi = diaChi,
                SoDienThoai = sdt,
                Email = email,
                MoTa = moTa,
                MaNCC = nccCu != null ? nccCu.MaNCC : SinhMaNCC()
            };

            DialogResult = true;
            Close();
        }

        private void ShowError(TextBox tb, string msg)
        {
            tb.ToolTip = msg;
            tb.Background = System.Windows.Media.Brushes.LightPink;
        }

        private void ResetError()
        {
            foreach (var tb in new[] { txtTenNhaCungCap, txtDiaChi, txtSoDienThoai, txtEmail })
            {
                tb.ToolTip = null;
                tb.Background = System.Windows.Media.Brushes.White;
            }
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void txtSoDienThoai_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d$");
        }
    }
}