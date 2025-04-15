using Project_QLTS_DNC.Models.TonKho;
using Supabase;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;
using Project_QLTS_DNC.Models.DuyetPhieu;



namespace Project_QLTS_DNC.Services
{
    public class TonKhoService
    {
        public static async Task CapNhatTuPhieuAsync(long maPhieuNhap, long maKho)
        {
            var client = await SupabaseService.GetClientAsync();

            var dsChiTiet = await client
                .From<ChiTietPhieuNhap>()
                .Filter("ma_phieu_nhap", Operator.Equals, maPhieuNhap)
                .Get();

            foreach (var ct in dsChiTiet.Models)
            {
                var tonKhoCheck = await client
                    .From<TonKho>()
                    .Filter("ma_kho", Operator.Equals, maKho)
                    .Filter("ma_nhom_ts", Operator.Equals, ct.MaNhomTS)
                    .Get();

                if (tonKhoCheck.Models.Any())
                {
                    var ton = tonKhoCheck.Models.First();
                    ton.SoLuongNhap += ct.SoLuong ?? 0;
                    ton.NgayCapNhat = DateTime.Now;
                    await client.From<TonKho>().Update(ton);
                }
                else
                {
                    var tonMoi = new TonKho
                    {
                        MaKho = maKho,
                        MaNhomTS = ct.MaNhomTS,
                        SoLuongNhap = ct.SoLuong ?? 0,
                        SoLuongXuat = 0,
                        NgayCapNhat = DateTime.Now
                    };
                    await client.From<TonKho>().Insert(tonMoi);
                }

            }
        }
    }
}
