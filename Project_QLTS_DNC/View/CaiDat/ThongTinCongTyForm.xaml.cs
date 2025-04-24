using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Win32;
using Newtonsoft.Json;
using Project_QLTS_DNC.Models.ThongTinCongTy;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using iText.IO.Font;
using iText.Kernel.Font;
using System.Diagnostics;

// Alias để tránh bị mập mờ
using PdfImage = iText.Layout.Element.Image;
using PdfAlignment = iText.Layout.Properties.HorizontalAlignment;
using PdfTextAlignment = iText.Layout.Properties.TextAlignment;

namespace Project_QLTS_DNC.View.CaiDat
{
    public partial class ThongTinCongTyForm : UserControl
    {
        private string selectedImagePath = string.Empty;
        private string filePath = "thongtincongty.json";
        private ThongTinCongTy congTy = new ThongTinCongTy();

        public ThongTinCongTyForm()
        {
            InitializeComponent();
            LoadThongTinCongTy();
        }

        private void LoadThongTinCongTy()
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
                    MessageBox.Show("Lỗi khi đọc thông tin công ty: " + ex.Message);
                }
            }
        }

        private void BtnChonAnh_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (dlg.ShowDialog() == true)
            {
                selectedImagePath = dlg.FileName;
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string logoDirectory = System.IO.Path.Combine(appDirectory, "Resources", "Logo");
                Directory.CreateDirectory(logoDirectory);

                string fileName = System.IO.Path.GetFileName(selectedImagePath);
                string newImagePath = System.IO.Path.Combine(logoDirectory, fileName);
                File.Copy(selectedImagePath, newImagePath, true);

                congTy.LogoPath = newImagePath;
                imgLogo.Source = new BitmapImage(new Uri(newImagePath));
                imgLogo.Source = new BitmapImage(new Uri(selectedImagePath));
            }
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra số điện thoại và mã số thuế có phải toàn số không
            if (!txtMaSoThue.Text.All(char.IsDigit))
            {
                MessageBox.Show("Mã số thuế chỉ được nhập số!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMaSoThue.Focus();
                return;
            }

            if (!txtSoDienThoai.Text.All(char.IsDigit))
            {
                MessageBox.Show("Số điện thoại chỉ được nhập số!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtSoDienThoai.Focus();
                return;
            }

            // Kiểm tra email hợp lệ
            if (!IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Email không đúng định dạng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return;
            }

            // Nếu mọi thứ hợp lệ, tiếp tục lưu
            congTy.Ten = txtTen.Text;
            congTy.MaSoThue = txtMaSoThue.Text;
            congTy.DiaChi = txtDiaChi.Text;
            congTy.SoDienThoai = txtSoDienThoai.Text;
            congTy.Email = txtEmail.Text;
            congTy.NguoiDaiDien = txtNguoiDaiDien.Text;
            congTy.GhiChu = txtGhiChu.Text;

            if (!string.IsNullOrEmpty(selectedImagePath))
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string logoDirectory = Path.Combine(appDirectory, "Resources", "Logo");
                Directory.CreateDirectory(logoDirectory);

                string fileName = Path.GetFileName(selectedImagePath);
                string newImagePath = Path.Combine(logoDirectory, fileName);
                congTy.LogoPath = newImagePath;
                imgLogo.Source = new BitmapImage(new Uri(newImagePath));

                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.UpdateLogo(congTy.LogoPath);
            }

            File.WriteAllText(filePath, JsonConvert.SerializeObject(congTy, Formatting.Indented));
            File.WriteAllText("logo_path.txt", congTy.LogoPath);
            MessageBox.Show("Thông tin công ty đã được lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void InPDF_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = "ThongTinCongTy.pdf",
                Title = "Lưu thông tin công ty dưới dạng PDF"
            };

            if (saveDialog.ShowDialog() == true)
            {
                string pdfPath = saveDialog.FileName;

                using (var writer = new PdfWriter(pdfPath))
                using (var pdf = new PdfDocument(writer))
                {
                    var doc = new Document(pdf);

                    string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                    var fontProgram = FontProgramFactory.CreateFont(fontPath);
                    var font = PdfFontFactory.CreateFont(fontProgram, PdfEncodings.IDENTITY_H);
                    doc.SetFont(font);

                    // Thêm logo
                    if (!string.IsNullOrEmpty(congTy.LogoPath) && File.Exists(congTy.LogoPath))
                    {
                        var imageData = ImageDataFactory.Create(congTy.LogoPath);
                        var logo = new PdfImage(imageData)
                            .ScaleToFit(100, 100)
                            .SetHorizontalAlignment(PdfAlignment.CENTER);
                        doc.Add(logo);
                    }

                    // Tiêu đề
                    doc.Add(new Paragraph("THÔNG TIN CÔNG TY")
                        .SetTextAlignment(PdfTextAlignment.CENTER)
                        .SetFontSize(18)
                        .SetBold()
                        .SetMarginBottom(20));

                    // Thông tin chi tiết
                    doc.Add(new Paragraph($"Tên công ty: {congTy.Ten}"));
                    doc.Add(new Paragraph($"Mã số thuế: {congTy.MaSoThue}"));
                    doc.Add(new Paragraph($"Địa chỉ: {congTy.DiaChi}"));
                    doc.Add(new Paragraph($"Số điện thoại: {congTy.SoDienThoai}"));
                    doc.Add(new Paragraph($"Email: {congTy.Email}"));
                    doc.Add(new Paragraph($"Người đại diện: {congTy.NguoiDaiDien}"));
                    doc.Add(new Paragraph($"Ghi chú: {congTy.GhiChu}"));

                    doc.Close();
                }

                congTy.PdfPath = pdfPath;
                File.WriteAllText(filePath, JsonConvert.SerializeObject(congTy, Formatting.Indented));

                MessageBox.Show("PDF đã được lưu thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnMoPDF_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(congTy.PdfPath) && File.Exists(congTy.PdfPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = congTy.PdfPath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show("Không tìm thấy file PDF.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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

        // Chỉ cho phép nhập số
        private void NumberOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;

            // Chỉ cho nhập số và tổng độ dài không vượt quá 11
            e.Handled = !e.Text.All(char.IsDigit) || textBox.Text.Length >= 11;
        }


        // Kiểm tra email khi người dùng rời khỏi ô
        private void TxtEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            var email = txtEmail.Text;
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Email không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Delay focus để tránh bị lặp lại LostFocus
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    txtEmail.Focus();
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
        }


        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}
