using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.ViewModel.TaiKhoan
{
    public class TaiKhoanViewModel
    {
        private readonly TaiKhoanService _taiKhoanService;

        
        public TaiKhoanViewModel()
        {
            _taiKhoanService = new TaiKhoanService();
        }

        
        public async Task<TaiKhoanModel> CreateTaiKhoanAsync(string tenTaiKhoan, string matKhau, int maLoaiTk, int maNv)
        {
            try
            {
                var taiKhoan = await _taiKhoanService.ThemTaiKhoanAsync(tenTaiKhoan, matKhau, maLoaiTk, maNv);
                return taiKhoan; // Return the created TaiKhoanModel
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., connection issues, invalid data)
                Console.WriteLine($"Error creating TaiKhoan: {ex.Message}");
                return null;
            }
        }
    }
}
