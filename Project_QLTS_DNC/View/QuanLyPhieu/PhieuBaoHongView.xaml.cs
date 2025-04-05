using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for PhieuBaoHongView.xaml
    /// </summary>
    public partial class PhieuBaoHongView : Window
    {
        // Tạo một bộ sưu tập các phiếu báo hỏng mẫu
        private ObservableCollection<PhieuBaoHong> _danhSachPhieuBaoHong;

        public PhieuBaoHongView()
        {
            InitializeComponent();

            // Khởi tạo dữ liệu mẫu
            KhoiTaoDuLieuMau();

            // Thiết lập các giá trị mặc định
            dtpNgayLap.SelectedDate = DateTime.Now;
            txtNguoiLap.Text = "Người dùng hiện tại";
            cboTrangThai.SelectedIndex = 0;

            // Đăng ký sự kiện
            btnThem.Click += BtnThem_Click;
            btnLuu.Click += BtnLuu_Click;
            btnHuy.Click += BtnHuy_Click;
        }

        private void KhoiTaoDuLieuMau()
        {
            // Tạo dữ liệu mẫu để hiển thị trong DataGrid
            _danhSachPhieuBaoHong = new ObservableCollection<PhieuBaoHong>
            {
                new PhieuBaoHong { MaPhieu = "PBH001", MaTaiSan = "TS001", TenTaiSan = "Máy tính xách tay Dell XPS 13",
                    NgayLap = DateTime.Now.AddDays(-5), NguoiLap = "Nguyễn Văn A", MucDoHong = "Nhẹ", TrangThai = "Đã xử lý" },
                new PhieuBaoHong { MaPhieu = "PBH002", MaTaiSan = "TS007", TenTaiSan = "Máy in HP LaserJet Pro",
                    NgayLap = DateTime.Now.AddDays(-3), NguoiLap = "Trần Thị B", MucDoHong = "Trung bình", TrangThai = "Đã duyệt" },
                new PhieuBaoHong { MaPhieu = "PBH003", MaTaiSan = "TS015", TenTaiSan = "Máy chiếu Epson EB-X41",
                    NgayLap = DateTime.Now.AddDays(-1), NguoiLap = "Lê Văn C", MucDoHong = "Nặng", TrangThai = "Mới tạo" }
            };

            // Thiết lập DataContext cho DataGrid
            dgPhieuBaoHong.ItemsSource = _danhSachPhieuBaoHong;
        }

        private void BtnThem_Click(object sender, RoutedEventArgs e)
        {
            // Xóa trắng các trường thông tin để tạo mới
            txtMaPhieu.Text = "PBH" + ((_danhSachPhieuBaoHong.Count + 1).ToString("D3"));
            txtMaTaiSan.Text = string.Empty;
            txtTenTaiSan.Text = string.Empty;
            txtBoPhanQuanLy.Text = string.Empty;
            cboMucDoHong.SelectedIndex = -1;
            dtpNgayLap.SelectedDate = DateTime.Now;
            txtMoTa.Text = string.Empty;

            // Focus vào trường đầu tiền
            txtMaTaiSan.Focus();

            MessageBox.Show("Vui lòng nhập thông tin phiếu báo hỏng mới", "Thêm mới", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra dữ liệu nhập vào
            if (string.IsNullOrWhiteSpace(txtMaPhieu.Text) ||
                string.IsNullOrWhiteSpace(txtMaTaiSan.Text) ||
                dtpNgayLap.SelectedDate == null ||
                string.IsNullOrWhiteSpace(txtNguoiLap.Text) ||
                cboMucDoHong.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin bắt buộc!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Thêm phiếu báo hỏng mới vào danh sách
            PhieuBaoHong phieuMoi = new PhieuBaoHong
            {
                MaPhieu = txtMaPhieu.Text,
                MaTaiSan = txtMaTaiSan.Text,
                TenTaiSan = txtTenTaiSan.Text,
                NgayLap = dtpNgayLap.SelectedDate.Value,
                NguoiLap = txtNguoiLap.Text,
                MucDoHong = ((ComboBoxItem)cboMucDoHong.SelectedItem).Content.ToString(),
                TrangThai = ((ComboBoxItem)cboTrangThai.SelectedItem).Content.ToString()
            };

            // Thêm vào danh sách
            _danhSachPhieuBaoHong.Add(phieuMoi);

            MessageBox.Show("Lưu thông tin phiếu báo hỏng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

            // Làm mới form để nhập phiếu tiếp theo
            BtnThem_Click(sender, e);
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            // Hỏi người dùng trước khi xóa dữ liệu đã nhập
            MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn hủy các thay đổi?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Làm mới form để nhập phiếu mới
                BtnThem_Click(sender, e);
            }
        }
    }

    // Lớp mô hình cho phiếu báo hỏng
    public class PhieuBaoHong
    {
        public string MaPhieu { get; set; }
        public string MaTaiSan { get; set; }
        public string TenTaiSan { get; set; }
        public DateTime NgayLap { get; set; }
        public string NguoiLap { get; set; }
        public string MucDoHong { get; set; }
        public string TrangThai { get; set; }
    }
}