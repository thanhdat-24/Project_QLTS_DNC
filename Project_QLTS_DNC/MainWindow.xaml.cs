﻿using LiveCharts;
using LiveCharts.Wpf;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Services.QLTaiSanService;
using Project_QLTS_DNC.View.QuanLyKho;
using Project_QLTS_DNC.View.QuanLyTaiSan;
using Project_QLTS_DNC.View.ThongSoKyThuat;
using Supabase.Gotrue;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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


using System.IO;

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
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadBarChartAsync();
            // Đọc đường dẫn logo từ file và hiển thị
            string logoPathFile = "logo_path.txt";
            if (File.Exists(logoPathFile))
            {
                string savedPath = File.ReadAllText(logoPathFile);
                if (File.Exists(savedPath))
                {
                    imgMainLogo.Source = new BitmapImage(new Uri(savedPath));
                }
            }
        }

        private async Task LoadBarChartAsync()
        {
            try
            {
                // 1. Lấy toàn bộ danh sách tài sản
                var allAssets = await TaiSanService.LayDanhSachTaiSanAsync();

                // 2. Lấy danh sách phiếu bảo trì để đếm tài sản đã được bảo trì
                var danhSachPhieu = await new PhieuBaoTriService().GetPhieuBaoTriAsync();
                int total = allAssets.Count;

                // Đếm tài sản "Đang bảo trì" từ danh sách phiếu bảo trì
                int daBaoTri = danhSachPhieu
                    .Where(p => p.MaTaiSan.HasValue)
                    .Select(p => p.MaTaiSan.Value)
                    .Distinct()
                    .Count();

                // Tính số tài sản "Đang dùng" bằng tổng tài sản trừ đi số tài sản "Đang bảo trì"
                int inUse = total - daBaoTri;
                //Gán giá trị lên các TextBlock thống kê
                txtTong.Text = total.ToString();
                txtDangDung.Text = inUse.ToString();
                txtBaoTri.Text = daBaoTri.ToString();
                // 3. Vẽ biểu đồ (loại bỏ cột "Bảo trì")
                assetChart.Series = new SeriesCollection
        {
            new ColumnSeries
            {
                Title = "Số lượng",
                Values = new ChartValues<int> { total, inUse, daBaoTri },
                Fill = new SolidColorBrush(Color.FromRgb(0x4D, 0x90, 0xFE))
            }
        };

                // 4. Cấu hình trục X (loại bỏ "Bảo trì")
                assetChart.AxisX.Clear();
                assetChart.AxisX.Add(new Axis
                {
                    Labels = new[] { "Tổng TS", "Đang sử dụng", "Bảo trì" },
                    Separator = new LiveCharts.Wpf.Separator { Step = 1, IsEnabled = false }
                });

                // 5. Cấu hình trục Y
                assetChart.AxisY.Clear();
                assetChart.AxisY.Add(new Axis
                {
                    Title = "Số lượng",
                    LabelFormatter = value => value.ToString()
                });

                assetChart.LegendLocation = LegendLocation.Bottom;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi khi tải biểu đồ: " + ex.Message);
            }
        }

        public void UpdateLogo(string logoPath)
        {
            if (File.Exists(logoPath))
            {
                imgMainLogo.Source = new BitmapImage(new Uri(logoPath));
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

        private void btnPhieubaotri_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyPhieu.PhieuBaoTriView();
        }
        private void btnDSbaotri_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyPhieu.DanhSachBaoTriUserControl();
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
            MainContentPanel.Content = new View.QuanLyPhieu.DanhSachBaoTriUserControl();
        }
        private void btnBanGiaoTaiSan_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyTaiSan.DanhSachBanGiaoView();

        }


        private void btnUserProfile_Click(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new UserProfileForm();
        }

        private void btnPhieuMuaMoi_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyPhieu.PhieuMuaMoiTSView();

        }

        private void btnChiTietPhieuMuaMoi_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyPhieu.ChiTietMuaMoiView();


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