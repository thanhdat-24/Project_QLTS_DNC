using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Project_QLTS_DNC.View.NhaCungCap
{
    /// <summary>
    /// Interaction logic for NhaCungCapForm.xaml
    /// </summary>
    public partial class NhaCungCapForm : UserControl
    {
        public NhaCungCapForm()
        {
            InitializeComponent();

        }

        private void btnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            ThemNhaCungCapForm frmthemNhaCungCap = new ThemNhaCungCapForm();
            frmthemNhaCungCap.ShowDialog();
        }
    }

}
