using System;
using System.Windows;
using System.Windows.Controls;
using Project_QLTS_DNC.Models.BaoHong;

namespace Project_QLTS_DNC.View.QuanLyPhieu
{
    /// <summary>
    /// Interaction logic for PhieuBaoHongInputForm.xaml
    /// </summary>
    public partial class PhieuBaoHongInputForm : Window
    {
        private PhieuBaoHong _phieuBaoHong;
        private bool _isEditMode = false;

        public PhieuBaoHong PhieuBaoHong => _phieuBaoHong;


       
    }
}