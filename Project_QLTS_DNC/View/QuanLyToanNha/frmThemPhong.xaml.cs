using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Services.QLToanNha;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Supabase;
using Supabase.Postgrest;



namespace Project_QLTS_DNC.View.QuanLyToanNha
{
    public partial class frmThemPhong : Window
    {
        public Phong PhongMoi { get; private set; }
        private List<Tang> DanhSachTang = new();

        private readonly Phong _phongHienTai; // dùng để nhận biết đang sửa

        public frmThemPhong()
        {
            InitializeComponent();
            Title = "Thêm phòng mới";
            Loaded += async (s, e) => await LoadTang();
          
        }

        public frmThemPhong(Phong phongHienTai)
        {
            InitializeComponent();
            Title = "Chỉnh sửa phòng";
            _phongHienTai = phongHienTai;

            Loaded += async (s, e) =>
            {
                await LoadTang();

                // Gán dữ liệu phòng hiện tại lên form
                cboTenTang.SelectedValue = phongHienTai.MaTang;
                txtTenP.Text = phongHienTai.TenPhong;
                txtSucChuaP.Text = phongHienTai.SucChua.ToString();
                txtMoTaP.Text = phongHienTai.MoTaPhong;
            };
        }

        private async Task LoadTang()
        {
            try
            {
                DanhSachTang = (await TangService.LayDanhSachTangAsync()).ToList();
                cboTenTang.ItemsSource = DanhSachTang;
                cboTenTang.DisplayMemberPath = "TenTang";
                cboTenTang.SelectedValuePath = "MaTang";
                // ✅ Chỉ định rõ không chọn gì cả
                cboTenTang.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách tầng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Supabase.Client _client;

        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (cboTenTang.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn tầng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTenP.Text))
            {
                MessageBox.Show("Vui lòng nhập tên phòng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtSucChuaP.Text, out int sucChua) || sucChua <= 0)
            {
                MessageBox.Show("Sức chứa phải là số nguyên dương!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Khởi tạo kết nối Supabase nếu cần
            if (_client == null)
            {
                string supabaseUrl = "https://hoybfwnugefnpctgghha.supabase.co";
                string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhveWJmd251Z2VmbnBjdGdnaGhhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDQxMDQ4OTEsImV4cCI6MjA1OTY4MDg5MX0.KxNfiOUFXHGgqZf3b3xOk6BR4sllMZG_-W-y_OPUwCI";

                var options = new SupabaseOptions { AutoConnectRealtime = false };
                _client = new Supabase.Client(supabaseUrl, supabaseKey, options);
                await _client.InitializeAsync();
            }

            PhongMoi = new Phong
            {
                MaTang = (int)cboTenTang.SelectedValue,
                TenPhong = txtTenP.Text.Trim(),
                SucChua = sucChua,
                MoTaPhong = txtMoTaP.Text.Trim()
            };
           
                

            var response = await _client
        .From<Phong>()
        .Insert(new List<Phong> { PhongMoi }, new Supabase.Postgrest.QueryOptions
        {
            Returning = Supabase.Postgrest.QueryOptions.ReturnType.Representation
        });

            PhongMoi = response.Models.FirstOrDefault();

            if (PhongMoi != null && PhongMoi.MaPhong > 0)
            {
                MessageBox.Show("Đã lưu phòng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // 👉 MỞ FORM NHẬP SỨC CHỨA và TRUYỀN MÃ PHÒNG
                var formSucChua = new suc_chua_phong_nhom(PhongMoi.MaPhong);
                formSucChua.ShowDialog();

                // 👉 Có thể cập nhật thông tin bổ sung ở đây nếu cần

                this.Close();
            }
            else
            {
                MessageBox.Show("Không lấy được mã phòng sau khi lưu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
