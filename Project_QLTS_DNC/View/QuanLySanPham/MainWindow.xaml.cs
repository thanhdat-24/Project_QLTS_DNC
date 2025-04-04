using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Project_QLTS_DNC.Models;

namespace Project_QLTS_DNC.View.QuanLySanPham
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<LoaiTaiSan> LoaiTaiSans { get; set; }
        public ObservableCollection<NhomTaiSan> NhomTaiSans { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            InitializeData();

            // Đặt DataContext
            this.DataContext = this;
        }
        private void InitializeData()
        {
            // Khởi tạo collections
            LoaiTaiSans = new ObservableCollection<LoaiTaiSan>();
            NhomTaiSans = new ObservableCollection<NhomTaiSan>();

            // Dữ liệu mẫu có thể được thêm vào đây sau này
            // Ví dụ:
            LoaiTaiSans.Add(new LoaiTaiSan { MaLoaiTaiSan = 1, TenLoaiTaiSan = "Bàn", MoTa = "Các thiết bị sử dụng trong phòng học" });
            LoaiTaiSans.Add(new LoaiTaiSan { MaLoaiTaiSan = 2, TenLoaiTaiSan = "Ghế", MoTa = "Các thiết bị sử dụng trong phòng học" });


            NhomTaiSans.Add(new NhomTaiSan { MaNhomTS = 1, ma_loai_ts = 1, TenNhom = "Bàn Học Sinh", SoLuong = 10, MoTa = "Bàn nhỏ sử dụng học tập" });
            NhomTaiSans.Add(new NhomTaiSan { MaNhomTS = 2, ma_loai_ts = 1, TenNhom = "Bàn Giáo Viên", SoLuong = 5, MoTa = "Bàn lớn sử dụng trong phòng học" });
            NhomTaiSans.Add(new NhomTaiSan { MaNhomTS = 3, ma_loai_ts = 1, TenNhom = "Bàn Hội Nghị", SoLuong = 1, MoTa = "Bàn lớn sử dụng trong phòng họp" });
            NhomTaiSans.Add(new NhomTaiSan { MaNhomTS = 4, ma_loai_ts = 2, TenNhom = "Ghế ngồi Sinh Viên", SoLuong = 7, MoTa = "Ghế nhỏ có tựa lưng sử dụng trong phòng họp" });
        }

        // Thêm các phương thức xử lý sự kiện bị thiếu
        private void Card_Loaded(object sender, RoutedEventArgs e)
        {
            // Chạy animation khi card được load
            if (sender is MaterialDesignThemes.Wpf.Card card)
            {
                var sb = this.FindResource("CardEnterStoryboard") as Storyboard;
                if (sb != null)
                {
                    Storyboard.SetTarget(sb, card);
                    sb.Begin();
                }
            }
        }

        private void MenuItemsListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Xử lý khi click vào menu item
            if (sender is ListBox listBox && listBox.SelectedItem is ListBoxItem selectedItem)
            {
                // Đóng drawer
                MenuToggleButton.IsChecked = false;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Đóng cửa sổ
            this.Close();
        }

        private void MenuToggleButton_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
