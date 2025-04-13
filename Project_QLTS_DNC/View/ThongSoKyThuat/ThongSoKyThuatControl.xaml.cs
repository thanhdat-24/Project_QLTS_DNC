using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Services.QLTaiSanService;
using Project_QLTS_DNC.View.QuanLyTaiSan;

namespace Project_QLTS_DNC.View.ThongSoKyThuat
{
    public partial class ThongSoKyThuatControl : UserControl
    {
        // Thuộc tính lưu thông tin nhóm tài sản đang được xem thông số
        public NhomTaiSan NhomTaiSan { get; private set; }

        // Collection dùng cho hiển thị thông số
        private ObservableCollection<ThongSoKyThuatDTO> DsThongSoKyThuat { get; set; }

        // Event để thông báo sự thay đổi dữ liệu
        public event Action OnBackRequested;
        public event Action OnDataChanged;

        // Tham chiếu đến MainWindow
        private MainWindow _mainWindow;

        public ThongSoKyThuatControl()
        {
            InitializeComponent();
            DsThongSoKyThuat = new ObservableCollection<ThongSoKyThuatDTO>();

            // Tìm MainWindow từ cây phần tử giao diện
            _mainWindow = Application.Current.MainWindow as MainWindow;
        }

        /// <summary>
        /// Khởi tạo thông tin hiển thị cho nhóm tài sản được chọn
        /// </summary>
        /// <param name="nhomTaiSan">Nhóm tài sản được chọn</param>
        public void KhoiTao(NhomTaiSan nhomTaiSan)
        {
            NhomTaiSan = nhomTaiSan;

            // Hiển thị thông tin nhóm tài sản
            txtTenNhom.Text = nhomTaiSan.TenNhom;
            txtMaNhom.Text = nhomTaiSan.MaNhomTS.ToString();

            // Tải dữ liệu thông số kỹ thuật
            LoadDuLieuThongSoKyThuat();
        }

        /// <summary>
        /// Tải dữ liệu thông số kỹ thuật từ cơ sở dữ liệu
        /// </summary>
        private async void LoadDuLieuThongSoKyThuat()
        {
            try
            {
                // Hiển thị thông báo đang tải
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.Cursor = System.Windows.Input.Cursors.Wait;
                }

                // Lấy danh sách thông số kỹ thuật theo mã nhóm tài sản
                var dsThongSo = await ThongSoKyThuatService.LayDanhSachThongSoAsync(NhomTaiSan.MaNhomTS);

                // Cập nhật collection hiển thị
                DsThongSoKyThuat.Clear();
                foreach (var thongSo in dsThongSo)
                {
                    DsThongSoKyThuat.Add(thongSo);
                }

                // Khôi phục con trỏ
                if (window != null)
                {
                    window.Cursor = null;
                }

                // Hiển thị dữ liệu
                dgThongSoKyThuat.ItemsSource = DsThongSoKyThuat;
            }
            catch (Exception ex)
            {
                // Khôi phục con trỏ
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.Cursor = null;
                }

                MessageBox.Show($"Lỗi khi tải dữ liệu thông số kỹ thuật: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Quay Lại
        /// </summary>
        private void btnQuayLai_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Gọi event quay lại
                OnBackRequested?.Invoke();

                // Kiểm tra xem MainWindow đã được tìm thấy chưa
                if (_mainWindow == null)
                {
                    _mainWindow = Application.Current.MainWindow as MainWindow;
                    if (_mainWindow == null)
                    {
                        MessageBox.Show("Không thể tìm thấy cửa sổ chính (MainWindow)",
                            "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Chuyển về giao diện Quản lý loại tài sản bằng cách kích hoạt button trong menu
                _mainWindow.btnQuanLyLoaiTaiSan.IsSelected = true;

                // Thay vì gọi method private, chúng ta sẽ giả lập việc click vào menu
                System.Windows.RoutedEventArgs routedEventArgs = new System.Windows.RoutedEventArgs(
                    System.Windows.Controls.TreeViewItem.SelectedEvent, _mainWindow.btnQuanLyLoaiTaiSan);

                // Thực thi event handler thông qua Reflection để bỏ qua access protection
                Type type = _mainWindow.GetType();
                System.Reflection.MethodInfo methodInfo = type.GetMethod("btnQuanLyLoaiTaiSan_Selected",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (methodInfo != null)
                {
                    methodInfo.Invoke(_mainWindow, new object[] { _mainWindow.btnQuanLyLoaiTaiSan, routedEventArgs });

                    // Chuyển tab sang tab Nhóm Tài Sản - đảm bảo đúng kiểu của QuanLyTaiSan
                    var content = _mainWindow.MainContentPanel.Content;
                    if (content != null && content.GetType().Name == "QuanLyTaiSan")
                    {
                        // Truy cập tabMain thông qua reflection vì là property public
                        var tabMainProperty = content.GetType().GetProperty("tabMain");
                        if (tabMainProperty != null)
                        {
                            var tabMain = tabMainProperty.GetValue(content) as TabControl;
                            if (tabMain != null)
                            {
                                // Chọn tab Nhóm Tài Sản (index 2)
                                tabMain.SelectedIndex = 2;
                            }
                        }
                    }
                }
                else
                {
                    // Fallback nếu không tìm thấy phương thức qua reflection
                    MessageBox.Show("Không thể quay về giao diện quản lý tài sản.",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi quay lại: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Sửa thông số
        /// </summary>
        private async void SuaThongSo_Click(object sender, RoutedEventArgs e)
        {
            var thongSo = dgThongSoKyThuat.SelectedItem as ThongSoKyThuatDTO;
            if (thongSo == null)
            {
                MessageBox.Show("Vui lòng chọn một thông số để sửa.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Mở cửa sổ cập nhật thông số kỹ thuật
            var capNhatThongSoWindow = new CapNhatThongSoKyThuatWindow(thongSo, NhomTaiSan);
            capNhatThongSoWindow.Owner = Window.GetWindow(this);

            // Hiển thị cửa sổ và kiểm tra kết quả
            bool? result = capNhatThongSoWindow.ShowDialog();

            if (result == true && capNhatThongSoWindow.ThongSoDaCapNhat != null)
            {
                try
                {
                    // Cập nhật trong danh sách hiển thị
                    int index = DsThongSoKyThuat.IndexOf(thongSo);
                    if (index >= 0)
                    {
                        DsThongSoKyThuat[index] = capNhatThongSoWindow.ThongSoDaCapNhat;
                    }

                    // Cập nhật lại DataGrid
                    dgThongSoKyThuat.Items.Refresh();

                    // Thông báo dữ liệu đã thay đổi
                    OnDataChanged?.Invoke();

                    MessageBox.Show("Cập nhật thông số kỹ thuật thành công!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật thông số kỹ thuật: {ex.Message}",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Xóa thông số
        /// </summary>
        private async void XoaThongSo_Click(object sender, RoutedEventArgs e)
        {
            var thongSo = dgThongSoKyThuat.SelectedItem as ThongSoKyThuatDTO;
            if (thongSo == null)
            {
                MessageBox.Show("Vui lòng chọn một thông số để xóa.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Xác nhận xóa
            MessageBoxResult xacNhan = MessageBox.Show($"Bạn có chắc chắn muốn xóa thông số '{thongSo.TenThongSo}'?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (xacNhan != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                // Hiển thị thông báo đang xử lý
                var window = Window.GetWindow(this);
                window.Cursor = System.Windows.Input.Cursors.Wait;

                // Thực hiện xóa dữ liệu từ cơ sở dữ liệu
                bool ketQuaXoa = await ThongSoKyThuatService.XoaThongSoAsync(thongSo.MaThongSo);

                // Khôi phục con trỏ
                window.Cursor = null;

                if (ketQuaXoa)
                {
                    // Xóa khỏi collection
                    DsThongSoKyThuat.Remove(thongSo);

                    // Thông báo dữ liệu đã thay đổi
                    OnDataChanged?.Invoke();

                    MessageBox.Show("Xóa thông số kỹ thuật thành công!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Không thể xóa thông số kỹ thuật. Vui lòng thử lại sau.",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                // Khôi phục con trỏ
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.Cursor = null;
                }

                MessageBox.Show($"Lỗi khi xóa thông số kỹ thuật: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Xử lý sự kiện khi nhấn nút Thêm mới thông số
        /// </summary>
        private async void btnThemMoiThongSo_Click(object sender, RoutedEventArgs e)
        {
            // Mở cửa sổ thêm mới thông số kỹ thuật
            var themThongSoWindow = new ThemThongSoKyThuatWindow(NhomTaiSan);
            themThongSoWindow.Owner = Window.GetWindow(this);

            // Hiển thị cửa sổ và kiểm tra kết quả
            bool? result = themThongSoWindow.ShowDialog();

            if (result == true && themThongSoWindow.DanhSachThongSoMoi.Count > 0)
            {
                try
                {
                    // Thêm tất cả thông số mới vào danh sách hiển thị
                    foreach (var thongSo in themThongSoWindow.DanhSachThongSoMoi)
                    {
                        DsThongSoKyThuat.Add(thongSo);
                    }

                    // Cập nhật lại DataGrid
                    dgThongSoKyThuat.Items.Refresh();

                    // Thông báo dữ liệu đã thay đổi
                    OnDataChanged?.Invoke();

                    MessageBox.Show($"Đã thêm {themThongSoWindow.DanhSachThongSoMoi.Count} thông số kỹ thuật thành công!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi thêm thông số kỹ thuật: {ex.Message}",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}