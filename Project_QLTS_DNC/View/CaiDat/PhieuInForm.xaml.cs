using Newtonsoft.Json;
using Project_QLTS_DNC.Models.ThongTinCongTy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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

namespace Project_QLTS_DNC.View.CaiDat
{
    /// <summary>
    /// Interaction logic for PhieuInForm.xaml
    /// </summary>
    public partial class PhieuInForm : UserControl
    {
        private const string MauPhieuFile = "mauphieu.json";
        private Dictionary<string, string> MauPhieuDict = new();
        public PhieuInForm()
        {
            InitializeComponent();
            cmbLoaiPhieu.ItemsSource = new List<string>
            {
                "Tất cả các phiếu",
                "Phiếu nhập kho",
                "Phiếu xuất kho",
                "Phiếu báo cáo",
                "Phiếu bảo trì"
                // thêm các loại phiếu khác nếu cần
            };
            cmbLoaiPhieu.SelectedIndex = 0;
            LoadMauPhieu();
        }


        private void LoadMauPhieu()
        {
            if (File.Exists(MauPhieuFile))
            {
                try
                {
                    var json = File.ReadAllText(MauPhieuFile);
                    MauPhieuDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi đọc mẫu phiếu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    MauPhieuDict = new Dictionary<string, string>();
                }
            }
            else
            {
                MauPhieuDict = new Dictionary<string, string>();
            }
        }


        private void cmbLoaiPhieu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string loaiPhieu = cmbLoaiPhieu.SelectedItem.ToString();
            if (MauPhieuDict.TryGetValue(loaiPhieu, out string noiDung))
                txtNoiDungMauPhieu.Text = noiDung;
            else if (MauPhieuDict.TryGetValue("Tất cả các phiếu", out string macDinh))
                txtNoiDungMauPhieu.Text = macDinh;
            else
                txtNoiDungMauPhieu.Text = "";
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (cmbLoaiPhieu.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn loại phiếu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string loaiPhieu = cmbLoaiPhieu.SelectedItem.ToString();
            string noiDung = txtNoiDungMauPhieu.Text;

            MauPhieuDict[loaiPhieu] = noiDung;

            // Nếu là tất cả các phiếu thì áp dụng cho tất cả các loại khác
            if (loaiPhieu == "Tất cả các phiếu")
            {
                foreach (var item in cmbLoaiPhieu.Items)
                {
                    string key = item.ToString();
                    if (key != "Tất cả các phiếu")
                        MauPhieuDict[key] = noiDung;
                }
            }

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(MauPhieuDict, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(MauPhieuFile, json);
            MessageBox.Show("Đã lưu mẫu phiếu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnInThu_Click(object sender, RoutedEventArgs e)
        {
            if (cmbLoaiPhieu.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn loại phiếu để in.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var congTy = DocThongTinCongTy();
            if (congTy == null)
                return;

            string loaiPhieu = cmbLoaiPhieu.SelectedItem.ToString();
            string noiDung = txtNoiDungMauPhieu.Text;
            string tenCongTy = congTy.Ten;
            string diaChi = congTy.DiaChi;
            string logoPath = congTy.LogoPath;
            string ngayLap = DateTime.Now.ToString("dd/MM/yyyy");

            FlowDocument doc = new FlowDocument
            {
                PagePadding = new Thickness(50),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 14
            };

            // ===== HEADER =====
            Table headerTable = new Table();
            headerTable.Columns.Add(new TableColumn() { Width = new GridLength(100) });
            headerTable.Columns.Add(new TableColumn());
            TableRow headerRow = new TableRow();
            TableRowGroup trg = new TableRowGroup();

            UIElement logoElement;

            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
            {
                BitmapImage logoBitmap = new BitmapImage(new Uri(System.IO.Path.GetFullPath(logoPath)));
                Image logoImg = new Image
                {
                    Source = logoBitmap,
                    Width = 80,
                    Height = 80
                };
                logoElement = logoImg;
            }
            else
            {
                logoElement = new Border
                {
                    Width = 80,
                    Height = 80,
                    Background = Brushes.Gray,
                    Child = new TextBlock
                    {
                        Text = "Logo",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = Brushes.White
                    }
                };
            }

            headerRow.Cells.Add(new TableCell(new BlockUIContainer(logoElement)));

            StackPanel sp = new StackPanel();
            sp.Children.Add(new TextBlock { Text = tenCongTy, FontWeight = FontWeights.Bold, FontSize = 16 });
            sp.Children.Add(new TextBlock { Text = diaChi, FontSize = 12 });
            headerRow.Cells.Add(new TableCell(new BlockUIContainer(sp)));

            trg.Rows.Add(headerRow);
            headerTable.RowGroups.Add(trg);
            doc.Blocks.Add(new Section { Blocks = { headerTable } });

            // ===== TIÊU ĐỀ PHIẾU =====
            doc.Blocks.Add(new Paragraph(new Run(loaiPhieu.ToUpper()))
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 30, 0, 20)
            });

            // ===== NỘI DUNG PHIẾU =====
            doc.Blocks.Add(new Paragraph(new Run(noiDung))
            {
                TextAlignment = TextAlignment.Left,
                Margin = new Thickness(0, 0, 0, 30)
            });

            // ===== FOOTER =====
            Table footerTable = new Table();
            footerTable.Columns.Add(new TableColumn());
            footerTable.Columns.Add(new TableColumn());
            TableRow footerRow = new TableRow();

            footerRow.Cells.Add(new TableCell(new Paragraph(new Run($"Ngày lập phiếu: {ngayLap}"))) { TextAlignment = TextAlignment.Left });
            footerRow.Cells.Add(new TableCell(new Paragraph(new Run("Người lập phiếu\n\n(Ký tên)"))) { TextAlignment = TextAlignment.Right });

            footerTable.RowGroups.Add(new TableRowGroup { Rows = { footerRow } });
            doc.Blocks.Add(footerTable);

            // ===== IN =====
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                IDocumentPaginatorSource idpSource = doc;
                printDialog.PrintDocument(idpSource.DocumentPaginator, "In thử mẫu phiếu");
            }
        }

        private ThongTinCongTy DocThongTinCongTy()
        {
            string filePath = "thongtincongty.json";
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Chưa cấu hình thông tin công ty.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ThongTinCongTy>(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi đọc thông tin công ty: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }


    }
}
