using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Project_QLTS_DNC.Helpers
{
    public static class NetworkHelper
    {
        /// <summary>
        /// Lấy địa chỉ IPv4 của máy tính hiện tại đang được sử dụng để kết nối internet
        /// </summary>
        /// <param name="port">Cổng dịch vụ (mặc định: 8080)</param>
        /// <returns>URL đầy đủ với địa chỉ IP và port</returns>
        public static string GetLocalIPv4Address(int port = 8080)
        {
            try
            {
                // Ưu tiên tìm IPv4 đang được sử dụng cho kết nối Internet
                string ip = GetActiveIPv4();

                if (!string.IsNullOrEmpty(ip))
                {
                    return $"http://{ip}:{port}";
                }

                // Fallback: nếu không tìm thấy IP chính, sử dụng localhost
                return $"http://localhost:{port}";
            }
            catch (Exception ex)
            {
                // Log lỗi
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy địa chỉ IP: {ex.Message}");
                // Trả về địa chỉ localhost nếu có lỗi
                return $"http://localhost:{port}";
            }
        }

        /// <summary>
        /// Phương thức tìm IPv4 đang được sử dụng cho kết nối mạng
        /// </summary>
        /// <returns>Địa chỉ IPv4 hoạt động</returns>
        private static string GetActiveIPv4()
        {
            string output = string.Empty;

            // Lấy tất cả các network interface đang hoạt động và không phải loopback
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                // Chỉ lấy các adapter đang UP và không phải loopback
                if (adapter.OperationalStatus == OperationalStatus.Up &&
                    adapter.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    // Lấy các địa chỉ IP từ adapter đang xét
                    IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                    UnicastIPAddressInformationCollection allAddresses = adapterProperties.UnicastAddresses;

                    foreach (UnicastIPAddressInformation address in allAddresses)
                    {
                        // Chỉ lấy IPv4, không phải loopback và không phải địa chỉ nội bộ 169.254.x.x
                        if (address.Address.AddressFamily == AddressFamily.InterNetwork &&
                            !IPAddress.IsLoopback(address.Address) &&
                            !address.Address.ToString().StartsWith("169.254."))
                        {
                            return address.Address.ToString();
                        }
                    }
                }
            }

            // Fallback: Cách khác để tìm địa chỉ IP
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                try
                {
                    // Kết nối đến một địa chỉ bất kỳ (8.8.8.8 - Google DNS)
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    if (endPoint != null)
                    {
                        return endPoint.Address.ToString();
                    }
                }
                catch
                {
                    // Bỏ qua lỗi
                }
            }

            return output;
        }
    }
}