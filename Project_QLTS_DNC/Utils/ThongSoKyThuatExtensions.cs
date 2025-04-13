using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Project_QLTS_DNC.DTOs;
using Project_QLTS_DNC.Models.ThongSoKT;

namespace Project_QLTS_DNC.Utils
{
    public static class ThongSoKyThuatExtensions
    {
        /// <summary>
        /// Chuyển đổi từ Model sang DTO
        /// </summary>
        public static ThongSoKyThuatDTO ToDTO(this ThongSoKyThuat model)
        {
            if (model == null) return null;

            return new ThongSoKyThuatDTO
            {
                MaThongSo = model.MaThongSo,
                MaNhomTS = model.MaNhomTS,
                TenThongSo = model.TenThongSo
            };
        }

        /// <summary>
        /// Chuyển đổi từ DTO sang Model
        /// </summary>
        public static ThongSoKyThuat ToModel(this ThongSoKyThuatDTO dto)
        {
            if (dto == null) return null;

            return new ThongSoKyThuat
            {
                MaThongSo = dto.MaThongSo,
                MaNhomTS = dto.MaNhomTS,
                TenThongSo = dto.TenThongSo
            };
        }

        /// <summary>
        /// Chuyển đổi danh sách Model sang danh sách DTO
        /// </summary>
        public static List<ThongSoKyThuatDTO> ToListDTO(this List<ThongSoKyThuat> models)
        {
            if (models == null) return new List<ThongSoKyThuatDTO>();

            return models.Select(m => m.ToDTO()).ToList();
        }

        /// <summary>
        /// Chuyển đổi danh sách Model sang ObservableCollection DTO
        /// </summary>
        public static ObservableCollection<ThongSoKyThuatDTO> ToListDTO(this IEnumerable<ThongSoKyThuat> models)
        {
            if (models == null) return new ObservableCollection<ThongSoKyThuatDTO>();

            return new ObservableCollection<ThongSoKyThuatDTO>(models.Select(m => m.ToDTO()));
        }

        /// <summary>
        /// Chuyển đổi danh sách DTO sang danh sách Model
        /// </summary>
        public static List<ThongSoKyThuat> ToListModel(this IEnumerable<ThongSoKyThuatDTO> dtos)
        {
            if (dtos == null) return new List<ThongSoKyThuat>();

            return dtos.Select(d => d.ToModel()).ToList();
        }
    }
}