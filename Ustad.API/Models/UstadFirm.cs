using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ustad.API.Models
{
    public class UstadFirm
    {
        public int FirmId { get; set; }
        public string FirmLongName { get; set; } = string.Empty;
        public string FirmShortName { get; set; } = string.Empty;
        public string FirmGuid { get; set; } = string.Empty;
        public string MenuCode { get; set; } = string.Empty;
        public string MenuCodeOld { get; set; } = string.Empty;
        public Int16 SectorTypeId { get; set; }
        public string DatabaseType { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string ServerNameIP { get; set; } = string.Empty;
        public string DbAuthentication { get; set; } = string.Empty;
        public string DbLoginName { get; set; } = string.Empty;
        public string DbPassword { get; set; } = string.Empty;
        public string MebbisCode { get; set; } = string.Empty;
        public string MebbisPass { get; set; } = string.Empty;

        /// <summary>
        /// District Type ID - Foreign key to UstadFirmsDistrictType.Id
        /// </summary>
        public int? DistrictTypeId { get; set; }

        /// <summary>
        /// City Type ID - Foreign key to ILTipi.Id
        /// </summary>
        public int? CityTypeId { get; set; }

        /// <summary>
        /// District name (populated from UstadFirmsDistrictType)
        /// </summary>
        public string? DistrictName { get; set; }

        /// <summary>
        /// Province name (populated from ILTipi)
        /// </summary>
        public string? ProvinceName { get; set; }

        public void Clear()
        {
            FirmId = 0;
            FirmLongName = "";
            FirmShortName = "";
            FirmGuid = "";
            MenuCode = "";
            MenuCodeOld = "";
            SectorTypeId = 0;
            MebbisCode = "";
            MebbisPass = "";
            DistrictTypeId = null;
            CityTypeId = null;
            DistrictName = null;
            ProvinceName = null;
        }
    }
}

