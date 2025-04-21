using Newtonsoft.Json;
using Project_QLTS_DNC.Models;
using Project_QLTS_DNC.Models.PhanQuyen;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Services.TaiKhoan
{
    public class PhanQuyenService : SupabaseService
    {
        public PhanQuyenService() : base() { }

        public async Task<List<PhanQuyenModel>> LayDanhSachQuyenTheoLoaiTkAsync(long maLoaiTk)
        {
            var _client = await GetClientAsync();
            var response = await _client.Rpc("lay_phan_quyen_theo_loai", new Dictionary<string, object>
            {
                { "ma_loai", maLoaiTk }
            });

            // Nếu không có dữ liệu thì trả về danh sách rỗng để tránh lỗi
            if (string.IsNullOrEmpty(response.Content) || response.Content == "null")
                return new List<PhanQuyenModel>();

            return JsonConvert.DeserializeObject<List<PhanQuyenModel>>(response.Content);
        }


        public async Task<bool> LuuDanhSachPhanQuyenAsync(List<PhanQuyenModel> danhSach)
        {
            var _client = await GetClientAsync();
            var jsonArray = Newtonsoft.Json.Linq.JArray.FromObject(danhSach); 

            var result = await _client.Rpc("luu_phan_quyen_theo_loai", new Dictionary<string, object>
            {
                { "ds_quyen_json", jsonArray }
            });

            return result.ResponseMessage.IsSuccessStatusCode;
        }

    }
}
