using LiveCharts;
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Project_QLTS_DNC.View.NhanVien;
using Project_QLTS_DNC.View.TaiKhoan;


using System.IO;
using Project_QLTS_DNC.Helpers;
using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.Services.TaiKhoan;
using Project_QLTS_DNC.Services.ThongBao;
using Project_QLTS_DNC.Models.ThongBao;
using System.ComponentModel;
using Project_QLTS_DNC.View.DangNhap;
using Project_QLTS_DNC.ViewModel.TaiKhoan;

namespace Project_QLTS_DNC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        //public event Action<string> TreeViewItemSelected;

        private TaiKhoanModel _taiKhoan;
        private List<TaiKhoanModel> _danhSachTaiKhoan;
        public UserProfileViewModel UserProfileVM { get; set; }

        private int _soThongBaoChuaDoc;
        public int SoThongBaoChuaDoc
        {
            get => _soThongBaoChuaDoc;
            set
            {
                _soThongBaoChuaDoc = value;
                OnPropertyChanged(nameof(SoThongBaoChuaDoc));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }




        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            // Lấy từ ThongTinDangNhap
            _taiKhoan = ThongTinDangNhap.TaiKhoanDangNhap;
            _danhSachTaiKhoan = new List<TaiKhoanModel>(); // hoặc null nếu không dùng

            // Bây giờ bạn đã có ThongTinDangNhap.LoaiTaiKhoanDangNhap.TenLoaiTk để check quyền
        }

        #region Window Loading Functions


        private async Task CapNhatSoThongBaoChuaDocAsync()
        {
            var thongBaoService = new ThongBaoService();
            var danhSach = await thongBaoService.LayThongBaoTheoNguoiDungAsync();
            SoThongBaoChuaDoc = danhSach.Count(tb => !tb.DaDoc);
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
            HienThiTreeViewTheoPhanQuyen();
             await CapNhatSoThongBaoChuaDocAsync();
        }
        #endregion
        private void HienThiTreeViewTheoPhanQuyen()
        {
            var controls = new Dictionary<string, TreeViewItem>
            {
                { "btnTrangChu", btnTrangChu },
                { "btnQuanLyTaiKhoan", btnQuanlyTaiKhoan },
                { "btnDanhSachTaiKhoan", btnDanhSachTaiKhoan },
                { "btnLoaiTaiKhoan", btnLoaiTaiKhoan },
                { "btnPhanQuyenTk", btnPhanQuyenTk },

                { "btnQuanLyNhanSu", btnQuanlyNhansu },
                { "btnNhanVien", btnNhanVien },
                { "btnChucVu", btnChucVu },

                { "btnQuanLyLoaiTaiSan", btnQuanLyLoaiTaiSan },

                { "btnQuanLyToaNha", btnQuanlyToaNha },
                { "btnToaNha", btnToaNha },
                { "btnTang", btnTang },
                { "btnPhong", btnPhong },
                { "btnPhongBan", btnPhongBan },

                { "btnQuanLyKho", btnQuanLyKho },
                { "btnDanhSachKho", btnDanhSachKho },
                { "btnNhapKho", btnNhapKho },
                //{ "btnXuatKho", btnXuatKho },
                { "btnTonKho", btnTonKho },
                { "btnBanGiaoTaiSan", btnBanGiaoTaiSan },

                { "btnNhaCungCap", btnNhaCungCap },
                { "btnTraCuuTaiSan", btnTraCuuTaiSan },
                {"btnDiChuyenTaiSan",btnDiChuyenTaiSan },

                { "btnQuanLyBaoTri", btnQuanLyBaotri },
                { "btnBaoTri", btnBaoTri },
                { "btnPhieuBaoTri", btnPhieubaotri },
                { "btnDSBaoTri", btnDSbaotri },

                { "btnQuanLyMuaMoi", btnQuanlymuaMoi },
                { "btnPhieuMuaMoi", btnPhieuMuaMoi },

                { "btnBaoCaoKiemKe", btnBaoCaoKiemKe },
                { "btnDuyetPhieu", btnDuyetPhieu },

                { "btnQuanLyCaiDat", btnQuanLyCaiDat },
                { "btnThongTinCongTy", btnThongTinCongTy },
                //{ "btnPhieuIn", btnPhieuIn },
            };

                    var parentItems = new List<TreeViewItem>
            {
                btnQuanlyTaiKhoan,
                btnQuanlyNhansu,
                btnQuanLyKho,
                btnQuanlyToaNha,
                btnQuanLyBaotri,
                btnQuanlymuaMoi,
                btnQuanLyCaiDat
            };

            //MessageBox.Show($"Tên tài khoản đăng nhập: {ThongTinDangNhap.LoaiTaiKhoanDangNhap?.TenLoaiTk ?? "null"}");
            //MessageBox.Show("Danh sách quyền: " + string.Join(", ", QuyenNguoiDungHelper.DanhSachMaManHinhDuocHienThi ?? new List<string>()));

            // Nếu là admin thì hiển thị tất cả
            if (ThongTinDangNhap.LoaiTaiKhoanDangNhap != null &&
                ThongTinDangNhap.LoaiTaiKhoanDangNhap.TenLoaiTk?.ToLower() == "admin")
            {
                foreach (var item in controls.Values)
                {
                    if (item != null) item.Visibility = Visibility.Visible;
                }

                foreach (var parent in parentItems)
                {
                    if (parent != null) parent.Visibility = Visibility.Visible;
                }

               // MessageBox.Show("Đang hiển thị toàn bộ TreeView vì là Admin");
                return;
            }

            // ❓ Nếu không phải admin, xử lý theo quyền
            var danhSach = QuyenNguoiDungHelper.DanhSachMaManHinhDuocHienThi;

            foreach (var kv in controls)
            {
                if (kv.Value != null)
                {
                    kv.Value.Visibility = danhSach.Contains(kv.Key) ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            foreach (var parent in parentItems)
            {
                bool coItemHien = false;
                foreach (TreeViewItem child in parent.Items)
                {
                    if (child.Visibility == Visibility.Visible)
                    {
                        coItemHien = true;
                        break;
                    }
                }

                parent.Visibility = coItemHien ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ThongBao_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is ThongBaoModel tb)
            {
                // Đánh dấu đã đọc
                _ = new ThongBaoService().DanhDauDaDocAsync(tb.Id);

                // Kiểm tra loại phiếu và mở chi tiết tương ứng
                //if (!string.IsNullOrEmpty(tb.LoaiPhieu) && tb.MaPhieu.HasValue)
                //{
                //    string loai = tb.LoaiPhieu.ToLower();

                //    if (loai.Contains("đề nghị"))
                //    {
                //        var form = new ChiTietPhieuDeNghiWindow(tb.MaPhieu.Value);
                //        form.ShowDialog();
                //    }
                //    else if (loai.Contains("bảo trì"))
                //    {
                //        var form = new ChiTietPhieuBaoTriWindow(tb.MaPhieu.Value);
                //        form.ShowDialog();
                //    }
                //    else
                //    {
                //        MessageBox.Show("Loại phiếu này chưa được hỗ trợ mở chi tiết.", "Thông báo");
                //    }
                //}
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
                // Đếm số tài sản tồn kho - tức là tài sản có MaPhong là null
                int inStock = allAssets.Count(asset => asset.MaPhong == null);

                // Kiểm tra nếu daBaoTri + inStock > total
                int inUse;
                if (daBaoTri + inStock > total)
                {
                    inUse = 0; // Mặc định tài sản đang sử dụng = 0
                }
                else
                {
                    // Tính số tài sản "Đang dùng" bằng tổng tài sản trừ đi số tài sản "Đang bảo trì" và tài sản tồn kho
                    inUse = total - daBaoTri - inStock;
                }

                // Gán giá trị lên các TextBlock thống kê
                txtTong.Text = total.ToString();
                txtDangDung.Text = inUse.ToString();
                txtBaoTri.Text = daBaoTri.ToString();
                txtTonKho.Text = inStock.ToString();
                // 3. Vẽ biểu đồ
                assetChart.Series = new SeriesCollection
        {
            new ColumnSeries
            {
                Title = "Số lượng",
                Values = new ChartValues<int> { total, inUse, daBaoTri, inStock },
                Fill = new SolidColorBrush(Color.FromRgb(0x4D, 0x90, 0xFE))
            }
        };
                // 4. Cấu hình trục X
                assetChart.AxisX.Clear();
                assetChart.AxisX.Add(new Axis
                {
                    Labels = new[] { "Tổng TS", "Đang sử dụng", "Bảo trì", "Tồn kho" },
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

        //private void btnPhieuIn_Selected(object sender, RoutedEventArgs e)
        //{
        //    MainContentPanel.Content = new View.CaiDat.CaiDatPhieuInForm();

        //}

        private void btnDuyetPhieu_Selected(object sender, RoutedEventArgs e)
        {
            if (!QuyenNguoiDungHelper.HasPermission("btnDuyetPhieu", "xem"))
            {
                MessageBox.Show("Bạn không có quyền truy cập chức năng này!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
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
            btnUserProfile.ContextMenu.PlacementTarget = btnUserProfile;
            btnUserProfile.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            btnUserProfile.ContextMenu.IsOpen = true;
        }

        private void ThongTinTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new UserProfileForm();
        }
        
        private void DangXuat_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new DangNhapForm();
            loginWindow.Show();

            this.Close();


        }
        private void btnPhieuMuaMoi_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyPhieu.PhieuMuaMoiTSView();

        }

       

        private async void btnBell_Click(object sender, RoutedEventArgs e)
        {
            var thongBaoService = new ThongBaoService();
            var danhSach = await thongBaoService.LayThongBaoTheoNguoiDungAsync();

            if (danhSach == null || danhSach.Count == 0)
            {
                danhSach = new List<ThongBaoModel>
        {
            new ThongBaoModel
            {
                NoiDung = "🫥 Không có thông báo nào.",
                ThoiGian = DateTime.Now
            }
        };
            }

            dsThongBao.ItemsSource = danhSach
                .OrderByDescending(tb => tb.ThoiGian) // Sắp xếp mới nhất trước
                .Take(10)                             // Rồi lấy 10 cái đầu
                .ToList();

            popupThongBao.IsOpen = true;
            SoThongBaoChuaDoc = danhSach.Count(tb => !tb.DaDoc);

        }
        private void btnXoaThongBaoDaDoc_Click(object sender, RoutedEventArgs e)
        {
            if (dsThongBao.ItemsSource is IEnumerable<ThongBaoModel> thongBaoList)
            {
                // Lọc lại: chỉ giữ những thông báo CHƯA đọc
                var chuaDocList = thongBaoList.Where(tb => !tb.DaDoc).ToList();
                dsThongBao.ItemsSource = chuaDocList;

                // 👇 Nếu bạn dùng badge đếm, nhớ cập nhật lại số
                SoThongBaoChuaDoc = chuaDocList.Count;
            }
        }

        private void btnDiChuyenTaiSan_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.LichSuDiChuyenTS.frmLichSuDiChuyenTS();

        }

        private void btnLichSuBaoTri_Selected(object sender, RoutedEventArgs e)
        {
            MainContentPanel.Content = new View.QuanLyPhieu.LichSuSuaChuaUserControl();
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