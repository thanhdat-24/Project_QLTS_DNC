using System;
using System.Collections.ObjectModel;
using System.Windows;
using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models.QLTaiSan;
using Project_QLTS_DNC.Services.QLTaiSanService;
using Project_QLTS_DNC.Services.BanGiaoTaiSanService;
using System.Threading.Tasks;
using System.Linq;
using Project_QLTS_DNC.Models.BanGiaoTaiSan;
using Project_QLTS_DNC.Models.QLLoaiTS;
using Project_QLTS_DNC.Models.QLNhomTS;
using Project_QLTS_DNC.Models.PhieuNhapKho;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Models.ToaNha;
using Project_QLTS_DNC.Models.ThongSoKT;
using Project_QLTS_DNC.Models.NhaCungCap;
using Project_QLTS_DNC.Services;


namespace Project_QLTS_DNC.View.QuanLySanPham
{
    public partial class ChiTietTaiSan : Window
    {
        private int _maTaiSan;
        private TaiSanDTO _taiSan;

        // Constructor
        public ChiTietTaiSan(int maTaiSan)
        {
            InitializeComponent();
            _maTaiSan = maTaiSan;
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                await LoadTaiSanInfoAsync();
                await LoadPhieuNhapInfoAsync();
                await LoadViTriInfoAsync();
                await LoadThongSoKyThuatAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu chi tiết tài sản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadTaiSanInfoAsync()
        {
            // Lấy thông tin tài sản
            var taiSanModel = await TaiSanService.LayTaiSanTheoMaAsync(_maTaiSan);
            _taiSan = TaiSanDTO.FromModel(taiSanModel);

            // Lấy thông tin nhóm tài sản
            int maNhomTS = -1;
            if (_taiSan.MaChiTietPN.HasValue)
            {
                var chiTietPN = await ChiTietPhieuNhapService.LayChiTietPhieuNhapTheoMaAsync(_taiSan.MaChiTietPN.Value);
                maNhomTS = chiTietPN.MaNhomTS;
                _taiSan.MaNhomTS = maNhomTS;
            }
            else
            {
                // Thử tìm nhóm tài sản bằng phương thức khác nếu không có mã chi tiết phiếu nhập
                maNhomTS = await ChiTietPhieuNhapService.TimNhomTaiSanTheoTaiSanAsync(_taiSan.MaTaiSan);
                _taiSan.MaNhomTS = maNhomTS;
            }

            // Lấy thông tin loại tài sản
            string tenLoaiTaiSan = "Không xác định";
            string tenNhomTaiSan = "Không xác định";

            if (maNhomTS > 0)
            {
                var dsNhomTaiSan = await NhomTaiSanService.LayDanhSachNhomTaiSanAsync();
                var nhomTaiSan = dsNhomTaiSan.FirstOrDefault(n => n.MaNhomTS == maNhomTS);

                if (nhomTaiSan != null)
                {
                    tenNhomTaiSan = nhomTaiSan.TenNhom;
                    _taiSan.TenNhomTS = tenNhomTaiSan;

                    // Lấy thông tin loại tài sản
                    var client = await SupabaseService.GetClientAsync();
                    var loaiTaiSanResponse = await client.From<LoaiTaiSan>()
                        .Where(l => l.MaLoaiTaiSan == nhomTaiSan.MaLoaiTaiSan)
                        .Get();

                    if (loaiTaiSanResponse.Models.Count > 0)
                    {
                        tenLoaiTaiSan = loaiTaiSanResponse.Models.First().TenLoaiTaiSan;
                    }
                }
            }

            // Hiển thị thông tin cơ bản
            txtTaiSanTitle.Text = _taiSan.TenTaiSan;
            txtMaTaiSan.Text = _taiSan.MaTaiSan.ToString();
            txtTenTaiSan.Text = _taiSan.TenTaiSan;
            txtLoaiTaiSan.Text = tenLoaiTaiSan;
            txtNhomTaiSan.Text = tenNhomTaiSan;
            txtSoSeri.Text = _taiSan.SoSeri ?? "Không có";
            txtMaQR.Text = _taiSan.MaQR ?? "Không có";
            txtNgaySuDung.Text = _taiSan.NgaySuDung.HasValue ? _taiSan.NgaySuDung.Value.ToString("dd/MM/yyyy") : "Chưa sử dụng";
            txtHanBH.Text = _taiSan.HanBH.HasValue ? _taiSan.HanBH.Value.ToString("dd/MM/yyyy") : "Không có";
            txtTinhTrang.Text = _taiSan.TinhTrangSP ?? "Không xác định";
            txtGhiChu.Text = _taiSan.GhiChu ?? "Không có";
        }

        private async Task LoadPhieuNhapInfoAsync()
        {
            if (!_taiSan.MaChiTietPN.HasValue)
            {
                // Không có thông tin phiếu nhập
                return;
            }

            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Lấy thông tin chi tiết phiếu nhập
                var chiTietPN = await ChiTietPhieuNhapService.LayChiTietPhieuNhapTheoMaAsync(_taiSan.MaChiTietPN.Value);

                if (chiTietPN != null)
                {
                    txtMaChiTietPN.Text = chiTietPN.MaChiTietPN.ToString();
                    txtDonGia.Text = chiTietPN.DonGia.HasValue ? string.Format("{0:N0} VNĐ", chiTietPN.DonGia) : "Không có";

                    // Lấy thông tin phiếu nhập
                    var phieuNhapResponse = await client.From<PhieuNhap>()
                        .Where(p => p.MaPhieuNhap == chiTietPN.MaPhieuNhap)
                        .Get();

                    if (phieuNhapResponse.Models.Count > 0)
                    {
                        var phieuNhap = phieuNhapResponse.Models.First();
                        txtMaPhieuNhap.Text = phieuNhap.MaPhieuNhap.ToString();
                        txtNgayNhap.Text = phieuNhap.NgayNhap.ToString("dd/MM/yyyy");
                        txtTrangThaiPhieuNhap.Text = phieuNhap.TrangThai.HasValue ?
                            (phieuNhap.TrangThai.Value ? "Đã duyệt" : "Từ chối") : "Chờ duyệt";

                        // Lấy thông tin kho nhập
                        var khoResponse = await client.From<Kho>()
                            .Where(k => k.MaKho == phieuNhap.MaKho)
                            .Get();

                        if (khoResponse.Models.Count > 0)
                        {
                            txtKhoNhap.Text = khoResponse.Models.First().TenKho;
                        }

                        // Lấy thông tin nhân viên nhập
                        var nhanVienResponse = await client.From<NhanVienModel>()
                            .Where(nv => nv.MaNV == phieuNhap.MaNV)
                            .Get();

                        if (nhanVienResponse.Models.Count > 0)
                        {
                            txtNhanVienNhap.Text = nhanVienResponse.Models.First().TenNV;
                        }

                        // Lấy thông tin nhà cung cấp
                        var nccResponse = await client.From<NhaCungCapClass>()
                            .Where(ncc => ncc.MaNCC == phieuNhap.MaNCC)
                            .Get();

                        if (nccResponse.Models.Count > 0)
                        {
                            txtNhaCungCap.Text = nccResponse.Models.First().TenNCC;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi tải thông tin phiếu nhập: {ex.Message}");
            }
        }

        private async Task LoadViTriInfoAsync()
        {
            if (!_taiSan.MaPhong.HasValue)
            {
                txtToaNha.Text = "Chưa có";
                txtTang.Text = "Chưa có";
                txtPhong.Text = "Chưa phân phòng";
                txtViTriPhong.Text = "Chưa có";
                txtPhieuBanGiao.Text = "Chưa có";
                txtNgayBanGiao.Text = "Chưa có";
                return;
            }

            try
            {
                var client = await SupabaseService.GetClientAsync();

                // Lấy thông tin phòng
                var phongResponse = await client.From<Phong>()
                    .Where(p => p.MaPhong == _taiSan.MaPhong.Value)
                    .Get();

                if (phongResponse.Models.Count > 0)
                {
                    var phong = phongResponse.Models.First();
                    txtPhong.Text = phong.TenPhong;

                    // Lấy thông tin tầng
                    var tangResponse = await client.From<Tang>()
                        .Where(t => t.MaTang == phong.MaTang)
                        .Get();

                    if (tangResponse.Models.Count > 0)
                    {
                        var tang = tangResponse.Models.First();
                        txtTang.Text = tang.TenTang;

                        // Lấy thông tin tòa nhà
                        var toaNhaResponse = await client.From<ToaNha>()
                            .Where(tn => tn.MaToaNha == tang.MaToa)
                            .Get();

                        if (toaNhaResponse.Models.Count > 0)
                        {
                            txtToaNha.Text = toaNhaResponse.Models.First().TenToaNha;
                        }
                    }
                }

                // Lấy thông tin bàn giao
                var chiTietBanGiaoResponse = await client.From<ChiTietBanGiaoModel>()
                    .Where(ct => ct.MaTaiSan == _taiSan.MaTaiSan)
                    .Order("ma_chi_tiet_bg", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();

                if (chiTietBanGiaoResponse.Models.Count > 0)
                {
                    var chiTietBG = chiTietBanGiaoResponse.Models.First();
                    txtViTriPhong.Text = $"Vị trí {chiTietBG.ViTriTS}";

                    // Lấy thông tin phiếu bàn giao
                    var banGiaoResponse = await client.From<BanGiaoTaiSanModel>()
                        .Where(bg => bg.MaBanGiaoTS == chiTietBG.MaBanGiaoTS)
                        .Get();

                    if (banGiaoResponse.Models.Count > 0)
                    {
                        var banGiao = banGiaoResponse.Models.First();
                        txtPhieuBanGiao.Text = $"Mã phiếu: {banGiao.MaBanGiaoTS}";
                        txtNgayBanGiao.Text = banGiao.NgayBanGiao.ToString("dd/MM/yyyy HH:mm");
                    }
                }
                else
                {
                    txtViTriPhong.Text = "Không xác định";
                    txtPhieuBanGiao.Text = "Không có";
                    txtNgayBanGiao.Text = "Không có";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi tải thông tin vị trí: {ex.Message}");
            }
        }

        private async Task LoadThongSoKyThuatAsync()
        {
            if (!_taiSan.MaNhomTS.HasValue)
            {
                return;
            }

            try
            {
                // Lấy thông số kỹ thuật chung của nhóm tài sản
                var thongSoChung = await ThongSoKyThuatService.LayDanhSachThongSoAsync(_taiSan.MaNhomTS.Value);
                dgThongSoChung.ItemsSource = thongSoChung;

                // Lấy thông số kỹ thuật riêng của tài sản này
                var client = await SupabaseService.GetClientAsync();
                var thongSoRiengResponse = await client.From<ThongSoTaiSan>()
                    .Where(ts => ts.MaTaiSan == _taiSan.MaTaiSan)
                    .Get();

                if (thongSoRiengResponse.Models.Count > 0)
                {
                    var danhSachThongSoRieng = thongSoRiengResponse.Models;

                    var dsThongSoRiengDTO = new ObservableCollection<ThongSoTaiSanDTO>();

                    // Nếu không có thông số riêng nào, thêm thông báo
                    if (danhSachThongSoRieng.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Không có thông số kỹ thuật riêng nào cho tài sản này");
                    }

                    foreach (var thongSo in danhSachThongSoRieng)
                    {
                        // Update the namespace reference to the correct type
                        var thongSoKTResponse = await client.From<Project_QLTS_DNC.Models.ThongSoKT.ThongSoKyThuat>()
                            .Where(ts => ts.MaThongSo == thongSo.MaThongSo)
                            .Get();

                        if (thongSoKTResponse.Models.Count > 0)
                        {
                            var thongSoKT = thongSoKTResponse.Models.First();

                            dsThongSoRiengDTO.Add(new ThongSoTaiSanDTO
                            {
                                MaThongSoTS = thongSo.MaThongSoTS,
                                MaTaiSan = thongSo.MaTaiSan,
                                MaThongSo = thongSo.MaThongSo,
                                TenThongSo = thongSoKT.TenThongSo,
                                GiaTri = thongSo.GiaTri,
                                NgayCapNhat = thongSo.NgayCapNhat,
                                HanBaoHanh = thongSo.HanBaoHanh,
                                TinhTrang = thongSo.TinhTrang,
                                MoTa = thongSo.MoTa
                            });
                        }
                    }

                    dgThongSoRieng.ItemsSource = dsThongSoRiengDTO;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi tải thông số kỹ thuật: {ex.Message}");
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Hiển thị thông báo tính năng đang phát triển
                MessageBox.Show("Tính năng in chi tiết đang được phát triển.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Đoạn code dưới đây có thể được bổ sung để thực hiện in chi tiết
                // PrintDialog printDialog = new PrintDialog();
                // if (printDialog.ShowDialog() == true)
                // {
                //     printDialog.PrintVisual(this, "In chi tiết tài sản");
                // }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in chi tiết: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // DTO cho thông số tài sản riêng
    public class ThongSoTaiSanDTO
    {
        public int MaThongSoTS { get; set; }
        public int MaTaiSan { get; set; }
        public int MaThongSo { get; set; }
        public string TenThongSo { get; set; }
        public string GiaTri { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public string HanBaoHanh { get; set; }
        public string TinhTrang { get; set; }
        public string MoTa { get; set; }
    }
}