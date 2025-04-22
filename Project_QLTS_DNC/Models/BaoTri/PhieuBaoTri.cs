using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.ComponentModel;
using Newtonsoft.Json;

[Table("baotri")]
public class PhieuBaoTri : BaseModel, INotifyPropertyChanged
{
    [PrimaryKey("ma_bao_tri", false)]
    public int MaBaoTri { get; set; }

    [Column("ma_tai_san")]
    public int? MaTaiSan { get; set; }

    [Column("ma_loai_bao_tri")]
    public int? MaLoaiBaoTri { get; set; }

    [Column("ngay_bao_tri")]
    public DateTime? NgayBaoTri { get; set; }

    [Column("ma_nv")]
    public int? MaNV { get; set; }

    [Column("noi_dung")]
    public string NoiDung { get; set; }

    [Column("trang_thai_sau_bao_tri")]
    public string TrangThai { get; set; }

    [Column("chi_phi")]
    public decimal? ChiPhi { get; set; }

    [Column("ghi_chu")]
    public string GhiChu { get; set; }

    // Sử dụng JsonIgnore để Supabase không cố gắng lưu trữ nó
    [JsonIgnore]
    public bool IsSelected { get; set; }

    // Thêm các thuộc tính phụ để hiển thị tên thay vì mã
    [JsonIgnore]
    public string TenLoaiBaoTri
    {
        get
        {
            switch (MaLoaiBaoTri)
            {
                case 1: return "Định kỳ";
                case 2: return "Đột xuất";
                case 3: return "Bảo hành";
                default: return "Không xác định";
            }
        }
    }

    [JsonIgnore]
    public string TenNguoiPhuTrach { get; set; }

    [JsonIgnore]
    public string TenTaiSan { get; set; }

    // Thêm thuộc tính mới để lưu số seri của tài sản
    [JsonIgnore]
    public string SoSeri { get; set; }

    // Thêm thuộc tính mới để lưu tình trạng của tài sản
    [JsonIgnore]
    public string TinhTrangTaiSan { get; set; }

    // Thêm thuộc tính mới để hiển thị thông tin tài sản đầy đủ
    [JsonIgnore]
    public string TaiSanChiTiet
    {
        get
        {
            return $"{TenTaiSan} - SN: {SoSeri} - {TinhTrangTaiSan}";
        }
    }

    // Phương thức thông báo sự thay đổi thuộc tính
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}