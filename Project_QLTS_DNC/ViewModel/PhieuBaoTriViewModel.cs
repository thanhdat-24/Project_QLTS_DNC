using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Project_QLTS_DNC.Helpers;
using System.Windows;
using Project_QLTS_DNC.Repositories;
using Project_QLTS_DNC.Models.BaoTri;

namespace Project_QLTS_DNC.ViewModel
{
    public class PhieuBaoTriViewModel : INotifyPropertyChanged
    {
        private readonly PhieuBaoTriRepository _repository;

        // Danh sách phiếu bảo trì
        private ObservableCollection<PhieuBaoTri> _danhSachPhieuBaoTri;
        public ObservableCollection<PhieuBaoTri> DanhSachPhieuBaoTri
        {
            get => _danhSachPhieuBaoTri;
            set
            {
                _danhSachPhieuBaoTri = value;
                OnPropertyChanged(nameof(DanhSachPhieuBaoTri));
            }
        }

        // Phiếu bảo trì được chọn
        private PhieuBaoTri _phieuDuocChon;
        public PhieuBaoTri PhieuDuocChon
        {
            get => _phieuDuocChon;
            set
            {
                _phieuDuocChon = value;
                OnPropertyChanged(nameof(PhieuDuocChon));
            }
        }

        // Các thuộc tính tìm kiếm
        private string _tuKhoa;
        public string TuKhoa
        {
            get => _tuKhoa;
            set
            {
                _tuKhoa = value;
                OnPropertyChanged(nameof(TuKhoa));
            }
        }

        private string _trangThai;
        public string TrangThai
        {
            get => _trangThai;
            set
            {
                _trangThai = value;
                OnPropertyChanged(nameof(TrangThai));
            }
        }

        // Các lệnh (Commands)
        public ICommand NapDanhSachCommand { get; }
        public ICommand ThemPhieuCommand { get; }
        public ICommand CapNhatPhieuCommand { get; }
        public ICommand XoaPhieuCommand { get; }
        public ICommand TimKiemCommand { get; }
        public ICommand LamMoiCommand { get; }

        // Constructor
        public PhieuBaoTriViewModel(PhieuBaoTriRepository repository)
        {
            _repository = repository;
            DanhSachPhieuBaoTri = new ObservableCollection<PhieuBaoTri>();

            // Khởi tạo các Commands
            NapDanhSachCommand = new RelayCommand(NapDanhSach);
            ThemPhieuCommand = new RelayCommand(ThemPhieu);
            CapNhatPhieuCommand = new RelayCommand(CapNhatPhieu, () => PhieuDuocChon != null);
            XoaPhieuCommand = new RelayCommand(XoaPhieu, () => PhieuDuocChon != null);
            TimKiemCommand = new RelayCommand(TimKiem);
            LamMoiCommand = new RelayCommand(LamMoi);

            // Nạp danh sách ban đầu
            NapDanhSach();
        }

        // Các phương thức xử lý
        private void NapDanhSach()
        {
            try
            {
                var danhSach = _repository.LayDanhSachPhieuBaoTri();
                DanhSachPhieuBaoTri.Clear();
                foreach (var phieu in danhSach)
                {
                    DanhSachPhieuBaoTri.Add(phieu);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi nạp danh sách: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThemPhieu()
        {
            try
            {
                // Tạo phiếu mới với giá trị mặc định
                var phieuMoi = new PhieuBaoTri
                {
                    NgayBaoTri = DateTime.Now,
                    TrangThai = "Chờ thực hiện"
                };

                // Gọi repository để thêm
                int maPhieu = _repository.ThemPhieuBaoTri(phieuMoi);
                phieuMoi.MaPhieu = maPhieu.ToString();

                // Thêm vào danh sách
                DanhSachPhieuBaoTri.Add(phieuMoi);

                MessageBox.Show("Thêm phiếu bảo trì thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thêm phiếu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CapNhatPhieu()
        {
            try
            {
                if (PhieuDuocChon == null)
                {
                    MessageBox.Show("Vui lòng chọn phiếu bảo trì để cập nhật!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool ketQua = _repository.CapNhatPhieuBaoTri(PhieuDuocChon);
                if (ketQua)
                {
                    // Làm mới danh sách để hiển thị dữ liệu cập nhật
                    NapDanhSach();
                    MessageBox.Show("Cập nhật phiếu bảo trì thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Cập nhật không thành công. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi cập nhật phiếu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void XoaPhieu()
        {
            try
            {
                if (PhieuDuocChon == null)
                {
                    MessageBox.Show("Vui lòng chọn phiếu bảo trì để xóa!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var xacNhan = MessageBox.Show(
                    "Bạn có chắc chắn muốn xóa phiếu bảo trì này?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (xacNhan == MessageBoxResult.Yes)
                {
                    bool ketQua = _repository.XoaPhieuBaoTri(PhieuDuocChon.MaPhieu);
                    if (ketQua)
                    {
                        DanhSachPhieuBaoTri.Remove(PhieuDuocChon);
                        MessageBox.Show("Xóa phiếu bảo trì thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Xóa không thành công. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa phiếu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TimKiem()
        {
            try
            {
                var ketQuaTimKiem = _repository.TimKiemPhieuBaoTri(TuKhoa, TrangThai);

                DanhSachPhieuBaoTri.Clear();
                foreach (var phieu in ketQuaTimKiem)
                {
                    DanhSachPhieuBaoTri.Add(phieu);
                }

                if (ketQuaTimKiem.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy phiếu bảo trì phù hợp.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LamMoi()
        {
            // Làm mới các điều kiện tìm kiếm
            TuKhoa = string.Empty;
            TrangThai = string.Empty;

            // Nạp lại toàn bộ danh sách
            NapDanhSach();
        }

        // Implement INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}