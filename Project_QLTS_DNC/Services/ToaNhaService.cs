using Project_QLTS_DNC.Models.QLLoaiTS;
using Project_QLTS_DNC.Models.ToaNha;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Supabase;
using Project_QLTS_DNC.Models.QLNhomTS;
using static Supabase.Postgrest.Constants;


namespace Project_QLTS_DNC.Services
{
    public class ToaNhaService
    {
        public static async Task<ObservableCollection<ToaNha>> LayDanhSachToaNhaAsync()
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();
                var response = await client.From<ToaNha>().Get();

                return new ObservableCollection<ToaNha>(response.Models);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy danh sách tòa nhà: {ex.Message}");
                throw;
            }
        }



        private static async Task<int> SinhMaLoaiTNAsync()
        {
            try
            {
                // Lấy danh sách loại tài sản hiện có
                var danhSachTN = await LayDanhSachToaNhaAsync();

                // Nếu không có dữ liệu, bắt đầu từ 1
                if (danhSachTN == null || danhSachTN.Count == 0)
                    return 1;

                // Tìm mã lớn nhất trong danh sách
                int maxMa = 0;
                foreach (var loaiTN in danhSachTN)
                {
                    if (int.TryParse(loaiTN.MaToaNha.ToString(), out int maSo))
                    {
                        if (maSo > maxMa)
                            maxMa = maSo;
                    }
                }

                // Trả về mã mới = mã lớn nhất + 1
                return maxMa + 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi sinh mã loại tài sản: {ex.Message}");
                throw;
            }
        }

        public static async Task<ToaNha> CapNhatToaNhaAsync(ToaNha toaNha)
        {
            try
            {
                if (toaNha.MaToaNha == null || toaNha.MaToaNha == 0)
                    throw new ArgumentException("Mã tòa nhà không hợp lệ.");

                if (string.IsNullOrWhiteSpace(toaNha.TenToaNha))
                    throw new ArgumentException("Tên tòa nhà không được để trống.");

                var client = await SupabaseService.GetClientAsync();

                var kiemTra = await client.From<ToaNha>()
                    .Where(t => t.MaToaNha == toaNha.MaToaNha)
                    .Get();

                if (kiemTra.Models.Count == 0)
                    throw new ArgumentException("Tòa nhà không tồn tại.");

                var response = await client.From<ToaNha>()
                    .Where(t => t.MaToaNha == toaNha.MaToaNha)
                    .Update(toaNha);

                if (response.Models.Count > 0)
                    return response.Models[0];
                throw new Exception("Không thể cập nhật tòa nhà!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi cập nhật tòa nhà: {ex.Message}");
                throw;
            }
        }

        public static async Task<ToaNha> ThemTNAsync(ToaNha ToaNha)
        {
            try
            {
                // Kiểm tra tính hợp lệ của dữ liệu
                if (string.IsNullOrWhiteSpace(ToaNha.TenToaNha))
                {
                    throw new ArgumentException("Tên loại tài sản không được để trống.");
                }

                // Sinh mã mới nếu chưa có
                if (ToaNha.MaToaNha == 0)
                {
                    ToaNha.MaToaNha = await SinhMaLoaiTNAsync();
                    MessageBox.Show($"Mã Tòa Nhà được sinh tự động là: {ToaNha.MaToaNha}", "Thông tin", MessageBoxButton.OK, MessageBoxImage.Information);
                }




                var client = await SupabaseService.GetClientAsync();

                // Tạo đối tượng mới để insert vào Supabase
                var response = await client.From<ToaNha>().Insert(ToaNha);

                if (response.Models.Count > 0)
                {
                    return response.Models[0];
                }

                throw new Exception("Không thể thêm mới loại tài sản!");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi thêm Loại Tài Sản: {ex.Message}, {ex.StackTrace}");
                throw;
            }
        }
        public static async Task<bool> XoaTNAsync(int matoanha)
        {
            try
            {
                var client = await SupabaseService.GetClientAsync();

                await client
                    .From<ToaNha>()
                    .Filter("ma_toa", Operator.Equals, matoanha)
                    .Delete(); // chỉ gọi await, không dùng var

                return true; // Giả định luôn thành công
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LỖI xoá tòa nhà: {ex.Message}");
                return false;
            }
        }










    }
}