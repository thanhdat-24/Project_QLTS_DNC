using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for PhieuBaoTriView.xaml
    /// </summary>
    public partial class PhieuBaoTriView : Window, INotifyPropertyChanged
    {
        private ObservableCollection<PhieuBaoTri> _danhSachPhieuBaoTri;
        private PhieuBaoTri _phieuBaoTriHienTai;
        private bool _isEditing = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<PhieuBaoTri> DanhSachPhieuBaoTri
        {
            get { return _danhSachPhieuBaoTri; }
            set
            {
                _danhSachPhieuBaoTri = value;
                OnPropertyChanged("DanhSachPhieuBaoTri");
            }
        }

        public PhieuBaoTri PhieuBaoTriHienTai
        {
            get { return _phieuBaoTriHienTai; }
            set
            {
                _phieuBaoTriHienTai = value;
                OnPropertyChanged("PhieuBaoTriHienTai");
            }
        }

        public PhieuBaoTriView()
        {
            InitializeComponent();

            // Khởi tạo dữ liệu mẫu
            KhoiTaoDuLieuMau();

            // Thiết lập DataContext
            this.DataContext = this;

            // Gán dữ liệu cho DataGrid
            dgPhieuBaoTri.ItemsSource = DanhSachPhieuBaoTri;

            // Đăng ký sự kiện
            DangKySuKien();

            // Thiết lập trạng thái ban đầu
            ThietLapTrangThaiBanDau();
        }

        private void KhoiTaoDuLieuMau()
        {
            DanhSachPhieuBaoTri = new ObservableCollection<PhieuBaoTri>
            {
                new PhieuBaoTri
                {
                    MaPhieu = "BT001",
                    MaTaiSan = "TS001",
                    TenTaiSan = "Máy tính văn phòng",
                    LoaiBaoTri = "Định kỳ",
                    NgayBaoTri = DateTime.Now.AddDays(-10),
                    NgayHoanThanh = DateTime.Now.AddDays(-8),
                    NguoiPhuTrach = "Nguyễn Văn A",
                    TrangThai = "Hoàn thành",
                    ChiPhiDuKien = 500000,
                    NoiDungBaoTri = "Vệ sinh máy tính, cập nhật phần mềm"
                },
                new PhieuBaoTri
                {
                    MaPhieu = "BT002",
                    MaTaiSan = "TS002",
                    TenTaiSan = "Máy in laser",
                    LoaiBaoTri = "Đột xuất",
                    NgayBaoTri = DateTime.Now.AddDays(-5),
                    NgayHoanThanh = DateTime.Now.AddDays(5),
                    NguoiPhuTrach = "Trần Thị B",
                    TrangThai = "Đang thực hiện",
                    ChiPhiDuKien = 1200000,
                    NoiDungBaoTri = "Thay thế linh kiện hỏng, tái cân chỉnh"
                },
                new PhieuBaoTri
                {
                    MaPhieu = "BT003",
                    MaTaiSan = "TS005",
                    TenTaiSan = "Điều hòa phòng họp",
                    LoaiBaoTri = "Bảo hành",
                    NgayBaoTri = DateTime.Now.AddDays(2),
                    NgayHoanThanh = DateTime.Now.AddDays(4),
                    NguoiPhuTrach = "Lê Văn C",
                    TrangThai = "Chờ thực hiện",
                    ChiPhiDuKien = 0,
                    NoiDungBaoTri = "Kiểm tra lỗi không làm lạnh, thay gas điều hòa"
                }
            };

            PhieuBaoTriHienTai = new PhieuBaoTri();
        }

        private void DangKySuKien()
        {
            // Sự kiện cho các nút
            btnThem.Click += BtnThem_Click;
            btnLuu.Click += BtnLuu_Click;
            btnHuy.Click += BtnHuy_Click;
            btnIn.Click += BtnIn_Click;

            // Sự kiện cho DataGrid
            dgPhieuBaoTri.SelectionChanged += DgPhieuBaoTri_SelectionChanged;

            // Sự kiện validate nhập liệu
            txtChiPhiDuKien.PreviewTextInput += TxtChiPhiDuKien_PreviewTextInput;

            // Sự kiện khi chọn một dòng trong DataGrid
            dgPhieuBaoTri.MouseDoubleClick += DgPhieuBaoTri_MouseDoubleClick;
        }

        private void DgPhieuBaoTri_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgPhieuBaoTri.SelectedItem != null)
            {
                _isEditing = true;
                SetReadOnlyMode(false);
            }
        }

        private void ThietLapTrangThaiBanDau()
        {
            // Thiết lập trạng thái ban đầu cho các controls
            SetReadOnlyMode(true);
            LamMoiPhieuBaoTri();

            // Thiết lập ngày mặc định
            dtpNgayBaoTri.SelectedDate = DateTime.Now;
            dtpNgayHoanThanh.SelectedDate = DateTime.Now.AddDays(7);

            // Thiết lập giá trị mặc định cho combobox
            if (cboLoaiBaoTri.Items.Count > 0)
                cboLoaiBaoTri.SelectedIndex = 0;

            if (cboDonViBaoTri.Items.Count > 0)
                cboDonViBaoTri.SelectedIndex = 0;

            if (cboTrangThai.Items.Count > 0)
                cboTrangThai.SelectedIndex = 0;
        }

        private void LamMoiPhieuBaoTri()
        {
            PhieuBaoTriHienTai = new PhieuBaoTri();

            txtMaPhieu.Text = TaoMaPhieuMoi();
            txtMaTaiSan.Text = string.Empty;
            txtTenTaiSan.Text = string.Empty;
            txtNguoiPhuTrach.Text = string.Empty;
            txtChiPhiDuKien.Text = "0";
            txtNoiDungBaoTri.Text = string.Empty;

            if (cboLoaiBaoTri.Items.Count > 0)
                cboLoaiBaoTri.SelectedIndex = 0;

            if (cboDonViBaoTri.Items.Count > 0)
                cboDonViBaoTri.SelectedIndex = 0;

            if (cboTrangThai.Items.Count > 0)
                cboTrangThai.SelectedIndex = 0;

            dtpNgayBaoTri.SelectedDate = DateTime.Now;
            dtpNgayHoanThanh.SelectedDate = DateTime.Now.AddDays(7);
        }

        private string TaoMaPhieuMoi()
        {
            // Logic tạo mã phiếu tự động
            int soPhieu = DanhSachPhieuBaoTri.Count + 1;
            return $"BT{soPhieu:000}";
        }

        private void SetReadOnlyMode(bool isReadOnly)
        {
            // Thiết lập trạng thái readonly cho các controls
            txtMaPhieu.IsReadOnly = true; // Mã phiếu luôn readonly
            txtMaTaiSan.IsReadOnly = isReadOnly;
            txtTenTaiSan.IsReadOnly = true; // Tên tài sản luôn readonly vì lấy từ database
            txtNguoiPhuTrach.IsReadOnly = isReadOnly;
            txtChiPhiDuKien.IsReadOnly = isReadOnly;
            txtNoiDungBaoTri.IsReadOnly = isReadOnly;

            cboLoaiBaoTri.IsEnabled = !isReadOnly;
            cboDonViBaoTri.IsEnabled = !isReadOnly;
            cboTrangThai.IsEnabled = !isReadOnly;

            dtpNgayBaoTri.IsEnabled = !isReadOnly;
            dtpNgayHoanThanh.IsEnabled = !isReadOnly;

            // Thiết lập trạng thái cho các nút
            btnLuu.IsEnabled = !isReadOnly;
            btnHuy.IsEnabled = !isReadOnly;
            btnThem.IsEnabled = isReadOnly;
            btnIn.IsEnabled = isReadOnly && !string.IsNullOrEmpty(txtMaPhieu.Text);
        }

        #region Xử lý sự kiện

        private void BtnThem_Click(object sender, RoutedEventArgs e)
        {
            _isEditing = false;
            LamMoiPhieuBaoTri();
            SetReadOnlyMode(false);
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!KiemTraDuLieu())
            {
                return;
            }

            // Cập nhật dữ liệu từ form vào đối tượng
            PhieuBaoTri phieuMoi = new PhieuBaoTri
            {
                MaPhieu = txtMaPhieu.Text,
                MaTaiSan = txtMaTaiSan.Text,
                TenTaiSan = txtTenTaiSan.Text,
                LoaiBaoTri = cboLoaiBaoTri.SelectedItem != null ? ((ComboBoxItem)cboLoaiBaoTri.SelectedItem).Content.ToString() : string.Empty,
                NgayBaoTri = dtpNgayBaoTri.SelectedDate ?? DateTime.Now,
                NgayHoanThanh = dtpNgayHoanThanh.SelectedDate,
                NguoiPhuTrach = txtNguoiPhuTrach.Text,
                TrangThai = cboTrangThai.SelectedItem != null ? ((ComboBoxItem)cboTrangThai.SelectedItem).Content.ToString() : string.Empty,
                ChiPhiDuKien = decimal.TryParse(txtChiPhiDuKien.Text, out decimal chiPhi) ? chiPhi : 0,
                NoiDungBaoTri = txtNoiDungBaoTri.Text
            };

            if (_isEditing)
            {
                // Cập nhật phiếu đã có
                int index = TimChiSoPhieuBaoTri(PhieuBaoTriHienTai.MaPhieu);
                if (index >= 0)
                {
                    DanhSachPhieuBaoTri[index] = phieuMoi;
                }
            }
            else
            {
                // Thêm phiếu mới
                DanhSachPhieuBaoTri.Add(phieuMoi);
            }

            // Cập nhật lại DataGrid
            dgPhieuBaoTri.Items.Refresh();
            SetReadOnlyMode(true);
            MessageBox.Show("Lưu phiếu bảo trì thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private int TimChiSoPhieuBaoTri(string maPhieu)
        {
            for (int i = 0; i < DanhSachPhieuBaoTri.Count; i++)
            {
                if (DanhSachPhieuBaoTri[i].MaPhieu == maPhieu)
                {
                    return i;
                }
            }
            return -1;
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            SetReadOnlyMode(true);

            if (_isEditing && PhieuBaoTriHienTai != null)
            {
                HienThiThongTinPhieu(PhieuBaoTriHienTai);
            }
            else
            {
                LamMoiPhieuBaoTri();
            }
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            if (PhieuBaoTriHienTai != null && !string.IsNullOrEmpty(PhieuBaoTriHienTai.MaPhieu))
            {
                MessageBox.Show($"Đang in phiếu bảo trì {PhieuBaoTriHienTai.MaPhieu}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                // TODO: Thêm mã in phiếu tại đây
            }
            else
            {
                MessageBox.Show("Vui lòng chọn phiếu bảo trì để in!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DgPhieuBaoTri_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = dgPhieuBaoTri.SelectedItem as PhieuBaoTri;
            if (selectedItem != null)
            {
                PhieuBaoTriHienTai = selectedItem;
                HienThiThongTinPhieu(selectedItem);
                // Cập nhật trạng thái nút In
                btnIn.IsEnabled = true;
            }
        }

        private void TxtChiPhiDuKien_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Chỉ cho phép nhập số
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        #endregion

        private bool KiemTraDuLieu()
        {
            if (string.IsNullOrWhiteSpace(txtMaPhieu.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã phiếu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                txtMaPhieu.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtMaTaiSan.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã tài sản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                txtMaTaiSan.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtNguoiPhuTrach.Text))
            {
                MessageBox.Show("Vui lòng nhập Người phụ trách!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                txtNguoiPhuTrach.Focus();
                return false;
            }

            if (dtpNgayBaoTri.SelectedDate == null)
            {
                MessageBox.Show("Vui lòng chọn Ngày bảo trì!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                dtpNgayBaoTri.Focus();
                return false;
            }

            if (dtpNgayHoanThanh.SelectedDate != null && dtpNgayHoanThanh.SelectedDate < dtpNgayBaoTri.SelectedDate)
            {
                MessageBox.Show("Ngày hoàn thành không thể trước Ngày bảo trì!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                dtpNgayHoanThanh.Focus();
                return false;
            }

            return true;
        }

        private void HienThiThongTinPhieu(PhieuBaoTri phieu)
        {
            if (phieu == null) return;

            txtMaPhieu.Text = phieu.MaPhieu;
            txtMaTaiSan.Text = phieu.MaTaiSan;
            txtTenTaiSan.Text = phieu.TenTaiSan;
            txtNguoiPhuTrach.Text = phieu.NguoiPhuTrach;
            txtChiPhiDuKien.Text = phieu.ChiPhiDuKien.ToString("N0");
            txtNoiDungBaoTri.Text = phieu.NoiDungBaoTri;

            dtpNgayBaoTri.SelectedDate = phieu.NgayBaoTri;
            dtpNgayHoanThanh.SelectedDate = phieu.NgayHoanThanh;

            // Tìm và chọn giá trị tương ứng trong ComboBox
            SelectComboBoxItem(cboLoaiBaoTri, phieu.LoaiBaoTri);
            SelectComboBoxItem(cboTrangThai, phieu.TrangThai);

            // Đơn vị bảo trì mặc định chọn đầu tiên
            if (cboDonViBaoTri.Items.Count > 0)
                cboDonViBaoTri.SelectedIndex = 0;
        }

        private void SelectComboBoxItem(ComboBox comboBox, string value)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString() == value)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PhieuBaoTri : INotifyPropertyChanged
    {
        private string _maPhieu;
        private string _maTaiSan;
        private string _tenTaiSan;
        private string _loaiBaoTri;
        private DateTime _ngayBaoTri;
        private DateTime? _ngayHoanThanh;
        private string _nguoiPhuTrach;
        private string _trangThai;
        private decimal _chiPhiDuKien;
        private string _noiDungBaoTri;
        private string _donViBaoTri;

        public event PropertyChangedEventHandler PropertyChanged;

        public string MaPhieu
        {
            get { return _maPhieu; }
            set
            {
                _maPhieu = value;
                OnPropertyChanged("MaPhieu");
            }
        }

        public string MaTaiSan
        {
            get { return _maTaiSan; }
            set
            {
                _maTaiSan = value;
                OnPropertyChanged("MaTaiSan");
            }
        }

        public string TenTaiSan
        {
            get { return _tenTaiSan; }
            set
            {
                _tenTaiSan = value;
                OnPropertyChanged("TenTaiSan");
            }
        }

        public string LoaiBaoTri
        {
            get { return _loaiBaoTri; }
            set
            {
                _loaiBaoTri = value;
                OnPropertyChanged("LoaiBaoTri");
            }
        }

        public DateTime NgayBaoTri
        {
            get { return _ngayBaoTri; }
            set
            {
                _ngayBaoTri = value;
                OnPropertyChanged("NgayBaoTri");
            }
        }

        public DateTime? NgayHoanThanh
        {
            get { return _ngayHoanThanh; }
            set
            {
                _ngayHoanThanh = value;
                OnPropertyChanged("NgayHoanThanh");
            }
        }

        public string NguoiPhuTrach
        {
            get { return _nguoiPhuTrach; }
            set
            {
                _nguoiPhuTrach = value;
                OnPropertyChanged("NguoiPhuTrach");
            }
        }

        public string TrangThai
        {
            get { return _trangThai; }
            set
            {
                _trangThai = value;
                OnPropertyChanged("TrangThai");
            }
        }

        public decimal ChiPhiDuKien
        {
            get { return _chiPhiDuKien; }
            set
            {
                _chiPhiDuKien = value;
                OnPropertyChanged("ChiPhiDuKien");
            }
        }

        public string NoiDungBaoTri
        {
            get { return _noiDungBaoTri; }
            set
            {
                _noiDungBaoTri = value;
                OnPropertyChanged("NoiDungBaoTri");
            }
        }

        public string DonViBaoTri
        {
            get { return _donViBaoTri; }
            set
            {
                _donViBaoTri = value;
                OnPropertyChanged("DonViBaoTri");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}