using Project_QLTS_DNC.Models.TaiKhoan;
using Project_QLTS_DNC.Services.TaiKhoan;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using Project_QLTS_DNC.Models.PhanQuyen;
using Project_QLTS_DNC.Helpers;
public class PhanQuyenFormViewModel : INotifyPropertyChanged
{
    private readonly LoaiTaiKhoanService _loaiTaiKhoanService;
    private ObservableCollection<LoaiTaiKhoanModel> _danhSachLoaiTaiKhoan;
    private LoaiTaiKhoanModel _loaiTaiKhoanDuocChon;
    private ObservableCollection<PhanQuyen> _danhSachPhanQuyen;

    public ObservableCollection<LoaiTaiKhoanModel> DanhSachLoaiTaiKhoan
    {
        get => _danhSachLoaiTaiKhoan;
        set
        {
            _danhSachLoaiTaiKhoan = value;
            OnPropertyChanged(nameof(DanhSachLoaiTaiKhoan));
        }
    }

    public LoaiTaiKhoanModel LoaiTaiKhoanDuocChon
    {
        get => _loaiTaiKhoanDuocChon;
        set
        {
            _loaiTaiKhoanDuocChon = value;
            LoadDanhSachPhanQuyen();
            OnPropertyChanged(nameof(LoaiTaiKhoanDuocChon));
        }
    }

    public ObservableCollection<PhanQuyen> DanhSachPhanQuyen
    {
        get => _danhSachPhanQuyen;
        set
        {
            _danhSachPhanQuyen = value;
            OnPropertyChanged(nameof(DanhSachPhanQuyen));
        }
    }

    public ICommand HuyCommand { get; }
    public ICommand LuuThayDoiCommand { get; }

    public PhanQuyenFormViewModel(LoaiTaiKhoanService loaiTaiKhoanService)
    {
        _loaiTaiKhoanService = loaiTaiKhoanService;

        HuyCommand = new RelayCommand(Huy);
        LuuThayDoiCommand = new RelayCommand(LuuThayDoi, () => LoaiTaiKhoanDuocChon != null);

        LoadDanhSachLoaiTaiKhoan();
    }

    private async void LoadDanhSachLoaiTaiKhoan()
    {
        try
        {
            var danhSach = await _loaiTaiKhoanService.LayDSLoaiTK();
            DanhSachLoaiTaiKhoan = new ObservableCollection<LoaiTaiKhoanModel>(danhSach);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo");
        }
    }

    private void LoadDanhSachPhanQuyen()
    {
        if (LoaiTaiKhoanDuocChon == null) return;

        DanhSachPhanQuyen = new ObservableCollection<PhanQuyen>
        {
            // Trang chủ
            new PhanQuyen {
                TenChucNang = "Trang chủ",
                IconKind = "Home",
                Xem = true,
                HienThi = true
            },

            // Quản lý tài khoản
            new PhanQuyen {
                TenChucNang = "Danh sách tài khoản",
                IconKind = "AccountList"
            },
            new PhanQuyen {
                TenChucNang = "Loại tài khoản",
                IconKind = "AccountKey"
            },
            new PhanQuyen {
                TenChucNang = "Phân quyền",
                IconKind = "AccountCog"
            },

            // Quản lý nhân sự
            new PhanQuyen {
                TenChucNang = "Danh sách nhân viên",
                IconKind = "AccountGroup"
            },
            new PhanQuyen {
                TenChucNang = "Chức vụ",
                IconKind = "AccountTie"
            },

            // Quản lý loại tài sản
            new PhanQuyen {
                TenChucNang = "Quản lý loại tài sản",
                IconKind = "Archive"
            },

            // Quản lý tòa nhà
            new PhanQuyen {
                TenChucNang = "Tòa nhà",
                IconKind = "Domain"
            },
            new PhanQuyen {
                TenChucNang = "Tầng",
                IconKind = "FloorPlan"
            },
            new PhanQuyen {
                TenChucNang = "Phòng",
                IconKind = "RoomService"
            },
            new PhanQuyen {
                TenChucNang = "Phòng ban",
                IconKind = "Office"
            },

            // Quản lý kho
            new PhanQuyen {
                TenChucNang = "Quản lý kho",
                IconKind = "Warehouse"
            },
            new PhanQuyen {
                TenChucNang = "Nhập kho",
                IconKind = "ArchiveDownload"
            },
            new PhanQuyen {
                TenChucNang = "Xuất kho",
                IconKind = "ArchiveUpload"
            },
            new PhanQuyen {
                TenChucNang = "Tồn kho",
                IconKind = "Archive"
            },

            // Các chức năng khác
            new PhanQuyen {
                TenChucNang = "Quản lý nhà cung cấp",
                IconKind = "Factory"
            },
            new PhanQuyen {
                TenChucNang = "Tra cứu tài sản",
                IconKind = "Magnify"
            },

            // Bảo trì
            new PhanQuyen {
                TenChucNang = "Loại bảo trì",
                IconKind = "Tools"
            },
            new PhanQuyen {
                TenChucNang = "Danh sách bảo trì",
                IconKind = "Wrench"
            },

            // Báo cáo và duyệt
            new PhanQuyen {
                TenChucNang = "Báo cáo kiểm kê",
                IconKind = "FileChart"
            },
            new PhanQuyen {
                TenChucNang = "Duyệt phiếu",
                IconKind = "FileCheck"
            },

            // Cài đặt
            new PhanQuyen {
                TenChucNang = "Thông tin công ty",
                IconKind = "Domain"
            },
            new PhanQuyen {
                TenChucNang = "Cài đặt phiếu in",
                IconKind = "Printer"
            }
        };
    }

    private void Huy()
    {
        // Reload danh sách phân quyền
        LoadDanhSachPhanQuyen();
    }

    private void LuuThayDoi()
    {
        try
        {
            // TODO: Lưu phân quyền vào database
            MessageBox.Show("Lưu phân quyền thành công!", "Thông báo");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
        }
    }

    // Implement INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}