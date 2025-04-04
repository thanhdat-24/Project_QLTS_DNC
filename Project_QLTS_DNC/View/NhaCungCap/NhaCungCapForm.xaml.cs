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
        public NhaCungCapForm()
        {
            InitializeComponent();
            LoadData();  // Gọi LoadData khi UserControl được khởi tạo
        }

        // Định nghĩa lớp NhaCungCapModel với các thuộc tính public
        public class NhaCungCapModel
        {
            public string MaNCC { get; set; }  // Các thuộc tính phải có getter và setter
            public string TenNCC { get; set; }
            public string DiaChi { get; set; }
            public string SDT { get; set; }
            public string Email { get; set; }
            public string MoTa { get; set; }
        }

        // Hàm LoadData sẽ được gọi để thêm dữ liệu vào DataGrid
        private void LoadData()
        {
            // Tạo ObservableCollection chứa dữ liệu mẫu
            ObservableCollection<NhaCungCapModel> nhaCungCapList = new ObservableCollection<NhaCungCapModel>
            {
                new NhaCungCapModel { MaNCC = "001", TenNCC = "Công ty A", DiaChi = "Hà Nội", SDT = "0987654321", Email = "a@company.com", MoTa = "Cung cấp vật liệu" },
                new NhaCungCapModel { MaNCC = "002", TenNCC = "Công ty B", DiaChi = "Hồ Chí Minh", SDT = "0123456789", Email = "b@company.com", MoTa = "Cung cấp thiết bị" }
            };

            // Gán dữ liệu cho DataGrid
            supplierDataGrid.ItemsSource = nhaCungCapList;
        }

        private void btnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            ThemNhaCungCapForm frmThemNCC = new ThemNhaCungCapForm();   
            frmThemNCC.ShowDialog();
        }
    }
}