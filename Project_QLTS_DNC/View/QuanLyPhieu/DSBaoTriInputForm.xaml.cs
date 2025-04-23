using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.BaoTri;
using BaoTriKiemKeTaiSan = Project_QLTS_DNC.Models.BaoTri.KiemKeTaiSan;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.View.BaoTri;
using Supabase;
using Supabase.Gotrue;
using Supabase.Postgrest;
using KiemKeDotKiemKe = Project_QLTS_DNC.Models.KiemKe.DotKiemKe;
using static Supabase.Postgrest.Constants;
using Project_QLTS_DNC.Models.KiemKe;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    public partial class DSBaoTriInputForm : Window, INotifyPropertyChanged
    {
        // Collection để lưu trữ dữ liệu cho các ComboBox
        private ObservableCollection<KiemKeDotKiemKe> _danhSachDotKiemKe;
        private ObservableCollection<TaiSanModel> _danhSachTaiSan;
        private ObservableCollection<Phong> _danhSachPhong;

        // Collection tìm kiếm tài sản
        private ObservableCollection<TaiSanViewModel> _taiSanSuggestions;

        // Tài sản đã chọn
        private TaiSanViewModel _selectedTaiSan;

        // Client để kết nối với Supabase
        private readonly Supabase.Client _supabaseClient;

        // Đối tượng kiểm kê hiện tại
        private BaoTriKiemKeTaiSan _kiemKeTaiSan;

        // Chế độ (thêm mới hay cập nhật)
        private bool _isUpdateMode;

        // Properties với INotifyPropertyChanged để binding
        public event PropertyChangedEventHandler PropertyChanged;

        // ViewModel cho tài sản hiển thị trong tìm kiếm
        public class TaiSanViewModel
        {
            public int MaTaiSan { get; set; }
            public string TenTaiSan { get; set; }
            public string SoSeri { get; set; }
            public string TinhTrangSP { get; set; }
            public DateTime? HanBH { get; set; }
            public int? MaPhong { get; set; }
        }

        public ObservableCollection<KiemKeDotKiemKe> DanhSachDotKiemKe
        {
            get => _danhSachDotKiemKe;
            set
            {
                _danhSachDotKiemKe = value;
                OnPropertyChanged(nameof(DanhSachDotKiemKe));
            }
        }

        public ObservableCollection<TaiSanModel> DanhSachTaiSan
        {
            get => _danhSachTaiSan;
            set
            {
                _danhSachTaiSan = value;
                OnPropertyChanged(nameof(DanhSachTaiSan));
            }
        }

        public ObservableCollection<Phong> DanhSachPhong
        {
            get => _danhSachPhong;
            set
            {
                _danhSachPhong = value;
                OnPropertyChanged(nameof(DanhSachPhong));
            }
        }

        public ObservableCollection<TaiSanViewModel> TaiSanSuggestions
        {
            get => _taiSanSuggestions;
            set
            {
                _taiSanSuggestions = value;
                OnPropertyChanged(nameof(TaiSanSuggestions));
            }
        }

        public TaiSanViewModel SelectedTaiSan
        {
            get => _selectedTaiSan;
            set
            {
                _selectedTaiSan = value;
                OnPropertyChanged(nameof(SelectedTaiSan));
                if (_selectedTaiSan != null)
                {
                    UpdatePhongInfo(_selectedTaiSan.MaPhong);
                }
            }
        }

        public BaoTriKiemKeTaiSan KiemKeTaiSan
        {
            get => _kiemKeTaiSan;
            set
            {
                _kiemKeTaiSan = value;
                OnPropertyChanged(nameof(KiemKeTaiSan));
            }
        }

        // Constructor cho trường hợp tạo mới phiếu bảo trì
        public DSBaoTriInputForm(Supabase.Client supabaseClient)
        {
            InitializeComponent();

            _supabaseClient = supabaseClient;
            _isUpdateMode = false;

            // Khởi tạo collections
            DanhSachDotKiemKe = new ObservableCollection<KiemKeDotKiemKe>();
            DanhSachTaiSan = new ObservableCollection<TaiSanModel>();
            DanhSachPhong = new ObservableCollection<Phong>();
            TaiSanSuggestions = new ObservableCollection<TaiSanViewModel>();

            // Khởi tạo đối tượng kiểm kê tài sản mới
            KiemKeTaiSan = new BaoTriKiemKeTaiSan();

            // Set DataContext cho binding
            DataContext = this;

            // Load dữ liệu khi khởi tạo form
            Loaded += DSBaoTriInputForm_Loaded;

            // Đăng ký sự kiện cho các nút
            btnLuu.Click += BtnLuu_Click;
            btnHuy.Click += BtnHuy_Click;

            // Đăng ký sự kiện validation cho các trường nhập liệu
            txtGhiChu.TextChanged += TextBox_TextChanged;
            txtViTriThucTe.TextChanged += TextBox_TextChanged;

            // Chỉ cho phép nhập số cho các trường số
            txtMaKiemKe.PreviewTextInput += NumericOnly_PreviewTextInput;

            // Thiết lập ListView tài sản suggestions
            lvTaiSanSuggestions.ItemsSource = TaiSanSuggestions;
        }

        // Constructor cho trường hợp cập nhật
        public DSBaoTriInputForm(Supabase.Client supabaseClient, BaoTriKiemKeTaiSan kiemKeTaiSan)
        {
            InitializeComponent();

            _supabaseClient = supabaseClient;
            _isUpdateMode = true;

            // Khởi tạo collections
            DanhSachDotKiemKe = new ObservableCollection<KiemKeDotKiemKe>();
            DanhSachTaiSan = new ObservableCollection<TaiSanModel>();
            DanhSachPhong = new ObservableCollection<Phong>();
            TaiSanSuggestions = new ObservableCollection<TaiSanViewModel>();

            // Sử dụng đối tượng kiểm kê tài sản đã có
            KiemKeTaiSan = kiemKeTaiSan;

            // Set DataContext cho binding
            DataContext = this;

            // Load dữ liệu khi khởi tạo form
            Loaded += DSBaoTriInputForm_Loaded;

            // Đăng ký sự kiện cho các nút
            btnLuu.Click += BtnLuu_Click;
            btnHuy.Click += BtnHuy_Click;

            // Đăng ký sự kiện validation cho các trường nhập liệu
            txtGhiChu.TextChanged += TextBox_TextChanged;
            txtViTriThucTe.TextChanged += TextBox_TextChanged;

            // Chỉ cho phép nhập số cho các trường số
            txtMaKiemKe.PreviewTextInput += NumericOnly_PreviewTextInput;

            // Thiết lập ListView tài sản suggestions
            lvTaiSanSuggestions.ItemsSource = TaiSanSuggestions;
        }

        // Xử lý sự kiện khi form được load
        private async void DSBaoTriInputForm_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Hiển thị thông báo loading
                ShowLoadingMessage(true);

                // Load dữ liệu cho các ComboBox
                await LoadDataForComboBoxes();

                // Cài đặt các giá trị mặc định hoặc giá trị từ đối tượng hiện tại nếu ở chế độ cập nhật
                SetupFormData();

                // Ẩn thông báo loading
                ShowLoadingMessage(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                ShowLoadingMessage(false);
            }
        }

        // Load dữ liệu cho các ComboBox
        private async Task LoadDataForComboBoxes()
        {
            try
            {
                // Tải dữ liệu đợt kiểm kê
                var dotkiemkeTable = _supabaseClient.Postgrest.Table<DotKiemKe>();
                var dotkiemkeResponse = await dotkiemkeTable.Get();
                DanhSachDotKiemKe = new ObservableCollection<DotKiemKe>(dotkiemkeResponse.Models);

                // Tạo một class ViewModel để đại diện cho đợt kiểm kê trong ComboBox
                var dotKiemKeItems = DanhSachDotKiemKe.Select(d => new DotKiemKeViewModel
                {
                    MaDotKiemKe = d.MaDotKiemKe,
                    DisplayText = d.TenDot ?? $"Đợt {d.MaDotKiemKe}"
                }).ToList();

                // Thiết lập ItemsSource và binding paths
                cboDotKiemKe.ItemsSource = dotKiemKeItems;
                cboDotKiemKe.DisplayMemberPath = "DisplayText";
                cboDotKiemKe.SelectedValuePath = "MaDotKiemKe";

                // Tải tất cả phòng
                var phongTable = _supabaseClient.Postgrest.Table<Phong>();
                var phongResponse = await phongTable.Get();
                DanhSachPhong = new ObservableCollection<Phong>(phongResponse.Models);

                // Tải danh sách tài sản với tình trạng "Cần kiểm tra"
                var taisanTable = _supabaseClient.Postgrest.Table<TaiSanModel>();
                var taisanFilter = taisanTable.Filter("tinh_trang_sp", Operator.Equals, "Cần kiểm tra");
                var taisanResponse = await taisanFilter.Get();
                DanhSachTaiSan = new ObservableCollection<TaiSanModel>(taisanResponse.Models);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Thêm lớp ViewModel này vào class của bạn
        public class DotKiemKeViewModel
        {
            public int MaDotKiemKe { get; set; }
            public string DisplayText { get; set; }

            public override string ToString()
            {
                return DisplayText;
            }
        }

        // Thiết lập dữ liệu cho form
        private async void SetupFormData()
        {
            if (_isUpdateMode)
            {
                // Chế độ cập nhật - Đặt tiêu đề và giá trị từ đối tượng hiện tại
                Title = "Cập nhật Phiếu Bảo Trì";
                txtMaKiemKe.Text = KiemKeTaiSan.MaKiemKeTS.ToString();

                // Tìm và chọn đợt kiểm kê tương ứng
                if (KiemKeTaiSan.MaDotKiemKe.HasValue)
                {
                    var dotKiemKe = DanhSachDotKiemKe.FirstOrDefault(d => d.MaDotKiemKe == KiemKeTaiSan.MaDotKiemKe.Value);
                    if (dotKiemKe != null)
                        cboDotKiemKe.SelectedItem = dotKiemKe;
                }

                // Tìm tài sản tương ứng và hiển thị
                if (KiemKeTaiSan.MaTaiSan.HasValue)
                {
                    var taiSan = DanhSachTaiSan.FirstOrDefault(t => t.MaTaiSan == KiemKeTaiSan.MaTaiSan.Value);
                    if (taiSan != null)
                    {
                        SelectedTaiSan = new TaiSanViewModel
                        {
                            MaTaiSan = taiSan.MaTaiSan,
                            TenTaiSan = taiSan.TenTaiSan,
                            SoSeri = taiSan.SoSeri,
                            TinhTrangSP = taiSan.TinhTrangSP,
                            HanBH = taiSan.HanBH,
                            MaPhong = taiSan.MaPhong
                        };

                        // Hiển thị thông tin tài sản đã chọn
                        ShowSelectedTaiSan(SelectedTaiSan);

                        // Cập nhật thông tin phòng
                        UpdatePhongInfo(taiSan.MaPhong);
                    }
                }

                // Đặt giá trị cho tình trạng
                if (!string.IsNullOrEmpty(KiemKeTaiSan.TinhTrang))
                {
                    foreach (ComboBoxItem item in cboTinhTrang.Items)
                    {
                        if (item.Content.ToString() == KiemKeTaiSan.TinhTrang)
                        {
                            cboTinhTrang.SelectedItem = item;
                            break;
                        }
                    }
                }

                txtViTriThucTe.Text = KiemKeTaiSan.ViTriThucTe;
                txtGhiChu.Text = KiemKeTaiSan.GhiChu;
            }
            else
            {
                // Chế độ thêm mới - Tạo mã kiểm kê mới
                Title = "Thêm Phiếu Bảo Trì";
                await GenerateNewId();

                // Thiết lập giá trị mặc định cho ComboBox TinhTrang (chọn item đầu tiên)
                if (cboTinhTrang.Items.Count > 0)
                    cboTinhTrang.SelectedIndex = 0;
            }
        }

        // Tạo mã kiểm kê mới
        private async Task GenerateNewId()
        {
            try
            {
                // Lấy mã kiểm kê lớn nhất hiện tại và tăng lên 1
                var table = _supabaseClient.Postgrest.Table<BaoTriKiemKeTaiSan>();
                table.Select("ma_kiem_ke_ts");
                table.Order("ma_kiem_ke_ts", Ordering.Descending);
                table.Limit(1);
                var response = await table.Get();

                if (response.Models.Count > 0)
                {
                    int maxId = response.Models[0].MaKiemKeTS;
                    KiemKeTaiSan.MaKiemKeTS = maxId + 1;
                }
                else
                {
                    KiemKeTaiSan.MaKiemKeTS = 1;
                }

                txtMaKiemKe.Text = KiemKeTaiSan.MaKiemKeTS.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo mã bảo trì: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Xử lý sự kiện tìm kiếm tài sản
        private void txtSearchTaiSan_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = txtSearchTaiSan.Text.Trim().ToLower();

            // Nếu ô tìm kiếm trống, ẩn danh sách gợi ý
            if (string.IsNullOrEmpty(searchText))
            {
                lvTaiSanSuggestions.Visibility = Visibility.Collapsed;
                return;
            }

            // Lọc danh sách tài sản theo từ khóa tìm kiếm
            var filteredItems = DanhSachTaiSan
                .Where(ts =>
                    (ts.TenTaiSan != null && ts.TenTaiSan.ToLower().Contains(searchText)) ||
                    (ts.SoSeri != null && ts.SoSeri.ToLower().Contains(searchText)) ||
                    (ts.TinhTrangSP != null && ts.TinhTrangSP.ToLower().Contains(searchText)))
                .Take(10) // Giới hạn số lượng kết quả
                .Select(ts => new TaiSanViewModel
                {
                    MaTaiSan = ts.MaTaiSan,
                    TenTaiSan = ts.TenTaiSan,
                    SoSeri = ts.SoSeri,
                    TinhTrangSP = ts.TinhTrangSP,
                    HanBH = ts.HanBH,
                    MaPhong = ts.MaPhong
                })
                .ToList();

            // Cập nhật danh sách gợi ý
            TaiSanSuggestions.Clear();
            foreach (var item in filteredItems)
            {
                TaiSanSuggestions.Add(item);
            }

            // Hiển thị danh sách gợi ý nếu có kết quả
            lvTaiSanSuggestions.Visibility = TaiSanSuggestions.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        // Xử lý khi nhấn phím trong ô tìm kiếm
        private void txtSearchTaiSan_KeyUp(object sender, KeyEventArgs e)
        {
            // Xử lý phím mũi tên lên/xuống để điều hướng trong danh sách gợi ý
            if (lvTaiSanSuggestions.Visibility == Visibility.Visible)
            {
                if (e.Key == Key.Down)
                {
                    if (lvTaiSanSuggestions.SelectedIndex < lvTaiSanSuggestions.Items.Count - 1)
                    {
                        lvTaiSanSuggestions.SelectedIndex++;
                    }
                    else
                    {
                        lvTaiSanSuggestions.SelectedIndex = 0;
                    }
                    lvTaiSanSuggestions.ScrollIntoView(lvTaiSanSuggestions.SelectedItem);
                    e.Handled = true;
                }
                else if (e.Key == Key.Up)
                {
                    if (lvTaiSanSuggestions.SelectedIndex > 0)
                    {
                        lvTaiSanSuggestions.SelectedIndex--;
                    }
                    else
                    {
                        lvTaiSanSuggestions.SelectedIndex = lvTaiSanSuggestions.Items.Count - 1;
                    }
                    lvTaiSanSuggestions.ScrollIntoView(lvTaiSanSuggestions.SelectedItem);
                    e.Handled = true;
                }
                else if (e.Key == Key.Enter && lvTaiSanSuggestions.SelectedItem != null)
                {
                    // Chọn tài sản khi nhấn Enter
                    SelectedTaiSan = (TaiSanViewModel)lvTaiSanSuggestions.SelectedItem;
                    ShowSelectedTaiSan(SelectedTaiSan);
                    lvTaiSanSuggestions.Visibility = Visibility.Collapsed;
                    txtSearchTaiSan.Text = string.Empty;
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    // Ẩn danh sách gợi ý khi nhấn Escape
                    lvTaiSanSuggestions.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                }
            }
        }

        // Xử lý khi chọn một tài sản từ danh sách gợi ý
        private void lvTaiSanSuggestions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvTaiSanSuggestions.SelectedItem is TaiSanViewModel selectedTaiSan)
            {
                SelectedTaiSan = selectedTaiSan;
                ShowSelectedTaiSan(SelectedTaiSan);
                lvTaiSanSuggestions.Visibility = Visibility.Collapsed;
                txtSearchTaiSan.Text = string.Empty;
            }
        }

        // Hiển thị thông tin tài sản đã chọn
        private void ShowSelectedTaiSan(TaiSanViewModel taiSan)
        {
            if (taiSan != null)
            {
                txtSelectedTaiSanName.Text = taiSan.TenTaiSan;
                txtSelectedTaiSanSeri.Text = taiSan.SoSeri;
                txtSelectedTaiSanStatus.Text = taiSan.TinhTrangSP;
                txtSelectedTaiSanWarranty.Text = taiSan.HanBH.HasValue
                    ? taiSan.HanBH.Value.ToString("dd/MM/yyyy")
                    : "Không có";

                selectedTaiSanInfo.Visibility = Visibility.Visible;
                UpdatePhongInfo(taiSan.MaPhong);
            }
        }

        // Cập nhật thông tin phòng dựa trên mã phòng của tài sản đã chọn
        private void UpdatePhongInfo(int? maPhong)
        {
            if (maPhong.HasValue)
            {
                var phong = DanhSachPhong.FirstOrDefault(p => p.MaPhong == maPhong.Value);
                if (phong != null)
                {
                    txtPhong.Text = phong.TenPhong;
                }
                else
                {
                    txtPhong.Text = $"Phòng (Mã: {maPhong.Value})";
                }
            }
            else
            {
                txtPhong.Text = "Chưa xác định";
            }
        }

        // Xóa tài sản đã chọn
        private void btnClearTaiSan_Click(object sender, RoutedEventArgs e)
        {
            SelectedTaiSan = null;
            selectedTaiSanInfo.Visibility = Visibility.Collapsed;
            txtPhong.Text = string.Empty;
        }

        // Xử lý sự kiện khi nhấn nút Lưu
        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra dữ liệu nhập
                if (!ValidateForm())
                {
                    return;
                }

                // Lấy dữ liệu từ form
                GetDataFromForm();

                // Hiển thị thông báo đang xử lý
                ShowLoadingMessage(true);

                // Lấy tham chiếu đến bảng
                var table = _supabaseClient.Postgrest.Table<BaoTriKiemKeTaiSan>();

                if (_isUpdateMode)
                {
                    // Cập nhật dữ liệu
                    await table.Update(KiemKeTaiSan);
                    MessageBox.Show("Cập nhật phiếu bảo trì thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Thêm mới dữ liệu
                    await table.Insert(KiemKeTaiSan);
                    MessageBox.Show("Thêm phiếu bảo trì thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                ShowLoadingMessage(false);

                // Đóng form
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowLoadingMessage(false);
                MessageBox.Show($"Lỗi khi lưu phiếu bảo trì: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Xử lý sự kiện khi nhấn nút Hủy
        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Kiểm tra dữ liệu nhập
        private bool ValidateForm()
        {
            if (cboDotKiemKe.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn đợt kiểm kê!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                cboDotKiemKe.Focus();
                return false;
            }

            if (SelectedTaiSan == null)
            {
                MessageBox.Show("Vui lòng chọn tài sản!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtSearchTaiSan.Focus();
                return false;
            }

            if (cboTinhTrang.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn tình trạng tài sản!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                cboTinhTrang.Focus();
                return false;
            }

            return true;
        }

        // Lấy dữ liệu từ form và gán vào đối tượng KiemKeTaiSan
        private void GetDataFromForm()
        {
            KiemKeTaiSan.MaKiemKeTS = int.Parse(txtMaKiemKe.Text);

            // Lấy mã đợt kiểm kê từ ComboBox
            if (cboDotKiemKe.SelectedItem is KiemKeDotKiemKe dotKiemKe)
            {
                KiemKeTaiSan.MaDotKiemKe = dotKiemKe.MaDotKiemKe;
            }

            // Lấy mã tài sản từ tài sản đã chọn
            if (SelectedTaiSan != null)
            {
                KiemKeTaiSan.MaTaiSan = SelectedTaiSan.MaTaiSan;
                KiemKeTaiSan.MaPhong = SelectedTaiSan.MaPhong;
            }

            // Lấy giá trị tình trạng
            if (cboTinhTrang.SelectedItem is ComboBoxItem tinhTrang)
            {
                KiemKeTaiSan.TinhTrang = tinhTrang.Content.ToString();
            }

            // Lấy các giá trị khác
            KiemKeTaiSan.ViTriThucTe = txtViTriThucTe.Text;
            KiemKeTaiSan.GhiChu = txtGhiChu.Text;
           
        }

        // Xử lý sự kiện khi nội dung TextBox thay đổi
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Có thể thêm xử lý validation khi cần
        }

        // Chỉ cho phép nhập số
        private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Hiển thị thông báo loading
        private void ShowLoadingMessage(bool isShow)
        {
            // Sử dụng con trỏ chuột để hiển thị đang xử lý
            Mouse.OverrideCursor = isShow ? Cursors.Wait : null;
        }

        // Phương thức hỗ trợ INotifyPropertyChanged
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}