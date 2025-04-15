using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.Services.ChucVu;
using Project_QLTS_DNC.ViewModels.NhanVien;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Project_QLTS_DNC.View.ChucVu
{
    public partial class ThemChucVuForm : Window
    {
        private ChucVuService _chucVuService;
        private ChucVuModel _updateCV;

        public ThemChucVuForm(Supabase.Client client)
        {
            InitializeComponent();
            _chucVuService = new ChucVuService();
        }

       
        public ThemChucVuForm(ChucVuModel chucVuUpdate)
        {
            InitializeComponent();
            _updateCV = chucVuUpdate;
            _chucVuService = new ChucVuService();  
            LoadChucVuData();
        }

        private async Task InitializeAsync(ChucVuModel chucVuUpdate)
        {
            _updateCV = chucVuUpdate;

            var client = await SupabaseService.GetClientAsync();  
            _chucVuService = new ChucVuService(); 
            LoadChucVuData();
        }

        // Lưu chức vụ
        async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tenChucVu = txtTenchucvu.Text.Trim();
                string moTa = txtMoTa.Text.Trim();

                if (string.IsNullOrEmpty(tenChucVu))
                {
                    MessageBox.Show("Tên chức vụ không được để trống.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var chucVu = new ChucVuModel
                {
                    TenChucVu = tenChucVu,
                    MoTa = moTa
                };

                bool result = false;

                if (_updateCV != null)
                {
                    try
                    {
                        chucVu.MaChucVu = _updateCV.MaChucVu;
                        var updatedChucVu = await _chucVuService.CapNhatChucVuAsync(chucVu);
                        result = updatedChucVu != null;

                        
                        System.Diagnostics.Debug.WriteLine($"Update result: {result}, Updated object: {(updatedChucVu != null ? "not null" : "null")}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Update exception: {ex.Message}");
                        MessageBox.Show($"Lỗi khi cập nhật: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    try
                    {
                        result = await _chucVuService.AddChucVuAsync(chucVu);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Add exception: {ex.Message}");
                        MessageBox.Show($"Lỗi khi thêm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                if (result)
                {
                    MessageBox.Show(_updateCV != null ? "Cập nhật chức vụ thành công." : "Thêm chức vụ thành công.",
                                   "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    
                    try
                    {
                        var parentWindow = System.Windows.Window.GetWindow(this);
                        if (parentWindow != null)
                        {
                            var danhSachChucVuViewModel = parentWindow.DataContext as DanhSachChucVuViewModel;
                            if (danhSachChucVuViewModel != null)
                            {
                                await danhSachChucVuViewModel.Refresh();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"LoadChucVuAsync exception: {ex.Message}");
                    }

                    this.Close();
                }
                else
                {
                    MessageBox.Show(_updateCV != null ? "Cập nhật chức vụ thất bại." : "Thêm chức vụ thất bại.",
                                   "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General exception: {ex.Message}");
                MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
        private void LoadChucVuData()
        {
            if (_updateCV != null)
            {
                txtMaChucVu.Text = _updateCV.MaChucVu.ToString();
                txtTenchucvu.Text = _updateCV.TenChucVu;
                txtMoTa.Text = _updateCV.MoTa;

                this.txtTieude.Text = "Cập nhật chức vụ";
                this.Title = "Cập nhật chức vụ";
                btnLuu.Content = "Cập nhật";
            }
        }

        // Hủy
        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
