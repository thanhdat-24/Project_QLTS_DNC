using Project_QLTS_DNC.Helpers;
using Project_QLTS_DNC.Models.NhanVien;
using Project_QLTS_DNC.Services.ChucVu;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

public class DanhSachChucVuViewModel : INotifyPropertyChanged
{
    private readonly ChucVuService _chucVuService;

    public ObservableCollection<ChucVuModel> DanhSachChucVu { get; set; } = new ObservableCollection<ChucVuModel>();
    public ICommand XoaChucVuCommand { get; }

    public DanhSachChucVuViewModel()
    {
        _chucVuService = new ChucVuService();
        XoaChucVuCommand = new RelayCommand<int>(async (maChucVu) => await XoaChucVuAsync(maChucVu));
        _ = LoadChucVuAsync();
    }

    public async Task LoadChucVuAsync()
    {
        var list = await _chucVuService.GetAllChucVuAsync();
        DanhSachChucVu.Clear();

        foreach (var item in list)
            DanhSachChucVu.Add(item);

        OnPropertyChanged(nameof(DanhSachChucVu));
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

    // Ensure this method accepts a string keyword
    public async Task<List<ChucVuModel>> TimKiemChucVuAsync(string keyword)
    {
        var chucVuService = new ChucVuService();
        var ketQua = await chucVuService.TimKiemChucVuAsync(keyword);
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

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
