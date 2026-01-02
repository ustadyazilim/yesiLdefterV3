using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Ustad.API.Models;

namespace Ustad.API.Services
{
    /// <summary>
    /// Service for caching e-src.net external data sync responses
    /// </summary>
    public class ESrcExternalDataCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<ESrcExternalDataCacheService> _logger;
        private readonly int _cacheTTLMinutes;

        public ESrcExternalDataCacheService(
            IMemoryCache memoryCache,
            ILogger<ESrcExternalDataCacheService> logger,
            int cacheTTLMinutes = 60)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _cacheTTLMinutes = cacheTTLMinutes;
        }

        /// <summary>
        /// Generates cache key for student sync
        /// </summary>
        private string GenerateCacheKey(int? studentId, string? tcNo)
        {
            if (studentId.HasValue && !string.IsNullOrWhiteSpace(tcNo))
            {
                return $"esrc_external_data_sync_{studentId}_{tcNo}";
            }
            if (studentId.HasValue)
            {
                return $"esrc_external_data_sync_{studentId}";
            }
            if (!string.IsNullOrWhiteSpace(tcNo))
            {
                return $"esrc_external_data_sync_tc_{tcNo}";
            }
            return $"esrc_external_data_sync_{Guid.NewGuid()}";
        }

        /// <summary>
        /// Gets cached response if available
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="tcNo">TC number</param>
        /// <returns>Cached response or null if not found</returns>
        public ESrcExternalDataSyncResponse? GetCachedResponse(int? studentId, string? tcNo)
        {
            try
            {
                string cacheKey = GenerateCacheKey(studentId, tcNo);
                if (_memoryCache.TryGetValue(cacheKey, out ESrcExternalDataSyncResponse? cachedResponse))
                {
                    _logger.LogInformation(
                        "[ESrcExternalDataCacheService] Cache hit for key: {CacheKey}",
                        cacheKey);
                    return cachedResponse;
                }

                _logger.LogDebug(
                    "[ESrcExternalDataCacheService] Cache miss for key: {CacheKey}",
                    cacheKey);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ESrcExternalDataCacheService] Error getting cached response");
                return null;
            }
        }

        /// <summary>
        /// Sets cached response with TTL
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="tcNo">TC number</param>
        /// <param name="response">Response to cache</param>
        public void SetCachedResponse(int? studentId, string? tcNo, ESrcExternalDataSyncResponse response)
        {
            try
            {
                string cacheKey = GenerateCacheKey(studentId, tcNo);
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheTTLMinutes),
                    SlidingExpiration = TimeSpan.FromMinutes(_cacheTTLMinutes / 2),
                    Priority = CacheItemPriority.Normal
                };

                _memoryCache.Set(cacheKey, response, cacheOptions);
                _logger.LogInformation(
                    "[ESrcExternalDataCacheService] Cached response for key: {CacheKey} (TTL: {TTL} minutes)",
                    cacheKey, _cacheTTLMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ESrcExternalDataCacheService] Error setting cached response");
            }
        }

        /// <summary>
        /// Invalidates cache for a specific student
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="tcNo">TC number (optional)</param>
        public void InvalidateCache(int? studentId, string? tcNo)
        {
            try
            {
                string cacheKey = GenerateCacheKey(studentId, tcNo);
                _memoryCache.Remove(cacheKey);
                _logger.LogInformation(
                    "[ESrcExternalDataCacheService] Invalidated cache for key: {CacheKey}",
                    cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ESrcExternalDataCacheService] Error invalidating cache");
            }
        }
    }
}

