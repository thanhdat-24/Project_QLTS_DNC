using Project_QLTS_DNC.Helpers;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Services.ChucVu;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using System;

public class DanhSachChucVuViewModel : INotifyPropertyChanged
{
    private readonly ChucVuService _chucVuService;

    // Danh sách chức vụ đầy đủ
    private List<ChucVuModel> _tatCaChucVu = new List<ChucVuModel>();

    public ObservableCollection<ChucVuModel> DanhSachChucVu { get; set; } = new ObservableCollection<ChucVuModel>();
    public ICommand XoaChucVuCommand { get; }

    public DanhSachChucVuViewModel()
    {
        _chucVuService = new ChucVuService();
        XoaChucVuCommand = new RelayCommand<int>(async (maChucVu) => await XoaChucVuAsync(maChucVu));

        VeTrangDauCommand = new RelayCommand(VeTrangDau);
        VeTrangTruocCommand = new RelayCommand(VeTrangTruoc);
        DenTrangSauCommand = new RelayCommand(DenTrangSau);
        DenTrangCuoiCommand = new RelayCommand(DenTrangCuoi);

        _ = LoadChucVuAsync();
    }

    public async Task LoadChucVuAsync()
    {
        _tatCaChucVu = await _chucVuService.GetAllChucVuAsync();

        // Tính toán phân trang
        TongSoBanGhi = _tatCaChucVu.Count;
        TongSoTrang = (int)Math.Ceiling((double)TongSoBanGhi / SoBanGhiMoiTrang);

        // Cập nhật trang đầu tiên
        CapNhatTrangHienTai();
    }

    private string _keyword;
    public string Keyword
    {
        get => _keyword;
        set
        {
            _keyword = value;
            OnPropertyChanged();
        }
    }

    public async Task<List<ChucVuModel>> TimKiemChucVuAsync(string keyword)
    {
        var chucVuService = new ChucVuService();
        var ketQua = await chucVuService.TimKiemChucVuAsync(keyword);

        // Cập nhật danh sách toàn bộ và phân trang
        _tatCaChucVu = ketQua;
        TongSoBanGhi = _tatCaChucVu.Count;
        TongSoTrang = (int)Math.Ceiling((double)TongSoBanGhi / SoBanGhiMoiTrang);
        TrangHienTai = 1; // Về trang đầu sau khi tìm kiếm

        return ketQua;
    }

    public async Task<bool> XoaChucVuAsync(int maChucVu)
    {
        var ketQua = await _chucVuService.XoaChucVuAsync(maChucVu);
        if (ketQua)
        {
            await LoadChucVuAsync();
        }
        return ketQua;
    }

    public async Task Refresh()
    {
        await LoadChucVuAsync();
    }

    // Các thuộc tính phân trang
    private int _trangHienTai = 1;
    public int TrangHienTai
    {
        get => _trangHienTai;
        set
        {
            _trangHienTai = value;
            OnPropertyChanged(nameof(TrangHienTai));
            CapNhatTrangHienTai();
        }
    }

    private int _tongSoTrang;
    public int TongSoTrang
    {
        get => _tongSoTrang;
        set
        {
            _tongSoTrang = value;
            OnPropertyChanged(nameof(TongSoTrang));
        }
    }

    private int _tongSoBanGhi;
    public int TongSoBanGhi
    {
        get => _tongSoBanGhi;
        set
        {
            _tongSoBanGhi = value;
            OnPropertyChanged(nameof(TongSoBanGhi));
        }
    }

    // Số bản ghi trên mỗi trang
    private const int SoBanGhiMoiTrang = 10;

    private void CapNhatTrangHienTai()
    {
        DanhSachChucVu.Clear();
        var danhSachTrang = _tatCaChucVu
            .Skip((TrangHienTai - 1) * SoBanGhiMoiTrang)
            .Take(SoBanGhiMoiTrang)
            .ToList();

        foreach (var item in danhSachTrang)
        {
            DanhSachChucVu.Add(item);
        }

        OnPropertyChanged(nameof(DanhSachChucVu));
    }

    private void VeTrangDau()
    {
        if (TrangHienTai > 1)
        {
            TrangHienTai = 1;
        }
    }

    private void VeTrangTruoc()
    {
        if (TrangHienTai > 1)
        {
            TrangHienTai--;
        }
    }

    private void DenTrangSau()
    {
        if (TrangHienTai < TongSoTrang)
        {
            TrangHienTai++;
        }
    }

    private void DenTrangCuoi()
    {
        if (TrangHienTai < TongSoTrang)
        {
            TrangHienTai = TongSoTrang;
        }
    }

    // Commands phân trang
    public ICommand VeTrangDauCommand { get; }
    public ICommand VeTrangTruocCommand { get; }
    public ICommand DenTrangSauCommand { get; }
    public ICommand DenTrangCuoiCommand { get; }

    // Thuộc tính để ẩn/hiện phân trang
    public bool ShowPagination => TongSoTrang > 1;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}