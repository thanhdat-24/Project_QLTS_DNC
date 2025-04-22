using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Models.ToaNha;
using Supabase;

namespace Project_QLTS_DNC.View.QuanLyToanNha
{
    public partial class suc_chua_phong_nhom : Window
    {
        private Supabase.Client _client;
        private readonly int _maPhong;
        private List<NhomTaiSan> _dsNhomTaiSan = new();
        private ObservableCollection<SucChuaPhongNhomTam> _danhSachTam = new();
        private SucChuaPhongNhomTam _selectedItem; // dòng đang chọn để sửa

        public suc_chua_phong_nhom(int maPhong)
        {
            InitializeComponent();
            _maPhong = maPhong;
            Loaded += suc_chua_phong_nhom_Loaded;
        }

        private async void suc_chua_phong_nhom_Loaded(object sender, RoutedEventArgs e)
        {
            txtMaPhong.Text = _maPhong.ToString();
            await InitializeSupabaseAsync();
            await LoadNhomTaiSanAsync();     // ⚡ Load nhóm tài sản trước
            await LoadSucChuaHienTaiAsync(); // ⚡ Load danh sách sức chứa hiện tại
        }

        private async Task InitializeSupabaseAsync()
        {
            string supabaseUrl = "https://hoybfwnugefnpctgghha.supabase.co";
            string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhveWJmd251Z2VmbnBjdGdnaGhhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDQxMDQ4OTEsImV4cCI6MjA1OTY4MDg5MX0.KxNfiOUFXHGgqZf3b3xOk6BR4sllMZG_-W-y_OPUwCI";

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false,
                AutoRefreshToken = false
            };

            _client = new Supabase.Client(supabaseUrl, supabaseKey, options);
            await _client.InitializeAsync();
        }

        private async Task LoadNhomTaiSanAsync()
        {
            try
            {
                var result = await _client.From<NhomTaiSan>().Get();
                _dsNhomTaiSan = result.Models ?? new List<NhomTaiSan>();
                cboNhomTS.ItemsSource = _dsNhomTaiSan;
                cboNhomTS.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải nhóm tài sản: " + ex.Message);
            }
        }

        private async Task LoadSucChuaHienTaiAsync()
        {
            try
            {
                var result = await _client.From<PhongNhomTS>()
                    .Where(x => x.MaPhong == _maPhong)
                    .Get();

                if (result.Models != null)
                {
                    _danhSachTam.Clear();
                    foreach (var item in result.Models)
                    {
                        var nhom = _dsNhomTaiSan.FirstOrDefault(x => x.MaNhomTS == item.MaNhomTS);
                        string tenNhom = nhom != null ? nhom.TenNhom : "Không xác định";

                        _danhSachTam.Add(new SucChuaPhongNhomTam
                        {
                            MaNhomTS = item.MaNhomTS,
                            TenNhomTaiSan = tenNhom,
                            SucChua = item.SucChua
                        });
                    }

                    gridSucChua.ItemsSource = null;
                    gridSucChua.ItemsSource = _danhSachTam;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải sức chứa hiện tại: " + ex.Message);
            }
        }

        private void gridSucChua_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridSucChua.SelectedItem is SucChuaPhongNhomTam selected)
            {
                _selectedItem = selected;
                cboNhomTS.SelectedValue = selected.MaNhomTS;
                txtSucChua.Text = selected.SucChua.ToString();
            }
        }


        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            if (cboNhomTS.SelectedItem is not NhomTaiSan nhom)
            {
                MessageBox.Show("Vui lòng chọn nhóm tài sản!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtSucChua.Text.Trim(), out int sucChua) || sucChua <= 0)
            {
                MessageBox.Show("Vui lòng nhập sức chứa hợp lệ!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_selectedItem != null)
            {
                // 👇 Nếu đang chọn dòng, cập nhật dòng đó
                _selectedItem.MaNhomTS = nhom.MaNhomTS;
                _selectedItem.TenNhomTaiSan = nhom.TenNhom;
                _selectedItem.SucChua = sucChua;
                gridSucChua.Items.Refresh(); // Cập nhật lại DataGrid

                // Reset chọn
                _selectedItem = null;
                gridSucChua.SelectedItem = null;
            }
            else
            {
                // 👇 Nếu không chọn dòng nào thì thêm mới
                if (_danhSachTam.Any(x => x.MaNhomTS == nhom.MaNhomTS))
                {
                    MessageBox.Show("Nhóm tài sản này đã tồn tại trong danh sách!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var item = new SucChuaPhongNhomTam
                {
                    MaNhomTS = nhom.MaNhomTS,
                    TenNhomTaiSan = nhom.TenNhom,
                    SucChua = sucChua
                };
                _danhSachTam.Add(item);
            }

            // Refresh danh sách
            gridSucChua.ItemsSource = null;
            gridSucChua.ItemsSource = _danhSachTam;

            // Reset ô nhập
            cboNhomTS.SelectedIndex = -1;
            txtSucChua.Clear();
        }

        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (_danhSachTam.Count == 0)
            {
                MessageBox.Show("Chưa có dữ liệu để lưu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // XÓA toàn bộ dữ liệu cũ của phòng
                var danhSachCu = await _client.From<PhongNhomTS>()
                    .Where(x => x.MaPhong == _maPhong)
                    .Get();

                if (danhSachCu.Models != null)
                {
                    foreach (var item in danhSachCu.Models)
                    {
                        await _client.From<PhongNhomTS>().Delete(item);
                    }
                }

                // INSERT lại danh sách mới
                foreach (var item in _danhSachTam)
                {
                    var model = new PhongNhomTS
                    {
                        MaPhong = _maPhong,
                        MaNhomTS = item.MaNhomTS,
                        SucChua = item.SucChua
                    };

                    await _client.From<PhongNhomTS>().Insert(model);
                }

                MessageBox.Show("Đã lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu: " + ex.Message);
            }
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class SucChuaPhongNhomTam
    {
        public int MaNhomTS { get; set; }
        public string TenNhomTaiSan { get; set; }
        public int SucChua { get; set; }
    }
}
