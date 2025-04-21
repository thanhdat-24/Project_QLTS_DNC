using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Models.PhanQuyen
{
    public class NhomChucNang
    {
        public string TenNhom { get; set; }
        public ObservableCollection<PhanQuyenModel> DanhSachQuyen { get; set; }

        // ✅ Để bind "Hiển thị nhóm" checkbox
        public bool HienThi
        {
            get => DanhSachQuyen != null && DanhSachQuyen.All(q => q.HienThi);
            set
            {
                if (DanhSachQuyen != null)
                {
                    foreach (var q in DanhSachQuyen)
                    {
                        q.HienThi = value;
                    }
                }
            }
        }
    }

}
