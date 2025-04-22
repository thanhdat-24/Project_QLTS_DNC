namespace Project_QLTS_DNC.DTOs
{
    // Lớp hỗ trợ cho việc hiển thị vị trí trong ComboBox
    public class ViTriTSItem
    {
        public int ViTri { get; set; }
        public bool DaDuocSuDung { get; set; }
        public bool IsConfigurationNeeded { get; set; } // Add this property

        public string HienThi => IsConfigurationNeeded
            ? "Cần cấu hình sức chứa"
            : $"Vị trí {ViTri}" + (DaDuocSuDung ? " (đã sử dụng)" : "");

        public override string ToString() => HienThi;
    }
}