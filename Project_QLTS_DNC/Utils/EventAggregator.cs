// Tạo file mới: Project_QLTS_DNC/Utils/EventAggregator.cs
using System;
using System.Collections.Generic;
using Project_QLTS_DNC.Models.BaoTri;

namespace Project_QLTS_DNC.Utils
{
    public static class EventAggregator
    {
        // Event để truyền dữ liệu kiểm kê đến lịch sử sửa chữa
        public static event EventHandler<List<KiemKeTaiSan>> KiemKeCanCapNhatLichSu;

        // Phương thức gửi dữ liệu kiểm kê
        public static void GuiDuLieuKiemKeCanBaoTri(object sender, List<KiemKeTaiSan> danhSach)
        {
            KiemKeCanCapNhatLichSu?.Invoke(sender, danhSach);
        }
    }
}