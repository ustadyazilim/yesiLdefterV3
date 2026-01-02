using System;
using System.ComponentModel.DataAnnotations;

namespace Ustad.API.Models
{
    /// <summary>
    /// Request model for e-src.net external data sync endpoint
    /// Supports both full payload and student ID only
    /// </summary>
    public class ESrcExternalDataSyncRequest
    {
        /// <summary>
        /// Student ID - if provided, will fetch full student data from database
        /// If not provided, full student data must be in StudentData property
        /// </summary>
        public int? StudentId { get; set; }

        /// <summary>
        /// Full student data - required if StudentId is not provided
        /// </summary>
        public StudentDataModel? StudentData { get; set; }
    }

    /// <summary>
    /// Student data model for e-src.net external data sync
    /// </summary>
    public class StudentDataModel
    {
        public string TC { get; set; } = string.Empty;
        public string ADI { get; set; } = string.Empty;
        public string SOYADI { get; set; } = string.Empty;
        public string EMAIL { get; set; } = string.Empty;
        public string IL { get; set; } = string.Empty;
        public string ILCE { get; set; } = string.Empty;
        public string ADRES { get; set; } = string.Empty;
        public string IMG { get; set; } = string.Empty;
        public string BELGE { get; set; } = string.Empty;
        public string CINSIYET { get; set; } = string.Empty;
        public string GSM { get; set; } = string.Empty;
        public decimal? BAKIYE { get; set; }
    }

    /// <summary>
    /// Response model for e-src.net external data sync endpoint
    /// </summary>
    public class ESrcExternalDataSyncResponse
    {
        /// <summary>
        /// Whether the sync was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Sync status message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Student ID that was synced
        /// </summary>
        public int? StudentId { get; set; }

        /// <summary>
        /// TC number of the student
        /// </summary>
        public string? TcNo { get; set; }

        /// <summary>
        /// Response from e-src.net API (if available)
        /// </summary>
        public string? ESrcExternalDataResponse { get; set; }

        /// <summary>
        /// Timestamp of the sync operation
        /// </summary>
        public DateTime SyncTimestamp { get; set; }

        /// <summary>
        /// Whether this response was served from cache
        /// </summary>
        public bool FromCache { get; set; }

        /// <summary>
        /// Number of retry attempts made (if any)
        /// </summary>
        public int RetryAttempts { get; set; }

        /// <summary>
        /// Error details (if sync failed)
        /// </summary>
        public string? ErrorDetails { get; set; }
    }

    /// <summary>
    /// Remote API model matching e-src.net's expected format
    /// </summary>
    public class RemoteApiModel
    {
        public string KURSMAIL { get; set; } = string.Empty;
        public string PASS { get; set; } = string.Empty;
        public string TC { get; set; } = string.Empty;
        public string ADI { get; set; } = string.Empty;
        public string SOYADI { get; set; } = string.Empty;
        public string EMAIL { get; set; } = string.Empty;
        public string IL { get; set; } = string.Empty;
        public string ILCE { get; set; } = string.Empty;
        public string ADRES { get; set; } = string.Empty;
        public string IMG { get; set; } = string.Empty;
        public string BELGE { get; set; } = string.Empty;
        public string CINSIYET { get; set; } = string.Empty;
        public decimal BAKIYE { get; set; }
        public string GSM { get; set; } = string.Empty;
        public string SUBESI { get; set; } = string.Empty;
        public string GRUP { get; set; } = string.Empty;
        public string DONEM { get; set; } = string.Empty;
    }

    /// <summary>
    /// e-src.net external data sync configuration model
    /// </summary>
    public class ESrcExternalDataCredentials
    {
        public string ApiUrl { get; set; } = string.Empty;
        public string KursMail { get; set; } = string.Empty;
        public string Pass { get; set; } = string.Empty;
        public string Subesi { get; set; } = string.Empty;
        public string Grup { get; set; } = string.Empty;
        public string Donem { get; set; } = string.Empty;
        public int CacheTTLMinutes { get; set; } = 60;
        public int RetryAttempts { get; set; } = 3;
        public int TimeoutSeconds { get; set; } = 30;
    }
}

