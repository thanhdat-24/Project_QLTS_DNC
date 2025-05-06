using Microsoft.Win32;
using Project_QLTS_DNC.Models.ThongTinCongTy;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Project_QLTS_DNC.View.CaiDat
{
    public partial class ThongTinCongTyForm : UserControl
    {
        private readonly string jsonPath = "thongtincongty.json";
        private ThongTinCongTy thongTin;

        public ThongTinCongTyForm()
        {
            InitializeComponent();
            LoadThongTin();
        }

        private void LoadThongTin()
        {
            if (File.Exists(jsonPath))
            {
                string json = File.ReadAllText(jsonPath);
                thongTin = JsonSerializer.Deserialize<ThongTinCongTy>(json);
            }
            else
            {
                thongTin = new ThongTinCongTy();
            }

            txtTen.Text = thongTin.Ten;
            txtMaSoThue.Text = thongTin.MaSoThue;
            txtDiaChi.Text = thongTin.DiaChi;
            txtSoDienThoai.Text = thongTin.SoDienThoai;
            txtEmail.Text = thongTin.Email;
            txtNguoiDaiDien.Text = thongTin.NguoiDaiDien;
            txtGhiChu.Text = thongTin.GhiChu;

            if (!string.IsNullOrEmpty(thongTin.LogoPath) && File.Exists(thongTin.LogoPath))
            {
                imgLogo.Source = new BitmapImage(new Uri(thongTin.LogoPath));
                // Cập nhật logo panel trái nếu có
                if (Application.Current.MainWindow is MainWindow mainWin)
                {
                    mainWin.imgMainLogo.Source = new BitmapImage(new Uri(thongTin.LogoPath));
                }
                btnChonAnh.Visibility = Visibility.Collapsed;
                btnDoiAnh.Visibility = Visibility.Visible;
            }
            else
            {
                btnChonAnh.Visibility = Visibility.Visible;
                btnDoiAnh.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveToJson()
        {
            string json = JsonSerializer.Serialize(thongTin, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonPath, json);
        }

        private void BtnChonAnh_Click(object sender, RoutedEventArgs e)
        {
            ChonVaLuuLogo();
        }

        private void BtnDoiAnh_Click(object sender, RoutedEventArgs e)
        {
            ChonVaLuuLogo();
        }

        private void ChonVaLuuLogo()
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Ảnh (*.png;*.jpg)|*.png;*.jpg",
                Title = "Chọn logo công ty"
            };

            if (dlg.ShowDialog() == true)
            {
                thongTin.LogoPath = dlg.FileName;
                imgLogo.Source = new BitmapImage(new Uri(dlg.FileName));

                if (Application.Current.MainWindow is MainWindow mainWin)
                {
                    mainWin.imgMainLogo.Source = new BitmapImage(new Uri(dlg.FileName));
                }

                btnChonAnh.Visibility = Visibility.Collapsed;
                btnDoiAnh.Visibility = Visibility.Visible;

                SaveToJson();
            }
        }

        private void BtnDoiThongTin_Click(object sender, RoutedEventArgs e)
        {
            SetEditable(true);
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            thongTin.Ten = txtTen.Text;
            thongTin.MaSoThue = txtMaSoThue.Text;
            thongTin.DiaChi = txtDiaChi.Text;
            thongTin.SoDienThoai = txtSoDienThoai.Text;
            thongTin.Email = txtEmail.Text;
            thongTin.NguoiDaiDien = txtNguoiDaiDien.Text;
            thongTin.GhiChu = txtGhiChu.Text;

            SaveToJson();

            MessageBox.Show("Đã lưu thông tin công ty!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            SetEditable(false);
        }

        private void SetEditable(bool enable)
        {
            txtTen.IsReadOnly = !enable;
            txtMaSoThue.IsReadOnly = !enable;
            txtDiaChi.IsReadOnly = !enable;
            txtSoDienThoai.IsReadOnly = !enable;
            txtEmail.IsReadOnly = !enable;
            txtNguoiDaiDien.IsReadOnly = !enable;
            txtGhiChu.IsReadOnly = !enable;

            btnDoiThongTin.Visibility = enable ? Visibility.Collapsed : Visibility.Visible;
            btnLuuThongTin.Visibility = enable ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
