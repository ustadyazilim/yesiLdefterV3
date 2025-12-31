using System;

namespace Ustad.API.Models
{
    /// <summary>
    /// Province (İl) entity model
    /// Represents Turkish provinces/cities
    /// </summary>
    public class ILTipi
    {
        /// <summary>
        /// Primary key - Province ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Province code (1-81 for Turkish provinces)
        /// </summary>
        public int IlKodu { get; set; }

        /// <summary>
        /// Province name in uppercase (e.g., "ANKARA", "İSTANBUL")
        /// </summary>
        public string IlAdiBUYUK { get; set; } = string.Empty;

        /// <summary>
        /// Province name in lowercase (e.g., "Ankara", "İstanbul")
        /// </summary>
        public string IlAdiKucuk { get; set; } = string.Empty;

        /// <summary>
        /// GIB (Tax Office) code for the province
        /// </summary>
        public string? IlGIBKodu { get; set; }

        /// <summary>
        /// Province code with leading zero (e.g., "01", "06", "34")
        /// </summary>
        public string? IlKoduSifirli { get; set; }
    }

    /// <summary>
    /// Province response DTO for API responses
    /// </summary>
    public class ProvinceResponse
    {
        /// <summary>
        /// Province ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Province code
        /// </summary>
        public int ProvinceCode { get; set; }

        /// <summary>
        /// Province name (display name)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Province name in uppercase
        /// </summary>
        public string NameUppercase { get; set; } = string.Empty;

        /// <summary>
        /// GIB code
        /// </summary>
        public string? GIBCode { get; set; }

        /// <summary>
        /// Province code with leading zero
        /// </summary>
        public string? ProvinceCodePadded { get; set; }
    }
}

