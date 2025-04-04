using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Project_QLTS_DNC.View.NhaCungCap
{
    /// <summary>
    /// Interaction logic for NhaCungCapForm.xaml
    /// </summary>
    public partial class NhaCungCapForm : UserControl
    {
        public ObservableCollection<NhaCungCap> NhaCungCapList { get; set; }

        public NhaCungCapForm()
        {
            InitializeComponent();
            NhaCungCapList = new ObservableCollection<NhaCungCap>
                {
                    new NhaCungCap("1", "Window", "Can Tho", "0123456789", "example@gmail.com", "Mô tả 1"),
                     new NhaCungCap("2", "Company A", "Hanoi", "0987654321", "companya@example.com", "Mô tả 2")
                };

            // Thiết lập DataContext để binding hoạt động
            this.DataContext = this;
        }
    }

    public class NhaCungCap
    {
        public string MaNCC { get; set; }
        public string TenNCC { get; set; }
        public string DiaChi { get; set; }
        public string SDT { get; set; }
        public string Email { get; set; }
        public string MoTa { get; set; } // Thêm thuộc tính MoTa

        // Constructor
        
    }

}
