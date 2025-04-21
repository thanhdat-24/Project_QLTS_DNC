using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Project_QLTS_DNC.Models.KiemKe;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Services;
using Supabase;

namespace Project_QLTS_DNC.View.KiemKe
{
    /// <summary>
    /// Interaction logic for DotKiemKeInput.xaml
    /// </summary>
    public partial class DotKiemKeInput : Window
    {
        private Supabase.Client _client;
        private List<NhanVienModel> _dsNhanVien = new();
        private DotKiemKe? _editingDot;
        public DotKiemKeInput()
        {
            InitializeComponent();
            Loaded += DotKiemKeInput_Loaded;
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


        public DotKiemKeInput(DotKiemKe? dotKiemKe = null)
        {
            InitializeComponent();
            _editingDot = dotKiemKe;
            Loaded += DotKiemKeInput_Loaded;
        }

        private async void DotKiemKeInput_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            await LoadNhanVienAsync();

            if (_editingDot != null)
            {
                txtTenDot.Text = _editingDot.TenDot;
                dpNgayBatDau.SelectedDate = _editingDot.NgayBatDau;
                dpNgayKetThuc.SelectedDate = _editingDot.NgayKetThuc;
                txtGhiChu.Text = _editingDot.GhiChu;
                cboNhanVien.SelectedValue = _editingDot.MaNV;
            }
        }      
        private async Task LoadNhanVienAsync()
        {
            var result = await _client.From<NhanVienModel>().Get();
            _dsNhanVien = result.Models;

            cboNhanVien.ItemsSource = _dsNhanVien;
            cboNhanVien.DisplayMemberPath = "TenNV";
            cboNhanVien.SelectedValuePath = "MaNV";
        }


        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTenDot.Text))
                {
                    MessageBox.Show("Vui lòng nhập tên đợt kiểm kê.");
                    return;
                }

                var client = await SupabaseService.GetClientAsync();

                DotKiemKe dot = _editingDot ?? new DotKiemKe();

                dot.TenDot = txtTenDot.Text.Trim();
                dot.NgayBatDau = dpNgayBatDau.SelectedDate;
                dot.NgayKetThuc = dpNgayKetThuc.SelectedDate;
                dot.MaNV = cboNhanVien.SelectedValue != null ? (int)cboNhanVien.SelectedValue : 0;
                dot.GhiChu = txtGhiChu.Text.Trim();

                if (_editingDot == null)
                {
                    var response = await client.From<DotKiemKe>().Insert(dot);
                    if (response.Models.Count > 0)
                    {
                        MessageBox.Show("Thêm đợt kiểm kê thành công!");

                        int maDotKiemKeMoi = response.Models.First().MaDotKiemKe;
                        var formKiemKe = new KiemKeTaiSan(maDotKiemKeMoi);
                        formKiemKe.Show(); // Mở form KiemKeTaiSan

                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Không thể thêm mới.");
                    }
                }
                else
                {
                    var all = await client.From<DotKiemKe>().Get();
                    var existing = all.Models.FirstOrDefault(x => x.MaDotKiemKe == dot.MaDotKiemKe);

                    if (existing != null)
                    {
                        existing.TenDot = dot.TenDot;
                        existing.NgayBatDau = dot.NgayBatDau;
                        existing.NgayKetThuc = dot.NgayKetThuc;
                        existing.MaNV = dot.MaNV;
                        existing.GhiChu = dot.GhiChu;

                        var update = await client.From<DotKiemKe>().Update(existing);

                        if (update.Models.Count > 0)
                        {
                            MessageBox.Show("Cập nhật thành công!");
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Không thể cập nhật.");
                        }
                    }
                }
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

}
