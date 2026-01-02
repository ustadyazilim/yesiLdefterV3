using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ustad.API.Models;

namespace Ustad.API.Services
{
    /// <summary>
    /// Service for syncing student data with e-src.net external API
    /// Implements retry logic with exponential backoff
    /// </summary>
    public class ESrcExternalDataService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ESrcExternalDataService> _logger;
        private readonly HttpClient _httpClient;
        private readonly ESrcExternalDataCredentials _credentials;
        private readonly int _retryAttempts;
        private readonly int _timeoutSeconds;

        public ESrcExternalDataService(
            IConfiguration configuration,
            ILogger<ESrcExternalDataService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _credentials = LoadCredentials();
            _retryAttempts = _credentials.RetryAttempts;
            _timeoutSeconds = _credentials.TimeoutSeconds;
            _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
        }

        /// <summary>
        /// Loads e-src.net external data credentials from configuration
        /// </summary>
        private ESrcExternalDataCredentials LoadCredentials()
        {
            return new ESrcExternalDataCredentials
            {
                ApiUrl = Environment.GetEnvironmentVariable("ESRC_EXTERNAL_DATA_API_URL") 
                    ?? _configuration["ESrcExternalData:ApiUrl"] 
                    ?? "https://e-src.net/api/StudentApi/NewStudent",
                KursMail = Environment.GetEnvironmentVariable("ESRC_EXTERNAL_DATA_KURSMAIL") 
                    ?? _configuration["ESrcExternalData:KursMail"] 
                    ?? string.Empty,
                Pass = Environment.GetEnvironmentVariable("ESRC_EXTERNAL_DATA_PASS") 
                    ?? _configuration["ESrcExternalData:Pass"] 
                    ?? string.Empty,
                Subesi = Environment.GetEnvironmentVariable("ESRC_EXTERNAL_DATA_SUBESI") 
                    ?? _configuration["ESrcExternalData:Subesi"] 
                    ?? string.Empty,
                Grup = Environment.GetEnvironmentVariable("ESRC_EXTERNAL_DATA_GRUP") 
                    ?? _configuration["ESrcExternalData:Grup"] 
                    ?? string.Empty,
                Donem = Environment.GetEnvironmentVariable("ESRC_EXTERNAL_DATA_DONEM") 
                    ?? _configuration["ESrcExternalData:Donem"] 
                    ?? string.Empty,
                CacheTTLMinutes = int.TryParse(
                    Environment.GetEnvironmentVariable("ESRC_EXTERNAL_DATA_CACHE_TTL_MINUTES") 
                    ?? _configuration["ESrcExternalData:CacheTTLMinutes"], 
                    out var cacheTTL) ? cacheTTL : 60,
                RetryAttempts = int.TryParse(
                    Environment.GetEnvironmentVariable("ESRC_EXTERNAL_DATA_RETRY_ATTEMPTS") 
                    ?? _configuration["ESrcExternalData:RetryAttempts"], 
                    out var retries) ? retries : 3,
                TimeoutSeconds = int.TryParse(
                    Environment.GetEnvironmentVariable("ESRC_EXTERNAL_DATA_TIMEOUT_SECONDS") 
                    ?? _configuration["ESrcExternalData:TimeoutSeconds"], 
                    out var timeout) ? timeout : 30
            };
        }

        /// <summary>
        /// Builds RemoteApiModel from student data and credentials
        /// </summary>
        public RemoteApiModel BuildRemoteApiModel(StudentDataModel studentData, decimal balance)
        {
            return new RemoteApiModel
            {
                KURSMAIL = _credentials.KursMail,
                PASS = _credentials.Pass,
                TC = studentData.TC ?? string.Empty,
                ADI = studentData.ADI ?? string.Empty,
                SOYADI = studentData.SOYADI ?? string.Empty,
                EMAIL = studentData.EMAIL ?? string.Empty,
                IL = studentData.IL ?? string.Empty,
                ILCE = studentData.ILCE ?? string.Empty,
                ADRES = studentData.ADRES ?? string.Empty,
                IMG = studentData.IMG ?? string.Empty,
                BELGE = studentData.BELGE ?? string.Empty,
                CINSIYET = studentData.CINSIYET ?? string.Empty,
                BAKIYE = balance,
                GSM = studentData.GSM ?? string.Empty,
                SUBESI = _credentials.Subesi,
                GRUP = _credentials.Grup,
                DONEM = _credentials.Donem
            };
        }

        /// <summary>
        /// Calls e-src.net external API with retry logic (exponential backoff)
        /// Attempt 1: Immediate
        /// Attempt 2: Wait 1 second
        /// Attempt 3: Wait 2 seconds
        /// Attempt 4: Wait 4 seconds
        /// </summary>
        public async Task<ESrcExternalDataSyncResponse> SyncStudentAsync(
            RemoteApiModel remoteApiModel,
            int? studentId,
            string? tcNo)
        {
            int attempt = 0;
            int totalAttempts = _retryAttempts + 1; // +1 for initial attempt

            while (attempt < totalAttempts)
            {
                try
                {
                    attempt++;
                    _logger.LogInformation(
                        "[ESrcExternalDataService] Attempting e-src.net external data sync (Attempt {Attempt}/{TotalAttempts}) for student {StudentId}",
                        attempt, totalAttempts, studentId);

                    var response = await CallESrcExternalDataApiAsync(remoteApiModel);

                    _logger.LogInformation(
                        "[ESrcExternalDataService] Successfully synced student {StudentId} to e-src.net on attempt {Attempt}",
                        studentId, attempt);

                    return new ESrcExternalDataSyncResponse
                    {
                        Success = true,
                        Message = "Successfully synced with e-src.net",
                        StudentId = studentId,
                        TcNo = tcNo,
                        ESrcExternalDataResponse = response,
                        SyncTimestamp = DateTime.UtcNow,
                        RetryAttempts = attempt - 1
                    };
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(
                        "[ESrcExternalDataService] HTTP error on attempt {Attempt}/{TotalAttempts} for student {StudentId}: {Error}",
                        attempt, totalAttempts, studentId, ex.Message);

                    if (attempt >= totalAttempts)
                    {
                        return new ESrcExternalDataSyncResponse
                        {
                            Success = false,
                            Message = $"Failed to sync with e-src.net after {totalAttempts} attempts",
                            StudentId = studentId,
                            TcNo = tcNo,
                            SyncTimestamp = DateTime.UtcNow,
                            RetryAttempts = attempt - 1,
                            ErrorDetails = ex.Message
                        };
                    }

                    // Exponential backoff: 1s, 2s, 4s
                    int delaySeconds = (int)Math.Pow(2, attempt - 1);
                    _logger.LogInformation(
                        "[ESrcExternalDataService] Waiting {DelaySeconds} seconds before retry...",
                        delaySeconds);
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), CancellationToken.None);
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogWarning(
                        "[ESrcExternalDataService] Timeout on attempt {Attempt}/{TotalAttempts} for student {StudentId}",
                        attempt, totalAttempts, studentId);

                    if (attempt >= totalAttempts)
                    {
                        return new ESrcExternalDataSyncResponse
                        {
                            Success = false,
                            Message = $"Request timeout after {totalAttempts} attempts",
                            StudentId = studentId,
                            TcNo = tcNo,
                            SyncTimestamp = DateTime.UtcNow,
                            RetryAttempts = attempt - 1,
                            ErrorDetails = "Request timeout"
                        };
                    }

                    // Exponential backoff
                    int delaySeconds = (int)Math.Pow(2, attempt - 1);
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "[ESrcExternalDataService] Unexpected error on attempt {Attempt}/{TotalAttempts} for student {StudentId}",
                        attempt, totalAttempts, studentId);

                    return new ESrcExternalDataSyncResponse
                    {
                        Success = false,
                        Message = "Unexpected error during sync",
                        StudentId = studentId,
                        TcNo = tcNo,
                        SyncTimestamp = DateTime.UtcNow,
                        RetryAttempts = attempt - 1,
                        ErrorDetails = ex.Message
                    };
                }
            }

            // Should not reach here, but return failure response
            return new ESrcExternalDataSyncResponse
            {
                Success = false,
                Message = "Failed to sync with e-src.net",
                StudentId = studentId,
                TcNo = tcNo,
                SyncTimestamp = DateTime.UtcNow,
                RetryAttempts = attempt - 1
            };
        }

        /// <summary>
        /// Makes HTTP POST request to e-src.net external API
        /// </summary>
        private async Task<string> CallESrcExternalDataApiAsync(RemoteApiModel remoteApiModel)
        {
            try
            {
                string jsonPayload = JsonConvert.SerializeObject(remoteApiModel);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                _logger.LogDebug(
                    "[ESrcExternalDataService] Sending POST request to {ApiUrl}",
                    _credentials.ApiUrl);

                var response = await _httpClient.PostAsync(_credentials.ApiUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "[ESrcExternalDataService] e-src.net API returned error: {StatusCode} - {ResponseBody}",
                        response.StatusCode, responseBody);
                    throw new HttpRequestException(
                        $"e-src.net API returned error: {response.StatusCode} - {responseBody}");
                }

                _logger.LogInformation(
                    "[ESrcExternalDataService] e-src.net API response: {StatusCode} - {ResponseBody}",
                    response.StatusCode, responseBody);

                return responseBody;
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("[ESrcExternalDataService] Request timeout");
                throw;
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ESrcExternalDataService] Error calling e-src.net API");
                throw new HttpRequestException($"Error calling e-src.net API: {ex.Message}", ex);
            }
        }
    }
}

