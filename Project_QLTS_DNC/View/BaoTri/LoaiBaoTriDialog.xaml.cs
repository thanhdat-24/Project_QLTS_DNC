using System;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Models.BaoTri;

namespace Project_QLTS_DNC.View.BaoTri
{
    /// <summary>
    /// Interaction logic for LoaiBaoTriDialog.xaml
    /// </summary>
    public partial class LoaiBaoTriDialog : Window
    {
        private bool isEditing = false;
        public LoaiBaoTri LoaiBaoTriResult { get; private set; }

        // Constructor cho trường hợp thêm mới
        public LoaiBaoTriDialog()
        {
            InitializeComponent();
            isEditing = false;
            txtTitle.Text = "THÊM LOẠI BẢO TRÌ MỚI";
            // Ẩn trường mã loại bảo trì vì sẽ được tự động tạo bởi Supabase
            txtMaLoaiBaoTri.IsEnabled = false;
            txtMaLoaiBaoTri.Text = "Tự động sinh";
            txtTenLoai.Focus();

            // Khởi tạo đối tượng mới
            LoaiBaoTriResult = new LoaiBaoTri();
        }

        // Constructor cho trường hợp chỉnh sửa
        public LoaiBaoTriDialog(LoaiBaoTri loaiBaoTriEdit)
        {
            InitializeComponent();
            isEditing = true;
            txtTitle.Text = "CHỈNH SỬA LOẠI BẢO TRÌ";

            // Hiển thị thông tin loại bảo trì cần chỉnh sửa
            txtMaLoaiBaoTri.Text = loaiBaoTriEdit.MaLoaiBaoTri.ToString();
            txtTenLoai.Text = loaiBaoTriEdit.TenLoai;
            txtMoTa.Text = loaiBaoTriEdit.MoTa;

            // Khóa không cho chỉnh sửa mã loại bảo trì
            txtMaLoaiBaoTri.IsEnabled = false;
            txtTenLoai.Focus();

            // Lưu lại đối tượng gốc để cập nhật
            LoaiBaoTriResult = new LoaiBaoTri
            {
                MaLoaiBaoTri = loaiBaoTriEdit.MaLoaiBaoTri,
                TenLoai = loaiBaoTriEdit.TenLoai,
                MoTa = loaiBaoTriEdit.MoTa
            };
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(txtTenLoai.Text))
            {
                MessageBox.Show("Vui lòng nhập tên loại bảo trì!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTenLoai.Focus();
                return;
            }

            // Cập nhật đối tượng loại bảo trì từ dữ liệu form
            LoaiBaoTriResult.TenLoai = txtTenLoai.Text.Trim();
            LoaiBaoTriResult.MoTa = txtMoTa.Text.Trim();

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