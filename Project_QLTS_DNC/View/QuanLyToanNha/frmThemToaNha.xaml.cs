using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Project_QLTS_DNC.View.QuanLyToanNha
{
    /// <summary>
    /// Interaction logic for frmThemToaNha.xaml
    /// </summary>
    public partial class frmThemToaNha : Window
    {
        // Đây là đối tượng Toà Nhà mới, sẽ được gán khi nhấn Lưu
        public ToaNha ToaNhaMoi { get; set; }

        public frmThemToaNha()
        {
            InitializeComponent();
        }
        public frmThemToaNha(ToaNha toaNhaCanSua) : this() // gọi constructor mặc định
        {
            ToaNhaMoi = new ToaNha
            {
                MaToaNha = toaNhaCanSua.MaToaNha,
                TenToaNha = toaNhaCanSua.TenToaNha,
                DiaChiTN = toaNhaCanSua.DiaChiTN,
                SoDienThoaiTN = toaNhaCanSua.SoDienThoaiTN,
                MoTaTN = toaNhaCanSua.MoTaTN
            };

            // Gán dữ liệu lên TextBox
            txtTenToaNha.Text = ToaNhaMoi.TenToaNha;
            txtDiaChiTN.Text = ToaNhaMoi.DiaChiTN;
            txtSoDienThoaiTN.Text = ToaNhaMoi.SoDienThoaiTN;
            txtMoTaTN.Text = ToaNhaMoi.MoTaTN;
        }


        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra dữ liệu
            if (string.IsNullOrWhiteSpace(txtTenToaNha.Text))
            {
                MessageBox.Show("Vui lòng nhập tên toà nhà!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDiaChiTN.Text))
            {
                MessageBox.Show("Vui lòng nhập địa chỉ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSoDienThoaiTN.Text))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {

                // Tạo object mới
                ToaNhaMoi = new ToaNha
                {
                    MaToaNha = null,
                    TenToaNha = txtTenToaNha.Text.Trim(),
                    DiaChiTN = txtDiaChiTN.Text.Trim(),
                    SoDienThoaiTN = txtSoDienThoaiTN.Text.Trim(),
                    MoTaTN = txtMoTaTN.Text.Trim()
                };



                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<ToaNha>().Insert(ToaNhaMoi);


                if (response.Models.Count > 0)
                {
                    // Gán lại dữ liệu (kèm MaToaNha nếu được Supabase tạo tự động)
                    ToaNhaMoi = response.Models[0];

                    MessageBox.Show("Thêm tòa nhà thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Thêm tòa nhà thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu tòa nhà lên Supabase: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnDong_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}