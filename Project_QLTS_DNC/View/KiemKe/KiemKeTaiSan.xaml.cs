using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models.BanGiaoTaiSan;
using Project_QLTS_DNC.Models.KiemKe;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Project_QLTS_DNC.View.KiemKe
{
    public partial class KiemKeTaiSan : Window
    {
        private Supabase.Client _client;
        private List<ChiTietBanGiaoDTO> _taiSanTheoPhong = new();
        private int _maDotKiemKe; // lưu mã đợt kiểm kê

        public KiemKeTaiSan()
        {
            InitializeComponent();
            Loaded += KiemKeTaiSan_Loaded;
        }

        public KiemKeTaiSan(int maDotKiemKe)
        {
            InitializeComponent();
            _maDotKiemKe = maDotKiemKe;
            Loaded += KiemKeTaiSan_Loaded;
        }

        private async void KiemKeTaiSan_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            await LoadTenDotKiemKeAsync(); // Load tên đợt kiểm kê vào TextBox
            await LoadPhongAsync();
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

        private async Task LoadTenDotKiemKeAsync()
        {
            var response = await _client
                .From<DotKiemKe>()
                .Where(x => x.MaDotKiemKe == _maDotKiemKe)
                .Get();

            if (response.Models.Any())
            {
                var dot = response.Models.First();
                txtTenDotKiemKe.Text = dot.TenDot;
            }
        }

        private async Task LoadPhongAsync()
        {
            var response = await _client
                .From<BanGiaoTaiSanModel>()
                .Where(x => x.TrangThai == true)
                .Get();

            var phongIds = response.Models
                .Where(x => x.MaPhong.HasValue)
                .Select(x => x.MaPhong.Value)
                .Distinct()
                .ToList();

            if (phongIds.Count > 0)
            {
                var phongList = phongIds.Select(maPhong => new PhongBanGiaoFilter
                {
                    MaPhong = maPhong,
                    TenPhong = $"Phòng {maPhong}"
                }).ToList();

                cboPhong.ItemsSource = phongList;
                cboPhong.DisplayMemberPath = "TenPhong";
                cboPhong.SelectedValuePath = "MaPhong";
            }
        }

        private async void cboPhong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboPhong.SelectedItem is PhongBanGiaoFilter selectedPhong)
            {
                await LoadTaiSanTheoPhongAsync(selectedPhong.MaPhong);
            }
        }

        public List<string> TinhTrangOptions { get; set; } = new List<string>
            {
                "Tốt",
                "Cần bảo trì",
                "Hỏng"
            };
        private void dataGridTaiSan_CurrentCellChanged(object sender, EventArgs e)
        {
            if (dataGridTaiSan.CommitEdit(DataGridEditingUnit.Row, true))
            {
                dataGridTaiSan.CommitEdit(); // ép DataGrid commit giá trị ComboBox mới
            }
        }



        private async Task LoadTaiSanTheoPhongAsync(int maPhong)
        {
            dataGridTaiSan.ItemsSource = null;
            dataGridTaiSan.Items.Clear(); // ✨ Cái này cực kỳ quan trọng

            // Load danh sách tài sản theo phòng...
            var banGiaoList = await _client
                .From<BanGiaoTaiSanModel>()
                .Where(x => x.MaPhong == maPhong && x.TrangThai == true)
                .Get();

            if (!banGiaoList.Models.Any())
                return;

            var listMaBanGiaoTS = banGiaoList.Models
                .Select(x => x.MaBanGiaoTS)
                .ToList();

            List<ChiTietBanGiaoModel> allChiTiet = new List<ChiTietBanGiaoModel>();

            foreach (var maBanGiaoTS in listMaBanGiaoTS)
            {
                var chiTietList = await _client
                    .From<ChiTietBanGiaoModel>()
                    .Where(x => x.MaBanGiaoTS == maBanGiaoTS)
                    .Get();

                allChiTiet.AddRange(chiTietList.Models);
            }

            _taiSanTheoPhong = allChiTiet.Select(ct => new ChiTietBanGiaoDTO
            {
                MaChiTietBG = ct.MaChiTietBG,
                MaBanGiaoTS = ct.MaBanGiaoTS,
                MaTaiSan = ct.MaTaiSan,
                ViTriTS = ct.ViTriTS,
                GhiChu = ct.GhiChu,
                TenTaiSan = $"Tài sản {ct.MaTaiSan}",
                TinhTrang = "Tốt" // Mặc định tốt nếu bạn muốn
            }).ToList();

            dataGridTaiSan.ItemsSource = _taiSanTheoPhong;
        }


        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (cboPhong.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn phòng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int maPhong = (int)cboPhong.SelectedValue;

            var kiemKeList = _taiSanTheoPhong.Select(item => new Models.KiemKe.KiemKeTaiSanModel
            {
                MaDotKiemKe = _maDotKiemKe, // <-- lấy từ constructor
                MaTaiSan = item.MaTaiSan,
                MaPhong = maPhong,
                TinhTrang = item.TinhTrang, // sau này nếu bạn cho phép chỉnh sửa
                ViTriThucTe = item.ViTriTS.ToString(),
                GhiChu = item.GhiChu
            }).ToList();

            foreach (var kk in kiemKeList)
            {
                await _client.From<Models.KiemKe.KiemKeTaiSanModel>().Insert(kk);
            }

            MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
