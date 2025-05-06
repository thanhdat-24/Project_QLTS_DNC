using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.IO;

namespace Project_QLTS_DNC.View.Common
{
    public partial class SuccessNotificationDialog : Window
    {
        private DispatcherTimer _autoCloseTimer;
        private int _remainingSeconds = 5;

        public string MessageTitle { get; set; }
        public string MessageText { get; set; }
        public string FilePath { get; set; }

        public SuccessNotificationDialog(string messageTitle, string messageText, string filePath = null)
        {
            InitializeComponent();

            MessageTitle = messageTitle;
            MessageText = messageText;
            FilePath = filePath;

            this.DataContext = this;

            // Set up auto-close timer
            _autoCloseTimer = new DispatcherTimer();
            _autoCloseTimer.Interval = TimeSpan.FromSeconds(1);
            _autoCloseTimer.Tick += AutoCloseTimer_Tick;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Start the progress bar animation
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 100,
                To = 0,
                Duration = TimeSpan.FromSeconds(10)
            };
            progressBar.BeginAnimation(System.Windows.Controls.ProgressBar.ValueProperty, animation);

            // Start auto-close timer
            _autoCloseTimer.Start();
        }

        private void AutoCloseTimer_Tick(object sender, EventArgs e)
        {
            _remainingSeconds--;

            if (_remainingSeconds <= 0)
            {
                _autoCloseTimer.Stop();
                this.Close();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            _autoCloseTimer.Stop();
            this.Close();
        }

        // Phương thức mở file PDF cho SuccessNotificationDialog
        private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(FilePath))
            {
                try
                {
                    // Kiểm tra file tồn tại
                    if (!File.Exists(FilePath))
                    {
                        MessageBox.Show("File không tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Sử dụng explorer.exe để mở file - cách này an toàn hơn và 
                    // không bị ảnh hưởng bởi cấu hình association của ứng dụng
                    var process = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"\"{FilePath}\"",
                        UseShellExecute = true
                    };
                    Process.Start(process);
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi
                    try
                    {
                        string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "OpenFileError.log");
                        string errorMessage = $"Thời gian: {DateTime.Now}\n" +
                                             $"Lỗi khi mở file: {FilePath}\n" +
                                             $"Chi tiết lỗi: {ex.Message}\n" +
                                             $"StackTrace: {ex.StackTrace}\n" +
                                             "---------------------------------------------------\n";
                        File.AppendAllText(logPath, errorMessage);
                    }
                    catch { }

                    // Thông báo lỗi cho người dùng với hướng dẫn thay thế
                    var result = MessageBox.Show(
                        $"Không thể mở file: {ex.Message}\n\nBạn có muốn mở thư mục chứa file để truy cập thủ công không?",
                        "Lỗi khi mở file",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Error);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            // Mở thư mục chứa file
                            string folder = Path.GetDirectoryName(FilePath);
                            Process.Start("explorer.exe", folder);
                        }
                        catch { }
                    }
                }
            }
        }
    }
}