using LiveCharts;
using LiveCharts.Wpf;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Supabase.Gotrue;

namespace Project_QLTS_DNC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private User _user; // dùng để truyền thông tin đăng nhập
        public MainWindow(User user)
        {
            InitializeComponent();
            LoadBarChart(); // Gọi hàm hiển thị biểu đồ
            _user = user;

        }

        private void btnThoat_Click(object sender, RoutedEventArgs e)
        {
            // Hiển thị hộp thoại xác nhận trước khi đóng ứng dụng
            MessageBoxResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất và đóng chương trình không?",
                "Xác nhận đăng xuất",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            // Chỉ đóng cửa sổ nếu người dùng chọn "Yes"
            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        private void btnNhaCungCap_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.NhaCungCap.NhaCungCapForm();
        }
        public void btnTraCuuTaiSan_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLySanPham.DanhSachSanPham();
        }

        private void btnQuanLyLoaiTaiSan_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyTaiSan.QuanLyTaiSan();
        }
        public void ChuyenDenTraCuuTaiSan()
        {
            // Đặt lại lựa chọn vào nút btnTraCuuTaiSan
            btnTraCuuTaiSan.IsSelected = true;

            // Chuyển đến trang Tra cứu tài sản
            MainContentPanel.Content = new View.QuanLySanPham.DanhSachSanPham();
        }

        private void btnTrangChu_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.TrangChu.TrangChuForm();

        }

     
       
        private void btnPhieuBaoTri_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyPhieu.PhieuBaoTriView();

        }
        private void btnPhieuBaoHong_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyPhieu.PhieuBaoHongView();

        }

        private void btnQuanLyKho_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyKho.QuanLyKhoView();

        }

        private void btnToaNha_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyToanNha.frmToaNha();

        }

        private void btnTang_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyToanNha.frmTang();

        }

        private void btnPhong_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyToanNha.frmPhong();

        }

        private void btnPhongBan_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyToanNha.frmPhongBan();

        }
        //Load hiện thị biểu đồ cột
        private void LoadBarChart()
        {
            assetChart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Số lượng",
                    Values = new ChartValues<int> { 150, 120, 10, 5 },
                    Fill = new SolidColorBrush(Color.FromRgb(0x4D, 0x90, 0xFE))
                }
            };

            assetChart.AxisX.Add(new Axis
            {
                Labels = new[] { "Tổng TS", "Đang dùng", "Bảo trì", "Sửa chữa" },
                Separator = new LiveCharts.Wpf.Separator { Step = 1, IsEnabled = false }
            });

            assetChart.AxisY.Add(new Axis
            {
                Title = "Số lượng",
                LabelFormatter = value => value.ToString()
            });

            assetChart.LegendLocation = LegendLocation.Top;
        }

      

        private void btnPhieuKiemKeTaiSan_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyPhieu.PhieuKiemKeView();

        }

        private void btnNhanVien_Selected(object sender, RoutedEventArgs e)
        {

            MainContentPanel.Content = new View.NhanVien.DanhSachNhanVienForm ();



        }

      

        private void btnBaoTri_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyPhieu.PhieuBaoTriView();

        }

        private void btnBaoCaoKiemKe_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyPhieu.PhieuKiemKeView();

        }

        private void btnNhapKho_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyKho.PhieuNhapKhoView();

        }

        private void btnXuatKho_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyKho.PhieuXuatKhoView();

        }
    }
}