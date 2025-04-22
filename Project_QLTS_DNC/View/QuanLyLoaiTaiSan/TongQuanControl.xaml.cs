using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading.Tasks;
using Project_QLTS_DNC.Models.QLLoaiTS;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Services.QLTaiSanService;
using Project_QLTS_DNC.DTOs;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class TongQuanControl : UserControl
    {
        // Public property để truy cập dữ liệu từ giao diện chính
        public ObservableCollection<LoaiTaiSan> DsLoaiTaiSan { get; set; }
        public ObservableCollection<NhomTaiSan> DsNhomTaiSan { get; set; }

        // Tham chiếu tới MainWindow
        private MainWindow _mainWindow;

        public TongQuanControl()
        {
            InitializeComponent();

            // Tìm MainWindow từ cây phần tử giao diện
            _mainWindow = Application.Current.MainWindow as MainWindow;

            // Kiểm tra nếu tìm thấy MainWindow
            if (_mainWindow == null)
            {
                MessageBox.Show("Không thể tìm thấy cửa sổ chính (MainWindow)", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Đăng ký sự kiện Loaded để cập nhật thống kê khi control được tải
            this.Loaded += (s, e) => {
                CapNhatThongKe();
            };
        }
        /// <summary>
        /// Lấy danh sách tài sản mới nhất và hiển thị lên DataGrid
        /// </summary>
        private async Task LoadTaiSanMoiNhat()
        {
            try
            {
                // Lấy toàn bộ danh sách tài sản
                var dsTaiSan = await TaiSanService.LayDanhSachTaiSanAsync();

                // Chuyển sang DTO để dễ hiển thị và thêm thông tin từ các bảng liên quan
                var dsTaiSanDTO = new ObservableCollection<TaiSanDTO>();

                foreach (var taiSan in dsTaiSan)
                {
                    var taiSanDTO = TaiSanDTO.FromModel(taiSan);

                    // Lấy thông tin nhóm tài sản nếu có
                    if (taiSan.MaChiTietPN.HasValue)
                    {
                        try
                        {
                            var maNhomTS = await ChiTietPhieuNhapService.TimNhomTaiSanTheoTaiSanAsync(taiSan.MaTaiSan);
                            var dsNhomTS = await NhomTaiSanService.LayDanhSachNhomTaiSanAsync();
                            var nhomTS = dsNhomTS.FirstOrDefault(n => n.MaNhomTS == maNhomTS);

                            if (nhomTS != null)
                            {
                                taiSanDTO.MaNhomTS = maNhomTS;
                                taiSanDTO.TenNhomTS = nhomTS.TenNhom;
                            }
                        }
                        catch { /* Xử lý lỗi nếu cần */ }
                    }

                    dsTaiSanDTO.Add(taiSanDTO);
                }

                // Sắp xếp theo ngày sử dụng giảm dần (mới nhất lên đầu)
                var dsTaiSanMoiNhat = new ObservableCollection<TaiSanDTO>(
                    dsTaiSanDTO.OrderByDescending(ts => ts.NgaySuDung)
                              .Take(15) // Lấy 10 tài sản mới nhất
                );

                // Hiển thị lên DataGrid
                dgTaiSanMoiNhat.ItemsSource = dsTaiSanMoiNhat;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách tài sản mới nhất: {ex.Message}",
                                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Cập nhật thông tin thống kê từ CSDL Supabase
        /// </summary>
        public async void CapNhatThongKe()
        {
            try
            {
                // Hiển thị thông báo đang tải
                txtTongLoaiTaiSan.Text = "...";
                txtTongNhomTaiSan.Text = "...";
                txtTongTaiSan.Text = "...";

                // 1. Lấy dữ liệu Loại Tài Sản từ Supabase
                if (DsLoaiTaiSan == null || DsLoaiTaiSan.Count == 0)
                {
                    DsLoaiTaiSan = await LoaiTaiSanService.LayDanhSachLoaiTaiSanAsync();
                }

                // 2. Lấy dữ liệu Nhóm Tài Sản từ Supabase
                if (DsNhomTaiSan == null || DsNhomTaiSan.Count == 0)
                {
                    DsNhomTaiSan = await NhomTaiSanService.LayDanhSachNhomTaiSanAsync();
                }

                // 3. Đếm số lượng Tài Sản
                int soLuongTaiSan = await TaiSanService.DemSoLuongTaiSanAsync();

                // Cập nhật UI với các giá trị thống kê
                txtTongLoaiTaiSan.Text = DsLoaiTaiSan.Count.ToString();
                txtTongNhomTaiSan.Text = DsNhomTaiSan.Count.ToString();
                txtTongTaiSan.Text = soLuongTaiSan.ToString();
                await LoadTaiSanMoiNhat();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật thống kê: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);

                // Đặt giá trị mặc định nếu xảy ra lỗi
                txtTongLoaiTaiSan.Text = "0";
                txtTongNhomTaiSan.Text = "0";
                txtTongTaiSan.Text = "0";
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn vào "Xem chi tiết" trên card Tổng Tài Sản
        /// </summary>
        private void btnXemChiTietTongTaiSan_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem MainWindow đã được khởi tạo chưa
            if (_mainWindow != null)
            {
                // Sử dụng phương thức ChuyenDenTraCuuTaiSan để chuyển trang
                _mainWindow.ChuyenDenTraCuuTaiSan();

                // Thông báo để kiểm tra sự kiện đã được kích hoạt
                MessageBox.Show("Đang chuyển đến trang Tra cứu tài sản", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Không thể tìm thấy cửa sổ chính (MainWindow)", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn vào card Loại Tài Sản hoặc nút Xem chi tiết
        /// </summary>
        private void CardLoaiTaiSan_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChuyenDenTabLoaiTaiSan();
        }

        private void btnXemChiTietLoaiTaiSan_Click(object sender, RoutedEventArgs e)
        {
            ChuyenDenTabLoaiTaiSan();
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn vào card Nhóm Tài Sản hoặc nút Xem chi tiết
        /// </summary>
        private void CardNhomTaiSan_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChuyenDenTabNhomTaiSan();
        }

        private void btnXemChiTietNhomTaiSan_Click(object sender, RoutedEventArgs e)
        {
            ChuyenDenTabNhomTaiSan();
        }

        /// <summary>
        /// Tìm phần tử cha có kiểu chỉ định trong cây phần tử trực quan
        /// </summary>
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            // Lấy phần tử cha
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            // Kiểm tra nếu không có phần tử cha
            if (parentObject == null)
                return null;

            // Kiểm tra nếu phần tử cha là kiểu mong muốn
            if (parentObject is T parent)
                return parent;

            // Đệ quy tìm kiếm phần tử cha tiếp theo
            return FindParent<T>(parentObject);
        }

        /// <summary>
        /// Chuyển đến tab Loại Tài Sản
        /// </summary>
        private void ChuyenDenTabLoaiTaiSan()
        {
            try
            {
                // Tìm QuanLyTaiSan control (parent của TongQuanControl)
                var quanLyTaiSan = FindParent<QuanLyTaiSan>(this);
                if (quanLyTaiSan != null)
                {
                    // Chuyển đến tab Loại Tài Sản (index 1)
                    if (quanLyTaiSan.tabMain != null)
                    {
                        quanLyTaiSan.tabMain.SelectedIndex = 1; // Index của tab Loại Tài Sản
                    }
                }
                else
                {
                    // Thử cách khác - tìm kiếm TabControl trong cây phần tử
                    var tabControl = FindTabControl();
                    if (tabControl != null)
                    {
                        tabControl.SelectedIndex = 1; // Index của tab Loại Tài Sản
                    }
                    else
                    {
                        MessageBox.Show("Không thể tìm thấy TabControl để chuyển tab.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chuyển tab: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Chuyển đến tab Nhóm Tài Sản
        /// </summary>
        private void ChuyenDenTabNhomTaiSan()
        {
            try
            {
                // Tìm QuanLyTaiSan control (parent của TongQuanControl)
                var quanLyTaiSan = FindParent<QuanLyTaiSan>(this);
                if (quanLyTaiSan != null)
                {
                    // Chuyển đến tab Nhóm Tài Sản (index 2)
                    if (quanLyTaiSan.tabMain != null)
                    {
                        quanLyTaiSan.tabMain.SelectedIndex = 2; // Index của tab Nhóm Tài Sản
                    }
                }
                else
                {
                    // Thử cách khác - tìm kiếm TabControl trong cây phần tử
                    var tabControl = FindTabControl();
                    if (tabControl != null)
                    {
                        tabControl.SelectedIndex = 2; // Index của tab Nhóm Tài Sản
                    }
                    else
                    {
                        MessageBox.Show("Không thể tìm thấy TabControl để chuyển tab.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chuyển tab: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Tìm kiếm TabControl trong cây phần tử
        /// </summary>
        private TabControl FindTabControl()
        {
            DependencyObject current = this;
            TabControl tabControl = null;

            // Đi lên cây phần tử để tìm TabItem
            TabItem tabItem = FindParent<TabItem>(this);
            if (tabItem != null)
            {
                // Nếu tìm thấy TabItem, lấy TabControl chứa nó
                tabControl = FindParent<TabControl>(tabItem);
            }
            else
            {
                // Nếu không tìm thấy TabItem, tìm trực tiếp TabControl
                tabControl = FindParent<TabControl>(this);
            }

            return tabControl;
        }
    }
}