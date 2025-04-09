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
        public ObservableCollection<NhaCungCap> DanhSachNCC { get; set; } = new ObservableCollection<NhaCungCap>();
        private List<NhaCungCap> DanhSachGoc { get; set; } = new List<NhaCungCap>();

        public NhaCungCapForm()
        {
            InitializeComponent();  
            DanhSachNCC = new ObservableCollection<NhaCungCap>();
            LoadData();  // Gọi LoadData khi UserControl được khởi tạo
            supplierDataGrid.ItemsSource = DanhSachNCC;
        }

        private void btnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            var themForm = new ThemNhaCungCapForm(DanhSachNCC); // truyền danh sách hiện tại
            var result = themForm.ShowDialog();
            if (result == true && themForm.NhaCungCapMoi != null)
            {
                DanhSachNCC.Add(themForm.NhaCungCapMoi);
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
            // Dữ liệu mẫu
            DanhSachGoc = new List<NhaCungCap>
        {
            new NhaCungCap { MaNCC = "001", TenNhaCungCap = "Công ty A", DiaChi = "Hà Nội", SoDienThoai = "0987654321", Email = "a@company.com", MoTa = "Cung cấp vật liệu" },
            new NhaCungCap { MaNCC = "002", TenNhaCungCap = "Công ty B", DiaChi = "Hồ Chí Minh", SoDienThoai = "0123456789", Email = "b@company.com", MoTa = "Cung cấp thiết bị" }
        };

            // Gán vào danh sách hiển thị
            DanhSachNCC.Clear();
            foreach (var ncc in DanhSachGoc)
            {
                DanhSachNCC.Add(ncc);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string tuKhoa = txtSearch.Text.Trim().ToLower();

            var ketQua = DanhSachGoc.Where(ncc =>
                (!string.IsNullOrEmpty(ncc.MaNCC) && ncc.MaNCC.ToLower().Contains(tuKhoa)) ||
                (!string.IsNullOrEmpty(ncc.TenNhaCungCap) && ncc.TenNhaCungCap.ToLower().Contains(tuKhoa)) ||
                (!string.IsNullOrEmpty(ncc.Email) && ncc.Email.ToLower().Contains(tuKhoa))
            ).ToList();

            DanhSachNCC.Clear();
            foreach (var ncc in ketQua)
            {
                DanhSachNCC.Add(ncc);
            }
        }

        private void btnLoadDuLieu_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
            DanhSachNCC.Clear();
            foreach (var ncc in DanhSachGoc)
            {
                DanhSachNCC.Add(ncc);
            }
        
    }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.DataContext is NhaCungCap nccCanSua)
            {
                var form = new ThemNhaCungCapForm(DanhSachNCC, nccCanSua); // Truyền danh sách và đối tượng cũ
                var result = form.ShowDialog();

                if (result == true && form.NhaCungCapMoi != null)
                {
                    // Cập nhật lại dữ liệu
                    nccCanSua.TenNhaCungCap = form.NhaCungCapMoi.TenNhaCungCap;
                    nccCanSua.DiaChi = form.NhaCungCapMoi.DiaChi;
                    nccCanSua.SoDienThoai = form.NhaCungCapMoi.SoDienThoai;
                    nccCanSua.Email = form.NhaCungCapMoi.Email;
                    nccCanSua.MoTa = form.NhaCungCapMoi.MoTa;

                    supplierDataGrid.Items.Refresh(); // Làm mới DataGrid
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.DataContext is NhaCungCap nccCanXoa)
            {
                var result = MessageBox.Show($"Bạn có chắc muốn xóa nhà cung cấp: {nccCanXoa.TenNhaCungCap}?",
                                             "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    DanhSachNCC.Remove(nccCanXoa);
                    // Nếu bạn có danh sách gốc DanhSachGoc thì xóa ở đó luôn
                    DanhSachGoc.Remove(nccCanXoa);
                }
            }
        }

     
    }

}
