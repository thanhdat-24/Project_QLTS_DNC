using Project_QLTS_DNC.Models.DuyetPhieu;
using System;
using System.Windows;

namespace Project_QLTS_DNC.View.DuyetPhieu
{
    public partial class frmSuaLoaiPhieu : Window
    {
        public LoaiPhieu LoaiPhieuDaSua { get; private set; }
        private readonly LoaiPhieu _goc;

        public frmSuaLoaiPhieu(LoaiPhieu loaiPhieu)
        {
            InitializeComponent();
            _goc = loaiPhieu;

            // Tạo bản sao để tránh chỉnh trực tiếp dữ liệu gốc
            LoaiPhieuDaSua = new LoaiPhieu
            {
                MaLoaiPhieu = loaiPhieu.MaLoaiPhieu,
                TenLoaiPhieu = loaiPhieu.TenLoaiPhieu,
                MoTaLP = loaiPhieu.MoTaLP
            };

            // Load dữ liệu lên textbox
            txtTenLoaiPhieu.Text = LoaiPhieuDaSua.TenLoaiPhieu;
            txtMoTa.Text = LoaiPhieuDaSua.MoTaLP;
        }

        private void btnCapNhat_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenLoaiPhieu.Text))
            {
                MessageBox.Show("Vui lòng nhập tên loại phiếu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LoaiPhieuDaSua.TenLoaiPhieu = txtTenLoaiPhieu.Text.Trim();
            LoaiPhieuDaSua.MoTaLP = txtMoTa.Text.Trim();

            this.DialogResult = true;
            this.Close();
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        }
    }

