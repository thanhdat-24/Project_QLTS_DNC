﻿using Project_QLTS_DNC.Models.DuyetPhieu;
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
using Project_QLTS_DNC.Models.LichSu;

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
        private async Task KhoiTaoDuLieuDuyetPhieu()
        {
            try
            {
                isLoadingData = true;
                SetLoadingState(true);

                cboLoaiPhieu.SelectedIndex = 0;
                dpTuNgay.SelectedDate = null;
                dpDenNgay.SelectedDate = null;
                txtSearch.Text = string.Empty;

                await LocDuLieuTheoLoaiVaNgay();
                LoadThongKePhieu();
            }
            finally
            {
                isLoadingData = false;
                SetLoadingState(false);
            }
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
                        catch { }
                    }

                    if (item.MaPhieuXuat.HasValue)
                    {
                        try
                        {
                            var px = await client.From<PhieuXuat>().Filter("ma_phieu_xuat", Operator.Equals, item.MaPhieuXuat.Value).Single();
                            DanhSachTatCaPhieu.Add(new PhieuCanDuyet { MaPhieu = $"PX{px.MaPhieuXuat}", NgayTaoPhieu = px.NgayXuat, TrangThaiBool = px.TrangThai, LoaiPhieu = "Phiếu xuất" });
                        }
                        catch { }
                    }

                    if (item.MaPhieuDeNghiMua.HasValue)
                    {
                        try
                        {
                            var pdn = await client.From<denghimua>().Filter("ma_phieu_de_nghi", Operator.Equals, item.MaPhieuDeNghiMua.Value).Single();
                            DanhSachTatCaPhieu.Add(new PhieuCanDuyet { MaPhieu = $"PDN{pdn.MaPhieuDeNghi}", NgayTaoPhieu = pdn.NgayDeNghiMua ?? DateTime.MinValue, TrangThaiBool = pdn.TrangThai, LoaiPhieu = "Phiếu đề nghị mua" });
                        }
                        catch { }
                    }

                    if (item.MaPhieuBaoHong.HasValue)
                    {
                        try
                        {
                            var pbh = await client.From<BaoHong>().Filter("ma_phieu_bao_hong", Operator.Equals, item.MaPhieuBaoHong.Value).Single();
                            DanhSachTatCaPhieu.Add(new PhieuCanDuyet { MaPhieu = $"PBH{pbh.MaPhieuBaoHong}", NgayTaoPhieu = pbh.NgayBaoHong, TrangThaiBool = pbh.TrangThai, LoaiPhieu = "Phiếu báo hỏng" });
                        }
                        catch { }
                    }

                    if (item.MaLichSuDiChuyen.HasValue)
                    {
                        try
                        {
                            var ls = await client.From<LichSuDiChuyenTaiSan>().Filter("ma_lich_su", Operator.Equals, item.MaLichSuDiChuyen.Value).Single();
                            DanhSachTatCaPhieu.Add(new PhieuCanDuyet { MaPhieu = $"LS{ls.MaLichSu}", NgayTaoPhieu = ls.NgayBanGiao ?? DateTime.MinValue, TrangThaiBool = ls.TrangThai, LoaiPhieu = "Phiếu lịch sử di chuyển tài sản" });
                        }
                        catch { }
                    }
                }

                DanhSachTatCaPhieu = new ObservableCollection<PhieuCanDuyet>(
                DanhSachTatCaPhieu
                .Where(p => p.NgayTaoPhieu != null)
                .OrderByDescending(p => p.NgayTaoPhieu)
  );

                dgPhieuCanDuyet.ItemsSource = DanhSachTatCaPhieu;
                UpdateItemCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi load dữ liệu tổng hợp: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isLoadingData = false;
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

                // Phiếu lịch sử di chuyển tài sản
                if (loaiPhieu == "Tất cả" || loaiPhieu == "Phiếu lịch sử di chuyển tài sản")
                {
                    var list = (await client.From<LichSuDiChuyenTaiSan>().Get()).Models;
                    foreach (var p in list)
                    {
                        ketQua.Add(new PhieuCanDuyet
                        {
                            MaPhieu = $"LS{p.MaLichSu}",
                            NgayTaoPhieu = p.NgayBanGiao ?? DateTime.Now, // fallback tránh lọc null
                            TrangThaiBool = p.TrangThai,
                            LoaiPhieu = "Phiếu lịch sử di chuyển tài sản"
                        });
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

       // private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) => TimKiemPhieu(txtSearch?.Text);

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
            else if (selected.MaPhieu.StartsWith("LS") && long.TryParse(selected.MaPhieu.Substring(2), out long maLichSu))
            {
                var frm = new frmXemChiTietLichSuDiChuyen();
                frm.LoadTheoMaPhieu(maLichSu);
                frm.OnPhieuDuyetThanhCong += () => daDuyetHoacTuChoi = true;

                window = new Window { Title = $"Chi tiết phiếu lịch sử di chuyển tài sản - {selected.MaPhieu}", Content = frm, Width = 1000, Height = 700 };
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

                int tongCanDuyet = 0;
                int tongDaDuyet = 0;
                int tongTuChoi = 0;

                // Phiếu nhập kho
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

                // Phiếu xuất kho
                var listPX = (await client.From<PhieuXuat>().Get()).Models;
                tongCanDuyet += listPX.Count(p => p.TrangThai == null);
                tongDaDuyet += listPX.Count(p => p.TrangThai == true);
                tongTuChoi += listPX.Count(p => p.TrangThai == false);

                // Phiếu bàn giao
                var listBG = (await client.From<BanGiaoTaiSanModel>().Get()).Models;
                tongCanDuyet += listBG.Count(p => p.TrangThai == null);
                tongDaDuyet += listBG.Count(p => p.TrangThai == true);
                tongTuChoi += listBG.Count(p => p.TrangThai == false);

                // Phiếu lịch sử di chuyển
                var listLS = (await client.From<LichSuDiChuyenTaiSan>().Get()).Models;
                tongCanDuyet += listLS.Count(p => p.TrangThai == null);
                tongDaDuyet += listLS.Count(p => p.TrangThai == true);
                tongTuChoi += listLS.Count(p => p.TrangThai == false);

                // Gán ra TextBlock
                txtTongPhieuCanDuyet.Text = tongCanDuyet.ToString();
                txtTongPhieuDaDuyet.Text = tongDaDuyet.ToString();
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
            txtSearch.Text = "";
            await KhoiTaoDuLieuDuyetPhieu();
        }



        private void btnIn_Click(object sender, RoutedEventArgs e)
        {
            if (dgPhieuCanDuyet?.SelectedItem is not PhieuCanDuyet selected)
            {
                MessageBox.Show("Vui lòng chọn một phiếu để in!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (selected.MaPhieu.StartsWith("PN") && long.TryParse(selected.MaPhieu.Substring(2), out long maPhieuNhap))
            {
                var formIn = new frmInPhieuNhap(maPhieuNhap)
                {
                    Title = $"In phiếu nhập - PN{maPhieuNhap}",
                    Width = 1000,
                    Height = 720,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                formIn.ShowDialog();
            }
            else if (selected.MaPhieu.StartsWith("PX") && long.TryParse(selected.MaPhieu.Substring(2), out long maPhieuXuat))
            {
                var formIn = new frmInPhieuXuat(maPhieuXuat)
                {
                    Title = $"In phiếu xuất - PX{maPhieuXuat}",
                    Width = 1000,
                    Height = 720,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                formIn.ShowDialog();
            }
            else if (selected.MaPhieu.StartsWith("PBH") && long.TryParse(selected.MaPhieu.Substring(3), out long maPhieuBH))
            {
                var formIn = new frmInPhieuBaoHong(maPhieuBH)
                {
                    Title = $"In phiếu báo hỏng - PBH{maPhieuBH}",
                    Width = 1000,
                    Height = 720,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                formIn.ShowDialog();
            }
            else if (selected.MaPhieu.StartsWith("BG") && int.TryParse(selected.MaPhieu.Substring(2), out int maPhieuBanGiao))
            {
                var formIn = new frmInPhieuBanGiao(maPhieuBanGiao)
                {
                    Title = $"In phiếu bàn giao - BG{maPhieuBanGiao}",
                    Width = 1000,
                    Height = 720,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                formIn.ShowDialog();
            }
            else if (selected.MaPhieu.StartsWith("PDN") && long.TryParse(selected.MaPhieu.Substring(3), out long maPhieuDeNghi))
            {
                var formIn = new frmInPhieuDeNghiMua(maPhieuDeNghi)
                {
                    Title = $"In phiếu đề nghị mua - PDN{maPhieuDeNghi}",
                    Width = 1000,
                    Height = 720,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                formIn.ShowDialog();
            }
            else if (selected.MaPhieu.StartsWith("LS") && long.TryParse(selected.MaPhieu.Substring(2), out long maLichSu))
            {
                var formIn = new frmInPhieuLichSuDiChuyenTaiSan(maLichSu)
                {
                    Title = $"In phiếu lịch sử di chuyển - LS{maLichSu}",
                    Width = 1000,
                    Height = 720,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                formIn.ShowDialog();
            }

            else
            {
                MessageBox.Show("Chức năng in cho loại phiếu này hiện chưa được hỗ trợ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



    }
}