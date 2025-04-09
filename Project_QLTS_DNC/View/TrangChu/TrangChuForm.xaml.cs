using LiveCharts;
using LiveCharts.Wpf;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Project_QLTS_DNC.View.TrangChu
{
    /// <summary>
    /// Interaction logic for TrangChuForm.xaml
    /// </summary>
    public partial class TrangChuForm : UserControl
    {
        public TrangChuForm()
        {
            InitializeComponent();
            LoadBarChart(); // Gọi hàm hiển thị biểu đồ

        }

        //Load hiện thị biểu đồ cột
        private void LoadBarChart()
        {
            assetChart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Số lượng",
                    Values = new ChartValues<int> { 150, 120, 10, 5 },
                    Fill = new SolidColorBrush(Color.FromRgb(0x4D, 0x90, 0xFE))
                }
            };

            assetChart.AxisX.Add(new Axis
            {
                Labels = new[] { "Tổng TS", "Đang dùng", "Bảo trì", "Sửa chữa" },
                Separator = new LiveCharts.Wpf.Separator { Step = 1, IsEnabled = false }
            });

            assetChart.AxisY.Add(new Axis
            {
                Title = "Số lượng",
                LabelFormatter = value => value.ToString()
            });

            assetChart.LegendLocation = LegendLocation.Top;
        }
    }
}
