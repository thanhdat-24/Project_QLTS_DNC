using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Project_QLTS_DNC.Services.QLTaiSanService;
using Project_QLTS_DNC.Helpers; // Thêm namespace cho NetworkHelper

namespace Project_QLTS_DNC.DTOs
{
    public class TaiSanQRDTO : TaiSanDTO
    {
        private int? _maNhomTS;
        private string _tenNhomTS;
        private string _maQrUrl;

        // Mã nhóm tài sản
        public int? MaNhomTS
        {
            get { return _maNhomTS; }
            set
            {
                if (_maNhomTS != value)
                {
                    _maNhomTS = value;
                    OnPropertyChanged(nameof(MaNhomTS));
                }
            }
        }

        // Tên nhóm tài sản
        public string TenNhomTS
        {
            get { return _tenNhomTS; }
            set
            {
                if (_tenNhomTS != value)
                {
                    _tenNhomTS = value;
                    OnPropertyChanged(nameof(TenNhomTS));
                }
            }
        }

        // URL cho mã QR (có thể là URL website hoặc dữ liệu JSON)
        public string MaQrUrl
        {
            get { return _maQrUrl; }
            set
            {
                if (_maQrUrl != value)
                {
                    _maQrUrl = value;
                    OnPropertyChanged(nameof(MaQrUrl));
                }
            }
        }

        // Constructor copy từ TaiSanDTO
        public static TaiSanQRDTO FromTaiSanDTO(TaiSanDTO taiSanDTO)
        {
            var qrDTO = new TaiSanQRDTO
            {
                MaTaiSan = taiSanDTO.MaTaiSan,
                MaChiTietPN = taiSanDTO.MaChiTietPN,
                TenTaiSan = taiSanDTO.TenTaiSan,
                SoSeri = taiSanDTO.SoSeri,
                MaQR = taiSanDTO.MaQR,
                NgaySuDung = taiSanDTO.NgaySuDung,
                HanBH = taiSanDTO.HanBH,
                TinhTrangSP = taiSanDTO.TinhTrangSP,
                GhiChu = taiSanDTO.GhiChu,
                MaPhong = taiSanDTO.MaPhong,
                TenPhong = taiSanDTO.TenPhong,
                TenNhomTS = taiSanDTO.TenNhomTS,
                IsSelected = taiSanDTO.IsSelected
            };

            // Tạo URL với IP tĩnh cho QR code sử dụng NetworkHelper
            string baseUrl = NetworkHelper.GetLocalIPv4Address(8080);
            qrDTO.MaQrUrl = $"{baseUrl}/qr?id={qrDTO.MaTaiSan}&seri={qrDTO.SoSeri}";

            return qrDTO;
        }

        // Lấy thông tin nhóm tài sản từ chi tiết phiếu nhập
        public async Task LoadNhomTaiSanInfoAsync()
        {
            try
            {
                if (MaChiTietPN.HasValue)
                {
                    var chiTietPN = await ChiTietPhieuNhapService.LayChiTietPhieuNhapTheoMaAsync(MaChiTietPN.Value);
                    MaNhomTS = chiTietPN.MaNhomTS;

                    // Lấy tên nhóm tài sản từ mã nhóm
                    var nhomTSList = await NhomTaiSanService.LayDanhSachNhomTaiSanAsync();
                    var nhomTS = nhomTSList.FirstOrDefault(n => n.MaNhomTS == MaNhomTS);
                    if (nhomTS != null)
                    {
                        TenNhomTS = nhomTS.TenNhom;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi tải thông tin nhóm tài sản: {ex.Message}");
            }
        }
    }
}