using LiveCharts;
using LiveCharts.Wpf;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Services.QLTaiSanService;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Project_QLTS_DNC.View.TrangChu
{
    public partial class TrangChuForm : UserControl
    {
        public TrangChuForm()
        {
            InitializeComponent();
            // Gọi bất đồng bộ, không chặn UI thread
            _ = LoadBarChartAsync();
        }

        /// <summary>
        /// Lấy dữ liệu từ Supabase, tính thống kê và vẽ biểu đồ
        /// </summary>
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

                // Tính số tài sản "Đang dùng" bằng tổng tài sản trừ đi số tài sản "Đang bảo trì" và tài sản tồn kho
                int inUse = total - daBaoTri - inStock;

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






    }
}
