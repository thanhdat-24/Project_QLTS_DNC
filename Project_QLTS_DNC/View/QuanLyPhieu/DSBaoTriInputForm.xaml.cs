using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.BaoTri;
using BaoTriKiemKeTaiSan = Project_QLTS_DNC.Models.BaoTri.KiemKeTaiSan;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.View.KiemKe;
using Supabase;
using Supabase.Gotrue;
using Supabase.Postgrest;
// Bỏ dòng này vì đã có trước đó: using Supabase.Postgrest.Models;
using KiemKeDotKiemKe = Project_QLTS_DNC.Models.KiemKe.DotKiemKe;
using static Supabase.Postgrest.Constants;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    public partial class DSBaoTriInputForm : Window, INotifyPropertyChanged
    {
        // Collection để lưu trữ dữ liệu cho các ComboBox
        private ObservableCollection<KiemKeDotKiemKe> _danhSachDotKiemKe;
        private ObservableCollection<TaiSanModel> _danhSachTaiSan;
        private ObservableCollection<Phong> _danhSachPhong;
        // Client để kết nối với Supabase
        private readonly Supabase.Client _supabaseClient;
        // Đối tượng kiểm kê hiện tại
        private KiemKeTaiSan _kiemKeTaiSan;
        // Chế độ (thêm mới hay cập nhật)
        private bool _isUpdateMode;
        // Properties với INotifyPropertyChanged để binding
        public event PropertyChangedEventHandler PropertyChanged;

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

        public KiemKeTaiSan KiemKeTaiSan
        {
            get => _kiemKeTaiSan;
            set
            {
                _kiemKeTaiSan = value;
                OnPropertyChanged(nameof(KiemKeTaiSan));
            }
        }

        // Constructor cho trường hợp tạo mới phiếu bảo trì (không có tài sản nào được chọn)
        public DSBaoTriInputForm(Supabase.Client supabaseClient)
        {
            InitializeComponent();
            _supabaseClient = supabaseClient;
            _isUpdateMode = false;
            // Khởi tạo collections
            DanhSachDotKiemKe = new ObservableCollection<KiemKeDotKiemKe>();
            DanhSachTaiSan = new ObservableCollection<TaiSanModel>();
            DanhSachPhong = new ObservableCollection<Phong>();
            // Khởi tạo đối tượng kiểm kê tài sản mới
            KiemKeTaiSan = new KiemKeTaiSan();
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
        }

        // Constructor cho trường hợp cập nhật
        public DSBaoTriInputForm(Supabase.Client supabaseClient, KiemKeTaiSan kiemKeTaiSan)
        {
            InitializeComponent();
            _supabaseClient = supabaseClient;
            _isUpdateMode = true;
            // Khởi tạo collections
            DanhSachDotKiemKe = new ObservableCollection<KiemKeDotKiemKe>();
            DanhSachTaiSan = new ObservableCollection<TaiSanModel>();
            DanhSachPhong = new ObservableCollection<Phong>();
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
                var dotkiemkeTable = _supabaseClient.Postgrest.Table<KiemKeDotKiemKe>();
                var dotkiemkeResponse = await dotkiemkeTable.Get();
                var dsDotKiemKe = dotkiemkeResponse.Models;
                // Tạo danh sách đối tượng hiển thị đơn giản
                var dsDotKiemKeBinding = dsDotKiemKe.Select(d => new ComboBoxItem
                {
                    ID = d.MaDotKiemKe,
                    DisplayText = d.TenDot ?? $"Đợt kiểm kê {d.MaDotKiemKe}"
                }).ToList();
                // Gán danh sách cho ComboBox
                cboDotKiemKe.ItemsSource = dsDotKiemKeBinding;
                cboDotKiemKe.DisplayMemberPath = "DisplayText"; // Chỉ hiển thị tên
                cboDotKiemKe.SelectedValuePath = "ID";          // Lấy giá trị ID
                // Tương tự với tài sản
                var taisanTable = _supabaseClient.Postgrest.Table<TaiSanModel>();
                var taisanResponse = await taisanTable.Get();
                var dsTaiSan = taisanResponse.Models;
                var dsTaiSanBinding = dsTaiSan.Select(t => new ComboBoxItem
                {
                    ID = t.MaTaiSan,
                    DisplayText = t.TenTaiSan ?? $"Tài sản {t.MaTaiSan}"
                }).ToList();
                cboTaiSan.ItemsSource = dsTaiSanBinding;
                cboTaiSan.DisplayMemberPath = "DisplayText";
                cboTaiSan.SelectedValuePath = "ID";
                // Tương tự với phòng
                var phongTable = _supabaseClient.Postgrest.Table<Phong>();
                var phongResponse = await phongTable.Get();
                var dsPhong = phongResponse.Models;
                var dsPhongBinding = dsPhong.Select(p => new ComboBoxItem
                {
                    ID = p.MaPhong,
                    DisplayText = p.TenPhong ?? $"Phòng {p.MaPhong}"
                }).ToList();
                cboPhong.ItemsSource = dsPhongBinding;
                cboPhong.DisplayMemberPath = "DisplayText";
                cboPhong.SelectedValuePath = "ID";

                // Lưu lại danh sách gốc để sử dụng sau này
                DanhSachDotKiemKe = new ObservableCollection<KiemKeDotKiemKe>(dsDotKiemKe);
                DanhSachTaiSan = new ObservableCollection<TaiSanModel>(dsTaiSan);
                DanhSachPhong = new ObservableCollection<Phong>(dsPhong);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Lớp hỗ trợ hiển thị trong ComboBox
        public class ComboBoxItem
        {
            public int ID { get; set; }
            public string DisplayText { get; set; }
            public override string ToString()
            {
                return DisplayText;
            }
        }

        // Thiết lập dữ liệu cho form
        private void SetupFormData()
        {
            if (_isUpdateMode)
            {
                // Chế độ cập nhật - Đặt tiêu đề và giá trị từ đối tượng hiện tại
                Title = "Cập nhật Phiếu Bảo Trì";
                txtMaKiemKe.Text = KiemKeTaiSan.MaKiemKeTS.ToString();
                // Tìm và chọn các giá trị tương ứng trong ComboBox
                if (KiemKeTaiSan.MaDotKiemKe.HasValue && cboDotKiemKe.Items.Count > 0)
                {
                    var dotKiemKeItem = cboDotKiemKe.Items.Cast<ComboBoxItem>()
                        .FirstOrDefault(x => x.ID == KiemKeTaiSan.MaDotKiemKe.Value);
                    if (dotKiemKeItem != null)
                        cboDotKiemKe.SelectedItem = dotKiemKeItem;
                }
                if (KiemKeTaiSan.MaTaiSan.HasValue && cboTaiSan.Items.Count > 0)
                {
                    var taiSanItem = cboTaiSan.Items.Cast<ComboBoxItem>()
                        .FirstOrDefault(x => x.ID == KiemKeTaiSan.MaTaiSan.Value);
                    if (taiSanItem != null)
                        cboTaiSan.SelectedItem = taiSanItem;
                }
                if (KiemKeTaiSan.MaPhong.HasValue && cboPhong.Items.Count > 0)
                {
                    var phongItem = cboPhong.Items.Cast<ComboBoxItem>()
                        .FirstOrDefault(x => x.ID == KiemKeTaiSan.MaPhong.Value);
                    if (phongItem != null)
                        cboPhong.SelectedItem = phongItem;
                }
                // Đặt giá trị cho tình trạng
                foreach (System.Windows.Controls.ComboBoxItem item in cboTinhTrang.Items)
                {
                    if (item.Content.ToString() == KiemKeTaiSan.TinhTrang)
                    {
                        cboTinhTrang.SelectedItem = item;
                        break;
                    }
                }
                txtViTriThucTe.Text = KiemKeTaiSan.ViTriThucTe;
                txtGhiChu.Text = KiemKeTaiSan.GhiChu;
            }
            else
            {
                // Chế độ thêm mới - Tạo mã kiểm kê mới
                Title = "Thêm Phiếu Bảo Trì";
                GenerateNewId();
                // Thiết lập giá trị mặc định cho ComboBox TinhTrang (chọn item đầu tiên)
                if (cboTinhTrang.Items.Count > 0)
                    cboTinhTrang.SelectedIndex = 0;
            }
        }

        // Tạo mã kiểm kê mới
        private async void GenerateNewId()
        {
            try
            {
                // Lấy mã kiểm kê lớn nhất hiện tại và tăng lên 1
                var table = _supabaseClient.Postgrest.Table<KiemKeTaiSan>();
                // Tạo query
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
                var table = _supabaseClient.Postgrest.Table<KiemKeTaiSan>();
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
            if (cboTaiSan.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn tài sản!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                cboTaiSan.Focus();
                return false;
            }
            if (cboPhong.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn phòng!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                cboPhong.Focus();
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
            // Lấy giá trị từ các ComboBox
            if (cboDotKiemKe.SelectedItem is ComboBoxItem dotKiemKe)
            {
                KiemKeTaiSan.MaDotKiemKe = dotKiemKe.ID;
            }
            if (cboTaiSan.SelectedItem is ComboBoxItem taiSan)
            {
                KiemKeTaiSan.MaTaiSan = taiSan.ID;
            }
            if (cboPhong.SelectedItem is ComboBoxItem phong)
            {
                KiemKeTaiSan.MaPhong = phong.ID;
            }
            // Lấy giá trị tình trạng
            if (cboTinhTrang.SelectedItem is System.Windows.Controls.ComboBoxItem tinhTrang)
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