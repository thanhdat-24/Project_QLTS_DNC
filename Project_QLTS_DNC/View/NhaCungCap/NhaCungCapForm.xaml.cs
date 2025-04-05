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
        public ObservableCollection<NhaCungCap> DanhSachNCC { get; set; }

        public NhaCungCapForm()
        {
            InitializeComponent();  
            DanhSachNCC = new ObservableCollection<NhaCungCap>();
            LoadData();  // Gọi LoadData khi UserControl được khởi tạo
            supplierDataGrid.ItemsSource = DanhSachNCC;
        }

        private void btnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            var themForm = new ThemNhaCungCapForm();
            var result = themForm.ShowDialog();
            if (result == true && themForm.NhaCungCapMoi != null)
            {
                DanhSachNCC.Add(themForm.NhaCungCapMoi);  // Thêm vào danh sách nếu người dùng nhấn "Lưu"
            }
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
            ObservableCollection<NhaCungCap> nhaCungCapList = new ObservableCollection<NhaCungCap>
    {
       new NhaCungCap { MaNCC = "001", TenNhaCungCap = "Công ty A", DiaChi = "Hà Nội", SoDienThoai = "0987654321", Email = "a@company.com", MoTa = "Cung cấp vật liệu" },
        new NhaCungCap { MaNCC = "002", TenNhaCungCap = "Công ty B", DiaChi = "Hồ Chí Minh", SoDienThoai = "0123456789", Email = "b@company.com", MoTa = "Cung cấp thiết bị" }
    };

            // Gán dữ liệu cho DataGrid
            foreach (var nhaCungCap in nhaCungCapList)
            {
                DanhSachNCC.Add(nhaCungCap);
            }
        }

    }

}
