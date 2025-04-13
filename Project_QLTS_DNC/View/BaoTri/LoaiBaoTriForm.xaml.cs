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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Project_QLTS_DNC.Models.BaoTri;
using Project_QLTS_DNC.Services.BaoTri;

namespace Project_QLTS_DNC.View.BaoTri
{
    /// <summary>
    /// Interaction logic for LoaiBaoTriForm.xaml
    /// </summary>
    public partial class LoaiBaoTriForm : UserControl
    {
        private readonly LoaiBaoTriService _loaiBaoTriService = new(); 
        public LoaiBaoTriForm()
        {
            InitializeComponent();
            _ = LoadLoaiBaoTri();

        }
        public async Task LoadLoaiBaoTri()
        {
            List<LoaiBaoTri> dsLoaiBaoTri = await _loaiBaoTriService.LayDanhSachLoaiBT();
            dgLoaiBaoTri.ItemsSource = dsLoaiBaoTri;
        }
    }
}
