using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.StyledXmlParser.Jsoup.Nodes;
using Microsoft.Win32;
using Newtonsoft.Json;
using Project_QLTS_DNC.Models.ThongTinCongTy;
using System;
using System.Collections.Generic;
using System.IO;
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
using iText.IO.Font;
using iText.Kernel.Font;

namespace Project_QLTS_DNC.View.CaiDat
{
    /// <summary>
    /// Interaction logic for ThongTinCongTyForm.xaml
    /// </summary>
    public partial class ThongTinCongTyForm : UserControl
    {
        public string PdfPath { get; set; }

        public ThongTinCongTyForm()
        {
            InitializeComponent();

            if (File.Exists(filePath))
            {
                congTy = JsonConvert.DeserializeObject<ThongTinCongTy>(File.ReadAllText(filePath));
                if (!string.IsNullOrEmpty(congTy.LogoPath) && File.Exists(congTy.LogoPath))
                {
                    imgLogo.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(congTy.LogoPath)));
                }

                // ✅ Load dữ liệu từ JSON lên form
                txtTen.Text = congTy.Ten;
                txtMaSoThue.Text = congTy.MaSoThue;
                txtDiaChi.Text = congTy.DiaChi;
                txtSoDienThoai.Text = congTy.SoDienThoai;
                txtEmail.Text = congTy.Email;
                txtNguoiDaiDien.Text = congTy.NguoiDaiDien;
                txtGhiChu.Text = congTy.GhiChu;
                txtPdfPath.Text = congTy.PdfPath;
            }
        }
        private string filePath = "thongtincongty.json";
        private ThongTinCongTy congTy = new ThongTinCongTy();
        private void BtnChonAnh_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Hình ảnh|*.png;*.jpg;*.jpeg";

            if (dialog.ShowDialog() == true)
            {
                string folderPath = "Images";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // ✅ Tạo tên file theo thời gian, tránh ghi đè file đang dùng
                string fileName = $"logo_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                string destPath = System.IO.Path.Combine(folderPath, fileName);

                try
                {
                    // ✅ Đọc ảnh từ file vào memory stream (tránh giữ lock file)
                    BitmapImage bitmap = new BitmapImage();
                    using (FileStream stream = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }

                    // ✅ Gán ảnh cho Image
                    imgLogo.Source = bitmap;

                    // ✅ Copy file về thư mục của ứng dụng
                    File.Copy(dialog.FileName, destPath);

                    // ✅ Lưu đường dẫn vào đối tượng
                    congTy.LogoPath = destPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi chọn ảnh: " + ex.Message);
                }
            }
        }





        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            congTy.Ten = txtTen.Text;
            congTy.MaSoThue = txtMaSoThue.Text;
            congTy.DiaChi = txtDiaChi.Text;
            congTy.SoDienThoai = txtSoDienThoai.Text;
            congTy.Email = txtEmail.Text;
            congTy.NguoiDaiDien = txtNguoiDaiDien.Text;
            congTy.GhiChu = txtGhiChu.Text;

            File.WriteAllText(filePath, JsonConvert.SerializeObject(congTy, Formatting.Indented));
            MessageBox.Show("Đã lưu thông tin!");
        }

        private void InPDF_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = "ThongTinCongTy.pdf",
                Title = "Lưu thông tin công ty dưới dạng PDF"
            };

            if (saveDialog.ShowDialog() == true)
            {
                string pdfPath = saveDialog.FileName;

                using (var writer = new iText.Kernel.Pdf.PdfWriter(pdfPath))
                using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
                {
                    var doc = new iText.Layout.Document(pdf);

                    string fontPath = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");

                    var fontProgram = iText.IO.Font.FontProgramFactory.CreateFont(fontPath);
                    var font = iText.Kernel.Font.PdfFontFactory.CreateFont(fontProgram, iText.IO.Font.PdfEncodings.IDENTITY_H);
                    doc.SetFont(font);

                    if (!string.IsNullOrEmpty(congTy.LogoPath) && File.Exists(congTy.LogoPath))
                    {
                        var imageData = iText.IO.Image.ImageDataFactory.Create(congTy.LogoPath);
                        var logo = new iText.Layout.Element.Image(imageData)
                            .ScaleToFit(100, 100)
                            .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
                        doc.Add(logo);
                    }

                    doc.Add(new iText.Layout.Element.Paragraph("THÔNG TIN CÔNG TY")
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetFontSize(18)
                        .SetBold()
                        .SetMarginBottom(20));

                    doc.Add(new iText.Layout.Element.Paragraph($"Tên công ty: {congTy.Ten}"));
                    doc.Add(new iText.Layout.Element.Paragraph($"Mã số thuế: {congTy.MaSoThue}"));
                    doc.Add(new iText.Layout.Element.Paragraph($"Địa chỉ: {congTy.DiaChi}"));
                    doc.Add(new iText.Layout.Element.Paragraph($"Số điện thoại: {congTy.SoDienThoai}"));
                    doc.Add(new iText.Layout.Element.Paragraph($"Email: {congTy.Email}"));
                    doc.Add(new iText.Layout.Element.Paragraph($"Người đại diện: {congTy.NguoiDaiDien}"));
                    doc.Add(new iText.Layout.Element.Paragraph($"Ghi chú: {congTy.GhiChu}"));

                    doc.Close();
                }

                // ✅ Lưu đường dẫn PDF và hiển thị
                congTy.PdfPath = pdfPath;
                txtPdfPath.Text = pdfPath;
                File.WriteAllText(filePath, JsonConvert.SerializeObject(congTy, Formatting.Indented));

                MessageBox.Show("PDF đã được lưu thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }



        private void BtnMoPDF_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(congTy.PdfPath) && File.Exists(congTy.PdfPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = congTy.PdfPath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show("Không tìm thấy file PDF.");
            }
        }


        private void ThongTinCongTyForm_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    congTy = JsonConvert.DeserializeObject<ThongTinCongTy>(File.ReadAllText(filePath));
                    txtTen.Text = congTy.Ten;
                    txtMaSoThue.Text = congTy.MaSoThue;
                    txtDiaChi.Text = congTy.DiaChi;
                    txtSoDienThoai.Text = congTy.SoDienThoai;
                    txtEmail.Text = congTy.Email;
                    txtNguoiDaiDien.Text = congTy.NguoiDaiDien;
                    txtGhiChu.Text = congTy.GhiChu;

                    if (!string.IsNullOrEmpty(congTy.LogoPath) && File.Exists(congTy.LogoPath))
                    {
                        imgLogo.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(congTy.LogoPath)));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi đọc dữ liệu công ty: " + ex.Message);
                }
            }
        }

        private void ThongTinCongTyForm_Unloaded(object sender, RoutedEventArgs e)
        {
            congTy.Ten = txtTen.Text;
            congTy.MaSoThue = txtMaSoThue.Text;
            congTy.DiaChi = txtDiaChi.Text;
            congTy.SoDienThoai = txtSoDienThoai.Text;
            congTy.Email = txtEmail.Text;
            congTy.NguoiDaiDien = txtNguoiDaiDien.Text;
            congTy.GhiChu = txtGhiChu.Text;

            File.WriteAllText(filePath, JsonConvert.SerializeObject(congTy, Formatting.Indented));
        }





    }
}
