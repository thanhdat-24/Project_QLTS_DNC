﻿using System;

namespace Project_QLTS_DNC.DTOs
{
    public class ThongSoKyThuatDTO
    {
        public int MaThongSo { get; set; }
        public int MaNhomTS { get; set; }
        public string TenThongSo { get; set; }

        // Các thuộc tính bổ sung cho hiển thị UI (nếu cần)
        // Ví dụ: Tên nhóm tài sản, v.v.
    }
}