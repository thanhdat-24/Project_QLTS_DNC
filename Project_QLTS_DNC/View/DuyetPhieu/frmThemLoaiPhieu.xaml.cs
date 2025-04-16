using Project_QLTS_DNC.Models.DuyetPhieu;
using Project_QLTS_DNC.Services;
using System;
using System.Windows;

namespace Project_QLTS_DNC.View.DuyetPhieu
{
    public partial class frmThemLoaiPhieu : Window
    {
        public LoaiPhieu LoaiPhieuMoi { get; set; }

        public frmThemLoaiPhieu()
        {
            InitializeComponent();
        }

        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenLoaiPhieu.Text))
            {
                MessageBox.Show("Vui lòng nhập tên loại phiếu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var loaiPhieu = new LoaiPhieu
                {
                    TenLoaiPhieu = txtTenLoaiPhieu.Text.Trim(),
                    MoTaLP = txtMoTa.Text.Trim()
                };

                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<LoaiPhieu>().Insert(loaiPhieu);

                if (response.Models.Count > 0)
                {
                    LoaiPhieuMoi = response.Models[0];
                    MessageBox.Show("Thêm loại phiếu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Không thể thêm loại phiếu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu loại phiếu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnDong_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
