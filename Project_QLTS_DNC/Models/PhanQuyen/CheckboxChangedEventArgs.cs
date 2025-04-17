using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Models.PhanQuyen
{
    public class CheckboxChangedEventArgs
    {
        public string TenChucNang { get; set; }
        public bool IsChecked { get; set; }
        public bool IsCheckedXem { get; set; }

    }
}
