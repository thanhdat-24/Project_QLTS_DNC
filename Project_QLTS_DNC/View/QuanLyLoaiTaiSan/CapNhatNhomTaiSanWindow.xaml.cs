using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Project_QLTS_DNC.Models;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class CapNhatNhomTaiSanWindow : Window
    {
        public NhomTaiSan NhomTaiSanSua { get; private set; }
        private ObservableCollection<LoaiTaiSan> _dsLoaiTaiSan;

        public CapNhatNhomTaiSanWindow(NhomTaiSan nhomTaiSan, ObservableCollection<LoaiTaiSan> dsLoaiTaiSan)
        {
            InitializeComponent();
        }

        // Chỉ cho phép nhập số
        private void txtSoLuong_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9]+"); // regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        // Cho phép di chuyển cửa sổ
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        // Phương thức xử lý sự kiện khi nhấn nút Hủy
        private void btnDong_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
}