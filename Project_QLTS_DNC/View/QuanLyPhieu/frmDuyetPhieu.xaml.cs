using Project_QLTS_DNC.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for frmDuyetPhieu.xaml
    /// </summary>
    public partial class frmDuyetPhieu : UserControl
    {
       public ObservableCollection<object> DanhSachPhieu { get; set; } = new ObservableCollection<object>();
        public ObservableCollection<object> DanhSachPhieuDaDuyet { get; set; } = new ObservableCollection<object>();
        public ObservableCollection<object> DanhSachPhieuTuChoi { get; set; } = new ObservableCollection<object>();

        private List<DuyetPhieu.PhieuNhapKho> phieuNhapKho;
        private List<DuyetPhieu.PhieuXuatKho> phieuXuatKho;
        private List<DuyetPhieu.PhieuBaoTri> phieuBaoTri;
        private List<DuyetPhieu.PhieuBaoHong> phieuBaoHong;

        public frmDuyetPhieu()
        {
            InitializeComponent();
           
            DataContext = this;
        }


        private void btnDuyet_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnTuChoi_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cboLoaiphieu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
