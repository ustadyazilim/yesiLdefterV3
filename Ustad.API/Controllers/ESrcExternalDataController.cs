using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ustad.API.Models;

namespace Ustad.API.Controllers
{
    /// <summary>
    /// Controller for e-src.net external data sync API integration
    /// Handles student data synchronization with external e-src.net platform
    /// </summary>
    [ApiController]
    [Route("api/esrc-external-data")]
    public class ESrcExternalDataController : ControllerBase
    {
        private const string CacheKeyPrefixTc = "esrc:sync:tc:";
        private const string CacheKeyPrefixId = "esrc:sync:id:";

        private readonly ILogger<ESrcExternalDataController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public ESrcExternalDataController(
            ILogger<ESrcExternalDataController> logger,
            IMemoryCache cache,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _cache = cache;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        private (string ApiUrl, string KursMail, string Pass, string Subesi, string Grup, string Donem, int CacheTTLMinutes, int TimeoutSeconds) GetESrcConfig()
        {
            var section = _configuration.GetSection("ESrcExternalData");
            return (
                section["ApiUrl"] ?? "https://e-src.net/api/StudentApi/NewStudent",
                section["KursMail"] ?? "",
                section["Pass"] ?? "",
                section["Subesi"] ?? "",
                section["Grup"] ?? "",
                section["Donem"] ?? "",
                section.GetValue("CacheTTLMinutes", 60),
                section.GetValue("TimeoutSeconds", 30)
            );
        }

        /// <summary>
        /// Syncs student data with e-src.net external API
        /// Supports full payload (StudentData). When only StudentId is provided, StudentData is required from caller context.
        /// </summary>
        [HttpPost("sync-student")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ESrcExternalDataSyncResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SyncStudent([FromBody] ESrcExternalDataSyncRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                StudentDataModel? studentData;
                int? studentId = request.StudentId;
                string? tcNo;

                if (request.StudentData != null)
                {
                    studentData = request.StudentData;
                    tcNo = studentData.TC;
                }
                else if (request.StudentId.HasValue)
                {
                    return BadRequest("StudentData is required for sync-student; provide full student payload when calling this endpoint.");
                }
                else
                {
                    return BadRequest("Either StudentId or StudentData must be provided");
                }

                if (string.IsNullOrWhiteSpace(studentData.TC))
                    return BadRequest("TC (Turkish ID) is required");
                if (string.IsNullOrWhiteSpace(studentData.ADI) || string.IsNullOrWhiteSpace(studentData.SOYADI))
                    return BadRequest("ADI and SOYADI are required");

                var config = GetESrcConfig();
                var cacheKey = CacheKeyPrefixTc + (tcNo ?? "");

                if (_cache.TryGetValue(cacheKey, out CachedSyncResult? cached) && cached != null)
                {
                    return Ok(new ESrcExternalDataSyncResponse
                    {
                        Success = cached.Success,
                        Message = cached.Message,
                        StudentId = studentId,
                        TcNo = tcNo,
                        ESrcMessages = cached.ESrcMessages,
                        SyncTimestamp = DateTime.UtcNow,
                        FromCache = true
                    });
                }

                var (success, message, messages) = await PostToESrcAsync(studentData, config).ConfigureAwait(false);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(config.CacheTTLMinutes));
                _cache.Set(cacheKey, new CachedSyncResult(success, message, messages), cacheOptions);
                if (studentId.HasValue)
                    _cache.Set(CacheKeyPrefixId + studentId.Value, new CachedSyncResult(success, message, messages), cacheOptions);

                return Ok(new ESrcExternalDataSyncResponse
                {
                    Success = success,
                    Message = message,
                    StudentId = studentId,
                    TcNo = tcNo,
                    ESrcMessages = messages,
                    SyncTimestamp = DateTime.UtcNow,
                    FromCache = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ESrcExternalDataController] Error syncing student with e-src.net");
                return StatusCode(500, new ESrcExternalDataSyncResponse
                {
                    Success = false,
                    Message = "Internal server error during sync",
                    ErrorDetails = ex.Message,
                    SyncTimestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Batch sync: send multiple students to e-src.net; each result includes MsgBox list and FromCache where applicable.
        /// </summary>
        [HttpPost("sync-batch")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ESrcBatchSyncResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SyncBatch([FromBody] ESrcBatchSyncRequest request)
        {
            try
            {
                if (request?.Students == null || request.Students.Count == 0)
                    return BadRequest("Students list is required and must not be empty");

                var config = GetESrcConfig();
                var results = new List<ESrcStudentSyncResult>();

                foreach (var student in request.Students)
                {
                    var tcNo = student.TC ?? "";
                    var cacheKey = CacheKeyPrefixTc + tcNo;
                    var studentName = $"{student.ADI} {student.SOYADI}".Trim();

                    if (_cache.TryGetValue(cacheKey, out CachedSyncResult? cached) && cached != null)
                    {
                        results.Add(new ESrcStudentSyncResult
                        {
                            TcNo = tcNo,
                            StudentName = studentName,
                            Success = cached.Success,
                            Message = cached.Message,
                            FromCache = true,
                            ESrcMessages = cached.ESrcMessages,
                            Timestamp = DateTime.UtcNow
                        });
                        continue;
                    }

                    var (success, message, messages) = await PostToESrcAsync(student, config).ConfigureAwait(false);

                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(config.CacheTTLMinutes));
                    _cache.Set(cacheKey, new CachedSyncResult(success, message, messages), cacheOptions);

                    results.Add(new ESrcStudentSyncResult
                    {
                        TcNo = tcNo,
                        StudentName = studentName,
                        Success = success,
                        Message = message,
                        FromCache = false,
                        ESrcMessages = messages,
                        Timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new ESrcBatchSyncResponse { Results = results });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ESrcExternalDataController] Error in batch sync with e-src.net");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private async Task<(bool Success, string Message, List<MsgBox>? Messages)> PostToESrcAsync(
            StudentDataModel student,
            (string ApiUrl, string KursMail, string Pass, string Subesi, string Grup, string Donem, int CacheTTLMinutes, int TimeoutSeconds) config)
        {
            var payload = new RemoteApiModel
            {
                KURSMAIL = config.KursMail,
                PASS = config.Pass,
                TC = student.TC ?? "",
                ADI = student.ADI ?? "",
                SOYADI = student.SOYADI ?? "",
                EMAIL = student.EMAIL ?? "",
                IL = student.IL ?? "",
                ILCE = student.ILCE ?? "",
                ADRES = student.ADRES ?? "",
                IMG = student.IMG ?? "",
                BELGE = student.BELGE ?? "",
                CINSIYET = student.CINSIYET ?? "",
                BAKIYE = student.BAKIYE ?? 0,
                GSM = student.GSM ?? "",
                SUBESI = config.Subesi,
                GRUP = config.Grup,
                DONEM = config.Donem
            };

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(config.ApiUrl, content).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            List<MsgBox>? messages = null;
            try
            {
                messages = JsonConvert.DeserializeObject<List<MsgBox>>(responseBody);
            }
            catch
            {
                // e-src may return non-JSON or different shape
            }

            if (messages == null)
                messages = new List<MsgBox>();

            var hasError = messages.Exists(m => !string.IsNullOrWhiteSpace(m.MessageError) || !string.IsNullOrWhiteSpace(m.MessagesDanger));
            var success = response.IsSuccessStatusCode && !hasError;
            var message = DeriveMessageFromMsgBox(messages);
            if (message.Length == 0 && !success)
                message = $"HTTP {(int)response.StatusCode}";
            return (success, message, messages);
        }

        private static string DeriveMessageFromMsgBox(List<MsgBox> messages)
        {
            var parts = new List<string>();
            foreach (var m in messages)
            {
                if (!string.IsNullOrWhiteSpace(m.MessageSuccess)) parts.Add(m.MessageSuccess);
                if (!string.IsNullOrWhiteSpace(m.MessageWarning)) parts.Add(m.MessageWarning);
                if (!string.IsNullOrWhiteSpace(m.MessageError)) parts.Add(m.MessageError);
                if (!string.IsNullOrWhiteSpace(m.MessagesDanger)) parts.Add(m.MessagesDanger);
            }
            return string.Join("; ", parts);
        }

        /// <summary>
        /// Invalidates cache for a specific student by studentId and/or tcNo.
        /// </summary>
        [HttpPost("invalidate-cache")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public IActionResult InvalidateCache([FromQuery] int? studentId, [FromQuery] string? tcNo)
        {
            try
            {
                if (!studentId.HasValue && string.IsNullOrWhiteSpace(tcNo))
                    return BadRequest("Either studentId or tcNo must be provided");

                if (studentId.HasValue)
                    _cache.Remove(CacheKeyPrefixId + studentId.Value);
                if (!string.IsNullOrWhiteSpace(tcNo))
                    _cache.Remove(CacheKeyPrefixTc + tcNo);

                return Ok(new { message = "Cache invalidated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ESrcExternalDataController] Error invalidating cache");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private sealed class CachedSyncResult
        {
            public bool Success { get; }
            public string Message { get; }
            public List<MsgBox>? ESrcMessages { get; }

            public CachedSyncResult(bool success, string message, List<MsgBox>? esrcMessages)
            {
                Success = success;
                Message = message;
                ESrcMessages = esrcMessages;
            }
        }
    }
}
