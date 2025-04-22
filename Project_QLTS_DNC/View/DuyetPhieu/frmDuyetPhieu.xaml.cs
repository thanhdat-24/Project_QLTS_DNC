using Project_QLTS_DNC.Models.DuyetPhieu;
using Project_QLTS_DNC.Models.BaoHong;
using Project_QLTS_DNC.Models.Phieu;
using Project_QLTS_DNC.Services;
using Project_QLTS_DNC.View.DuyetPhieu.ChiTietPhieu;
using Project_QLTS_DNC.View.QuanLyKho;
using Supabase;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Supabase.Postgrest.Constants;
using Project_QLTS_DNC.Models.BanGiaoTaiSan;
using Project_QLTS_DNC.View.DuyetPhieu.InPhieu;

namespace Project_QLTS_DNC.View.DuyetPhieu
{
    public partial class frmDuyetPhieu : UserControl
    {
        private ObservableCollection<PhieuCanDuyet> DanhSachTatCaPhieu = new();
        private bool isLoadingData = false;

        public frmDuyetPhieu()
        {
            InitializeComponent();
            Loaded += dgPhieuCanDuyet_Loaded;
        }

        private async void dgPhieuCanDuyet_Loaded(object sender, RoutedEventArgs e)
        {
            cboLoaiPhieu.SelectedIndex = 0; // đảm bảo là "Tất cả"
            await LocDuLieuTheoLoaiVaNgay();
            LoadThongKePhieu();
        }

        private async Task LoadDuLieuTongHopPhieuAsync()
        {

            try
            {
                isLoadingData = true;

                // Hiển thị loading indicator nếu cần
                SetLoadingState(true);

                var client = await SupabaseService.GetClientAsync();
                DanhSachTatCaPhieu.Clear();

                var tongHop = await client.From<TongHopPhieu>().Get();

                foreach (var item in tongHop.Models)
                {
                    if (item.MaPhieuNhap.HasValue)
                    {
                        try
                        {
                            var pn = await client.From<PhieuNhapKho>().Filter("ma_phieu_nhap", Operator.Equals, item.MaPhieuNhap.Value).Single();
                            DanhSachTatCaPhieu.Add(new PhieuCanDuyet { MaPhieu = $"PN{pn.MaPhieuNhap}", NgayTaoPhieu = pn.NgayNhap, TrangThaiBool = pn.TrangThai, LoaiPhieu = "Phiếu nhập" });
                        }
                        catch (Exception ex)
                        {
                            // Bỏ qua lỗi khi không tìm thấy phiếu nhập
                            Console.WriteLine($"Không tìm thấy phiếu nhập: {ex.Message}");
                        }
                    }

                    if (item.MaPhieuXuat.HasValue)
                    {
                        try
                        {
                            var px = await client.From<PhieuXuat>().Filter("ma_phieu_xuat", Operator.Equals, item.MaPhieuXuat.Value).Single();
                            DanhSachTatCaPhieu.Add(new PhieuCanDuyet { MaPhieu = $"PX{px.MaPhieuXuat}", NgayTaoPhieu = px.NgayXuat, TrangThaiBool = px.TrangThai, LoaiPhieu = "Phiếu xuất" });
                        }
                        catch (Exception ex)
                        {
                            // Bỏ qua lỗi khi không tìm thấy phiếu xuất
                            Console.WriteLine($"Không tìm thấy phiếu xuất: {ex.Message}");
                        }
                    }

                    if (item.MaPhieuDeNghiMua.HasValue)
                    {
                        try
                        {
                            var pdn = await client.From<denghimua>().Filter("ma_phieu_de_nghi", Operator.Equals, item.MaPhieuDeNghiMua.Value).Single();
                            DanhSachTatCaPhieu.Add(new PhieuCanDuyet
                            {
                                MaPhieu = $"PDN{pdn.MaPhieuDeNghi}",
                                NgayTaoPhieu = pdn.NgayDeNghiMua ?? DateTime.MinValue,
                                TrangThaiBool = pdn.TrangThai,
                                LoaiPhieu = "Phiếu đề nghị mua"
                            });
                        }
                        catch (Exception ex)
                        {
                            // Bỏ qua lỗi khi không tìm thấy phiếu đề nghị
                            Console.WriteLine($"Không tìm thấy phiếu đề nghị: {ex.Message}");
                        }
                    }

                    if (item.MaPhieuBaoHong.HasValue)
                    {
                        try
                        {
                            var pbh = await client.From<BaoHong>().Filter("ma_phieu_bao_hong", Operator.Equals, item.MaPhieuBaoHong.Value).Single();
                            DanhSachTatCaPhieu.Add(new PhieuCanDuyet { MaPhieu = $"PBH{pbh.MaPhieuBaoHong}", NgayTaoPhieu = pbh.NgayBaoHong, TrangThaiBool = pbh.TrangThai, LoaiPhieu = "Phiếu báo hỏng" });
                        }
                        catch (Exception ex)
                        {
                            // Bỏ qua lỗi khi không tìm thấy phiếu báo hỏng
                            Console.WriteLine($"Không tìm thấy phiếu báo hỏng: {ex.Message}");
                        }
                    }
                }

                // Sắp xếp phiếu theo ngày tạo mới nhất
                var sortedList = new ObservableCollection<PhieuCanDuyet>(
                    DanhSachTatCaPhieu.OrderByDescending(p => p.NgayTaoPhieu)
                );
                DanhSachTatCaPhieu = sortedList;
                dgPhieuCanDuyet.ItemsSource = DanhSachTatCaPhieu;

                // Cập nhật UI hiển thị số lượng
                UpdateItemCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi load dữ liệu tổng hợp: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isLoadingData = false;
                // Ẩn loading indicator
                SetLoadingState(false);
            }
        }

        private async Task LocDuLieuTheoLoaiVaNgay()
        {

            if (isLoadingData) return;

            // Kiểm tra ComboBox đã được khởi tạo chưa
            if (cboLoaiPhieu == null)
            {
                return;
            }

            try
            {
                isLoadingData = true;
                SetLoadingState(true);

                var client = await SupabaseService.GetClientAsync();

                // Lấy giá trị từ ComboBox an toàn hơn
                string loaiPhieu = "Tất cả"; // Giá trị mặc định
                ComboBoxItem selectedItem = cboLoaiPhieu.SelectedItem as ComboBoxItem;
                if (selectedItem != null)
                {
                    loaiPhieu = selectedItem.Content?.ToString() ?? "Tất cả";
                }

                DateTime? tuNgay = dpTuNgay?.SelectedDate;
                DateTime? denNgay = dpDenNgay?.SelectedDate;

                var ketQua = new ObservableCollection<PhieuCanDuyet>();

                // Phiếu nhập
                if (loaiPhieu == "Tất cả" || loaiPhieu == "Phiếu nhập")
                {
                    var list = (await client.From<PhieuNhapKho>().Get()).Models;
                    foreach (var p in list)
                    {
                        if ((!tuNgay.HasValue || p.NgayNhap.Date >= tuNgay.Value.Date) &&
                            (!denNgay.HasValue || p.NgayNhap.Date <= denNgay.Value.Date))
                        {
                            ketQua.Add(new PhieuCanDuyet { MaPhieu = $"PN{p.MaPhieuNhap}", NgayTaoPhieu = p.NgayNhap, TrangThaiBool = p.TrangThai, LoaiPhieu = "Phiếu nhập" });
                        }
                    }
                }

                // Phiếu xuất
                if (loaiPhieu == "Tất cả" || loaiPhieu == "Phiếu xuất")
                {
                    var list = (await client.From<PhieuXuat>().Get()).Models;
                    foreach (var p in list)
                    {
                        if ((!tuNgay.HasValue || p.NgayXuat.Date >= tuNgay.Value.Date) &&
                            (!denNgay.HasValue || p.NgayXuat.Date <= denNgay.Value.Date))
                        {
                            ketQua.Add(new PhieuCanDuyet { MaPhieu = $"PX{p.MaPhieuXuat}", NgayTaoPhieu = p.NgayXuat, TrangThaiBool = p.TrangThai, LoaiPhieu = "Phiếu xuất" });
                        }
                    }
                }

                // Phiếu đề nghị mua
                if (loaiPhieu == "Tất cả" || loaiPhieu == "Phiếu đề nghị mua")
                {
                    var list = (await client.From<denghimua>().Get()).Models;
                    foreach (var p in list)
                    {
                        if ((!tuNgay.HasValue || (p.NgayDeNghiMua.HasValue && p.NgayDeNghiMua.Value.Date >= tuNgay.Value.Date)) &&
                            (!denNgay.HasValue || (p.NgayDeNghiMua.HasValue && p.NgayDeNghiMua.Value.Date <= denNgay.Value.Date)))
                        {
                            ketQua.Add(new PhieuCanDuyet
                            {
                                MaPhieu = $"PDN{p.MaPhieuDeNghi}",
                                NgayTaoPhieu = p.NgayDeNghiMua ?? DateTime.MinValue, // fallback nếu null
                                TrangThaiBool = p.TrangThai,
                                LoaiPhieu = "Phiếu đề nghị mua"
                            });
                        }
                    }
                }


                // Phiếu báo hỏng
                if (loaiPhieu == "Tất cả" || loaiPhieu == "Phiếu báo hỏng")
                {
                    var list = (await client.From<BaoHong>().Get()).Models;
                    foreach (var p in list)
                    {
                        if ((!tuNgay.HasValue || p.NgayBaoHong.Date >= tuNgay.Value.Date) &&
                            (!denNgay.HasValue || p.NgayBaoHong.Date <= denNgay.Value.Date))
                        {
                            ketQua.Add(new PhieuCanDuyet { MaPhieu = $"PBH{p.MaPhieuBaoHong}", NgayTaoPhieu = p.NgayBaoHong, TrangThaiBool = p.TrangThai, LoaiPhieu = "Phiếu báo hỏng" });
                        }
                    }
                }
                // Phiếu bàn giao tài sản
                if (loaiPhieu == "Tất cả" || loaiPhieu == "Phiếu bàn giao tài sản")
                {
                    var list = (await client.From<BanGiaoTaiSanModel>().Get()).Models;
                    foreach (var p in list)
                    {
                        if ((!tuNgay.HasValue || p.NgayBanGiao.Date >= tuNgay.Value.Date) &&
     (!denNgay.HasValue || p.NgayBanGiao.Date <= denNgay.Value.Date))
                        {
                            ketQua.Add(new PhieuCanDuyet
                            {
                                MaPhieu = $"BG{p.MaBanGiaoTS}",
                                NgayTaoPhieu = p.NgayBanGiao, // Không cần dùng ??
                                TrangThaiBool = p.TrangThai,
                                LoaiPhieu = "Phiếu bàn giao"
                            });
                        }

                    }
                }

                // Sắp xếp phiếu theo ngày tạo mới nhất
                var sortedList = new ObservableCollection<PhieuCanDuyet>(
                    ketQua.OrderByDescending(p => p.NgayTaoPhieu)
                );

                DanhSachTatCaPhieu = sortedList;

                // Kiểm tra DataGrid đã được khởi tạo chưa
                if (dgPhieuCanDuyet != null)
                {
                    dgPhieuCanDuyet.ItemsSource = DanhSachTatCaPhieu;
                }

                // Cập nhật UI hiển thị số lượng
                UpdateItemCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lọc dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isLoadingData = false;
                SetLoadingState(false);
            }
        }

        // Thêm phương thức để hiển thị/ẩn trạng thái đang tải
        private void SetLoadingState(bool isLoading)
        {
            // Có thể thêm ProgressBar hiển thị khi đang tải
            // Ví dụ: loadingIndicator.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateItemCount()
        {
            // Cập nhật thông tin hiển thị số lượng phiếu sau khi lọc/tìm kiếm
            if (dgPhieuCanDuyet != null && dgPhieuCanDuyet.Items != null)
            {
                int count = dgPhieuCanDuyet.Items.Count;
                // Hiển thị số lượng phiếu đang hiển thị
                // Ví dụ: lblItemCount.Content = $"Hiển thị {count} phiếu";
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) => TimKiemPhieu(txtSearch?.Text);

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) => TimKiemPhieu(txtSearch?.Text);

        private void TimKiemPhieu(string tuKhoa)
        {
            if (isLoadingData || dgPhieuCanDuyet == null) return;

            if (string.IsNullOrWhiteSpace(tuKhoa))
            {
                dgPhieuCanDuyet.ItemsSource = DanhSachTatCaPhieu;
                return;
            }

            tuKhoa = tuKhoa.Trim().ToLower();

            var ketQua = DanhSachTatCaPhieu.Where(p =>
                p.MaPhieu.ToLower().Contains(tuKhoa) ||
                p.NgayTaoPhieu.ToString("dd/MM/yyyy").Contains(tuKhoa) ||
                p.TrangThai.ToLower().Contains(tuKhoa) ||
                p.LoaiPhieu.ToLower().Contains(tuKhoa)
            ).ToList();

            dgPhieuCanDuyet.ItemsSource = ketQua;

            // Cập nhật UI hiển thị số lượng
            UpdateItemCount();
        }

        private void btnDetail_Click(object sender, RoutedEventArgs e)
        {
            if (dgPhieuCanDuyet?.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một phiếu để xem chi tiết!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Gọi hàm xem chi tiết
            btnXemChiTiet_Click(sender, e);
        }

        private async void btnXemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            if (dgPhieuCanDuyet?.SelectedItem is not PhieuCanDuyet selected)
            {
                MessageBox.Show("Vui lòng chọn một phiếu để xem chi tiết.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool daDuyetHoacTuChoi = false;
            Window window = null;

            if (selected.MaPhieu.StartsWith("PN") && long.TryParse(selected.MaPhieu.Substring(2), out long maPhieuNhap))
            {
                var frm = new frmXemChiTietNhap();
                frm.LoadTheoMaPhieu(maPhieuNhap);
                frm.OnPhieuDuyetThanhCong += () => daDuyetHoacTuChoi = true;

                window = new Window { Title = $"Chi tiết phiếu nhập - {selected.MaPhieu}", Content = frm, Width = 1200, Height = 800 };
            }
            else if (selected.MaPhieu.StartsWith("PBH") && long.TryParse(selected.MaPhieu.Substring(3), out long maPhieuBaoHong))
            {
                var frm = new frmXemChiTietBaoHong();
                frm.LoadTheoMaPhieu(maPhieuBaoHong);
                frm.OnPhieuDuyetThanhCong += () => daDuyetHoacTuChoi = true;

                window = new Window { Title = $"Chi tiết phiếu báo hỏng - {selected.MaPhieu}", Content = frm, Width = 1000, Height = 700 };
            }
            else if (selected.MaPhieu.StartsWith("PDN") && long.TryParse(selected.MaPhieu.Substring(3), out long maPhieuDeNghi))
            {
                var frm = new frmXemChiTietDeNghiMua();
                frm.LoadTheoMaPhieu(maPhieuDeNghi);
                frm.OnPhieuDuyetThanhCong += () => daDuyetHoacTuChoi = true;

                window = new Window { Title = $"Chi tiết phiếu đề nghị mua - {selected.MaPhieu}", Content = frm, Width = 1100, Height = 750 };
            }
            else if (selected.MaPhieu.StartsWith("PX") && long.TryParse(selected.MaPhieu.Substring(2), out long maPhieuXuat))
            {
                var frm = new frmXemChiTietXuat();
                frm.LoadTheoMaPhieu(maPhieuXuat);
                frm.OnPhieuDuyetThanhCong += () => daDuyetHoacTuChoi = true;

                window = new Window { Title = $"Chi tiết phiếu xuất - {selected.MaPhieu}", Content = frm, Width = 1100, Height = 750 };
            }
            else if (selected.MaPhieu.StartsWith("BG") && long.TryParse(selected.MaPhieu.Substring(2), out long maPhieuBanGiao))
            {
                var frm = new frmXemChiTietBanGiao((int)maPhieuBanGiao);
                frm.OnPhieuDuyetThanhCong += () => daDuyetHoacTuChoi = true;

                window = new Window { Title = $"Chi tiết phiếu bàn giao - {selected.MaPhieu}", Content = frm, Width = 1100, Height = 750 };
            }
            else
            {
                MessageBox.Show($"Chức năng xem chi tiết đang được phát triển cho loại phiếu: {selected.LoaiPhieu}",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.ResizeMode = ResizeMode.CanResize;
            window.ShowDialog();

            // ✅ Sau khi form chi tiết đóng → reload nếu có duyệt / từ chối
            if (daDuyetHoacTuChoi)
            {
                await LocDuLieuTheoLoaiVaNgay();
                LoadThongKePhieu();
            }
        }

        private async void LoadThongKePhieu()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Tổng cộng
                int tongCanDuyet = 0;
                int tongDaDuyet = 0;
                int tongTuChoi = 0;

                // Phiếu nhập
                var listPN = (await client.From<PhieuNhapKho>().Get()).Models;
                tongCanDuyet += listPN.Count(p => p.TrangThai == null);
                tongDaDuyet += listPN.Count(p => p.TrangThai == true);
                tongTuChoi += listPN.Count(p => p.TrangThai == false);

                // Phiếu báo hỏng
                var listPBH = (await client.From<BaoHong>().Get()).Models;
                tongCanDuyet += listPBH.Count(p => p.TrangThai == null);
                tongDaDuyet += listPBH.Count(p => p.TrangThai == true);
                tongTuChoi += listPBH.Count(p => p.TrangThai == false);

                // Phiếu đề nghị mua
                var listPDN = (await client.From<denghimua>().Get()).Models;
                tongCanDuyet += listPDN.Count(p => p.TrangThai == null);
                tongDaDuyet += listPDN.Count(p => p.TrangThai == true);
                tongTuChoi += listPDN.Count(p => p.TrangThai == false);

                // Gán thống kê ra TextBlock nếu có
                if (txtTongPhieuCanDuyet != null)
                    txtTongPhieuCanDuyet.Text = tongCanDuyet.ToString();

                if (txtTongPhieuDaDuyet != null)
                    txtTongPhieuDaDuyet.Text = tongDaDuyet.ToString();

                if (txtTongPhieuTuChoi != null)
                    txtTongPhieuTuChoi.Text = tongTuChoi.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi load thống kê: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void cboLoaiPhieu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Đảm bảo chỉ xử lý nếu người dùng thực sự thay đổi lựa chọn
            if (cboLoaiPhieu?.SelectedItem != null)
            {
                await LocDuLieuTheoLoaiVaNgay();
            }
        }

        private async void dpTuNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            await LocDuLieuTheoLoaiVaNgay();
        }

        private async void dpDenNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            await LocDuLieuTheoLoaiVaNgay();
        }

        // Thêm sự kiện cho nút Refresh
        private async void btnLoadDuLieu_Click(object sender, RoutedEventArgs e)
        {
            await LoadDuLieuTongHopPhieuAsync();
        }



        private void btnIn_Click(object sender, RoutedEventArgs e)
        {
            if (dgPhieuCanDuyet?.SelectedItem is not PhieuCanDuyet selected)
            {
                MessageBox.Show("Vui lòng chọn một phiếu để in!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Chỉ xử lý in phiếu nhập
            if (selected.MaPhieu.StartsWith("PN") && long.TryParse(selected.MaPhieu.Substring(2), out long maPhieuNhap))
            {
                var formIn = new frmInPhieuNhap(maPhieuNhap);
                formIn.Title = $"In phiếu nhập - PN{maPhieuNhap}";
                formIn.Width = 1000;
                formIn.Height = 720;
                formIn.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                formIn.ShowDialog();
            }
            else
            {
                MessageBox.Show("Chức năng in hiện đang phát triển.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}