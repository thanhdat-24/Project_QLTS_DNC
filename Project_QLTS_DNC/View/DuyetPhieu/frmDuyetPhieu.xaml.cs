using Project_QLTS_DNC.Models.DuyetPhieu;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.View.DuyetPhieu.ChiTietPhieu;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Supabase.Postgrest.Constants;

namespace Project_QLTS_DNC.View.DuyetPhieu
{
    public partial class frmDuyetPhieu : UserControl
    {
        private ObservableCollection<PhieuCanDuyet> DanhSachTatCaPhieu = new();

        public frmDuyetPhieu()
        {
            InitializeComponent();
            LoadThongKePhieu();
        }

        private async void dgPhieuCanDuyet_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDuLieuTatCaPhieuAsync(); // Tải mặc định phiếu nhập
        }

        // ✅ Load toàn bộ phiếu nhập từ Supabase
        private async Task LoadDuLieuTatCaPhieuAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                DanhSachTatCaPhieu.Clear();

                var phieuNhap = await client.From<PhieuNhapKhoInput>().Get();

                foreach (var pn in phieuNhap.Models)
                {
                    DanhSachTatCaPhieu.Add(new PhieuCanDuyet
                    {
                        MaPhieu = $"PN{pn.MaPhieuNhap}",
                        NgayTaoPhieu = pn.NgayNhap,
                        TrangThai = string.IsNullOrEmpty(pn.TrangThai) ? "Chưa duyệt" : pn.TrangThai,
                        LoaiPhieu = "Phiếu nhập"
                    });
                }

                dgPhieuCanDuyet.ItemsSource = DanhSachTatCaPhieu;

                // ✅ Tự động chọn dòng đầu tiên
                if (DanhSachTatCaPhieu.Count > 0)
                    dgPhieuCanDuyet.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi load phiếu nhập: {ex.Message}");
            }
        }

        // ✅ Khi chọn loại phiếu từ ComboBox
        private async void cboLoaiPhieu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboLoaiPhieu.SelectedItem is ComboBoxItem selectedItem)
            {
                string loai = selectedItem.Content.ToString();

                if (loai == "Tất cả" || loai == "Phiếu nhập")
                {
                    await LoadDuLieuTatCaPhieuAsync(); // Load lại toàn bộ phiếu nhập
                }
                else
                {
                    // Nếu sau này thêm phiếu xuất, bảo trì... thì xử lý lọc tại đây
                    dgPhieuCanDuyet.ItemsSource = DanhSachTatCaPhieu
                        .Where(p => p.LoaiPhieu == loai)
                        .ToList();
                }
            }
        }

        // ✅ Nút "Xem chi tiết"
        private void btnDetail_Click(object sender, RoutedEventArgs e)
        {
            var frm = new frmChiTietPhieuNhap();
            var window = new Window
            {
                Title = "Chi tiết phiếu nhập",
                Content = frm,
                Width = 1000,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            window.ShowDialog();
        }

        // ✅ Load thống kê phiếu vào 3 TextBlock
        private async void LoadThongKePhieu()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<PhieuNhapKhoInput>().Get();
                var danhSach = response.Models;

                int daDuyet = danhSach.Count(p => p.TrangThai == "Đã duyệt");
                int tuChoi = danhSach.Count(p => p.TrangThai == "Từ chối duyệt");
                int canDuyet = danhSach.Count(p => string.IsNullOrEmpty(p.TrangThai) || p.TrangThai == null);

                txtTongPhieuCanDuyet.Text = canDuyet.ToString();
                txtTongPhieuDaDuyet.Text = daDuyet.ToString();
                txtTongPhieuTuChoi.Text = tuChoi.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load thống kê phiếu: " + ex.Message);
            }
        }
    }
}