using Project_QLTS_DNC.Models.KiemKe;
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
using Project_QLTS_DNC.Models.NhanVien;
using Supabase;


namespace Project_QLTS_DNC.View.KiemKe
{
    /// <summary>
    /// Interaction logic for DotKiemKeView.xaml
    /// </summary>
    public partial class DotKiemKeView : UserControl
    {
        private Supabase.Client _client;
        private List<DotKiemKe> _dsDotKiemKe;
        private List<NhanVienModel> _dsNhanVien;
        public DotKiemKeView()
        {
            InitializeComponent();
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
        private async Task LoadDotKiemKeAsync()
        {
            var dotResult = await _client.From<DotKiemKe>().Get();
            var nvResult = await _client.From<NhanVienModel>().Get();

            _dsDotKiemKe = dotResult.Models;
            _dsNhanVien = nvResult.Models;

            foreach (var dot in _dsDotKiemKe)
            {
                dot.TenNhanVien = _dsNhanVien.FirstOrDefault(nv => nv.MaNV == dot.MaNV)?.TenNV;
            }

            dgDotKiemKe.ItemsSource = _dsDotKiemKe;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var inputForm = new DotKiemKeInput(); // Đây là Window
            inputForm.ShowDialog();

            _ = LoadDotKiemKeAsync(); // Load lại sau khi đóng form
        }
    }



}
