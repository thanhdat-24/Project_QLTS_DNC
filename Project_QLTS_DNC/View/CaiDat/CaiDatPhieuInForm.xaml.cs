using Newtonsoft.Json;
using Project_QLTS_DNC.Models.ThongTinCongTy;
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
using System.IO;
using IOPath = System.IO.Path; 
namespace Project_QLTS_DNC.View.CaiDat
{
    /// <summary>
    /// Interaction logic for CaiDatPhieuInForm.xaml
    /// </summary>
    public partial class CaiDatPhieuInForm : UserControl
    {
        private string filePath = "thongtincongty.json";

        public CaiDatPhieuInForm()
        {
            InitializeComponent();
            LoadHeaderThongTinCongTy();
        }

        private void LoadHeaderThongTinCongTy()
        {
            if (File.Exists(filePath))
            {
                try
                {
                    var congTy = JsonConvert.DeserializeObject<ThongTinCongTy>(File.ReadAllText(filePath));
                    if (congTy == null) return;

                    // Set thông tin vào các control trong header
                    txtCompanyName.Text = congTy.Ten;
                    txtCompanyInfo1.Text = $"MST: {congTy.MaSoThue} | ĐT: {congTy.SoDienThoai}";
                    txtCompanyInfo2.Text = $"Địa chỉ: {congTy.DiaChi}";

                    if (!string.IsNullOrEmpty(congTy.LogoPath) && File.Exists(congTy.LogoPath))
                    {
                        imgLogo.Source = new BitmapImage(new Uri(IOPath.GetFullPath(congTy.LogoPath))); // ✅ sửa ở đây
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể load thông tin công ty: " + ex.Message);
                }
            }
        }
    }
}
