using System;

namespace Ustad.API.Models
{
    /// <summary>
    /// District (İlçe) entity model
    /// Represents Turkish districts within provinces
    /// </summary>
    public class UstadFirmsDistrictType
    {
        /// <summary>
        /// Primary key - District ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Province code (IlKodu) - Foreign key to ILTipi.IlKodu
        /// Links district to its parent province
        /// </summary>
        public int IlKodu { get; set; }

        /// <summary>
        /// District code (unique within province)
        /// </summary>
        public int DistrictKodu { get; set; }

        /// <summary>
        /// District name (e.g., "SEYHAN", "ÇANKAYA", "KADIKÖY")
        /// </summary>
        public string DistrictAdi { get; set; } = string.Empty;
    }

    /// <summary>
    /// District response DTO for API responses
    /// </summary>
    public class DistrictResponse
    {
        /// <summary>
        /// District ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Province code (parent province)
        /// </summary>
        public int ProvinceCode { get; set; }

        /// <summary>
        /// District code
        /// </summary>
        public int DistrictCode { get; set; }

        /// <summary>
        /// District name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Province information (if included in response)
        /// </summary>
        public ProvinceResponse? Province { get; set; }
    }

    /// <summary>
    /// District with province information DTO
    /// </summary>
    public class DistrictWithProvinceResponse
    {
        /// <summary>
        /// District ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// District code
        /// </summary>
        public int DistrictCode { get; set; }

        /// <summary>
        /// District name
        /// </summary>
        public string DistrictName { get; set; } = string.Empty;

        /// <summary>
        /// Province code
        /// </summary>
        public int ProvinceCode { get; set; }

        /// <summary>
        /// Province name
        /// </summary>
        public string ProvinceName { get; set; } = string.Empty;

        /// <summary>
        /// Full location string (e.g., "Çankaya, Ankara")
        /// </summary>
        public string FullLocation => $"{DistrictName}, {ProvinceName}";
    }
}

