using System.ComponentModel;

public class PhieuMuaMoiTSViewModel : INotifyPropertyChanged
{
    private MuaMoiTS _phieu;
    private bool _isEditMode;

    // Các thuộc tính binding cho form
    private string _donViDeNghi;
    public string DonViDeNghi
    {
        get => _donViDeNghi;
        set
        {
            _donViDeNghi = value;
            OnPropertyChanged(nameof(DonViDeNghi));
        }
    }

    private string _lyDo;
    public string LyDo
    {
        get => _lyDo;
        set
        {
            _lyDo = value;
            OnPropertyChanged(nameof(LyDo));
        }
    }

    private DateTime _ngayDeNghi;
    public DateTime NgayDeNghi
    {
        get => _ngayDeNghi;
        set
        {
            _ngayDeNghi = value;
            OnPropertyChanged(nameof(NgayDeNghi));
        }
    }

    private int _maNV;
    public int MaNV
    {
        get => _maNV;
        set
        {
            _maNV = value;
            OnPropertyChanged(nameof(MaNV));
        }
    }

    public bool IsEditMode => _isEditMode;
    public int? MaPhieuDeNghi => _phieu?.MaPhieuDeNghi;

    // Constructor mặc định cho thêm mới
    public PhieuMuaMoiTSViewModel()
    {
        _phieu = new MuaMoiTS();
        _isEditMode = false;
        NgayDeNghi = DateTime.Now;
    }

    // Phương thức để khởi tạo với dữ liệu có sẵn (cho edit)
    public void LoadForEdit(MuaMoiTS phieu)
    {
        _phieu = phieu;
        _isEditMode = true;

        // Cập nhật các thuộc tính binding từ phiếu
        DonViDeNghi = phieu.DonViDeNghi;
        LyDo = phieu.LyDo;
        NgayDeNghi = phieu.NgayDeNghi;
        MaNV = (int)phieu.MaNV;
    }

    // Phương thức để lấy phiếu đã được cập nhật
    public MuaMoiTS GetUpdatedPhieu()
    {
        _phieu.DonViDeNghi = DonViDeNghi;
        _phieu.LyDo = LyDo;
        _phieu.NgayDeNghi = NgayDeNghi;
        _phieu.MaNV = MaNV;
        return _phieu;
    }

    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}