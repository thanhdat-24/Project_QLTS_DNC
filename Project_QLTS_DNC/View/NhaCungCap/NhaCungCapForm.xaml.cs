using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Project_QLTS_DNC.Models.NhaCungCap;
using Project_QLTS_DNC.Models.PhieuNhapKho;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Project_QLTS_DNC.View.NhaCungCap
{
    public partial class NhaCungCapForm : UserControl
    {
        public ObservableCollection<NhaCungCapClass> DanhSachNCC { get; set; } = new ObservableCollection<NhaCungCapClass>();
        private List<NhaCungCapClass> DanhSachGoc { get; set; } = new List<NhaCungCapClass>();

        public NhaCungCapForm()
        {
            InitializeComponent();
            supplierDataGrid.ItemsSource = DanhSachNCC;
            _ = InitializeSupabaseAndLoadData(); // gọi async method
        }

        private async Task InitializeSupabaseAndLoadData()
        {
            await SupabaseConfig.InitializeAsync();
            await LoadDataFromSupabase();
        }

        private async Task LoadDataFromSupabase()
        {
            var response = await SupabaseConfig.SupabaseClient
                .From<NhaCungCapClass>()
                .Get();

            DanhSachGoc = response.Models.ToList();
            DanhSachNCC.Clear();
            foreach (var ncc in DanhSachGoc)
            {
                DanhSachNCC.Add(ncc);
            }
        }

        private async void btnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            var themForm = new ThemNhaCungCapForm(DanhSachNCC);
            if (themForm.ShowDialog() == true && themForm.NhaCungCapMoi != null)
            {
                try
                {
                    // Gửi lên Supabase
                    var response = await SupabaseConfig.SupabaseClient
                        .From<NhaCungCapClass>()
                        .Insert(themForm.NhaCungCapMoi);

                    if (response.Models.Count > 0)
                    {
                        var ncc = response.Models.First();
                        DanhSachNCC.Add(ncc);
                        DanhSachGoc.Add(ncc);
                    }
                    else
                    {
                        MessageBox.Show("Không thể thêm nhà cung cấp vào Supabase.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi thêm vào Supabase: {ex.Message}");
                }
            }
        }


        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is NhaCungCapClass ncc)
            {
                var form = new ThemNhaCungCapForm(DanhSachNCC, ncc);
                if (form.ShowDialog() == true && form.NhaCungCapMoi != null)
                {
                    // Cập nhật giá trị
                    ncc.TenNCC = form.NhaCungCapMoi.TenNCC;
                    ncc.DiaChi = form.NhaCungCapMoi.DiaChi;
                    ncc.SoDienThoai = form.NhaCungCapMoi.SoDienThoai;
                    ncc.Email = form.NhaCungCapMoi.Email;
                    ncc.MoTa = form.NhaCungCapMoi.MoTa;
                    supplierDataGrid.Items.Refresh();

                    try
                    {
                        // Cập nhật lên Supabase
                        var response = await SupabaseConfig.SupabaseClient
                            .From<NhaCungCapClass>()
                            .Update(ncc);

                        if (!response.Models.Any())
                            MessageBox.Show("Không thể cập nhật nhà cung cấp trên Supabase.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi cập nhật Supabase: {ex.Message}");
                    }
                }
            }
        }


        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is NhaCungCapClass ncc)
            {
                var confirm = MessageBox.Show($"Bạn có chắc muốn xóa '{ncc.TenNCC}' không?", "Xác nhận", MessageBoxButton.YesNo);
                if (confirm == MessageBoxResult.Yes)
                {
                    try
                    {
                        // 1. Kiểm tra xem nhà cung cấp có đang được sử dụng trong phiếu nhập không
                        var phieuNhapLienQuan = await SupabaseConfig.SupabaseClient
                            .From<PhieuNhap>()
                            .Where(p => p.MaNCC == ncc.MaNCC)
                            .Get();

                        if (phieuNhapLienQuan.Models.Any())
                        {
                            MessageBox.Show("Không thể xóa vì nhà cung cấp này đang được sử dụng trong phiếu nhập.");
                            return;
                        }

                        // 2. Tiến hành xóa khỏi Supabase
                        var response = await SupabaseConfig.SupabaseClient
                            .From<NhaCungCapClass>()
                            .Delete(ncc);

                        if (!response.Models.Any())
                        {
                            MessageBox.Show("Không thể xoá nhà cung cấp khỏi Supabase.");
                            return;
                        }

                        // 3. Xóa khỏi danh sách trong app
                        DanhSachNCC.Remove(ncc);
                        DanhSachGoc.Remove(ncc);

                        // 4. Nếu danh sách rỗng thì reset sequence
                        if (!DanhSachGoc.Any())
                        {
                            await ResetSequenceAsync(); // Gọi lại sequence về 1 nếu không còn NCC
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xoá khỏi Supabase: {ex.Message}");
                    }
                }
            }
        }

        public async Task ResetSequenceAsync()
        {
            try
            {
                await SupabaseConfig.SupabaseClient
     .Rpc("reset_ma_ncc_sequence", new Dictionary<string, object>());

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi reset sequence: {ex.Message}");
            }
        }



        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var keyword = txtSearch.Text.Trim().ToLower();

            var ketQua = DanhSachGoc.Where(ncc =>
                ncc.MaNCC.ToString().ToLower().Contains(keyword) || // Chuyển MaNCC thành string
                (!string.IsNullOrEmpty(ncc.TenNCC) && ncc.TenNCC.ToLower().Contains(keyword)) ||
                (!string.IsNullOrEmpty(ncc.Email) && ncc.Email.ToLower().Contains(keyword))
            ).ToList();

            DanhSachNCC.Clear();
            foreach (var item in ketQua)
            {
                DanhSachNCC.Add(item);
            }
        }


        private async void btnLoadDuLieu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadDataFromSupabase();
                MessageBox.Show("Dữ liệu đã được tải lại thành công.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}");
            }
        }

    }
}