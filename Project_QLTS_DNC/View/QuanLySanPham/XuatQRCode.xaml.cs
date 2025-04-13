using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.Rendering;
// Thêm các using cần thiết
using System.Drawing;
using System.Drawing.Imaging;
// Tránh xung đột namespace với iText
using iTextImage = iText.Layout.Element.Image;
using iTextTextAlignment = iText.Layout.Properties.TextAlignment;
using iTextErrorCorrectionLevel = iText.Barcodes.Qrcode.ErrorCorrectionLevel;
using ZXingErrorCorrectionLevel = ZXing.QrCode.Internal.ErrorCorrectionLevel;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;


namespace Project_QLTS_DNC.View.QuanLySanPham
{
    public class BitmapRenderer : ZXing.Rendering.IBarcodeRenderer<Bitmap>
    {
        public Bitmap Render(BitMatrix matrix, BarcodeFormat format, string content)
        {
            return Render(matrix, format, content, new EncodingOptions());
        }

        public Bitmap Render(BitMatrix matrix, BarcodeFormat format, string content, EncodingOptions options)
        {
            int width = matrix.Width;
            int height = matrix.Height;

            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bitmap.SetPixel(x, y, matrix[x, y] ? Color.Black : Color.White);
                }
            }

            return bitmap;
        }
    }

    public partial class XuatQRCode : Window
    {

        public XuatQRCode()
        {
            InitializeComponent();
          
        }

        private void CboNhomTS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TxtSearchSeri_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            
        }

        private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void BtnUnselectAll_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void UpdateStatusText()
        {

        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateStatusText();
        }

        private void BtnExportQR_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void ExportQRCodeToPDF(string filePath)
        {
            
        }
    }
}