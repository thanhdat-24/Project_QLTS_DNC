﻿using Microsoft.IdentityModel.Tokens;
using Project_QLTS_DNC.Models.NhanVien;
using Supabase;
using Supabase.Postgrest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.Json.Serialization;
using Project_QLTS_DNC.Models.ToaNha;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for PhieuMuaMoiTS.xaml
    /// </summary>
    public partial class PhieuMuaMoiTS : Window
    {
        private Supabase.Client _client;
        private PhieuMuaMoiTSViewModel _viewModel;
        private bool _isEditMode = false;
        private MuaMoiTS _existingPhieu = null;

        public PhieuMuaMoiTS()
        {
            _viewModel = new PhieuMuaMoiTSViewModel();
            InitializeComponent();
            Loaded += PhieuMuaMoiTS_Loaded;
        }
        private async void PhieuMuaMoiTS_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            await LoadNhanVienAsync();
            await LoadKhoAsync();
        }

        private async Task InitializeSupabaseAsync()
        {
            string supabaseUrl = "https://hoybfwnugefnpctgghha.supabase.co";
            string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhveWJmd251Z2VmbnBjdGdnaGhhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDQxMDQ4OTEsImV4cCI6MjA1OTY4MDg5MX0.KxNfiOUFXHGgqZf3b3xOk6BR4sllMZG_-W-y_OPUwCI";

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false,
                AutoRefreshToken = false
            };

            _client = new Supabase.Client(supabaseUrl, supabaseKey, options);
            await _client.InitializeAsync();
        }
        private static async Task<int> SinhMaPhieuMuaAsync(Supabase.Client client)
        {
            var danhSach = await client.From<MuaMoiTS>().Get();

            if (danhSach?.Models == null || danhSach.Models.Count == 0)
                return 1;

            return danhSach.Models.Max(p => p.MaPhieuDeNghi) + 1;
        }
        public async Task SetEditMode(int maPhieu)
        {
            try
            {
                var result = await _client
                    .From<MuaMoiTS>()
                    .Where(p => p.MaPhieuDeNghi == maPhieu)
                    .Get();

                _existingPhieu = result.Models.FirstOrDefault();
                if (_existingPhieu != null)
                {
                    _isEditMode = true;

                    // Cập nhật các control với dữ liệu từ phiếu
                    cboDonViDeNghi.Text = _existingPhieu.DonViDeNghi;
                    txtLyDo.Text = _existingPhieu.LyDo;
                    txtGhiChu.Text = _existingPhieu.GhiChu;
                    dpNgayDeNghi.SelectedDate = _existingPhieu.NgayDeNghi;

                    // Chọn nhân viên trong combo box
                    cboMaNhanVien.SelectedValue = _existingPhieu.MaNV;

                    // Cập nhật tiêu đề cửa sổ
                    this.Title = "Sửa Phiếu Đề Nghị Mua Mới";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin phiếu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task LoadKhoAsync()
        {
            var result = await _client.From<Kho>().Get();

            if (result.Models != null)
            {
                cboDonViDeNghi.ItemsSource = result.Models;
                cboDonViDeNghi.DisplayMemberPath = "TenKho";
                cboDonViDeNghi.SelectedValuePath = "MaKho";
            }
            else
            {
                cboDonViDeNghi.ItemsSource = null;
            }
        }
        private async Task LoadNhanVienAsync()
        {
            var result = await _client.From<NhanVienModel>().Get();

            if (result.Models != null)
            {
                cboMaNhanVien.ItemsSource = result.Models;
                cboMaNhanVien.DisplayMemberPath = "TenNV";
                cboMaNhanVien.SelectedValuePath = "MaNV";
            }
            else
            {
                cboMaNhanVien.ItemsSource = null;
            }
        }
        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        public void SetPhieuForEdit(MuaMoiTS phieu)
        {
            if (phieu != null)
            {
                _existingPhieu = phieu;
                _isEditMode = true;

                // Cập nhật các control với dữ liệu từ phiếu
                cboDonViDeNghi.Text = _existingPhieu.DonViDeNghi;
                txtLyDo.Text = _existingPhieu.LyDo;
                txtGhiChu.Text = _existingPhieu.GhiChu ?? string.Empty;
                dpNgayDeNghi.SelectedDate = _existingPhieu.NgayDeNghi;

                // Chọn nhân viên trong combo box
                cboMaNhanVien.SelectedValue = _existingPhieu.MaNV;

                // Cập nhật tiêu đề cửa sổ
                this.Title = "Sửa Phiếu Đề Nghị Mua Mới";
            }
        }
        private async void btnCapNhat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra các trường bắt buộc
                if (cboMaNhanVien.SelectedValue == null)
                {
                    MessageBox.Show("Vui lòng chọn nhân viên.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(cboDonViDeNghi.Text))
                {
                    MessageBox.Show("Vui lòng nhập đơn vị đề nghị.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtLyDo.Text))
                {
                    MessageBox.Show("Vui lòng nhập lý do.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (dpNgayDeNghi.SelectedDate == null)
                {
                    MessageBox.Show("Vui lòng chọn ngày đề nghị.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cboLoaiPhieu.SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn loại phiếu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Lấy loại phiếu đã chọn
                string loaiPhieu = "";
                if (cboLoaiPhieu.SelectedItem is ComboBoxItem selectedLoaiPhieuItem)
                {
                    loaiPhieu = selectedLoaiPhieuItem.Content.ToString();
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn loại phiếu hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Khai báo biến để lưu mã phiếu
                int maPhieuMoi;

                if (_isEditMode && _existingPhieu != null)
                {
                    // Cập nhật phiếu hiện có
                    maPhieuMoi = _existingPhieu.MaPhieuDeNghi;

                    var updatedPhieu = new MuaMoiTS
                    {
                        MaPhieuDeNghi = maPhieuMoi,
                        NgayDeNghi = dpNgayDeNghi.SelectedDate.Value,
                        MaNV = Convert.ToInt32(cboMaNhanVien.SelectedValue),
                        DonViDeNghi = cboDonViDeNghi.Text,
                        LyDo = txtLyDo.Text,
                        GhiChu = txtGhiChu.Text ?? string.Empty,
                        TrangThai = _existingPhieu.TrangThai
                    };

                    await _client
                        .From<MuaMoiTS>()
                        .Where(p => p.MaPhieuDeNghi == maPhieuMoi)
                        .Update(updatedPhieu);

                    MessageBox.Show("Đã cập nhật phiếu đề nghị thành công", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Tạo mới phiếu
                    maPhieuMoi = await SinhMaPhieuMuaAsync(_client);

                    var newPhieu = new MuaMoiTS
                    {
                        MaPhieuDeNghi = maPhieuMoi,
                        NgayDeNghi = dpNgayDeNghi.SelectedDate.Value,
                        MaNV = Convert.ToInt32(cboMaNhanVien.SelectedValue),
                        DonViDeNghi = cboDonViDeNghi.Text,
                        LyDo = txtLyDo.Text,
                        GhiChu = txtGhiChu.Text ?? string.Empty,
                        TrangThai = false // Mặc định trạng thái là false cho phiếu mới
                    };

                    await _client
                        .From<MuaMoiTS>()
                        .Insert(newPhieu);

                    MessageBox.Show("Đã lưu phiếu đề nghị mới thành công", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Mở form chi tiết tương ứng
                if (loaiPhieu == "Phiếu mua mới")
                {
                    var ctForm = new CTMuaMoi(maPhieuMoi);
                    ctForm.ShowDialog();
                }
                else if (loaiPhieu == "Phiếu mua bổ sung")
                {
                    var ctForm = new CTMuaBoSung(maPhieuMoi);
                    ctForm.ShowDialog();
                }
                try
                {
                    this.DialogResult = true;
                }
                catch
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu phiếu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}