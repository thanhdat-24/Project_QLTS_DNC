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
using Project_QLTS_DNC.Models.Kho;

using Supabase;



namespace Project_QLTS_DNC.View.QuanLyKho

{
    /// <summary>
    /// Interaction logic for TonKhoView.xaml
    /// </summary>
    public partial class TonKhoView : UserControl
    {
        private Supabase.Client _client;


        public TonKhoView()
        {           
            InitializeComponent();
            Loaded += TonKhoView_Loaded;

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

        private async Task LoadTonKhoAsync()
        {
            try
            {
                var result = await _client.From<TonKho>().Get();

                if (result.Models != null && result.Models.Count > 0)
                {
                    dgTonKho.ItemsSource = result.Models;
                }
                else
                {
                    dgTonKho.ItemsSource = null;
                    MessageBox.Show("Không có dữ liệu tồn kho.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu tồn kho: {ex.Message}");
            }
        }
                     
        private async void TonKhoView_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeSupabaseAsync();
            await LoadTonKhoAsync();
        }




    }
}
