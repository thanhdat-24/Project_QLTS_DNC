using Project_QLTS_DNC.Models.PhanQuyen;
using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.Services.TaiKhoan;
using Project_QLTS_DNC.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

public class PhanQuyenFormViewModel : INotifyPropertyChanged
{
    private readonly LoaiTaiKhoanService _loaiTaiKhoanService;
    private readonly PhanQuyenService _phanQuyenService = new PhanQuyenService();

    private ObservableCollection<LoaiTaiKhoanModel> _danhSachLoaiTaiKhoan;
    private LoaiTaiKhoanModel _loaiTaiKhoanDuocChon;
    private ObservableCollection<NhomChucNang> _danhSachNhomChucNang;

    public ObservableCollection<LoaiTaiKhoanModel> DanhSachLoaiTaiKhoan
    {
        get => _danhSachLoaiTaiKhoan;
        set { _danhSachLoaiTaiKhoan = value; OnPropertyChanged(); }
    }

    public LoaiTaiKhoanModel LoaiTaiKhoanDuocChon
    {
        get => _loaiTaiKhoanDuocChon;
        set { _loaiTaiKhoanDuocChon = value; LoadDanhSachPhanQuyen(); OnPropertyChanged(); }
    }

    public ObservableCollection<NhomChucNang> DanhSachNhomChucNang
    {
        get => _danhSachNhomChucNang;
        set { _danhSachNhomChucNang = value; OnPropertyChanged(); }
    }

    public ICommand HuyCommand { get; }
    public ICommand LuuThayDoiCommand { get; }

    public PhanQuyenFormViewModel(LoaiTaiKhoanService loaiTaiKhoanService)
    {
        _loaiTaiKhoanService = loaiTaiKhoanService;

        HuyCommand = new RelayCommand(Huy);
        LuuThayDoiCommand = new RelayCommand(async () => await LuuThayDoi(), () => LoaiTaiKhoanDuocChon != null);
    }

    public async Task LoadDataAsync()
    {
        await LoadDanhSachLoaiTaiKhoan();
    }

    private async Task LoadDanhSachLoaiTaiKhoan()
    {
        try
        {
            var danhSach = await _loaiTaiKhoanService.LayDSLoaiTK();
            DanhSachLoaiTaiKhoan = new ObservableCollection<LoaiTaiKhoanModel>(danhSach);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải loại tài khoản: {ex.Message}", "Thông báo");
        }
    }

    private async void LoadDanhSachPhanQuyen()
    {
        if (LoaiTaiKhoanDuocChon == null) return;

        try
        {
            var danhSach = await _phanQuyenService.LayDanhSachQuyenTheoLoaiTkAsync(LoaiTaiKhoanDuocChon.MaLoaiTk);

            if (danhSach == null || !danhSach.Any())
            {
                MessageBox.Show("Danh sách quyền rỗng hoặc không tải được dữ liệu.", "Cảnh báo");
                return;
            }

            foreach (var pq in danhSach)
            {
                if (string.IsNullOrWhiteSpace(pq.TenChucNang))
                {
                    pq.TenChucNang = $"Màn hình {pq.MaManHinh}";
                }
            }

            var danhSachPhanQuyen = danhSach.Select(pq => new PhanQuyen
            {
                TenChucNang = pq.TenChucNang,
                MaQuyen = pq.MaQuyen,
                MaManHinh = pq.MaManHinh,
                Xem = pq.Xem,
                Them = pq.Them,
                Sua = pq.Sua,
                Xoa = pq.Xoa,
                HienThi = pq.HienThi,
                IconKind = "CheckboxMarked"
            });

            DanhSachNhomChucNang = new ObservableCollection<NhomChucNang>(
                danhSachPhanQuyen
                    .GroupBy(p => LayNhomTuMaManHinh(p.MaManHinh))
                    .Select(g => new NhomChucNang
                    {
                        TenNhom = g.Key,
                        DanhSachQuyen = new ObservableCollection<PhanQuyen>(g.ToList())
                    })
            );

            QuyenNguoiDungHelper.DanhSachMaManHinhDuocHienThi = DanhSachNhomChucNang
                .SelectMany(n => n.DanhSachQuyen)
                .Where(p => p.HienThi)
                .Select(p => p.MaManHinh)
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải phân quyền: {ex.Message}", "Lỗi");
        }
    }

    private string LayNhomTuMaManHinh(string ma)
    {
        if (string.IsNullOrWhiteSpace(ma)) return "Khác";

        return ma switch
        {
            "btnTrangChu" => "Trang chủ",
            "btnQuanlyTaiKhoan" or "btnDanhSachTaiKhoan" or "btnLoaiTaiKhoan" or "btnPhanQuyenTk" => "Quản lý tài khoản",
            "btnQuanlyNhansu" or "btnNhanVien" or "btnChucVu" => "Quản lý nhân sự",
            "btnQuanLyLoaiTaiSan" => "Loại tài sản",
            "btnQuanlyToaNha" or "btnToaNha" or "btnTang" or "btnPhong" or "btnPhongBan" => "Tòa nhà",
            "btnQuanLyKho" or "btnDanhSachKho" or "btnNhapKho" or "btnXuatKho" or "btnTonKho" or "btnBanGiaoTaiSan" => "Quản lý kho",
            "btnNhaCungCap" => "Nhà cung cấp",
            "btnTraCuuTaiSan" => "Tra cứu tài sản",
            "btn_Baotri" or "btnBaoTri" or "btnPhieubaotri" or "btnDSbaotri" => "Bảo trì",
            "btn_muaMoi" or "btnPhieuMuaMoi" or "btnChiTietPhieuMuaMoi" => "Mua mới",
            "btnBaoCaoKiemKe" => "Báo cáo kiểm kê",
            "btnDuyetPhieu" => "Duyệt phiếu",
            "btn_CaiDat" or "btnThongTinCongTy" or "btnPhieuIn" => "Cài đặt",
            _ => "Khác"
        };
    }

    private void Huy()
    {
        LoadDanhSachPhanQuyen();
    }

    private async Task LuuThayDoi()
    {
        try
        {
            if (LoaiTaiKhoanDuocChon == null) return;

            var danhSach = DanhSachNhomChucNang
                .SelectMany(n => n.DanhSachQuyen)
                .Select(pq => new PhanQuyenModel
                {
                    MaLoaiTk = LoaiTaiKhoanDuocChon.MaLoaiTk,
                    MaQuyen = pq.MaQuyen,
                    Xem = pq.Xem,
                    Them = pq.Them,
                    Sua = pq.Sua,
                    Xoa = pq.Xoa,
                    HienThi = pq.HienThi
                }).ToList();

            bool result = await _phanQuyenService.LuuDanhSachPhanQuyenAsync(danhSach);

            if (result)
                MessageBox.Show("Lưu phân quyền thành công!", "Thông báo");
            else
                MessageBox.Show("Không thể lưu phân quyền.", "Lỗi");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
        }
    }

    public async Task LuuThayDoiAsync()
    {
        await LuuThayDoi();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class PhanQuyen : INotifyPropertyChanged
{
    public long MaQuyen { get; set; }
    public string TenChucNang { get; set; }
    public string MaManHinh { get; set; }
    public string IconKind { get; set; }

    private bool _xem;
    private bool _them;
    private bool _sua;
    private bool _xoa;
    private bool _hienThi;

    public bool Xem { get => _xem; set { _xem = value; OnPropertyChanged(); } }
    public bool Them { get => _them; set { _them = value; OnPropertyChanged(); } }
    public bool Sua { get => _sua; set { _sua = value; OnPropertyChanged(); } }
    public bool Xoa { get => _xoa; set { _xoa = value; OnPropertyChanged(); } }
    public bool HienThi { get => _hienThi; set { _hienThi = value; OnPropertyChanged(); } }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class NhomChucNang
{
    public string TenNhom { get; set; }
    public ObservableCollection<PhanQuyen> DanhSachQuyen { get; set; }

    public bool HienThi
    {
        get => DanhSachQuyen != null && DanhSachQuyen.All(q => q.HienThi);
        set
        {
            if (DanhSachQuyen != null)
            {
                foreach (var q in DanhSachQuyen)
                {
                    q.HienThi = value;
                }
            }
        }
    }
}