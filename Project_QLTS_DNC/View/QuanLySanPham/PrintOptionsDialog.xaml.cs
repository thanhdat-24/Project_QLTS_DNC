using System.Windows;

namespace Project_QLTS_DNC.View.QuanLyTaiSan
{
    public partial class PrintOptionsDialog : Window
    {
        public bool IsPrintDirectly { get; private set; }

        public PrintOptionsDialog()
        {
            InitializeComponent();
            radioPrintDirect.IsChecked = true;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            IsPrintDirectly = radioPrintDirect.IsChecked == true;
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}