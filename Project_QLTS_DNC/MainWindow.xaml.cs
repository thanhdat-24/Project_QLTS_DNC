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
using Project_QLTS_DNC.View.QuanLyKho;
using Project_QLTS_DNC.View.QuanLyTaiSan;
using Project_QLTS_DNC.View.ThongSoKyThuat;
using Project_QLTS_DNC.Models.QLNhomTS;
using System.Windows.Controls;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.View.NhanVien;
using Project_QLTS_DNC.View.TaiKhoan;

namespace Project_QLTS_DNC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public event Action<string> TreeViewItemSelected;

        private TaiKhoanModel _taiKhoan;
        private List<TaiKhoanModel> _danhSachTaiKhoan;
        


        public MainWindow(TaiKhoanModel taiKhoan, List<TaiKhoanModel> danhSachTaiKhoan = null)
        {
            InitializeComponent();
            _taiKhoan = taiKhoan;
            _danhSachTaiKhoan = danhSachTaiKhoan;

            if (_taiKhoan.MaLoaiTk == 1 && _danhSachTaiKhoan != null)
            {
                //danhSachTaiKhoan.ItemsSource = _danhSachTaiKhoan; 
            }
        }

        //private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        //{
        //    if (e.NewValue is TreeViewItem selectedItem)
        //    {
        //        string selectedHeader = selectedItem.Header.ToString();
        //        MessageBox.Show($"Bạn đã chọn: {selectedHeader}");
        //    }
        //}




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
            // Hiển thị form quản lý kho chính
            var quanLyKhoForm = new QuanLyKhoView(); // Giả sử bạn đã tạo form QuanLyKhoForm
            MainContentPanel.Content = quanLyKhoForm; // Gán form vào ContentControl
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

            
            MainContentPanel.Content = new View.NhanVien.DanhSachNhanVienForm();



        }



        private void btnBaoTri_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyPhieu.PhieuBaoTriView();

        }

        private void btnBaoCaoKiemKe_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.KiemKe.DotKiemKeView();

        }




        private void btnNhapKho_Selected(object sender, RoutedEventArgs e)
        {
            // Hiển thị form nhập kho
            var nhậpKhoForm = new PhieuNhapKhoView(); // Giả sử bạn đã tạo form NhapKhoForm
            MainContentPanel.Content = nhậpKhoForm; // Thay thế nội dung của ContentControl
        }

        private void btnXuatKho_Selected(object sender, RoutedEventArgs e)
        {
            // Hiển thị form xuất kho
            var xuấtKhoForm = new PhieuXuatKhoView(); // Giả sử bạn đã tạo form XuatKhoForm
            MainContentPanel.Content = xuấtKhoForm; // Thay thế nội dung của ContentControl
        }

        private void btnTonKho_Selected(object sender, RoutedEventArgs e)
        {
            // Hiển thị form tồn kho
            var tồnKhoForm = new TonKhoView(); // Giả sử bạn đã tạo form TonKhoForm
            MainContentPanel.Content = tồnKhoForm; // Thay thế nội dung của ContentControl
        }

      

       

        private void btnThongTinCongTy_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.CaiDat.ThongTinCongTyForm();

        }

        private void btnDanhSachTaiKhoan_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.TaiKhoan.DanhSachTaiKhoanForm();
        }

        private void btnLoaiTaiKhoan_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.TaiKhoan.LoaiTaiKhoanForm();
        }


        private void btnLoaiBaoTri_Selected(object sender, RoutedEventArgs e)
        {
           MainContentPanel.Content = new View.BaoTri.LoaiBaoTriForm();

        }

        private void btnDSbaotri_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyPhieu.PhieuBaoTriView();
        }

        private void btnChucVu_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.ChucVu.ChucVuForm();
        }

        private void btnPhieuIn_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.CaiDat.PhieuInForm();

        }

        private void btnDuyetPhieu_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.DuyetPhieu.frmDuyetPhieu();

        }

        private void btnPhanQuyenTk_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.TaiKhoan.PhanQuyenForm();
        }

        private void btnBaoTri_Selected_1(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.BaoTri.LoaiBaoTriForm();
        }
        private void btnDSbaotri_Selected_1(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyPhieu.PhieuBaoTriView();
        }
        private void btnBanGiaoTaiSan_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyTaiSan.DanhSachBanGiaoView();

        }

        private void btnUserProfile_Click(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new UserProfileForm();
        }
    }
    public static class MainWindowExtensions
    {
        public static T GetContentControl<T>(this MainWindow mainWindow) where T : Control
        {
            return mainWindow.MainContentPanel.Content as T;
        }

        // Phương thức mở rộng để chọn tab Nhóm Tài Sản
        public static void SelectNhomTaiSanTab(this MainWindow mainWindow)
        {
            // Lấy đối tượng QuanLyTaiSan từ MainContentPanel
            var quanLyTaiSan = mainWindow.GetContentControl<View.QuanLyTaiSan.QuanLyTaiSan>();

            // Nếu tìm thấy, chọn tab Nhóm Tài Sản (tab thứ 3, index = 2)
            if (quanLyTaiSan != null && quanLyTaiSan.tabMain != null)
            {
                quanLyTaiSan.tabMain.SelectedIndex = 2;
            }
        }


       


       

       
    }
}