using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Project_QLTS_DNC.Helpers;
using System.Windows;
using Project_QLTS_DNC.Repositories;
using Project_QLTS_DNC.Models.BaoHong;

namespace Project_QLTS_DNC.ViewModel
{
    public class PhieuBaoHongViewModel : INotifyPropertyChanged
    {
        private readonly PhieuBaoHongRepository _repository;

        // Danh sách phiếu báo hỏng
        private ObservableCollection<PhieuBaoHong> _danhSachPhieuBaoHong;
        public ObservableCollection<PhieuBaoHong> DanhSachPhieuBaoHong
        {
            get => _danhSachPhieuBaoHong;
            set
            {
                _danhSachPhieuBaoHong = value;
                OnPropertyChanged(nameof(DanhSachPhieuBaoHong));
            }
        }

        // Phiếu báo hỏng được chọn
        private PhieuBaoHong _phieuDuocChon;
        public PhieuBaoHong PhieuDuocChon
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

        private string _mucDoHong;
        public string MucDoHong
        {
            get => _mucDoHong;
            set
            {
                _mucDoHong = value;
                OnPropertyChanged(nameof(MucDoHong));
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
        public PhieuBaoHongViewModel(PhieuBaoHongRepository repository)
        {
            _repository = repository;
            DanhSachPhieuBaoHong = new ObservableCollection<PhieuBaoHong>();

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
                var danhSach = _repository.LayDanhSachPhieuBaoHong();
                DanhSachPhieuBaoHong.Clear();
                foreach (var phieu in danhSach)
                {
                    DanhSachPhieuBaoHong.Add(phieu);
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
                // Mở form nhập liệu để thêm phiếu mới
                var phieuMoi = new PhieuBaoHong
                {
                    NgayLap = DateTime.Now,
                    TrangThai = "Mới tạo"
                };

                // Gọi repository để thêm
                int maphieu = _repository.ThemPhieuBaoHong(phieuMoi);
                phieuMoi.MaPhieu = maphieu.ToString();

                // Thêm vào danh sách
                DanhSachPhieuBaoHong.Add(phieuMoi);

                MessageBox.Show("Thêm phiếu báo hỏng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    MessageBox.Show("Vui lòng chọn phiếu cần cập nhật!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool ketQua = _repository.CapNhatPhieuBaoHong(PhieuDuocChon);
                if (ketQua)
                {
                    MessageBox.Show("Cập nhật phiếu báo hỏng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    MessageBox.Show("Vui lòng chọn phiếu cần xóa!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var ketQua = MessageBox.Show("Bạn có chắc chắn muốn xóa phiếu này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (ketQua == MessageBoxResult.Yes)
                {
                    bool xoaThanhCong = _repository.XoaPhieuBaoHong(PhieuDuocChon.MaPhieu);
                    if (xoaThanhCong)
                    {
                        DanhSachPhieuBaoHong.Remove(PhieuDuocChon);
                        MessageBox.Show("Xóa phiếu báo hỏng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
                var ketQuaTimKiem = _repository.TimKiemPhieuBaoHong(TuKhoa, TrangThai, MucDoHong);

                DanhSachPhieuBaoHong.Clear();
                foreach (var phieu in ketQuaTimKiem)
                {
                    DanhSachPhieuBaoHong.Add(phieu);
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
            MucDoHong = string.Empty;

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